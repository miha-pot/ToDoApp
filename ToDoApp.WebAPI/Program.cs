using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ToDoApp.WebAPI.Endpoints.v1;
using ToDoApp.WebAPI.Extensions;
using ToDoApp.WebAPI.Extensions.Main;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// REGISTER SERVICES (Using Extension Methods)
// ==========================================

builder.AddSerilogLogging();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddIdentityAndAuth(builder.Configuration);
builder.Services.AddSwaggerAndVersioning();
builder.Services.AddProblemDetails();
builder.Services.AddCustomRateLimiter();


var hc = builder.Services.AddHealthChecks();
hc.AddCheck(name: "self-live",
            check: () => HealthCheckResult.Healthy("The service is running."),
            tags: ["live"]);

hc.AddNpgSql(
    connectionString: builder.Configuration.GetConnectionString("PostgresConnection")!,
    name: "postgresql-db",
    tags: ["ready"],
    timeout: TimeSpan.FromSeconds(3)
);


// ==========================================
// MIDDLEWARE PIPELINE
// ==========================================

var app = builder.Build();

app.UseStatusCodePages();
app.UseExceptionHandler();


// Configure the HTTP request pipeline. 
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //app.UseHttpsRedirection();
}

//app.UseHsts();

app.UseRouting();
app.UseCors();
app.UseRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.UseHealthChecks("/health", new HealthCheckOptions
{
    AllowCachingResponses = true,
    Predicate = (check) => check.Tags.Contains("live"),
});

app.UseHealthChecks("/ready", new HealthCheckOptions
{
    AllowCachingResponses = true,
    Predicate = (check) => check.Tags.Contains("ready")
});

if (app.Configuration.GetValue<bool>("Migrations:RunOnStartup"))
{
    await app.ApplyMigrations();
}

// ==========================================
// ENDPOINTS & VERSIONING
// ==========================================

var apiVersionSet = app.NewApiVersionSet()
                   .HasApiVersion(new ApiVersion(1, 0))
                   .Build();

var versionedGroup = app.MapGroup("api/v{version:apiVersion}")
                        .WithApiVersionSet(apiVersionSet);

versionedGroup.MapAuthEndpoints();
versionedGroup.MapTagEndpoints();
versionedGroup.MapToDoEndpoints();

app.Run();
using Asp.Versioning;
using ToDoApp.WebAPI.Endpoints.v1;
using ToDoApp.WebAPI.Extensions;
using ToDoApp.WebAPI.Extensions.Main;

var builder = WebApplication.CreateBuilder(args);
Console.OutputEncoding = System.Text.Encoding.UTF8;
// ==========================================
// REGISTER SERVICES (Using Extension Methods)
// ==========================================

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddIdentityAndAuth(builder.Configuration);
builder.Services.AddSwaggerAndVersioning();
builder.Services.AddProblemDetails();
builder.Services.AddCustomRateLimiter();

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
}

app.UseHsts();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

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
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Application.RepositoryContracts;
using ToDoApp.Application.ServiceContracts;
using ToDoApp.Application.ServiceContracts.Identity;
using ToDoApp.Application.Services;
using ToDoApp.Application.Services.Identity;
using ToDoApp.Domain.RepositoryContracts;
using ToDoApp.Infrastructure.DatabaseContext;
using ToDoApp.Infrastructure.Repositories;
using ToDoApp.Infrastructure.Repositories.EF;
using ToDoApp.Infrastructure.Services;
using ToDoApp.Shared.AuthDTO.Register;
using ToDoApp.WebAPI.Exceptions;

namespace ToDoApp.WebAPI.Extensions.Main;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DbConnection") ??
                throw new InvalidOperationException("Connection string 'DbConnection' not found.");

            options.UseSqlServer(connectionString);
        });

        // Repositories
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITodoItemRepository, TodoItemRepository>();
        services.AddScoped<IQueryRepository, QueryRepository>();

        // Business Services
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserRepository, CurrentUserRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IToDoService, ToDoService>();

        // System Utilities
        services.AddHttpContextAccessor();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policyBuilder =>
            {
                var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ??
                    throw new InvalidOperationException("AllowedOrigins not found.");

                policyBuilder.WithOrigins(allowedOrigins)
                             .AllowAnyMethod()
                             .AllowAnyHeader();
            });
        });

        return services;
    }
}

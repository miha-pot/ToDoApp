using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ToDoApp.Infrastructure.DatabaseContext;

namespace ToDoApp.Infrastructure.IntegrationTests;

public class ToDoApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Odstrani obstoječo registracijo baze
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // 2. Dodaj In-Memory bazo za testiranje
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryTestDb");
            });

            services.AddTransient<FakeAuthorizationHandler>();

            //// Dodaj ga v verigo HttpClienta
            //services.AddHttpClient<ApiService>(client =>
            //{
            //    client.BaseAddress = new Uri("http://localhost:5000/");
            //})
            //.AddHttpMessageHandler<FakeAuthorizationHandler>();
        });
    }
}

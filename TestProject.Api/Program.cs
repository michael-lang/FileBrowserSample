using TestProject.Storage;

namespace TestProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // ##########  Start Application specific DI services
            // inject scoped for a new instance every web request. Singleton would also be ok for these.
            builder.Services.AddScoped<IFileValidationService, DefaultFileValidationService>();
            // can inject a single service that implements more than one interface
            builder.Services.AddScoped<IFileBrowser, DiskFileBrowser>();
            builder.Services.AddScoped<IFileContentData, DiskFileBrowser>();
            // ##########  End Application specific DI services

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }
    }
}
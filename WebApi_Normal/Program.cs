using System.Text;
using System.Text.Json;
using Renci.SshNet;
using WebApi_Normal.Config;
using WebApi_Normal.Infraestructure.Messaging;
using WebApi_Normal.Infraestructure.Providers;
using WebApi_Normal.Infraestructure.Repositories;
using WebApi_Normal.Interfaces;   

namespace WebApi_Normal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));
            builder.Services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppSettings>>().Value);

            builder.Services.AddScoped<ITecnicoRepository, TecnicoCsvRepository>();
            builder.Services.AddHttpClient<ISmsService, SmsService>();

            var tempConfig = builder.Configuration.GetSection("App").Get<AppSettings>() ?? new AppSettings();
            switch ((tempConfig.ProviderMode ?? "auto").ToLowerInvariant())
            {
                case "process":
                    builder.Services.AddScoped<IIncidenteProvider, PythonProcessProvider>();
                    break;
                case "ssh":
                    builder.Services.AddScoped<IIncidenteProvider, PythonSshProvider>();
                    break;
                default: // auto
                    if (OperatingSystem.IsWindows())
                        builder.Services.AddScoped<IIncidenteProvider, PythonProcessProvider>();
                    else
                        builder.Services.AddScoped<IIncidenteProvider, PythonSshProvider>();
                    break;
            }

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();

            

        }
    }
}

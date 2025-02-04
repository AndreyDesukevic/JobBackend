
using JobMonitor.Application.Interfaces;
using JobMonitor.Infrastructure.HttpClients;
using JobMonitor.Infrastructure.Services;

namespace job_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddUserSecrets<Program>();
            string apiKey = builder.Configuration["OpenAI:ApiKey"];

            builder.Services.AddScoped<OpenAiService>(provider =>
                new OpenAiService(apiKey));

            builder.Services.AddHttpClient<IHeadHunterHttpClient, HeadHunterHttpClient>();
            builder.Services.AddScoped<IHeadHunterService, HeadHunterService>();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

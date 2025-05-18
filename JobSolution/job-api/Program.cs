using JobMonitor.Application.Interfaces;
using JobMonitor.Application.Models;
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
            string apiKeyOpenAi = builder.Configuration["OpenAI:ApiKey"];
            string apiKeyDeepSeek = builder.Configuration["DeepSeek:ApiKey"];

            builder.Services.Configure<HeadHunterConfig>(builder.Configuration.GetSection("HeadHunter"));
            builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("Jwt"));

            builder.Services.AddScoped<OpenAiService>(provider =>
                new OpenAiService(apiKeyOpenAi));
            builder.Services.AddScoped<DeepSeekHttpClient>(provider =>
               new DeepSeekHttpClient(apiKeyDeepSeek));

            builder.Services.AddHttpClient<HeadHunterAuthService>();
            builder.Services.AddScoped<HeadHunterAuthService>();

            builder.Services.AddHttpClient<IHeadHunterHttpClient, HeadHunterHttpClient>();
            builder.Services.AddScoped<IHeadHunterService, HeadHunterService>();

            builder.Services.AddScoped<DeepSeekService>();

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:5173");
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.AllowCredentials();
                });
            });

            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseCors();
            app.UseHttpsRedirection();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

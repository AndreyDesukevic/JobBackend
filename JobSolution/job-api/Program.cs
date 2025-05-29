using JobMonitor.Application.Services;
using JobMonitor.Domain.Interfaces;
using JobMonitor.Domain.Interfaces.Repositories;
using JobMonitor.Domain.Interfaces.Services;
using JobMonitor.Domain.Models.Configs;
using JobMonitor.Infrastructure.Database;
using JobMonitor.Infrastructure.Database.Repositories;
using JobMonitor.Infrastructure.HttpClients;
using JobMonitor.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace job_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddUserSecrets<Program>();
            string apiKeyOpenAi = builder.Configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI ApiKey is not configured.");
            string apiKeyDeepSeek = builder.Configuration["DeepSeek:ApiKey"] ?? throw new InvalidOperationException("DeepSeek ApiKey is not configured.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
              options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<HeadHunterConfig>(builder.Configuration.GetSection("HeadHunter"));
            builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("Jwt"));

            builder.Services.AddScoped<OpenAiService>(provider =>
                new OpenAiService(apiKeyOpenAi));
            builder.Services.AddScoped<DeepSeekHttpClient>(provider =>
               new DeepSeekHttpClient(apiKeyDeepSeek));

            builder.Services.AddHttpClient<HeadHunterAuthService>();
            builder.Services.AddScoped<HeadHunterAuthService>();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAppTokenService, AppTokenService>();
            builder.Services.AddScoped<IHhTokenService, HhTokenService>();

            builder.Services.AddScoped<IAppTokenRepository, AppTokenRepository>();
            builder.Services.AddScoped<IHhTokenRepository, HhTokenRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

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

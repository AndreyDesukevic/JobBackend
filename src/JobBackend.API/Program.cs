using Hangfire;
using Hangfire.PostgreSql;
using JobBackend.Application.Attributes;
using JobBackend.Application.RabbitMq;
using JobBackend.Application.Services;
using JobBackend.Application.SignalR;
using JobBackend.Domain.Interfaces;
using JobBackend.Domain.Interfaces.RabbitMq;
using JobBackend.Domain.Interfaces.Repositories;
using JobBackend.Domain.Interfaces.Services;
using JobBackend.Domain.Models.Configs;
using JobBackend.Infrastructure.Database;
using JobBackend.Infrastructure.Database.Repositories;
using JobBackend.Infrastructure.HttpClients;
using JobBackend.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Text;

namespace JobBackend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

#if !DEBUG
    
        builder.Configuration.AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: false);
#endif

        string apiKeyOpenAi = builder.Configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI ApiKey is not configured.");
        string apiKeyDeepSeek = builder.Configuration["DeepSeek:ApiKey"] ?? throw new InvalidOperationException("DeepSeek ApiKey is not configured.");
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
          options.UseNpgsql(connectionString));

        builder.Services.AddHangfire((serviceProvider, configuration) =>
        {
            configuration.UsePostgreSqlStorage(
                options =>
                {
                    options.UseNpgsqlConnection(connectionString);
                },
                new PostgreSqlStorageOptions
                {
                    SchemaName = "hangfire",
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(5)
                });
        });

        builder.Services.AddHangfireServer();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                        ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.")))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/searchStatusHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        Log.Logger = new LoggerConfiguration()
           .ReadFrom.Configuration(builder.Configuration)
           .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .WriteTo.File(
               path: "Logs/log-.txt",
               rollingInterval: RollingInterval.Day,
               retainedFileCountLimit: 7,
               fileSizeLimitBytes: 10_000_000,
               rollOnFileSizeLimit: true,
               shared: true,
               flushToDiskInterval: TimeSpan.FromSeconds(1))
           .CreateLogger();

        builder.Host.UseSerilog();

        builder.Services.Configure<HeadHunterConfig>(builder.Configuration.GetSection("HeadHunter"));
        builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("Jwt"));
        builder.Services.Configure<RabbitMqConfig>(builder.Configuration.GetSection("RabbitMQ"));
        builder.Services.AddSignalR();

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
        builder.Services.AddScoped<IJobTrackerRepository, JobTrackerRepository>();
        builder.Services.AddSingleton<RabbitMqService>();
        builder.Services.AddScoped<ISearchQueueService, SearchQueueService>();
        builder.Services.AddScoped<HhAuthorizationFilter>();
        builder.Services.AddScoped<IJobService, JobService>();

        builder.Services.AddHttpClient<IHeadHunterHttpClient, HeadHunterHttpClient>();
        builder.Services.AddScoped<IHeadHunterService, HeadHunterService>();

        builder.Services.AddScoped<DeepSeekService>();

        // Add services to the container.

        builder.Services.AddControllers();

        //builder.Services.AddCors(options =>
        //{
        //    options.AddPolicy("frontendRequest", policy =>
        //    {
        //        policy.WithOrigins("http://localhost:5173");
        //        policy.AllowAnyHeader();
        //        policy.AllowAnyMethod();
        //        policy.AllowCredentials();
        //    });
        //});
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin",
                builder =>
                {
                    builder.WithOrigins("http://159.223.19.114") // сюда ваш фронтенд-адрес
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials(); // если используете куки/авторизацию
                });
        });

        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        // Панель мониторинга Hangfire
        app.UseHangfireDashboard("/hangfire");

        //app.UseCors("frontendRequest");
        app.UseCors("AllowSpecificOrigin");
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<SearchStatusHub>("/searchStatusHub");

        app.Run();
    }
}

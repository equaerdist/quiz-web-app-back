using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using quiz_web_app.Hubs;
using quiz_web_app.Infrastructure;
using quiz_web_app.Infrastructure.Consumers.QuizCreatedEventConsumer;
using quiz_web_app.Infrastructure.Extensions;
using quiz_web_app.Infrastructure.Middlewares;
using quiz_web_app.Services.BackgroundServices;
using Serilog;

internal class Program
{
    private static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.ConfigureSerilog();
        var config = builder.Configuration.Get<AppConfig>(opt => opt.BindNonPublicProperties = true) ?? throw new ArgumentNullException();
        #region Сервисы авторизции и аутентификации
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearerWithConfig(config);
      
        builder.Services.AddAuthorization(opts =>
        {
            opts.AddPolicy("confirmed", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    var user = context.User;
                    var hasClaim = user.HasClaim(c => c.Type == "Confirmed" && c.Value == "True");
                    return hasClaim;
                });
            });
        });
        #endregion
        builder.Services.AddServices(config);
        builder.Services.AddHostedService<RemoveUnacceptUsers>();
        builder.Services.AddControllers().AddNewtonsoftJson();
        builder.Services.AddSignalR();
        builder.Services.AddCors();
        var app = builder.Build();

        app.UseRouting();

        app.UseCors(options => options
            .WithOrigins("https://localhost:5173", "http://localhost")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
        app.UseSerilogRequestLogging();
        app.UseAuthentication();
        app.UseAuthorization();
     

        app.UseMiddleware<GlobalExceptionHandler>();
        app.MapHub<QuizHub>("api/quizHub");
        app.MapControllers();
        app.Run();
    }
}
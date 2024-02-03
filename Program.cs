using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using quiz_web_app.Hubs;
using quiz_web_app.Infrastructure;
using quiz_web_app.Infrastructure.Extensions;
using quiz_web_app.Infrastructure.Middlewares;
using quiz_web_app.Services.BackgroundServices;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.ConfigureSerilog();
        var config = builder.Configuration.Get<AppConfig>(opt => opt.BindNonPublicProperties = true) ?? throw new ArgumentNullException();
        #region Сервисы авторизции и аунтефикации
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearerWithConfig(config);
        builder.Services.AddHostedService<RemoveUnacceptUsers>();
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
        builder.Services.AddControllers().AddNewtonsoftJson();
        builder.Services.AddSignalR();
        builder.Services.AddCors();
        var app = builder.Build();
        app.UseCors(opt => opt.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                );
        app.MapHub<QuizHub>("api/quiz");
        app.UseMiddleware<GlobalExceptionHandler>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapControllers();
        app.UseAuthorization();
        app.Map("/test", async (IAmazonS3 client) =>
        {
            var response = await client.GetObjectAsync("quizwebapp", "index.html");

            return Results.File(response.ResponseStream, response.Headers.ContentType);
        });
        app.Run();
    }
}
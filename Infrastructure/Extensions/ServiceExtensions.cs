﻿
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using FluentValidation;
using Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using quiz_web_app.Data;
using quiz_web_app.Infrastructure.Middlewares;
using quiz_web_app.Infrastructure.ValidationModels;
using quiz_web_app.Services.Auth_Service;
using quiz_web_app.Services.Email;
using quiz_web_app.Services.Hasher;
using quiz_web_app.Services.IYAGpt;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace quiz_web_app.Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        static AWSCredentials LoadSsoCredentials(string profile)
        {
            var chain = new CredentialProfileStoreChain();
            if (!chain.TryGetAWSCredentials(profile, out var credentials))
                throw new Exception($"Failed to find the {profile} profile");
            return credentials;
        }
        #region Конфигурация jwtToken на работу с куки и настройка валидации токена
        public static AuthenticationBuilder AddJwtBearerWithConfig(this AuthenticationBuilder builder, AppConfig config)
        {
            return builder.AddJwtBearer((options) =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = config.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Key)),

                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["Authorization"];
                        return Task.CompletedTask;
                    }
                };
            });
        }
        #endregion
        public static IServiceCollection AddServices(this IServiceCollection services, AppConfig config)
        {
            var smtpClient = new SmtpClient(config.SmtpServer, System.Convert.ToInt32(config.SmtpPort));
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(config.EmailUsername, config.EmailPassword);
            smtpClient.EnableSsl = true;

            var credentials = LoadSsoCredentials("default");
            
            services.AddFluentEmail(config.EmailUsername, "quiz mesh application")
                            .AddRazorRenderer()
                            .AddSmtpSender(smtpClient);

            return services.AddSingleton(config)
                            .AddScoped<ITokenDistributor, TokenDistributor>()
                            .AddDbContext<QuizAppContext>()
                            .AddValidation()
                            .AddTransient<GlobalExceptionHandler>()
                            .AddAutoMapper(typeof(MapperProfiles.MapperProfiles))
                            .AddSingleton<IHasher, Hasher>()
                            .AddScoped<IEmailService, EmailService>()
                            .AddSingleton<IAmazonS3>(opt => new AmazonS3Client(credentials))
                            .AddScoped<IYAGpt, YAGptClient>()
                            .AddHttpClient();
        }
        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            return services.AddScoped<IValidator<UserDto>, UserValidation>();
        }
    }
}

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
using MassTransit;
using quiz_web_app.Infrastructure.Consumers.QuizCreatedEventConsumer;
using quiz_web_app.Infrastructure.Consumers.UserRegisteredEventConsumer;
using GreenPipes;
using StackExchange.Redis;
using Microsoft.AspNetCore.SignalR;
using quiz_web_app.Hubs;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using quiz_web_app.Services.Repositories.QuizRepository;
using Newtonsoft.Json;
using quiz_web_app.Infrastructure.Consumers.UserCompleteQuizEventConsumer;
using quiz_web_app.Services.KeyResolver;

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
                        context.Token = context.Request.Headers[config.AuthorizationAlias];
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
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            { 
                ReferenceLoopHandling  = ReferenceLoopHandling.Ignore,
            };
            services.AddFluentEmail(config.EmailUsername, "quiz mesh application")
                            .AddRazorRenderer()
                            .AddSmtpSender(smtpClient);
            return services.AddSingleton(config)
                            .AddScoped<ITokenDistributor, TokenDistributor>()
                            .AddDbContext<QuizAppContext>()
                            .AddValidation()
                            .AddSingleton<IKeyResolver, KeyResolver>()
                            .AddSingleton<IUserIdProvider, CustomUserIdProvider>()
                            .AddTransient<GlobalExceptionHandler>()
                            .AddAutoMapper(typeof(MapperProfiles.MapperProfiles))
                            .AddSingleton<IHasher, Hasher>()
                            .AddScoped<IEmailService, EmailService>()
                            .AddSingleton<IAmazonS3>(opt => new AmazonS3Client(credentials))
                            .AddScoped<IYAGpt, YAGptClient>()
                            .AddScoped<IQuizRepository, QuizRepository>()
                            .AddHttpClient()
                            .AddStackExchangeRedisCache(conf =>
                            {
                                conf.Configuration = config.RedisString;
                                conf.InstanceName = "app";
                            })
                            .AddSingleton<ConnectionMultiplexer>(opt => ConnectionMultiplexer.Connect(config.RedisString))
                            .AddSingleton<RedLockFactory>(
                                 opt => RedLockFactory.Create(
                                    new List<RedLockMultiplexer>()
                                    {
                                        opt.GetRequiredService<ConnectionMultiplexer>()
                                    }
                                )
                            )
                            .AddMassTransit(opt =>
                            {
                                opt.SetKebabCaseEndpointNameFormatter();
                                opt.AddConsumer<QuizCreatedEventConsumer>(x => x.UseMessageRetry(t => t.Interval(2, 700)));
                                opt.AddConsumer<UserRegisteredEventConsumer>(x => x.UseMessageRetry(t => t.Interval(2, 1000)));
                                opt.AddConsumer<UserCompleteQuizEventConsumer>(x => x.UseMessageRetry(t => t.Interval(2, 1000)));
                                opt.UsingRabbitMq((ctx, cfg) =>
                                {
                                    cfg.Host(config.RabbitHost, h =>
                                    {
                                        h.Username(config.RabbitUser);
                                        h.Password(config.RabbitPassword);
                                    });
                                    cfg.Message<UserCompleteQuizEvent>(x => x.SetEntityName("user_complete_quiz"));
                                    cfg.Publish<UserCompleteQuizEvent>(opt =>
                                    {
                                        opt.Durable = true;
                                        opt.AutoDelete = true;
                                    });
                                    cfg.ReceiveEndpoint("single-queue", options =>
                                    {
                                        options.ConfigureConsumeTopology = false;
                                        options.Bind("user_complete_quiz", x =>
                                        {
                                            x.Durable = true;
                                            x.AutoDelete = true;
                                        });
                                        options.ConfigureConsumer<UserCompleteQuizEventConsumer>(ctx);
                                    });
                                    cfg.ConfigureEndpoints(ctx);
                                });
                            }).AddMassTransitHostedService();
        }
        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            return services.AddScoped<IValidator<UserDto>, UserValidation>();
        }
    }
}

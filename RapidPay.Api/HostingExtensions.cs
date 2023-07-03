using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RapidPay.Core.DTOs.Card;
using RapidPay.Core.DTOs.Payment;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;
using RapidPay.Core.Interfaces.Services;
using RapidPay.Core.Services;
using RapidPay.Core.Services.Utils;
using RapidPay.Core.Validators;
using RapidPay.Infrastructure.Data;
using RapidPay.Infrastructure.Data.Repositories;

namespace RapidPay.Api;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        InjectDependencies(builder);
        ConfigureJWTBearerAuthenticationService(builder, configuration);

        builder.Services.AddControllers(opt => { opt.Filters.Add(new AuthorizeFilter()); });

        builder.Services.AddFluentValidationAutoValidation();

        ConfigureSwaggerGenService(builder, configuration);

        return builder.Build();
    }

    private static void ConfigureSwaggerGenService(WebApplicationBuilder builder, ConfigurationManager configuration)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            var scheme = new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl =
                            new Uri(configuration.GetSection("Auth:Swagger:AuthorizationUrl").Get<string>()!),
                        TokenUrl = new Uri(configuration.GetSection("Auth:Swagger:TokenUrl").Get<string>()!)
                    }
                },
                Type = SecuritySchemeType.OAuth2
            };

            options.AddSecurityDefinition("OAuth", scheme);

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Id = "OAuth", Type = ReferenceType.SecurityScheme }
                    },
                    new List<string> { }
                }
            });
        });
    }

    private static void ConfigureJWTBearerAuthenticationService(WebApplicationBuilder builder,
        ConfigurationManager configuration)
    {
        builder.Services
            .AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = configuration.GetSection("Auth:Authority").Get<string>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false
                };
            });
    }

    private static void InjectDependencies(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<RapidPayDbContext>();
        builder.Services.AddScoped<ICardRepository, EFCardRepository>();
        
        builder.Services.AddScoped<ICardNumberUtils, CardNumberUtils>();

        builder.Services.AddSingleton<IUniversalFeeExchangeService, UniversalFeeExchangeService>();
        builder.Services.AddScoped<ICardService, CardService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();

        builder.Services.AddScoped<IValidator<CreateCardRequestDTO>, CreateCardRequestDTOValidator>();
        builder.Services.AddScoped<IValidator<PaymentRequestDTO>, PaymentRequestDTOValidator>();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseStaticFiles();

        ConfigureSwaggerInPipeline(app);

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    private static void ConfigureSwaggerInPipeline(WebApplication app)
    {
        app
            .UseSwagger() // NOTE: Invalid grant on authorize after logout is known issue with swagger: https://community.smartbear.com/t5/Swagger-Open-Source-Tools/Swagger-UI-authorization-modal-logout-authorize-issue/td-p/206773#:~:text=It%20is%20not%20possible%20to%20authorize%20again%20inside,do%20proper%20auth%20actions%20in%20a%20separate%20tab.
            .UseSwaggerUI(options =>
            {
                options.EnablePersistAuthorization();
                options.OAuthClientId("api-swagger");
                options.OAuthScopes("profile", "openid", "api");
                options.OAuthUsePkce();

                options.InjectStylesheet("/content/swagger-extras.css");
            });
    }
}
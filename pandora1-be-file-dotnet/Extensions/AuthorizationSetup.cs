﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using pandora1_be_file_dotnet.Helpers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace pandora1_be_file_dotnet.Extensions
{
    public static class AuthorizationSetup
    {
        public static void AddAuthorizationSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));


            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudience = Appsettings.app(new string[] { "JwtSettings", "Audience" }),
                        ValidIssuer = Appsettings.app(new string[] { "JwtSettings", "Issuer" }),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Appsettings.app(new string[] { "JwtSettings", "SecretKey" }))),
                    };
                });
        }
    }
}

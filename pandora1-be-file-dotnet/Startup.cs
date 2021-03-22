using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pandora1_be_file_dotnet.Extensions;
using pandora1_be_file_dotnet.Helpers;
using pandora1_be_file_dotnet.Middlewares;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace pandora1_be_file_dotnet
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacModuleRegister());
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new Appsettings(Configuration));
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddCorsSetup();
            services.AddSqlsugarSetup();
            services.AddSwaggerSetup();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwaggerMildd(() => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("pandora1_be_file_dotnet.index.html"));

            app.UseCors("AllowCorsPolicys");

            var option = new RewriteOptions();
            option.AddRedirect("^$", "spec"); 
            app.UseRewriter(option);

            app.UseWelcomePage(new WelcomePageOptions
            {
                Path = "/welcome"
            });


            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot")),
                RequestPath = new PathString("/assets")
            });

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

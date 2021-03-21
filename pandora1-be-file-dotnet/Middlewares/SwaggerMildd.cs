using log4net;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace pandora1_be_file_dotnet.Middlewares
{
    public static class SwaggerMildd
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SwaggerMildd));
        public static void UseSwaggerMildd(this IApplicationBuilder app, Func<Stream> streamHtml)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/file/swagger.json", "文件模块");

                    c.EnableFilter();
                    if (streamHtml.Invoke() == null)
                    {
                        var msg = "index.html的属性，必须设置为嵌入的资源";
                        log.Error(msg);
                        throw new Exception(msg);
                    }
                    c.IndexStream = streamHtml;
                    c.RoutePrefix = "spec";
                    c.DocumentTitle = "File SPEC";
                    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                    c.DefaultModelsExpandDepth(-1);
                });
            });
        }
    }
}

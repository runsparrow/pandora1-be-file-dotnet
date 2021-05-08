using Microsoft.AspNetCore.Mvc;
using pandora1_be_file_dotnet.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pandora1_be_file_dotnet.Controllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "file")]
    [Route("v1/api/[Controller]/[action]")]
    public class HealthController : ControllerBase
    {
        [HttpPost]
        public async Task<string> Check_Health()
        {
               return Appsettings.app(new string[] { "Env"});
        }
    }
}

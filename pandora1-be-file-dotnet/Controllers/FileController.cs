using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;

        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get_Env()
        {
            return Appsettings.app(new string[] { "Env" });
        }
    }
}

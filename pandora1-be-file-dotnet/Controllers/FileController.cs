using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using pandora1_be_file_dotnet.Helpers;
using pandora1_be_file_dotnet.Models.Dto;
using System;
using System.Collections.Generic;
using System.IO;
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

        [HttpPost]
        public async Task<ApiResponse<FileResponseDto>> Upload_Big_File([FromServices] IWebHostEnvironment environment)
        {
            string envPath = environment.WebRootPath+ Appsettings.app(new string[] { "TempPath"});
            var fileName = Request.Form["name"];
            var index = Request.Form["chunk"].ToString().ObjToInt();
            var maxChunk = Request.Form["maxChunk"].ToString().ObjToInt();
            var guid = Request.Form["guid"];
            var dir = Path.Combine(envPath, guid);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var filePath = Path.Combine(dir, index.ToString());

            var file = Request.Form.Files["file"];
            var filePathWithFileName = string.Concat(filePath, fileName);
            using (var stream = new FileStream(filePathWithFileName, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            ApiResponse<FileResponseDto> response = new ApiResponse<FileResponseDto>();
            var fileResponseDto = new FileResponseDto();
            if (index == maxChunk - 1)
            {
                fileResponseDto.Completed = true;
            }
            response.Data = fileResponseDto;
            return response;
        }
    }
}

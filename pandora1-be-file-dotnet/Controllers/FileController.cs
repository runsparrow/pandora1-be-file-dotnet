using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;

        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task MergeFileAsync(string rootDir, string fileName, string tempDir)
        {
            var yearDir = DateTime.Now.ToString("yyyy");
            var monthDir = DateTime.Now.ToString("MM");
            var dayDir = DateTime.Now.ToString("dd");
            string envPath = Path.Combine(rootDir , Appsettings.app(new string[] { "UploadFilePath", "VideoPath" }), yearDir, monthDir, dayDir);
            var dir = tempDir;
            var files = Directory.GetFiles(dir);
            var finalDir = envPath;
            if (!Directory.Exists(finalDir))
            {
                Directory.CreateDirectory(finalDir);
            }
            var finalPath = Path.Combine(finalDir, fileName);
            using (var fs = new FileStream(finalPath, FileMode.Create))
            {
                var fileParts = files.OrderBy(x => x.Length).ThenBy(x => x);
                foreach (var part in fileParts)
                {
                    var bytes = await System.IO.File.ReadAllBytesAsync(part);
                    await fs.WriteAsync(bytes, 0, bytes.Length);
                    bytes = null;
                    System.IO.File.Delete(part);
                }
                await fs.FlushAsync();
                fs.Close();
                Directory.Delete(dir);
            }
        }

        [HttpPost]
        public async Task<ApiResponse<FileResponseDto>> Upload_Big_File([FromServices] IWebHostEnvironment environment)
        {
            //List<string> allowPicSuffixAr = new List<string> { ".jpg", ".png", ".jpeg", ".gif",".bmp" };
            //List<string> allowViewSuffixAr = new List<string> { ".mp4", ".mkv", ".mov", ".m4v",".wmv",".avi" , ".flv" };
            ApiResponse<FileResponseDto> response = new ApiResponse<FileResponseDto>();
            if (Request.ContentLength==0)
            {
                throw new ServiceException(ErrorDescriptor.FILE_NULL);
            }
            var file = Request.Form.Files["file"];
            string suffix = Request.Form["suffix"];
            //int flag = -1;
            //if (allowPicSuffixAr.Contains(suffix))
            //{
            //    flag = 0;
            //}
            //if (allowViewSuffixAr.Contains(suffix))
            //{
            //    flag = 1;
            //}
            //if (flag==-1)
            //{
            //    throw new ServiceException(ErrorDescriptor.FILE_FORMAT_ERROR);
            //}
            string envPath = Path.Combine(environment.WebRootPath, Appsettings.app(new string[] { "UploadFilePath", "TempPath" }));
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

            
            var filePathWithFileName = string.Concat(filePath, fileName);
            using (var stream = new FileStream(filePathWithFileName, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
           
            var fileResponseDto = new FileResponseDto();
            if (index == maxChunk - 1)
            {
                await MergeFileAsync(environment.WebRootPath, fileName, dir);
                fileResponseDto.Completed = true;
            }
            response.Data = fileResponseDto;
            return response;
        }
    }
}

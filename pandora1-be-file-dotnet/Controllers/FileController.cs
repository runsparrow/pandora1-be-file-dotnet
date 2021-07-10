using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using pandora1_be_file_dotnet.Helpers;
using pandora1_be_file_dotnet.Models.Dto;
using pandora1_be_file_dotnet.Models.ProxyDto;
using pandora1_be_file_dotnet.Tools;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace pandora1_be_file_dotnet.Controllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "file")]
    [Route("v1/api/[Controller]/[action]")]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private RestClient _client;
        private readonly IHttpContextAccessor _accessor;

        public FileController(ILogger<FileController> logger,IHttpContextAccessor accessor)
        {
            _logger = logger;
            _accessor = accessor;
            _client = new RestClient(Appsettings.app(new string[] { "BaseAPIUrl" }));
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task MergeFileAsync(string rootDir, string fileName, string tempDir)
        {
            List<string> allowPicSuffixAr = new List<string> { ".jpg", ".png", ".jpeg", ".gif", ".bmp" };
            List<string> allowViewSuffixAr = new List<string> { ".mp4", ".mkv", ".mov", ".m4v", ".wmv", ".avi", ".flv" };
            string suffix = Path.GetExtension(fileName);
            int isImage = 1;
            string fileAppSettingPath = "";
            if (allowPicSuffixAr.IndexOf(suffix)!=-1)
            {
                isImage = 0;
                fileAppSettingPath = Appsettings.app(new string[] { "UploadFilePath", "PicPath" });
            }
            if (allowViewSuffixAr.IndexOf(suffix) != -1)
            {
                isImage = 1;
                fileAppSettingPath = Appsettings.app(new string[] { "UploadFilePath", "VideoPath" });
            }
            var yearDir = DateTime.Now.ToString("yyyy");
            var monthDir = DateTime.Now.ToString("MM");
            var dayDir = DateTime.Now.ToString("dd");
            string envPath = Path.Combine(rootDir , fileAppSettingPath, yearDir, monthDir, dayDir);
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
                FileProxyDto dto = new FileProxyDto();
                dto.status = new StatusProxyDto();
                dto.name = fileName;
                dto.classifyName = "";
                dto.isImage = isImage;
                dto.ext = fileName.Substring(fileName.LastIndexOf(".") + 1);
                dto.statusKey = "cms.goods.init";


                dto.url = "/" + finalPath.Substring(finalPath.IndexOf("uploadFiles")).Replace("\\","/");
                dto.isImage = 1;
              
                RestRequest request = new RestRequest("/MIS/CMS/MemberAction/Upload", Method.POST);
                string token = _accessor.HttpContext.Request.Headers["Authorization"];
                token = token.Replace("Bearer ", "");
                dto.memberId = AuthHelper.GetClaimFromToken(token).Id;
                dto.memberName = dto.ownerName  = AuthHelper.GetClaimFromToken(token).Name;
                _logger.LogInformation(dto.memberId+"||"+dto.memberName);
                _client.AddDefaultHeader("Authorization","Bearer "+ token);
                request.AddJsonBody(dto);
                var res=await _client.ExecuteAsync(request);
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

        [HttpPost]
        public async Task<ApiResponse<SingleFileResponseDto>> Upload_Single_File(IFormFile file,[FromServices] IWebHostEnvironment environment)
        {
            ApiResponse<SingleFileResponseDto> response = new ApiResponse<SingleFileResponseDto>();
            var yearDir = DateTime.Now.ToString("yyyy");
            var monthDir = DateTime.Now.ToString("MM");
            var dayDir = DateTime.Now.ToString("dd");
            var uploadFIle = file;
            string returnToRelativePath = Path.Combine( Appsettings.app(new string[] { "UploadFilePath", "PicPath" }), yearDir, monthDir, dayDir).Replace("\\", "/");
            string uploadsFolder = Path.Combine(environment.WebRootPath, Appsettings.app(new string[] { "UploadFilePath", "PicPath" }), yearDir, monthDir, dayDir).Replace("\\", "/");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + uploadFIle.FileName;

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }


            var filePathWithFileName = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePathWithFileName, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            response.Data = new SingleFileResponseDto { RelativePath = Appsettings.app(new string[] { "UploadFilePath", "Uri" })+returnToRelativePath+"/"+ uniqueFileName, FileName = uploadFIle.FileName };
            return response;
        }

        [HttpPost]
        public FileStreamResult FileDownload(FileDownloadDto fileDto, [FromServices] IWebHostEnvironment environment)
        {
            string foldername = "";
            string filepath = Path.Combine(environment.WebRootPath, fileDto.FileUrl);
            var stream = System.IO.File.OpenRead(filepath);
            string fileExt = fileDto.FileUrl.Substring(fileDto.FileUrl.LastIndexOf('.'));  // 这里可以写一个获取文件扩展名的方法，获取扩展名
            //获取文件的ContentType
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            var memi = provider.Mappings[fileExt];
            var fileName = Path.GetFileName(filepath);
            return File(stream, memi, HttpUtility.UrlEncode(fileName, Encoding.GetEncoding("UTF-8")));
        }
    }
}

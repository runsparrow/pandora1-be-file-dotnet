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
using System.Drawing;
using System.Drawing.Imaging;
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
        private static PixelFormat[] indexedPixelFormats = { PixelFormat.Undefined, PixelFormat.DontCare,
                PixelFormat.Format16bppArgb1555, PixelFormat.Format1bppIndexed, PixelFormat.Format4bppIndexed,
                PixelFormat.Format8bppIndexed};

        public FileController(ILogger<FileController> logger, IHttpContextAccessor accessor)
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
            long bytesLength = 0;
            string suffix = Path.GetExtension(fileName);
            int isImage = 1;
            string fileAppSettingPath = "";
            if (allowViewSuffixAr.IndexOf(suffix) != -1)
            {
                isImage = 0;
                fileAppSettingPath = Appsettings.app(new string[] { "UploadFilePath", "VideoPath" });
            }
            if (allowPicSuffixAr.IndexOf(suffix) != -1)
            {
                isImage = 1;
                fileAppSettingPath = Appsettings.app(new string[] { "UploadFilePath", "PicPath" });
            }
            var yearDir = DateTime.Now.ToString("yyyy");
            var monthDir = DateTime.Now.ToString("MM");
            var dayDir = DateTime.Now.ToString("dd");
            string envPath = Path.Combine(rootDir, fileAppSettingPath, yearDir, monthDir, dayDir);
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
                    bytesLength += bytes.Length;
                    await fs.WriteAsync(bytes, 0, bytes.Length);
                    bytes = null;
                    System.IO.File.Delete(part);
                }
                await fs.FlushAsync();
                Image img = Image.FromStream(fs);
                fs.Close();
                Directory.Delete(dir);
                FileProxyDto dto = new FileProxyDto();
                if (allowViewSuffixAr.IndexOf(suffix) != -1)
                {
                    //vidoe
                }
                if (allowPicSuffixAr.IndexOf(suffix) != -1)
                {
                    //pic
                    dto.dpi = img.Width + "*" + img.Height;
                    if (IsPixelFormatIndexed(img.PixelFormat))
                    {
                        Bitmap bmp = new Bitmap(img);
                        var font = new Font(FontFamily.GenericSansSerif, 800, FontStyle.Bold, GraphicsUnit.Pixel);
                        var color = Color.FromArgb(128, 255, 255, 255);
                        var brush = new SolidBrush(color);
                        var point = new Point((img.Width / 2) - ((img.Width / 2) / 2) - 20, (img.Height / 2) - ((img.Height / 2) / 2));
                        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.DrawImage(bmp, 0, 0);

                        g.DrawString("t-pic.cn", font, brush, point);
                        g.Dispose();
                    }
                    else
                    {
                        using (var graphic = Graphics.FromImage(img))
                        {
                            var font = new Font(FontFamily.GenericSansSerif, 800, FontStyle.Bold, GraphicsUnit.Pixel);
                            var color = Color.FromArgb(128, 255, 255, 255);
                            var brush = new SolidBrush(color);
                            var point = new Point((img.Width / 2) - ((img.Width / 2) / 2) - 20, (img.Height / 2) - ((img.Height / 2) / 2));

                            graphic.DrawString("t-pic.cn", font, brush, point);
                        }
                    }
                  
                    finalPath = finalPath.Replace(fileName, "$" + fileName);
                    img.Save(finalPath);
                }


                dto.status = new StatusProxyDto();
                dto.name = fileName;
                dto.size = bytesLength + "";
                dto.classifyName = "";
                dto.isImage = isImage;
                dto.ext = fileName.Substring(fileName.LastIndexOf(".") + 1);
                dto.statusKey = "cms.goods.init";
                dto.dpi = img.Width + "*" + img.Height;


                dto.url = "/" + finalPath.Substring(finalPath.IndexOf("uploadFiles")).Replace("\\", "/");

                RestRequest request = new RestRequest("/MIS/CMS/MemberAction/Upload", Method.POST);
                string token = _accessor.HttpContext.Request.Headers["Authorization"];
                token = token.Replace("Bearer ", "");
                dto.memberId = AuthHelper.GetClaimFromToken(token).Id;
                dto.memberName = dto.ownerName = AuthHelper.GetClaimFromToken(token).Name;
                _logger.LogInformation(dto.memberId + "||" + dto.memberName);
                _client.AddDefaultHeader("Authorization", "Bearer " + token);
                request.AddJsonBody(dto);
                var res = await _client.ExecuteAsync(request);
            }
        }

        [NonAction]
        private static bool IsPixelFormatIndexed(PixelFormat imgPixelFormat)
        {
            foreach (PixelFormat pf in indexedPixelFormats)
            {
                if (pf.Equals(imgPixelFormat)) return true;
            }

            return false;
        }

        [HttpPost]
        public async Task<ApiResponse<FileResponseDto>> Upload_Big_File([FromServices] IWebHostEnvironment environment)
        {
            //List<string> allowPicSuffixAr = new List<string> { ".jpg", ".png", ".jpeg", ".gif",".bmp" };
            //List<string> allowViewSuffixAr = new List<string> { ".mp4", ".mkv", ".mov", ".m4v",".wmv",".avi" , ".flv" };
            ApiResponse<FileResponseDto> response = new ApiResponse<FileResponseDto>();
            if (Request.ContentLength == 0)
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
        public async Task<ApiResponse<SingleFileResponseDto>> Upload_Single_File(IFormFile file, [FromServices] IWebHostEnvironment environment)
        {
            ApiResponse<SingleFileResponseDto> response = new ApiResponse<SingleFileResponseDto>();
            string suffix = Path.GetExtension(file.FileName);
            List<string> allowPicSuffixAr = new List<string> { ".jpg", ".png", ".jpeg", ".gif", ".bmp" };
            List<string> allowViewSuffixAr = new List<string> { ".mp4", ".mkv", ".mov", ".m4v", ".wmv", ".avi", ".flv" };
            string fileAppSettingPath = "";
            if (allowViewSuffixAr.IndexOf(suffix) != -1)
            {
                fileAppSettingPath = Appsettings.app(new string[] { "UploadFilePath", "VideoPath" });
            }
            if (allowPicSuffixAr.IndexOf(suffix) != -1)
            {
                fileAppSettingPath = Appsettings.app(new string[] { "UploadFilePath", "PicPath" });
            }

            var yearDir = DateTime.Now.ToString("yyyy");
            var monthDir = DateTime.Now.ToString("MM");
            var dayDir = DateTime.Now.ToString("dd");
            var uploadFIle = file;
            string returnToRelativePath = Path.Combine(fileAppSettingPath, yearDir, monthDir, dayDir).Replace("\\", "/");
            string uploadsFolder = Path.Combine(environment.WebRootPath, fileAppSettingPath, yearDir, monthDir, dayDir).Replace("\\", "/");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + uploadFIle.FileName;

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            FileProxyDto dto = new FileProxyDto();
            var filePathWithFileName = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePathWithFileName, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            string dpi = "";
            if (allowViewSuffixAr.IndexOf(suffix) != -1)
            {
                //vidoe
            }
            if (allowPicSuffixAr.IndexOf(suffix) != -1)
            {
                //pic
                Image img = Image.FromFile(filePathWithFileName);
                dpi = img.Width + "*" + img.Height;
                using (var graphic = Graphics.FromImage(img))
                {
                    var font = new Font(FontFamily.GenericSansSerif, 800, FontStyle.Bold, GraphicsUnit.Pixel);
                    var color = Color.FromArgb(128, 255, 255, 255);
                    var brush = new SolidBrush(color);
                    var point = new Point((img.Width / 2) - ((img.Width / 2) / 2) - 20, (img.Height / 2) - ((img.Height / 2) / 2));

                    graphic.DrawString("t-pic.cn", font, brush, point);
                }
                filePathWithFileName = filePathWithFileName.Replace(uniqueFileName, "$" + uniqueFileName);
                img.Save(filePathWithFileName);
            }

            //dto.status = new StatusProxyDto();
            //dto.name = filePathWithFileName.Substring(filePathWithFileName.LastIndexOf("\\")+1);
            //dto.size = file.Length + "";
            //dto.classifyName = "";
            //dto.isImage = 1;
            //dto.ext = suffix;
            //dto.statusKey = "cms.goods.init";

            //dto.url = "/" + filePathWithFileName.Substring(filePathWithFileName.IndexOf("uploadFiles")).Replace("\\", "/");

            //RestRequest request = new RestRequest("/MIS/CMS/MemberAction/Upload", Method.POST);
            //string token = _accessor.HttpContext.Request.Headers["Authorization"];
            //token = token.Replace("Bearer ", "");
            //dto.memberId = AuthHelper.GetClaimFromToken(token).Id;
            //dto.memberName = dto.ownerName = AuthHelper.GetClaimFromToken(token).Name;
            //_logger.LogInformation(dto.memberId + "||" + dto.memberName);
            //_client.AddDefaultHeader("Authorization", "Bearer " + token);
            //request.AddJsonBody(dto);
            //var res = await _client.ExecuteAsync(request);

            response.Data = new SingleFileResponseDto { RelativePath = Appsettings.app(new string[] { "UploadFilePath", "Uri" }) + returnToRelativePath + "/" + "$" + uniqueFileName, FileName = uploadFIle.FileName, Dpi = dpi };
            return response;
        }

        [HttpPost]
        public async Task<ApiResponse<SingleFileResponseDto>> Upload_HeaderLogo_Single_File(IFormFile file, [FromServices] IWebHostEnvironment environment)
        {
            ApiResponse<SingleFileResponseDto> response = new ApiResponse<SingleFileResponseDto>();
            string suffix = Path.GetExtension(file.FileName);
            List<string> allowPicSuffixAr = new List<string> { ".jpg", ".png", ".jpeg", ".gif", ".bmp" };
            List<string> allowViewSuffixAr = new List<string> { ".mp4", ".mkv", ".mov", ".m4v", ".wmv", ".avi", ".flv" };
            string fileAppSettingPath = "";
            if (allowViewSuffixAr.IndexOf(suffix) != -1)
            {
                fileAppSettingPath = Appsettings.app(new string[] { "UploadFilePath", "VideoPath" });
            }
            if (allowPicSuffixAr.IndexOf(suffix) != -1)
            {
                fileAppSettingPath = Appsettings.app(new string[] { "UploadFilePath", "PicPath" });
            }

            var yearDir = DateTime.Now.ToString("yyyy");
            var monthDir = DateTime.Now.ToString("MM");
            var dayDir = DateTime.Now.ToString("dd");
            var uploadFIle = file;
            string returnToRelativePath = Path.Combine(fileAppSettingPath, yearDir, monthDir, dayDir).Replace("\\", "/");
            string uploadsFolder = Path.Combine(environment.WebRootPath, fileAppSettingPath, yearDir, monthDir, dayDir).Replace("\\", "/");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + uploadFIle.FileName;

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            FileProxyDto dto = new FileProxyDto();
            var filePathWithFileName = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePathWithFileName, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            string dpi = "";
            if (allowViewSuffixAr.IndexOf(suffix) != -1)
            {
                //vidoe
            }
            if (allowPicSuffixAr.IndexOf(suffix) != -1)
            {
                //pic
                Image img = Image.FromFile(filePathWithFileName);
                dpi = img.Width + "*" + img.Height;
            }

            //dto.status = new StatusProxyDto();
            //dto.name = filePathWithFileName.Substring(filePathWithFileName.LastIndexOf("\\")+1);
            //dto.size = file.Length + "";
            //dto.classifyName = "";
            //dto.isImage = 1;
            //dto.ext = suffix;
            //dto.statusKey = "cms.goods.init";

            //dto.url = "/" + filePathWithFileName.Substring(filePathWithFileName.IndexOf("uploadFiles")).Replace("\\", "/");

            //RestRequest request = new RestRequest("/MIS/CMS/MemberAction/Upload", Method.POST);
            //string token = _accessor.HttpContext.Request.Headers["Authorization"];
            //token = token.Replace("Bearer ", "");
            //dto.memberId = AuthHelper.GetClaimFromToken(token).Id;
            //dto.memberName = dto.ownerName = AuthHelper.GetClaimFromToken(token).Name;
            //_logger.LogInformation(dto.memberId + "||" + dto.memberName);
            //_client.AddDefaultHeader("Authorization", "Bearer " + token);
            //request.AddJsonBody(dto);
            //var res = await _client.ExecuteAsync(request);

            response.Data = new SingleFileResponseDto { RelativePath = Appsettings.app(new string[] { "UploadFilePath", "Uri" }) + returnToRelativePath + "/" + uniqueFileName, FileName = uploadFIle.FileName, Dpi = dpi };
            return response;
        }

        [HttpPost]
        public FileStreamResult FileDownload(FileDownloadDto fileDto, [FromServices] IWebHostEnvironment environment)
        {
            string foldername = "";
            string filepath = Path.Combine(environment.WebRootPath, fileDto.FileUrl.Replace("$", ""));
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

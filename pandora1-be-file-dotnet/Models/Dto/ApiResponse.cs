using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pandora1_be_file_dotnet.Models.Dto
{
    public class ApiResponse
    {
        public long Code { get; set; }
        public string Message { get; set; }

        public ApiResponse()
        {
            Code = 200;
            Message = "success";
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }
    }
}

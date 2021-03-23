using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pandora1_be_file_dotnet.Helpers
{
    public class ErrorDescriptor
    {
        private readonly long _errorCode;
        private readonly string _errorMessage;

        public ErrorDescriptor(long errorCode, string errorMessage)
        {
            _errorCode = errorCode;
            _errorMessage = errorMessage;
        }
        public long errorCode { get { return _errorCode; } }
        public string errorMessage { get { return _errorMessage; } }

        public static ErrorDescriptor FILE_FORMAT_ERROR = new ErrorDescriptor(10002,  "file format error");
        public static ErrorDescriptor FILE_NULL = new ErrorDescriptor(10001, "please upload file");
    }
}

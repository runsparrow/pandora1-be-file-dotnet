using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pandora1_be_file_dotnet.Helpers
{
    public class ServiceException : Exception
    {
        public ErrorDescriptor Descriptor { get; set; }
        /// <param name="descriptor"></param>
        public ServiceException(ErrorDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        public override string ToString()
        {
            var baseContent = base.ToString();
            return $" errorCode:{Descriptor.errorCode};\r\n errorMessage:{Descriptor.errorMessage};\r\n trackingData:{baseContent}";
        }
    }
}

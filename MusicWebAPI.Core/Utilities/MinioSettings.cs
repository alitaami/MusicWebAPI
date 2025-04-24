using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Core
{
    public class MinioOptions
    {
        public string URL { get; set; } = null!;
        public string ROOT_USER { get; set; } = null!;
        public string ROOT_PASSWORD { get; set; } = null!;
        public string BUCKET_NAME { get; set; } = null!;
        public string FILE_SERVER_ADDRESS { get; set; } = null!;
    }
}

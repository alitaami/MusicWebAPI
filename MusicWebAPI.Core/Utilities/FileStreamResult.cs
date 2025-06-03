using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Core.Utilities
{
    public class FileStreamResult
    {
        public Stream Stream { get; set; }
        public bool IsPartial { get; set; }
        public string ContentRange { get; set; }
        public string ContentType { get; set; }
    }
}

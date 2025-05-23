using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Core.Utilities
{
    public class RedisSettings
    { 
        public int SlidingExpiration { get; set; }
        public string ConnectionString { get; set; }
        public string ApplicationName { get; set; }
        public bool ByPassCache { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.AWS.Model
{
    public class S3Settings
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string imageUploadPath { get; set; }
        public string imageBucket { get; set; }

    }
}

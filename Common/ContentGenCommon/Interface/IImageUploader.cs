using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Content.Interface
{
    public interface IImageUploader
    {
        Task<string> UploadImage(string imagePath, string bucketName);
        Task<string> UploadImageFromUrl(string imageUrl, string SaveAsImageName);
    }

}

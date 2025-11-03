using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadPath;

        public FileService()
        {
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            var safeFileName = Path.GetFileName(fileName);
            var filePath = Path.Combine(_uploadPath, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            return $"/uploads/{safeFileName}";
        }

        public Task DeleteFileAsync(string filePath)
        {
            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);

            return Task.CompletedTask;
        }

        public Task<Stream> DownloadFileAsync(string filePath)
        {
            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
            if (!File.Exists(physicalPath))
                throw new FileNotFoundException("File not found");

            var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read);
            return Task.FromResult<Stream>(stream);
        }
    }
}

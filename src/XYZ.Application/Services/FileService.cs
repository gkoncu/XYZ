using System;
using System.IO;
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

            await using (var stream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read))
            {
                await fileStream.CopyToAsync(stream);
            }

            return $"/uploads/{safeFileName}";
        }

        public Task DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return Task.CompletedTask;

                var physicalPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    filePath.TrimStart('/'));

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            return Task.CompletedTask;
        }

        public Task<Stream> DownloadFileAsync(string filePath)
        {
            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
            if (!File.Exists(physicalPath))
                throw new FileNotFoundException("File not found");

            var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Task.FromResult<Stream>(stream);
        }
    }
}

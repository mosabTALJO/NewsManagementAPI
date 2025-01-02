using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace NewsManagementAPI.Services
{
    public interface IFileService
    {
        public Task<string[]?> CreateNewFiles(IFormFile[] files);
        public Task<string?> CreateNewFile(IFormFile file);
        public Task<string> CreateNewFile(Uri uri);
        public string GetFilePath(string fileId);
        public string GetFileUrl(string fileId);
        public string GetFilePath(string fileId = "", int w = 150, int h = 150, bool crop = true);
        public string GetFilePath(string fileId = "", int w = 150, bool crop = true);
        public string? GetProductDescription(int ProductId, string language);
        public void SaveProductDescription(int ProductId, string language, string FullDescription);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _staticUrlBase = "app/Images";
        public FileService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<string?> CreateNewFile(IFormFile file)
        {
            if (file == null)
                return null;
            return await GenerateFile(file);
        }
        public async Task<string[]?> CreateNewFiles(IFormFile[] files)
        {
            if (files == null || files.Length == 0)
                return null;

            var fileIds = new List<string>();
            foreach (var file in files)
                fileIds.Add(await GenerateFile(file));

            return fileIds.ToArray();
        }
        public async Task<string> CreateNewFile(Uri uri)
        {
            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
            var extenstion = Path.GetExtension(uriWithoutQuery);
            var newFileId = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;
            var dir = Path.Combine(_env.ContentRootPath, "images", now.Year.ToString(), now.Month.ToString(), now.Day.ToString());

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var fullPath = Path.Combine(dir, newFileId + extenstion);

            using var httpClient = new HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(uri);
            await File.WriteAllBytesAsync(fullPath, imageBytes);

            return $"{now.Year}_{now.Month}_{now.Day}_{newFileId + extenstion}";
        }
        public string GetFilePath(string fileId)
        {
            var tmp = fileId.Split("_");
            string physicalPath = tmp.Length == 4
                ? Path.Combine(_env.ContentRootPath, "images", tmp[0], tmp[1], tmp[2], tmp[3])
                : tmp.Length == 2 ? Path.Combine(_env.ContentRootPath, "images", tmp[0], tmp[1])
                : Path.Combine(_env.ContentRootPath, "images", fileId);

            if (!File.Exists(physicalPath))
                physicalPath = _env.ContentRootPath + "\\images\\default.png";

            return physicalPath;
        }
        public string GetFilePath(string fileId = "", int w = 150, int h = 150, bool crop = true)
        {
            var tmp = fileId.Split("_");
            string physicalPath;
            if (tmp.Length == 4)
            {
                physicalPath = Path.Combine(_env.ContentRootPath, "images", tmp[0], tmp[1], tmp[2], tmp[3]);
                if (File.Exists(physicalPath))
                {
                    var thumbPhysicalPath = Path.Combine(_env.ContentRootPath, "images", tmp[0], tmp[1], tmp[2], $"_{w}x{h}{(crop ? "x" : "")}{tmp[3]}");
                    if (!File.Exists(thumbPhysicalPath))
                    {
                        var resizeOptions = new ResizeOptions { Size = new Size(w, h), Mode = crop ? ResizeMode.Crop : ResizeMode.Max, };
                        using var image = Image.Load(physicalPath);
                        image.Mutate(x => x.Resize(resizeOptions));
                        image.Save(thumbPhysicalPath, new JpegEncoder() { Quality = 85 });
                    }
                    physicalPath = thumbPhysicalPath;
                }
            }
            else
                physicalPath = Path.Combine(_env.ContentRootPath, "images", fileId);

            if (!File.Exists(physicalPath))
                physicalPath = Path.Combine(_env.ContentRootPath, "images", "default.png");

            return physicalPath;
        }
        public string GetFilePath(string fileId = "", int w = 150, bool crop = true)
        {
            var tmp = fileId.Split("_");
            string physicalPath;
            if (tmp.Length == 4)
            {
                physicalPath = Path.Combine(_env.ContentRootPath, "images", tmp[0], tmp[1], tmp[2], tmp[3]);
                if (File.Exists(physicalPath))
                {
                    var thumbPhysicalPath = Path.Combine(_env.ContentRootPath, "images", tmp[0], tmp[1], tmp[2], $"_{w}{(crop ? "x" : "")}{tmp[3]}");
                    if (!File.Exists(thumbPhysicalPath))
                    {
                        var resizeOptions = new ResizeOptions { Size = new Size(w), Mode = crop ? ResizeMode.Crop : ResizeMode.Max, };
                        using var image = Image.Load(physicalPath);
                        image.Mutate(x => x.Resize(resizeOptions));
                        image.Save(thumbPhysicalPath, new JpegEncoder() { Quality = 85 });
                    }
                    physicalPath = thumbPhysicalPath;
                }
            }
            else
                physicalPath = Path.Combine(_env.ContentRootPath, "images", fileId);

            if (!File.Exists(physicalPath))
                physicalPath = Path.Combine(_env.ContentRootPath, "images", "default.png");

            return physicalPath;
        }
        public string GetFileUrl(string fileId = "")
        {
            var tmp = fileId.Split("_");
            if (tmp.Length > 0)
            {
                var request = _httpContextAccessor.HttpContext!.Request;
                var url = $"{request.Scheme}://{request.Host}/{_staticUrlBase}";
                foreach (var seg in tmp)
                    url += $"/{seg}";
                return url;
            }
            return string.Empty;
        }
        private async Task<string> GenerateFile(IFormFile file)
        {
            var extenstion = Path.GetExtension(file.FileName);
            var newFileId = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;
            var dir = Path.Combine(_env.ContentRootPath, "images", now.Year.ToString(), now.Month.ToString(), now.Day.ToString());

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var fullPath = Path.Combine(dir, newFileId + extenstion);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return $"{now.Year}_{now.Month}_{now.Day}_{newFileId + extenstion}";
        }
        public void SaveProductDescription(int ProductId, string language, string FullDescription)
        {
            var path = Path.Combine(_env.ContentRootPath, "Descriptions", language, ProductId + "_" + language);
            File.WriteAllText(path, FullDescription);
        }
        public string? GetProductDescription(int ProductId, string language)
        {
            var path = Path.Combine(_env.ContentRootPath, "Descriptions", language, ProductId + "_" + language);
            if (File.Exists(path))
                return File.ReadAllText(path);
            return null;
        }
    }
}
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Fly.Server.Controllers
{
    public class VideosController : Controller
    {
        IHostingEnvironment Hosting;

        public VideosController(IHostingEnvironment hosting)
        {
            Hosting = hosting;
        }

        [HttpGet, Route("~/Videos/Upload")]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpGet, Route("~/Videos")]
        public IActionResult Index()
        {
            ViewBag.Videos = Directory.EnumerateFileSystemEntries(Hosting.WebRootPath, "*.mp4", SearchOption.AllDirectories)
                .Select(x => new FileInfo(x).Name)
                .ToArray();
            return View();
        }

        [HttpPost, Route("~/Videos/Upload/{id}/{name}")]
        public void Upload(string id, string name)
        {
            var fileName = System.Uri.UnescapeDataString(name);

            var fullPath = Path.Combine(Hosting.WebRootPath, "Videos", id, fileName);
            fullPath = fullPath.Replace("/", "\\");

            if (System.IO.File.Exists(fullPath) == false)
            {
                var dir = Path.GetDirectoryName(fullPath);
                Directory.CreateDirectory(dir);

                using (var f = System.IO.File.Open(fullPath, FileMode.OpenOrCreate))
                {
                    Request.Body.CopyTo(f);
                }

                if (fullPath.EndsWith(".bin") && fullPath != "blackbox.bin")
                {
                    //ConvertVideo(fullPath);
                    ExtractCoordinates(fullPath);
                }
            }
        }

        //private static void ConvertVideo(string fullPath)
        //{
        //    var startInfo = new ProcessStartInfo()
        //    {
        //        FileName = @"C:\Users\xunil\Desktop\ffmpeg-20160522-git-566be4f-win64-shared\bin\ffmpeg.exe",
        //        Arguments = $"-codec:v:0 h264 -i \"{fullPath}\" -map 0:0 -vcodec h264 -pix_fmt yuv420p -profile:v baseline -movflags +faststart {fullPath}.mp4",
        //        UseShellExecute = false,
        //        RedirectStandardOutput = true
        //    };

        //    var process = Process.Start(startInfo);
        //    process.WaitForExit();
        //    var result = process.StandardOutput.ReadToEnd();

        //    System.IO.File.WriteAllText(fullPath + ".txt", result);
        //}

        private static void ExtractCoordinates(string fullPath)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = @"C:\Users\xunil\Desktop\avifilewrapperdemo_src\Fly.Blackbox.Console\bin\Debug\Fly.Blackbox.Console.exe",
                Arguments = $"-i \"{fullPath}\" -c \"{fullPath}.route.json\" -m \"{fullPath}.time.json\" -f \"{fullPath}.mp4\"",
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();
        }

        [HttpGet, Route("~/Videos/{file}")]
        public Stream GetVideo(string file)
        {
            var path = Directory.EnumerateFileSystemEntries(Hosting.WebRootPath, "*" + file, SearchOption.AllDirectories)
                .Select(x => new FileInfo(x).FullName)
                .ToArray()[0];
            return System.IO.File.Open(path, FileMode.Open);
        }

        [HttpGet, Route("~/Videos/route/{file}")]
        public Stream GetRouteJson(string file)
        {
            var path = Directory.EnumerateFileSystemEntries(Hosting.WebRootPath, "*" + file.Replace(".mp4","") + ".route.json", SearchOption.AllDirectories)
                .Select(x => new FileInfo(x).FullName)
                .ToArray()[0];
            return System.IO.File.Open(path, FileMode.Open);
        }

        [HttpGet, Route("~/Videos/time/{file}")]
        public Stream GetTimeJson(string file)
        {
            var path = Directory.EnumerateFileSystemEntries(Hosting.WebRootPath, "*" + file.Replace(".mp4", "") + ".time.json", SearchOption.AllDirectories)
                .Select(x => new FileInfo(x).FullName)
                .ToArray()[0];
            return System.IO.File.Open(path, FileMode.Open);
        }
    }
}

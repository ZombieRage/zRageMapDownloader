using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace zRageMapDownloader.Core
{
    public class MapManager
    {
        private HttpClient _httpClient;
        private string _tempFolder;
        private ServerModel _server;
        public bool Canceled { get; set; }

        public MapManager(ServerModel server)
        {
            _tempFolder = Path.GetTempPath() + Guid.NewGuid();
            Directory.CreateDirectory(_tempFolder);

            _httpClient = new HttpClient();
            _server = server;
            Canceled = false;
        }

        public async Task Download(string mapName)
        {
            Canceled = false;

            var mapFile = _server.BuildMapFile(mapName);
            var remoteFile = $"{Utils.NormalizeUrl(_server.FastdlUrl)}{mapFile}";
            var tempFile = Path.Combine(_tempFolder, mapFile);

            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, remoteFile));
            var parallelDownloadSuported = response.Headers.AcceptRanges.Contains("bytes");
            var contentLength = response.Content.Headers.ContentLength ?? 0;

            if (parallelDownloadSuported)
            {
                const double numberOfParts = 5.0;
                var tasks = new List<Task>();
                var partSize = (long)Math.Ceiling(contentLength / numberOfParts);

                File.Create(tempFile).Dispose();

                for (var i = 0; i < numberOfParts; i++)
                {
                    var start = i * partSize + Math.Min(1, i);
                    var end = Math.Min((i + 1) * partSize, contentLength);

                    tasks.Add(
                        Task.Run(() => DownloadPart(remoteFile, tempFile, start, end))
                        );
                }

                await Task.WhenAll(tasks);
            }
        }

        private async void DownloadPart(string url, string saveAs, long start, long end)
        {
            using (var httpClient = new HttpClient())
            using (var fileStream = new FileStream(saveAs, FileMode.Open, FileAccess.Write, FileShare.Write))
            {
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                message.Headers.Add("Range", string.Format("bytes={0}-{1}", start, end));

                fileStream.Position = start;
                await httpClient.SendAsync(message).Result.Content.CopyToAsync(fileStream);
            }
        }

        public void Decompress(string mapName)
        {
            var mapFile = _server.BuildMapFile(mapName);
            var tempFile = Path.Combine(_tempFolder, mapFile);


            FileInfo zipFileName = new FileInfo(tempFile);
            using (FileStream fileToDecompressAsStream = zipFileName.OpenRead())
            {
                string decompressedFileName = Path.Combine(_tempFolder, Path.GetFileName(mapFile)).Replace(".bz2", "");
                using (FileStream decompressedStream = File.Create(decompressedFileName))
                {
                    try
                    {
                        BZip2.Decompress(fileToDecompressAsStream, decompressedStream, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public bool MoveToMapsFolder(string mapName)
        {
            var mapFile = _server.BuildMapFile(mapName);
            var tempFile = Path.Combine(_tempFolder, mapFile);
            var finalFile = Path.Combine(_server.GetMapsDirectory(), mapFile.Replace(".bz2", ""));

            if (File.Exists(finalFile))
            {
                try
                {
                    File.Delete(finalFile);
                }
                catch (Exception)
                {
                    Console.WriteLine("Can't delete " + finalFile);
                    return false;
                }
            }

            try
            {
                File.Move(tempFile, finalFile);
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("Can't move " + finalFile);
                return false;
            }
        }

        public void Cancel()
        {
            Canceled = true;
        }
    }
}

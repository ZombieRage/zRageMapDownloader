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
            _httpClient.Timeout = TimeSpan.FromMinutes(15);
            _server = server;
            Canceled = false;
        }

        public void Download(string mapName)
        {
            var mapFile = _server.BuildMapFile(mapName);
            var remoteFile = $"{Utils.NormalizeUrl(_server.FastdlUrl)}{mapFile}";
            var tempFile = Path.Combine(_tempFolder, mapFile);

            using (var wc = new WebClient())
            {
                wc.DownloadFile(remoteFile, tempFile);
            }
        }

        public void Decompress(string mapName)
        {
            var mapFile = _server.BuildMapFile(mapName);
            var tempFile = Path.Combine(_tempFolder, mapFile);

            int tries = 0;
            bool success = false;
            do
            {
                try
                {
                    FileInfo zipFileName = new FileInfo(tempFile);
                    using (FileStream fileToDecompressAsStream = zipFileName.OpenRead())
                    {
                        string decompressedFileName = Path.Combine(_tempFolder, Path.GetFileName(mapFile)).Replace(".bz2", "");
                        using (FileStream decompressedStream = File.Create(decompressedFileName))
                        {
                            BZip2.Decompress(fileToDecompressAsStream, decompressedStream, true);
                            success = true;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Can't decompress " + mapName);
                }
            } while (tries < 5);

            if (!success)
            {
                throw new Exception("Can't decompress " + mapName);
            }
        }

        public bool MoveToMapsFolder(string mapName)
        {
            var mapFile = _server.BuildMapFile(mapName).Replace(".bz2", "");
            var tempFile = Path.Combine(_tempFolder, mapFile);
            var finalFile = Path.Combine(_server.GetMapsDirectory(), mapFile);

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

            int tries = 0;
            do
            {
                try
                {
                    File.Move(tempFile, finalFile);
                    return true;
                }
                catch (Exception)
                {
                    Console.WriteLine("Can't move " + finalFile);
                    tries++;

                    Thread.Sleep(1000);
                }
            } while (tries < 5);

            return false;
        }

        public void Cancel()
        {
            Canceled = true;
        }

        public void DeleteAllTempFiles()
        {
            var di = new DirectoryInfo(_tempFolder);
            var files = di.EnumerateFiles();

            foreach (var file in files)
            {
                TryDeleteFile(file.FullName);
            }

            // delete the folder if its empty
            if (!di.GetFiles().Any())
            {
                Directory.Delete(_tempFolder);
            }
        }

        private void TryDeleteFile(string file)
        {
            int tries = 0;
            do
            {
                try
                {
                    File.Delete(file);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("can't delete " + file);
                    tries++;
                    Thread.Sleep(1000);
                }
            } while (tries < 5);
        }
    }
}

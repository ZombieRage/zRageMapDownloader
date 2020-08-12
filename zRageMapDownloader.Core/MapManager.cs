using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace zRageMapDownloader.Core
{
    public class MapManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly string MainTempFolder = Path.Combine(Path.GetTempPath(), "zrageTempMaps");
        private readonly string _tempFolder;
        public string MapsDirectory { get; set; }
        private readonly ServerModel _server;

        public bool Canceled { get; set; }

        public MapManager(ServerModel server)
        {
            _tempFolder = Path.Combine(MainTempFolder, Guid.NewGuid().ToString().ToUpper());
            Directory.CreateDirectory(_tempFolder);

            _server = server;
            Canceled = false;
        }

        public void Download(MapModel map)
        {
            var tempFile = Path.Combine(_tempFolder, map.DownloadableFileName);

            using (var wc = new WebClient())
            {
                wc.DownloadFile(map.RemoteFileName, tempFile);
            }
        }

        public void Decompress(MapModel map)
        {
            var tempFile = Path.Combine(_tempFolder, map.DownloadableFileName);

            int tries = 0;
            bool success = false;
            do
            {
                try
                {
                    FileInfo zipFileName = new FileInfo(tempFile);
                    using (FileStream fileToDecompressAsStream = zipFileName.OpenRead())
                    {
                        string decompressedFileName = Path.Combine(_tempFolder, map.LocalFileName);
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
                    Console.WriteLine("Can't decompress " + map);
                }
            } while (tries < 5);

            if (!success)
            {
                throw new Exception("Can't decompress " + map);
            }
        }

        public bool MoveToMapsFolder(MapModel map)
        {
            var tempFile = Path.Combine(_tempFolder, map.LocalFileName);
            var finalFile = Path.Combine(MapsDirectory ?? _server.GetMapsDirectory(), map.LocalFileName);

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

        public static void DeleteAllTempFiles()
        {
            var di = new DirectoryInfo(MainTempFolder);
            var files = di.EnumerateFiles("*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                TryDeleteFile(file.FullName);
            }
        }

        private static void TryDeleteFile(string file)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Windows.Controls;
using System.Windows;
using System.Threading;
using Tea_Launcher.Models;
using System.Security.Cryptography;

namespace Tea_Launcher
{
    class GameManager
    {
        public static async void DownloadGameTask(Game game, IProgress<string> progress, IProgress<float> progressBar, IProgress<float> progressBarMax)
        {

            List<GameFile>[] filesDownload = CheckGameFiles(game.Title);

            //удаляем лишние файлы
            progress.Report("Removing garbage...");
            foreach (GameFile file in filesDownload[0])
                File.Delete(@"Games\" + file.WinPath);


            progressBarMax.Report(filesDownload[1].Count);

            // пока не знаю как прервать async без гемороя, потому отложу эту тему
            //if (filesDownload[1].Count == 0)
            //    progress.Report("No update required");

            string pathRemote = "/var/www/html/public/Games/";
            string pathLocalFile = @"Games\";
            SecretInfo secretInfo = new SecretInfo();
            using (SftpClient sftp = new SftpClient(secretInfo.host, secretInfo.username, secretInfo.password))
            {
                try
                {
                    sftp.Connect();

                    int i = 0;
                    foreach (GameFile gameFile in filesDownload[1])
                    {
                        await DownloadFileAsync(pathRemote + gameFile.UnixPath, pathLocalFile + gameFile.WinPath, sftp);
                        progress.Report(gameFile.WinPath);
                        game.DownloadingFile = ++i;
                        progressBar.Report(++i);
                    }
                    sftp.Disconnect();
                }
                catch (Exception er)
                {
                    MessageBox.Show("An exception has been caught " + er.ToString() + "\n" + er.Message, "Error!");
                }
            }
            progress.Report("Download complete!");
            //progressBar.Report(0);
        }

        static async Task DownloadFileAsync(string source, string destination, SftpClient sftp)
        {
            //Создаем директорию, дабы сука ОНО КАЧАЛОСЬ!
            //если директория есть ниче не произойдет
            DirectoryInfo di = Directory.CreateDirectory(Path.GetDirectoryName(destination));
            using (var saveFile = File.OpenWrite(destination))
            {
                var task = Task.Factory.FromAsync(sftp.BeginDownloadFile(source, saveFile), sftp.EndDownloadFile);
                await task;
            }
        }

        public static List<Game> GetGamesList()
        {	
            WebRequest request = WebRequest.Create("http://170295.simplecloud.ru/launcher/getGames");
            request.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();


            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            //-----------------------
            string[] gameRows = responseFromServer.Split('#');
            Array.Resize(ref gameRows, gameRows.Length - 1); //убираем последний элемент, он пустой (если я не починил это на бекенде)
            //Это рано или поздно наебнется, надо както по другому убрать пустой элемент
            List<Game> games = new List<Game>();
            foreach (string gameRow in gameRows)
            {
                string[] gameParams = gameRow.Split(';');
                Game game = new Game() { Title = gameParams[0], Description = gameParams[1], Screenshot = gameParams[2], Launcher = gameParams[3] };
                games.Add(game);
            }


            return games;
        }

        public static List<GameFile> GetGameFilesRemote(string title)
        {
		
            WebRequest request = WebRequest.Create("http://170295.simplecloud.ru/launcher/getGameFiles/" + title); // Create a request for the URL. 
            request.Credentials = CredentialCache.DefaultCredentials; // If required by the server, set the credentials
            HttpWebResponse response = (HttpWebResponse)request.GetResponse(); // Get the response.

            string SD = response.StatusDescription; // Display the status
            Stream dataStream = response.GetResponseStream(); // Get the stream containing content returned by the server
            StreamReader reader = new StreamReader(dataStream); // Open the stream using a StreamReader for easy access
            string responseFromServer = reader.ReadToEnd(); // Read the content.

            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();

            //-----------------------

            string[] files = responseFromServer.Split('#');
            Array.Resize(ref files, files.Length - 1); //убираем последний элемент, он пустой (если я не починил это на бекенде)
            //Это рано или поздно наебнется, надо както по другому убрать пустой элемент
            List<GameFile> gameFiles = new List<GameFile>();
            foreach (string file in files)
            {
                string[] fileParams = file.Split(';');
                GameFile gameFile = new GameFile() { UnixPath = fileParams[0], Hash = fileParams[1] };
                gameFiles.Add(gameFile);
            }

            return gameFiles;
        }

        public static List<GameFile> GetGameFilesLocal(string title)
        {
            List<GameFile> gameFiles = new List<GameFile>();
            DirSearch(@"Games\" + title, ref gameFiles);
            return gameFiles;
        }

        public static List<GameFile>[] CheckGameFiles(string title)
        {
            List<GameFile> remote = GetGameFilesRemote(title);
            List<GameFile> local = GetGameFilesLocal(title);
            List<GameFile> remove = new List<GameFile>();

            bool flag;
            foreach (GameFile l in local)
            {
                flag = false;
                foreach (GameFile r in remote)
                {
                    if (l.UnixPath == r.UnixPath)
                    {
                        if (l.Hash == r.Hash)
                            remote.Remove(r);
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    remove.Add(l);

            }
            List<GameFile>[] files = new List<GameFile>[2];
            files[0] = remove; //локальные файлы, которые надо удалить
            files[1] = remote; //серверные файлы, которые надо скачаеш :@
            return files;
        }


        public Game JsonToGame(string json)
        {
            json = @"{""user"":{""name"":""asdf"",""teamname"":""b"",""email"":""c"",""players"":[""1"",""2""]}}";
            Game game = new Game();

            return new Game();
        }

        static void DirSearch(string sDir, ref List<GameFile> gameFiles)
        {
            DirectoryInfo di = Directory.CreateDirectory(sDir);
            foreach (string f in Directory.GetFiles(sDir))
            {
                //Console.WriteLine(f);
                string path = f.Replace("Games\\", "");
                string hash = CalculateMD5(f);
                gameFiles.Add(new GameFile() { WinPath = path, Hash = hash });
            }
            foreach (string d in Directory.GetDirectories(sDir))
            {
                DirSearch(d, ref gameFiles);
            }

        }

        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}

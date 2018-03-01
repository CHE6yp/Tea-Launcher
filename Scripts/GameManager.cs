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

namespace Tea_Launcher
{
    class GameManager
    {
        public static string[] GetFileNames()
        {
            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create("http://170295.simplecloud.ru/?n=Burger%20Joint");
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // Display the status.
            string SD = response.StatusDescription;
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();

            //-----------------------
            string[] paths = responseFromServer.Split('!');
            Array.Resize(ref paths, paths.Length - 1); //убираем последний элемент, он пустой (если я не починил это на бекенде)
            //Это рано или поздно наебнется, надо както по другому убрать пустой элемент

            return paths;
        }

        public static async void DownloadGameTask()
        {
            SecretInfo secretInfo = new SecretInfo();
            string[] paths = GetFileNames();

            string pathRemote = "/var/www/html/";

            string pathLocalFile = @"Games\";

            using (SftpClient sftp = new SftpClient(secretInfo.host, secretInfo.username, secretInfo.password))
            {
                try
                {
                    sftp.Connect();
                    
                    foreach (string file in paths)
                    {
                        string winPath = file.Replace('/', '\\');
                        await DownloadFileAsync(pathRemote + file, pathLocalFile + winPath, sftp, new Progress<string>(Notify));
                    }
                    sftp.Disconnect();
                }
                catch (Exception er)
                {
                    MessageBox.Show("An exception has been caught " + er.ToString() + "\n" + er.Message, "Error!");
                }
            }
            MessageBox.Show("Download finished!", "Done");
        }

        static async Task DownloadFileAsync(string source, string destination, SftpClient sftp, IProgress<string> progress)
        {
            //Создаем директорию, дабы сука ОНО КАЧАЛОСЬ!
            //если директория есть ниче не произойдет
            DirectoryInfo di = Directory.CreateDirectory(Path.GetDirectoryName(destination));
            using (var saveFile = File.OpenWrite(destination))
            {
                var task = Task.Factory.FromAsync(sftp.BeginDownloadFile(source, saveFile), sftp.EndDownloadFile);
                await task;
                progress.Report(destination);
            }
        }

        //костыль. Надо понять как работает DataBinding
        public static MainWindow GetMainWindow()
        {
            Window mainWindow = new Window();
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    mainWindow = window;
                }
            }
            return (MainWindow)mainWindow;
        }

        static void Notify(string msg)
        {
            //костыль, основанный на предыдущем костыле
            GetMainWindow().textBox.AppendText(msg + "\n");
        }
    }
}

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

            return paths;
        }



        public static void DownloadGame(string host, string username, string password, ref TextBox textBox)
        {
            string[] paths = GetFileNames();
            //string host = @"yourSftpServer.com";
            //string username = "root";
            //string password = @"p4ssw0rd";

            // Path to file on SFTP server
            //string pathRemoteFile = "/var/www/html/Burger Joint/Burger Joint.exe";
            string pathRemote = "/var/www/html/";
            // Path where the file should be saved once downloaded (locally)
            //string pathLocalFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "bg.exe");
            string pathLocalFile = @"Games\";
            textBox.Clear();

            using (SftpClient sftp = new SftpClient(host, username, password))
            {
                try
                {
                    sftp.Connect();
                    
                    //Console.WriteLine("Downloading {0}", pathRemoteFile);
                    foreach (string file in paths)
                        using (Stream fileStream = File.OpenWrite(pathLocalFile + file.Replace('/', '\\')))
                        {
                            textBox.AppendText(file.Replace('/', '\\'));
                            sftp.DownloadFile(pathRemote + file, fileStream);
                        }

                    sftp.Disconnect();
                }
                catch (Exception er)
                {
                    //Console.WriteLine("An exception has been caught " + er.ToString());
                    MessageBox.Show(er.Message, "An exception has been caught " + er.ToString());
                }
            }

        }
    }
}

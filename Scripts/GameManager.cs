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



        public static void DownloadGame(ref TextBox textBox)
        {
            SecretInfo secretInfo = new SecretInfo();
            string[] paths = GetFileNames();

            string pathRemote = "/var/www/html/";

            string pathLocalFile = @"Games\";
            textBox.Clear();

            using (SftpClient sftp = new SftpClient(secretInfo.host, secretInfo.username, secretInfo.password))
            {
                try
                {
                    sftp.Connect();

                    foreach (string file in paths)
                    {
                        string winPath = file.Replace('/', '\\');
                        //Создаем директорию, дабы сука ОНО КАЧАЛОСЬ!
                        DirectoryInfo di = Directory.CreateDirectory(pathLocalFile + Path.GetDirectoryName(winPath)); 
                        using (Stream fileStream = File.OpenWrite(pathLocalFile + winPath))
                        {
                            textBox.AppendText(winPath);
                            sftp.DownloadFile(pathRemote + file, fileStream);
                        }
                    }
                    sftp.Disconnect();
                }
                catch (Exception er)
                {
                    MessageBox.Show("An exception has been caught " + er.ToString()+"\n"+er.Message,"Error!");
                }
            }

        }


        public static void DownloadGameAsync( ref TextBox textBox)
        {
            SecretInfo secretInfo = new SecretInfo();
            string[] paths = GetFileNames();

            string pathRemote = "/var/www/html/";

            string pathLocalFile = @"Games\";
            TextBox t = textBox;
            t.Clear();

            Thread myThread = new Thread(delegate () {

                using (SftpClient sftp = new SftpClient(secretInfo.host, secretInfo.username, secretInfo.password))
                {
                    try
                    {
                        sftp.Connect();

                        foreach (string file in paths)
                        {
                            string winPath = file.Replace('/', '\\');
                            //Создаем директорию, дабы сука ОНО КАЧАЛОСЬ!
                            DirectoryInfo di = Directory.CreateDirectory(pathLocalFile + Path.GetDirectoryName(winPath));
                            using (Stream fileStream = File.OpenWrite(pathLocalFile + winPath))
                            {
                                //t.AppendText(winPath+"\n");
                                sftp.DownloadFile(pathRemote + file, fileStream);
                            }
                        }
                        sftp.Disconnect();
                    }
                    catch (Exception er)
                    {
                        MessageBox.Show("An exception has been caught " + er.ToString() + "\n" + er.Message, "Error!");
                    }
                }
                MessageBox.Show("Download finished!", "Done");
            });

            myThread.Start();
        }


    }
}

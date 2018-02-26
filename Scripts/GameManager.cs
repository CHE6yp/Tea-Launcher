using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Windows;
using Newtonsoft.Json;


namespace Tea_Launcher
{
    class GameManager
    {

        public static void GetFileNames()
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

            string[] paths = responseFromServer.Split('!');

            Window mainWindow = new Window();
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    mainWindow = window;
                }
            }

            (mainWindow as MainWindow).textBox.Clear();
            
            foreach (string path in paths)
            {
                (mainWindow as MainWindow).textBox.AppendText(path);
                if (path != paths.Last())
                    (mainWindow as MainWindow).textBox.AppendText("\n");
            }
        }
    }
}

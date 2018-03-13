using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tea_Launcher.Models;

namespace Tea_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            fileProgress.Content = "Preparing for download...";
            allProgress.Value = 0;
            var progressHandler = new Progress<string>(value =>
            {
                fileProgress.Content = value;
            });
            var progressBarHandler = new Progress<float>(value =>
            {
                allProgress.Value = value;
            });
            var progress = progressHandler as IProgress<string>;
            var progressBar = progressBarHandler as IProgress<float>;
            GameManager.DownloadGameTask(progress, progressBar);
        }

        private void RefreshGameList_Click(object sender, RoutedEventArgs e)
        {
            List<Game> games = GameManager.GetGamesList();
            foreach (Game game in games)
            {
                Button newBtn = new Button();

                newBtn.DataContext = game;
                newBtn.Content = game.Title;
                newBtn.Name = game.Title.Replace(' ','_') +"_page";

                GamesList.Children.Add(newBtn);
            }
        }
    }
}

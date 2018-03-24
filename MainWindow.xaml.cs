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
using System.Diagnostics;

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
            Button button = (Button)sender;
            Game game = (Game)button.DataContext;

            fileProgress.Content = "Preparing for download...";
            allProgress.Value = 0;
            var progressHandler = new Progress<string>(value =>
            {
                fileProgress.Content = value;
            });
            var progressBarMaxHandler = new Progress<float>(value =>
            {
                //allProgress.Value = value;
                game.FileCount = value;
            });
            var progressBarHandler = new Progress<float>(value =>
            {
                //allProgress.Value = value;
                game.DownloadingFile = value;
            });
            var progress = progressHandler as IProgress<string>;
            var progressBar = progressBarHandler as IProgress<float>;
            var progressBarMax = progressBarMaxHandler as IProgress<float>;



            GameManager.DownloadGameTask(game, progress, progressBar, progressBarMax);
        }

        private void ChooseGame(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            BindingExpression bindingExpression = button.GetBindingExpression(ContentProperty);
            Game game = (Game)bindingExpression.DataItem;
            //Binding parentBinding = bindingExpression.ParentBinding;
            //Title.SetBinding(ContentProperty, parentBinding);
            Title.Content = game.Title;
            Description.Text = game.Description;
            DownloadButton.DataContext = game;
            PlayButton.DataContext = game;

            var fullFilePath = @"http://170295.simplecloud.ru/Screenshots/"+game.Title+"/"+game.Screenshot;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            bitmap.EndInit();
            Screenshot.Source = bitmap;

            allProgress.Maximum = game.FileCount;
            Binding bind = new Binding();
            bind.Source = game;
            bind.Path = new PropertyPath("DownloadingFile");
            bind.Mode = BindingMode.TwoWay;
            bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            BindingOperations.SetBinding(allProgress, ProgressBar.ValueProperty, bind);


        }

        private void RefreshGameList_Click(object sender, RoutedEventArgs e)
        {
            RefreshGameListFunc();
        }

        private void RefreshGameListFunc()
        {
            GamesList.Children.Clear();
            List<Game> games = GameManager.GetGamesList();
            foreach (Game game in games)
            {
                Button newBtn = new Button();
                Binding bind = new Binding();
                bind.Source = game;
                bind.Path = new PropertyPath("Title");
                //bind.Mode = BindingMode.TwoWay;
                //bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                BindingOperations.SetBinding(newBtn, ContentProperty, bind);
                newBtn.Name = game.Title.Replace(' ', '_') + "_page";
                newBtn.Click += ChooseGame;

                GamesList.Children.Add(newBtn);
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Game game = (Game)button.DataContext;
            try
            {
                Process.Start("Games\\" + game.Title + "\\" + game.Launcher);
            }
            catch
            {
                MessageBox.Show("Не вариант запустить! Чето не то!");
            }
        }

        private void GamesList_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshGameListFunc();
        }
    }
}

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
using System.IO;
using IWshRuntimeLibrary;

namespace BackupManager2._0
{
    public partial class MainWindow : Window
    {
        bool StopBackup = false;
        List<string> SpecialExeptionFolders = new List<string>();
        Point DragPoint = new Point();
        bool Draging = false;
        int StandartMargin = 5;

        System.Windows.Forms.FolderBrowserDialog SelectFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(Directory.GetCurrentDirectory() + "\\cfg.txt"))
                LoadBackupSchedule();
            if (System.IO.File.Exists(Directory.GetCurrentDirectory() + "\\ExcludeFolders.txt"))
                LoadExcludeFolders();
            if ((bool)StartWhenOpenCheckBox.IsChecked)
                await RunBackup();
        }

        private void DragBarGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragPoint = e.GetPosition(this);
            Draging = true;

            Mouse.Capture(DragBarGrid);
        }

        private void DragBarGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (Draging)
            {
                Left = (System.Windows.Forms.Cursor.Position.X - DragPoint.X);
                Top = (System.Windows.Forms.Cursor.Position.Y - DragPoint.Y);
            }
        }

        private void DragBarGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Draging = false;
            Mouse.Capture(null);
        }

        private async void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            StopBackup = true;
            while (!StartBackupButton.IsEnabled)
                await Task.Delay(10);
            SaveBackupSchedule();
            Application.Current.Shutdown();
        }

        void LoadExcludeFolders()
        {
            try
            {
                string[] Lines = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + "\\ExcludeFolders.txt", Encoding.UTF8);

                for (int i = 1; i < Lines.Length; i++)
                {
                    SpecialExeptionFolders.Add(Lines[i]);
                }
            }
            catch {  }
        }

        private async void StartBackupButton_Click(object sender, RoutedEventArgs e)
        {
            await RunBackup();
        }

        private void StopBackupButton_Click(object sender, RoutedEventArgs e)
        {
            StopBackupButton.IsEnabled = false;
            StopBackup = true;
        }

        async Task RunBackup()
        {
            bool ErrorOccurred = false;
            StopBackupButton.IsEnabled = true;
            StartBackupButton.IsEnabled = false;
            AddLocationButton.IsEnabled = false;

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "cmd.exe";

            string ExpectioFolders = "";

            foreach (string s in SpecialExeptionFolders)
                ExpectioFolders += s + " ";

            foreach (UIElement Element in FolderViewStackPanel.Children)
            {
                startInfo.Arguments = "/C robocopy \"" + ((Element as Grid).Children[1] as Label).Content + " \" \"" + ((Element as Grid).Children[3] as Label).Content + "\\ \" " + ParametersTextBox.Text + " /XD " + ExpectioFolders;
                if ((bool)DebugModeCheckBox.IsChecked)
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                else
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.StartInfo = startInfo;
                process.Start();
                while (!process.HasExited)
                {
                    for (int i = 0; i < 255; i += 10)
                    {
                        (Element as Grid).Background = new SolidColorBrush(Color.FromArgb(255, (byte)i, (byte)i, 0));
                        await Task.Delay(1);
                    }
                    if (StopBackup)
                    {
                        process.CloseMainWindow();
                        process.Kill();
                        break;
                    }
                }

                if (process.ExitCode == 16)
                {
                    ErrorOccurred = true;
                    (Element as Grid).Background = new SolidColorBrush(Color.FromArgb(255, 149, 49, 54));
                }
                else
                {
                    (Element as Grid).Background = new SolidColorBrush(Color.FromArgb(255, 47, 149, 54));
                }

                if (StopBackup)
                    break;
            }

            foreach (UIElement Element in FolderViewStackPanel.Children)
            {
                (Element as Grid).Background = new SolidColorBrush(Color.FromArgb(255, 47, 49, 54));
            }

            StartBackupButton.IsEnabled = true;
            AddLocationButton.IsEnabled = true;
            StopBackupButton.IsEnabled = false;

            SaveBackupSchedule();

            if (!StopBackup)
                if ((bool)CloseWhenDoneCheckBox.IsChecked)
                    if (!ErrorOccurred)
                        Application.Current.Shutdown();

            StopBackup = false;
        }

        private void AddLocationButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewFolder(null, null);
        }

        void AddNewFolder(string _From, string _To)
        {
            Grid NewGrid = new Grid();
            NewGrid.Background = new SolidColorBrush(Color.FromArgb(255, 47, 49, 54));
            NewGrid.Width = FolderViewStackPanel.Width;
            NewGrid.Height = 40;
            NewGrid.Margin = new Thickness(StandartMargin);
            NewGrid.ColumnDefinitions.Add(new ColumnDefinition());
            NewGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(30) });
            NewGrid.ColumnDefinitions.Add(new ColumnDefinition());
            NewGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(30) });

            Button RemoveFolderButton = new Button();
            RemoveFolderButton.Content = "X";
            RemoveFolderButton.Width = 25;
            RemoveFolderButton.Height = 25;
            RemoveFolderButton.Click += RemoveFolderButton_Click;
            RemoveFolderButton.Style = Resources["StandartExitButtonStyle"] as Style;
            Grid.SetColumn(RemoveFolderButton, 3);
            NewGrid.Children.Add(RemoveFolderButton);

            if (_From == null)
            {
                while (SelectFolderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) { }
                _From = SelectFolderDialog.SelectedPath;
            }

            Label FromFolderLabel = new Label();
            FromFolderLabel.Content = _From;
            FromFolderLabel.ToolTip = _From;
            FromFolderLabel.Style = Resources["StandartLabelStyle"] as Style;
            FromFolderLabel.FontSize = 10;
            Grid.SetColumn(FromFolderLabel, 0);
            NewGrid.Children.Add(FromFolderLabel);

            Label ArrowLabel = new Label();
            ArrowLabel.Content = ">";
            ArrowLabel.Style = Resources["StandartLabelStyle"] as Style;
            Grid.SetColumn(ArrowLabel, 1);
            NewGrid.Children.Add(ArrowLabel);

            if (_To == null)
            {
                while (SelectFolderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) { }
                _To = SelectFolderDialog.SelectedPath;
            }

            Label ToFolderLabel = new Label();
            ToFolderLabel.Content = _To;
            ToFolderLabel.ToolTip = _To;
            ToFolderLabel.Style = Resources["StandartLabelStyle"] as Style;
            ToFolderLabel.FontSize = 10;
            Grid.SetColumn(ToFolderLabel, 2);
            NewGrid.Children.Add(ToFolderLabel);

            FolderViewStackPanel.Children.Add(NewGrid);
        }

        private void RemoveFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Button SenderButton = sender as Button;
            FolderViewStackPanel.Children.Remove(SenderButton.Parent as Grid);
        }

        void SaveBackupSchedule()
        {
            try
            {
                using (StreamWriter SaveFile = new StreamWriter(Directory.GetCurrentDirectory() + "\\cfg.txt", false))
                {
                    SaveFile.WriteLine(RunAtStartupCheckBox.IsChecked + ";" + CloseWhenDoneCheckBox.IsChecked + ";" + StartWhenOpenCheckBox.IsChecked + ";" + DebugModeCheckBox.IsChecked + ";" + Left + ";" + Top);
                    SaveFile.WriteLine(ParametersTextBox.Text);
                    string SerialOut = "";
                    foreach (UIElement c in FolderViewStackPanel.Children)
                    {
                        if (c is Grid)
                        {
                            Grid InnerGrid = c as Grid;
                            SerialOut = (InnerGrid.Children[1] as Label).Content + ";" + (InnerGrid.Children[3] as Label).Content;
                            SaveFile.WriteLine(SerialOut);
                        }
                    }
                }
            }
            catch {  }
        }

        void LoadBackupSchedule()
        {
            try
            {
                string[] Lines = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + "\\cfg.txt", Encoding.UTF8);

                RunAtStartupCheckBox.IsChecked = Convert.ToBoolean(Lines[0].Split(';')[0]);
                CloseWhenDoneCheckBox.IsChecked = Convert.ToBoolean(Lines[0].Split(';')[1]);
                StartWhenOpenCheckBox.IsChecked = Convert.ToBoolean(Lines[0].Split(';')[2]);
                DebugModeCheckBox.IsChecked = Convert.ToBoolean(Lines[0].Split(';')[3]);
                Left = Convert.ToDouble(Lines[0].Split(';')[4]);
                Top = Convert.ToDouble(Lines[0].Split(';')[5]);

                ParametersTextBox.Text = Lines[1];

                for (int i = 2; i < Lines.Length; i++)
                {
                    string[] Split = Lines[i].Split(';');
                    AddNewFolder(Split[0], Split[1]);
                }
            }
            catch {  }
        }

        private void RunAtStartupButton_Unchecked(object sender, RoutedEventArgs e)
        {
            System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\BackupManager2.0.lnk");
        }

        private void RunAtStartupButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\BackupManager2.0.lnk"))
            {
                WshShell shell = new WshShell();
                string shortcutAddress = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\BackupManager2.0.lnk";
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                shortcut.Description = "New shortcut for a Backup Manager 2.0";
                shortcut.TargetPath = Directory.GetCurrentDirectory() + "\\BackupManager2.0.exe";
                shortcut.Save();
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            WindowBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 27, 33, 46));
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            WindowBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 37, 56, 75));
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;
using System.Security.Permissions;
using System.Data.SQLite;
using System.Security.AccessControl;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace CSCDWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Rcranston
    /// CSCD 371 Midterm
    /// this software will watch a selected directory and have the posiblity to save to a database
    /// </summary>
   [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public partial class MainWindow : Window
    {

        private Boolean run = false;
        private Boolean IsValid = false;
        private Boolean DBwrite = true;
        private string dir="";
        private FileSystemWatcher watcher = new FileSystemWatcher();
        private DBWindow dbWindow;
        private int num = 0;

        SQLiteConnection sql_con;
        SQLiteCommand sql_cmd;
        SQLiteDataReader sql_red;



        public MainWindow()
        {
            sql_con = new SQLiteConnection("Data Source=database.db;Version=3;New=True;Compress=True;");
            sql_con.Open();
            sql_cmd= sql_con.CreateCommand();

            //sql_red = sql_cmd.ExecuteReader();

            //sql_cmd.CommandText = "drop table if exists FSW";
            //sql_cmd.ExecuteNonQuery();
            sql_cmd.CommandText = "CREATE TABLE if not exists FSW (id integer primary key, Name varchar(100), FullPath varchar(100), Change varchar(100), Time varchar(100));";
            sql_cmd.ExecuteNonQuery();
            InitializeComponent();
        }


        private void SetupWatcher()
        {

            
            watcher.NotifyFilter = NotifyFilters.LastAccess 
                                    | NotifyFilters.LastWrite
                                    | NotifyFilters.FileName 
                                    | NotifyFilters.DirectoryName;

            if (!extSelct.Text.Equals("Extension to Watch / Enter your Own"))
                watcher.Filter = "*"+extSelct.Text;
            else
                watcher.Filter = "*.*";

            watcher.IncludeSubdirectories = true;
            watcher.Path = dir;

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;

            Status.Content = $" Begining to watch: {watcher.Path} for {watcher.Filter} type files";
            //watcher.Created += Watcher_Created;

            watcher.EnableRaisingEvents = true;
           
            //throw new NotImplementedException();
        }
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
                Dispatcher.BeginInvoke(
                   (Action)(() =>
                   {
                   // Notify user
                   FSEtable.Items.Add(new { First = (e.Name).Split('\\')[(e.Name).Split('\\').Length - 1], Second = e.FullPath, Third = e.ChangeType, Four = DateTime.Now });
                   num++;
                   }));
            
            //throw new NotImplementedException();
        }
        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Dispatcher.BeginInvoke(
               (Action)(() =>
               {
                   // Notify user
                   FSEtable.Items.Add(new { First = (e.Name).Split('\\')[(e.Name).Split('\\').Length - 1], Second = e.OldFullPath, Third = $"renamed to {e.FullPath}", Four = DateTime.Now });
                   num++;
               }));
            //throw new NotImplementedException();
        }

        private void mnuFileExit_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("File System Watcher", "Are you sure you want to Exit?\ncollected data maybe lost","yes");
            

            if (!DBwrite) 
            { 
                const string message ="Would you like to save before Quiting?";
                const string caption = "WARNING";
                var result = MessageBox.Show(message, caption,
                                             (MessageBoxButton)MessageBoxButtons.YesNo,
                                             (MessageBoxImage)MessageBoxIcon.Question);
                if ((int)result == 7)
                {
                    sql_con.Close();
                    Environment.Exit(0);
                }
                for (int z = 0; z < num; z++)
                {
                    Object x = FSEtable.Items.GetItemAt(z);
                    sql_cmd.CommandText = $"INSERT OR REPLACE INTO FSW (Name, FullPath, Change, Time) VALUES ('{x.ToString().Split(',')[0].Substring(10)}','{x.ToString().Split(',')[1].Substring(10)}','{x.ToString().Split(',')[2].Substring(9)}','{x.ToString().Split(',')[3].Substring(8, (x.ToString().Split(',')[3].Substring(8).Length) - 1)}');";
                    sql_cmd.ExecuteNonQuery();
                }
                StDButton.IsEnabled = false;
                Status.Content = "Successfuly Submitted to Database";
                DBwrite = true;
                sql_con.Close();
                Environment.Exit(0);
            }
            sql_con.Close();
            Environment.Exit(0);
        }

        private void mnuHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("File System Watcher, watches the selected file directory an outputs to a database located next to where the app was launched, \nBased on the .Net Framework and developed using WPF \nConstructed by Ryan Cranston");
        }

        private void mnuFileDBWindow_Click(object sender, RoutedEventArgs e)
        {
            this.dbWindow = new DBWindow();
            this.dbWindow.ShowDialog();
        }

        private void smtButton_Click(object sender, RoutedEventArgs e)
        {
            dir = System.IO.Path.GetFullPath(SubText.Text);
            if (!Directory.Exists(dir))
            {
                IsValid = false;
                SubValid.IsChecked = false;
                MessageBox.Show("Invalid Entry, Try again");
                Status.Content = "Please enter a Valid Address to watch";
                strButton.IsEnabled = false;
            }
            else
            {
                IsValid = true;
                SubValid.IsChecked = true;
                strButton.IsEnabled = true;
                Status.Content = "Ready to Begin";
            }
        }

        private void strButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsValid)
            {
                strButton.IsEnabled = false;
                stpButton.IsEnabled = true;
                StDButton.IsEnabled = false;
                run = true;
                DBwrite = false;
                SetupWatcher();
            }
        }

        private void stpButton_Click(object sender, RoutedEventArgs e)
        {
            stpButton.IsEnabled = false;
            run = false;
            watcher.EnableRaisingEvents = false;
            Status.Content = $" Ended watch of :{watcher.Path} for {watcher.Filter} type files";
            strButton.IsEnabled = true;
            StDButton.IsEnabled = true;
        }

        private void mnuClear (object sender, RoutedEventArgs e)
        {
            //FSEtable.Items.Clear();
            Object x =FSEtable.Items.GetItemAt(1);
           Console.WriteLine(x.ToString().Split(',')[0].Substring(10));
            Console.WriteLine(x.ToString().Split(',')[1].Substring(10));
            Console.WriteLine(x.ToString().Split(',')[2].Substring(9));
            Console.WriteLine(x.ToString().Split(',')[3].Substring(8, (x.ToString().Split(',')[3].Substring(8).Length)-1));
        }

        private void StDButton_Click(object sender, RoutedEventArgs e)
        {
            for(int z=0;z<num;z++)
            {
                Object x = FSEtable.Items.GetItemAt(z);
                sql_cmd.CommandText = $"INSERT OR REPLACE INTO FSW (Name, FullPath, Change, Time) VALUES ('{x.ToString().Split(',')[0].Substring(10)}','{x.ToString().Split(',')[1].Substring(10)}','{x.ToString().Split(',')[2].Substring(9)}','{x.ToString().Split(',')[3].Substring(8, (x.ToString().Split(',')[3].Substring(8).Length) - 1)}');";
                sql_cmd.ExecuteNonQuery();
            }
            StDButton.IsEnabled = false;
            Status.Content = "Successfuly Submitted to Database";
            DBwrite = true;
        }

        private void mnuClearDB(object sender, RoutedEventArgs e)
        {
            const string message = "Are you sure youd like to clear the database?";
            const string caption = "WARNING";
            var result = MessageBox.Show(message, caption,
                                         (MessageBoxButton)MessageBoxButtons.YesNo,
                                         (MessageBoxImage)MessageBoxIcon.Question);
            if ((int)result != 7)
            {
                sql_cmd.CommandText = "drop table if exists FSW";
                sql_cmd.ExecuteNonQuery();
                sql_cmd.CommandText = "CREATE TABLE if not exists FSW (id integer primary key, Name varchar(100), FullPath varchar(100), Change varchar(100), Time varchar(100));";
                sql_cmd.ExecuteNonQuery();
                Status.Content = "Successfuly clearned the Database";
            }

        }
    }
}

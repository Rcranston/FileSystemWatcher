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
using System.Windows.Shapes;
using System.Data.SQLite;

namespace CSCDWPF
{
    /// <summary>
    /// Interaction logic for DBWindow.xaml
    /// Rcranston
    /// CSCD 371 Midterm
    /// this software will watch a selected directory and have the posiblity to save to a database
    /// </summary>

    public partial class DBWindow : Window
    {
        SQLiteConnection sql_con;
        SQLiteCommand sql_cmd;
        SQLiteDataReader sql_red;

        String Qur = "";
        public DBWindow()
        {
            InitializeComponent();
            sql_con = new SQLiteConnection("Data Source=database.db;Version=3;New=True;Compress=True;");
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();
        }

        private void SubButtin_Click(object sender, RoutedEventArgs e)
        {
            DBtable.Items.Clear();
            Qur = usrQur.Text;
            try
            {
                sql_cmd.CommandText ="SELECT * FROM FSW " + Qur;
                sql_red = sql_cmd.ExecuteReader();


                while (sql_red.Read()) // Read() returns true if there is still a result line to read
                {
                    // Print out the content of the text field: ,,,
                    //System.Console.WriteLine(sql_red["Name"]);
                    DBtable.Items.Add(new { Zero = sql_red["id"], First = sql_red["Name"], Second = sql_red["FullPath"], Third = sql_red["Change"], Four = sql_red["Time"] });
                }
            }
             catch (Exception x)
            {
                MessageBox.Show("Invalid Entry, Try again\n",x.ToString());
            }
        }
    }
}

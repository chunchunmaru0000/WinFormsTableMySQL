using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace TableMySQL
{
    /// <summary>
    /// Represents a custom connection string for MySQL database, containing server, user credentials, and database information.
    /// </summary>
    public class MyConnectionString
    {
        /// <summary>
        /// Gets or sets the server address for the database connection.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the username for database access.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the password for database access.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the name of the database to connect to.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Initializes a new instance of the MyConnectionString class with the specified server, user, password, and database.
        /// </summary>
        /// <param name="server">The server address of the MySQL database.</param>
        /// <param name="user">The username for database access.</param>
        /// <param name="password">The password for database access.</param>
        /// <param name="database">The name of the database to connect to.</param>
        public MyConnectionString(string server, string user, string password, string database) {
            Server = server;
            User = user;
            Password = password;
            Database = database;
        }

        /// <summary>
        /// Retrieves the constructed connection string based on the provided server, user, password, and database.
        /// </summary>
        /// <returns>The constructed connection string for MySQL database.</returns>
        public string Get()
        {
            return $"server={Server};user id={User};password={Password};database={Database};";
        }
    }

    /// <summary>
    /// Provides methods for working with tables in MySQL and displaying results in RichTextBox and DataGridView controls.
    /// </summary>
    public static class TableHandlerMySQL
    {
        /// <summary>
        /// Displays a success message in the specified RichTextBox control and focuses on the main window.
        /// </summary>
        /// <param name="richTextBox">The RichTextBox control to display the message in.</param>
        /// <param name="mainTextBox">The main RichTextBox control.</param>
        /// <param name="text">The text of the success message.</param>
        public static void Good(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, string text)
        {
            richTextBox.SelectionColor = Color.Green;
            richTextBox.AppendText("success: " + text + "\n");
            richTextBox.Focus();
            mainTextBox.Focus();
        }

        /// <summary>
        /// Displays an error message in the specified RichTextBox control and focuses on the main window.
        /// </summary>
        /// <param name="richTextBox">The RichTextBox control to display the message in.</param>
        /// <param name="mainTextBox">The main RichTextBox control.</param>
        /// <param name="text">The text of the error message.</param>
        /// <param name="code">The error code (default is "none code").</param>
        public static void Bad(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, string text, string code = "none code")
        {
            richTextBox.SelectionColor = Color.Red;
            richTextBox.AppendText($"error code: {code}\n{text}\n");
            richTextBox.Focus();
            mainTextBox.Focus();
        }

        /// <summary>
        /// Executes an SQL query to read data from a MySQL table and displays the result in a DataGridView control.
        /// </summary>
        /// <param name="richTextBox">The RichTextBox control to output information about the query.</param>
        /// <param name="mainTextBox">The main RichTextBox control.</param>
        /// <param name="table">The DataGridView control to display the query results.</param>
        /// <param name="mode">The auto-size mode for columns in the DataGridView.</param>
        /// <param name="connectionString">The connection string to the database.</param>
        /// <param name="query">The SQL query (default is "show databases").</param>
        public static void SqlReadInTable(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, 
                                          ref DataGridView table, DataGridViewAutoSizeColumnMode mode,
                                          string connectionString ,string query = "show databases")
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            table.Columns.Clear();

                            List<string> colls = new List<string>();
                            short readings = 0;

                            while (reader.Read())
                            {
                                if (readings == 0)
                                {
                                    short i = 0;
                                    while (true)
                                    {
                                        try
                                        {
                                            colls.Add(reader.GetName(i));
                                            var coll = new DataGridViewTextBoxColumn()
                                            {
                                                HeaderText = colls.Last(),
                                                AutoSizeMode = mode
                                            };
                                            table.Columns.Add(coll);
                                        }
                                        catch (Exception) { break; }
                                        i++;
                                    }
                                }
                                readings++;
                                if (readings == short.MaxValue) break;

                                List<string> datas = new List<string>();
                                foreach (string coll in colls)
                                {
                                    datas.Add(reader[coll].ToString());
                                }
                                table.Rows.Add(datas.ToArray());
                            }
                        }
                    }
                    table.ClearSelection();
                    connection.Close();
                    Good(ref richTextBox, ref mainTextBox, query);
                }
            }
            catch (MySqlException error) { Bad(ref richTextBox, ref mainTextBox , error.ToString(), query); }
        }

        /// <summary>
        /// Populates a DataGridView control with data from a SQL query without logging, using the specified connection string.
        /// </summary>
        /// <param name="table">The DataGridView control to be populated with data.</param>
        /// <param name="mode">The DataGridViewAutoSizeColumnMode to be applied.</param>
        /// <param name="connectionString">The connection string for accessing the database.</param>
        /// <param name="query">The SQL query to retrieve data (default is "show databases").</param>
        public static void NoLogSqlReadInTable(ref DataGridView table, DataGridViewAutoSizeColumnMode mode,
                                               string connectionString, string query = "show databases")
        {
            var _0 = new RichTextBox();
            SqlReadInTable(ref _0, ref _0, ref table, mode, connectionString, query);
        }
    }

    /// <summary>
    /// Provides methods for handling database operations related to MySQL, including sending requests, testing connections, and connecting to databases.
    /// </summary>
    public static class OtherHandlerMySQL
    {
        /// <summary>
        /// Sends a SQL request to the MySQL database and handles the response.
        /// </summary>
        /// <param name="richTextBox">The RichTextBox control for displaying information about the request.</param>
        /// <param name="mainTextBox">The main RichTextBox control.</param>
        /// <param name="connectionString">The connection string to the database.</param>
        /// <param name="query">The SQL query to be executed.</param>
        static void SendRequest(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, string connectionString, ref string query)
        {
            try
            {
                 MySqlConnection connection = new MySqlConnection(connectionString);
                 connection.Open();
                 using (var command = new MySqlCommand(query, connection))
                 {
                     command.ExecuteNonQuery();
                 }
                 connection.Close();
                 TableHandlerMySQL.Good(ref richTextBox, ref mainTextBox, query);
            }
            catch (MySqlException error) { TableHandlerMySQL.Bad(ref richTextBox, ref mainTextBox, error.ToString(), query); }
        }

        /// <summary>
        /// Tests the connection to the MySQL database using the specified connection string.
        /// </summary>
        /// <param name="connectionStr">The connection string to be tested.</param>
        public static void TestConnection(string connectionStr)
        {
            MySqlConnection connection = new MySqlConnection(connectionStr);
            connection.Open();
            connection.Close();
        }

        /// <summary>
        /// Connects to the specified MySQL database using the provided connection string and handles success or failure.
        /// </summary>
        /// <param name="richTextBox">The RichTextBox control for displaying information about the connection status.</param>
        /// <param name="mainTextBox">The main RichTextBox control.</param>
        /// <param name="connectionString">The original connection string.</param>
        /// <param name="database">The name of the database to connect to.</param>
        public static void ConnectDatabase(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, ref MyConnectionString connectionString, string database)
        {
            var oldconnstr = new MyConnectionString(
                connectionString.Server,
                connectionString.User,
                connectionString.Password,
                connectionString.Database
                );
            try
            {
                connectionString.Database = database;
                TestConnection(connectionString.Get());
                TableHandlerMySQL.Good(ref richTextBox, ref mainTextBox, "use " + database);
            }
            catch (MySqlException error)
            {
                connectionString = oldconnstr;
                TableHandlerMySQL.Bad(ref richTextBox, ref mainTextBox, error.ToString(), "use " + database);
            }
        }
    }

    /// <summary>
    /// Provides methods for handling database operations related to MySQL, including displaying messages in a RichTextBox control and executing SQL queries.
    /// </summary>
    public class ThisHandlerMySQL
    {
        private RichTextBox richTextBox;
        private RichTextBox mainTextBox;
        private MyConnectionString connectionString;

        /// <summary>
        /// Initializes a new instance of the ThisHandlerMySQL class with the specified RichTextBox controls and connection string.
        /// </summary>
        /// <param name="richTextBox">The RichTextBox control for displaying information.</param>
        /// <param name="mainTextBox">The main RichTextBox control.</param>
        /// <param name="connectionString">The connection string to the database.</param>
        public ThisHandlerMySQL(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, MyConnectionString connectionString)
        {
            this.richTextBox = richTextBox;
            this.mainTextBox = mainTextBox;
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Displays a success message in the RichTextBox control with green text color.
        /// </summary>
        /// <param name="text">The text of the success message.</param>
        public void Good(string text)
        {
            richTextBox.SelectionColor = Color.Green;
            richTextBox.AppendText("success: " + text + "\n");
            richTextBox.Focus();
            mainTextBox.Focus();
        }

        /// <summary>
        /// Displays an error message in the RichTextBox control with red text color and an optional error code.
        /// </summary>
        /// <param name="text">The text of the error message.</param>
        /// <param name="code">The error code (default is "none code").</param>
        public void Bad(string text, string code = "none code")
        {
            richTextBox.SelectionColor = Color.Red;
            richTextBox.AppendText($"error code: {code}\n{text}\n");
            richTextBox.Focus();
            mainTextBox.Focus();
        }

        /// <summary>
        /// Executes an SQL query to read data from a MySQL table and displays the result in a DataGridView control.
        /// </summary>
        /// <param name="table">The DataGridView control to display the query results.</param>
        /// <param name="mode">The auto-size mode for columns in the DataGridView.</param>
        /// <param name="query">The SQL query to be executed (default is "show databases").</param>
        public void SqlReadInTable(ref DataGridView table, DataGridViewAutoSizeColumnMode mode, string query = "show databases")
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString.Get()))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            table.Columns.Clear();

                            List<string> colls = new List<string>();
                            short readings = 0;

                            while (reader.Read())
                            {
                                if (readings == 0)
                                {
                                    short i = 0;
                                    while (true)
                                    {
                                        try
                                        {
                                            colls.Add(reader.GetName(i));
                                            var coll = new DataGridViewTextBoxColumn()
                                            {
                                                HeaderText = colls.Last(),
                                                AutoSizeMode = mode
                                            };
                                            table.Columns.Add(coll);
                                        }
                                        catch (Exception) { break; }
                                        i++;
                                    }
                                }
                                readings++;
                                if (readings == short.MaxValue) break;

                                List<string> datas = new List<string>();
                                foreach (string coll in colls)
                                {
                                    datas.Add(reader[coll].ToString());
                                }
                                table.Rows.Add(datas.ToArray());
                            }
                        }
                    }
                    table.ClearSelection();
                    connection.Close();
                    Good(query);
                }
            }
            catch (MySqlException error) { Bad(error.ToString(), query); }
        }

        /// <summary>
        /// Sends an SQL request to the MySQL database using the current connection string and handles success or failure.
        /// </summary>
        /// <param name="query">The SQL query to be executed.</param>
        public void SendRequest(string query)
        {
            if (!(query.Any(c => c != '\0' && c != '\n') || string.IsNullOrEmpty(query)))
            {
                try
                {
                    MySqlConnection connection = new MySqlConnection(connectionString.Get());
                    connection.Open();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                    Good(query);
                }
                catch (MySqlException error) { Bad(error.ToString(), query); }
            }
            else
            {
                Bad("Empty query", query);
            }
        }

        /// <summary>
        /// Tests the connection to the MySQL database using the specified connection string.
        /// </summary>
        /// <param name="connectionStr">The connection string to be tested.</param>
        public void TestConnection(string connectionStr)
        {
            MySqlConnection connection = new MySqlConnection(connectionStr);
            connection.Open();
            connection.Close();
        }

        /// <summary>
        /// Connects to the specified MySQL database using the provided database name and handles success or failure.
        /// </summary>
        /// <param name="database">The name of the database to connect to.</param>
        public void ConnectDatabase(string database)
        {
            var oldconnstr = new MyConnectionString(
                connectionString.Server,
                connectionString.User,
                connectionString.Password,
                connectionString.Database
                );
            try
            {
                connectionString.Database = database;
                TestConnection(connectionString.Get());
                TableHandlerMySQL.Good(ref richTextBox, ref mainTextBox, "use " + database);
            }
            catch (MySqlException error)
            {
                connectionString = oldconnstr;
                TableHandlerMySQL.Bad(ref richTextBox, ref mainTextBox, error.ToString(), "use " + database);
            }
        }
    }
}

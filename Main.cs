using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;

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
        /// <param name="longError">If false then will show only first strike of error else all the text of an error</param>
        public static void Bad(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, string text, string code = "none code", bool longError = false)
        {
            richTextBox.SelectionColor = Color.Red;
            richTextBox.AppendText("error code: " + code + '\n' + (longError ? text : text.Split('\n')[0]) + '\n');
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
        /// <param name="longError">If false then will show only first strike of error else all the text of an error</param>
        public static void SqlReadInTable(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, 
                                          ref DataGridView table, DataGridViewAutoSizeColumnMode mode,
                                          string connectionString ,string query = "show databases", bool longError = false)
        {
            query = query.Replace('\n', ' ');
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
            catch (MySqlException error) { Bad(ref richTextBox, ref mainTextBox , error.ToString(), query, longError); }
        }

        /// <summary>
        /// Populates a DataGridView control with data from a SQL query without logging, using the specified connection string.
        /// </summary>
        /// <param name="table">The DataGridView control to be populated with data.</param>
        /// <param name="mode">The DataGridViewAutoSizeColumnMode to be applied.</param>
        /// <param name="connectionString">The connection string for accessing the database.</param>
        /// <param name="query">The SQL query to retrieve data (default is "show databases").</param>
        /// <param name="longError">If false then will show only first strike of error else all the text of an error</param>
        public static void NoLogSqlReadInTable(ref DataGridView table, DataGridViewAutoSizeColumnMode mode,
                                               string connectionString, string query = "show databases", bool longError = false)
        {
            var _0 = new RichTextBox();
            SqlReadInTable(ref _0, ref _0, ref table, mode, connectionString, query, longError);
        }

        /// <summary>
        /// Just colored log in RichTextBox
        /// </summary>
        /// <param name="richTextBox">To where log to</param>
        /// <param name="color">Color of logging text</param>
        /// <param name="text">Text of log</param>
        public static void ColoredLog(ref RichTextBox richTextBox, Color color, string text)
        {
            richTextBox.SelectionColor = color;
            richTextBox.AppendText("log: " + text + '\n');
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
        public static void SendRequest(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, string connectionString, string query, bool longError = false)
        {
            query = query.Replace('\n', ' ');
            if (!string.IsNullOrWhiteSpace(query))
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
                catch (MySqlException error) { TableHandlerMySQL.Bad(ref richTextBox, ref mainTextBox, error.ToString(), query, longError); }
            }
            else
                TableHandlerMySQL.Bad(ref richTextBox, ref mainTextBox, "Empty query", query, longError);
            }

        /// <summary>
        /// Sends a SQL request to the MySQL database without logging.
        /// </summary>
        /// <param name="connectionString">The connection string to the database.</param>
        /// <param name="query">The SQL query to be executed.</param>
        public static void NoLogSendRequest(string connectionString, string query)
        {
            var _0 = new RichTextBox();
            SendRequest(ref _0, ref _0, connectionString, query);
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
        /// <param name="query">Contains the name of the database to connect to.</param>
        public static void ConnectDatabase(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, ref MyConnectionString connectionString, string query, bool longError = false)
        {
            query = query.Replace('\n', ' ');
            string database = "";
            byte words = 0;
            foreach (string word in query.Split())
            {
                if (!string.IsNullOrEmpty(word))
                {
                    words++;
                    database = word;
                }
            }
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
                TableHandlerMySQL.Bad(ref richTextBox, ref mainTextBox, error.ToString(), "use " + database, longError);
            }
        }
    }

    /// <summary>
    /// Provides methods for handling database operations related to MySQL, including displaying messages in a RichTextBox control and executing SQL queries.
    /// </summary>
    public class ThisHandlerMySQL
    {
        /// <summary>
        /// Box for logging
        /// </summary>
        public RichTextBox LogTextBox;

        /// <summary>
        /// Box for code of you
        /// </summary>
        public RichTextBox MainTextBox;

        /// <summary>
        /// String for connection
        /// </summary>
        public MyConnectionString ConnectionString;

        /// <summary>
        /// Initializes a new instance of the ThisHandlerMySQL class with the specified LogTextBox controls and connection string.
        /// </summary>
        /// <param name="logTextBox">The RichTextBox control for displaying log information.</param>
        /// <param name="mainTextBox">The main RichTextBox control for code.</param>
        /// <param name="connectionString">The connection string to the database.</param>
        public ThisHandlerMySQL(ref RichTextBox logTextBox, ref RichTextBox mainTextBox, MyConnectionString connectionString)
        {
            LogTextBox = logTextBox;
            MainTextBox = mainTextBox;
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Displays a success message in the LogTextBox control with green text color.
        /// </summary>
        /// <param name="text">The text of the success message.</param>
        public void Good(string text)
        {
            LogTextBox.SelectionColor = Color.Green;
            LogTextBox.AppendText("success: " + text + "\n");
            LogTextBox.Focus();
            MainTextBox.Focus();
        }

        /// <summary>
        /// Displays an error message in the LogTextBox control with red text color and an optional error code.
        /// </summary>
        /// <param name="text">The text of the error message.</param>
        /// <param name="code">The error code (default is "none code").</param>
        /// <param name="longError">If false then will show only first strike of error else all the text of an error</param>
        public void Bad(string text, string code = "none code", bool longError = false)
        {
            LogTextBox.SelectionColor = Color.Red;
            LogTextBox.AppendText("error code: " + code + '\n' + (longError ? text : text.Split('\n')[0]) + '\n');
            LogTextBox.Focus();
            MainTextBox.Focus();
        }

        /// <summary>
        /// Retrieves the first column of a specified table or query as an array of strings.
        /// </summary>
        /// <param name="text">Table name or query</param>
        /// <param name="table">Determines whether the 'text' parameter is just the name of the table or the entire query</param>
        /// <returns>An array of strings representing the first column of the specified table or query</returns>
        public string[] GetFirstColumnAsArray(string text, bool table = true)
        {
            List<string> output = new List<string>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConnectionString.Get()))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(table ? $"show columns from {text}" : text, connection);
                    MySqlDataReader reader = command.ExecuteReader();
                    reader.Read();
                    short i = 0;
                    while (true)
                    {
                        try
                        {
                            output.Add(reader.GetName(i));
                            i++;
                        }
                        catch (Exception) { break; }
                    }
                    reader.Close();
                    connection.Close();
                } 
            }
            catch (Exception error) { Console.WriteLine(error); /* for why? */ }
            return output.ToArray();
        }

        /*
        /// <summary>
        /// Creates a context menu strip based on the first column of the specified table.
        /// </summary>
        /// <param name="table">Name of the table</param>
        /// /// <param name="tabled">Determines whether the 'text' parameter is just the name of the table or the entire query</param>
        /// <returns>The created ContextMenuStrip</returns>
        public ContextMenuStrip CreateContextFirstColumn(string table, bool tabled = true)
        {
            var context = new ContextMenuStrip();
            var firstCollumn = GetFirstColumnAsArray(table, tabled).Select(item => new ToolStripMenuItem() { Text = item }).ToArray();
            context.Items.AddRange(firstCollumn);
            return context;
        }
*/
        /// <summary>
        /// Executes an SQL query to read data from a MySQL table and displays the result in a DataGridView control.
        /// </summary>
        /// <param name="table">The DataGridView control to display the query results.</param>
        /// <param name="mode">The auto-size mode for columns in the DataGridView.</param>
        /// <param name="query">The SQL query to be executed (default is "show databases").</param>
        /// <param name="longError">If false then will show only first strike of error else all the text of an error</param>
        // lame <param name="columned">Determines if grid table row will have ContextMenuStrip with is SQL table columns</param>
        public void SqlReadInTable(ref DataGridView table, DataGridViewAutoSizeColumnMode mode, string query = "show databases", bool longError = false)
        {
            query = query.Replace('\n', ' ');
            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConnectionString.Get()))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            table.Columns.Clear();

                            List<string> cols = new List<string>();
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
                                            cols.Add(reader.GetName(i));
                                            
                                            var col = new DataGridViewTextBoxColumn()
                                            {
                                                HeaderText = cols.Last(),
                                                AutoSizeMode = mode,
                                            };
                                            table.Columns.Add(col);
                                        }
                                        catch (Exception) { break; }
                                        i++;
                                    }
                                }
                                readings++;
                                if (readings == short.MaxValue) break;

                                
                                /* lame
                                if (columned && readings % 2 == 1 && readings > 1)
                                {
                                    var context = CreateContextFirstColumn(reader[cols[0]].ToString(), true);
                                    //context.MouseDown += (sender, e) => { context.Show(); };
                                    var row = new DataGridViewRow()
                                    {
                                        ReadOnly = !columned,
                                        ContextMenuStrip = context
                                    };
                                    row.Cells[cols[0]].Value = reader[cols[0]].ToString();
                                    //row.CreateCells();
                                    //row.Rows.Add(reader[cols[0]].ToString());
                                    //table.CurrentRow.CreateCells(row);
                                    table.Rows.Add(row);

                                    table.MouseDown += (sender, e) => 
                                    { 
                                        if (e.Button == MouseButtons.Right)
                                        {
                                            //var hit = table.HitTest(e.X, e.Y);
                                            context.Show(Cursor.Position);
                                        }
                                    };
                                }
                                else
                                */
            //    ?????                DataGridViewButtonCell
                                {
                                    List<string> datas = new List<string>();
                                    foreach (var col in cols)
                                            datas.Add(reader[col].ToString());
                                    table.Rows.Add(datas.ToArray());
                                }
                            }
                        }
                    }
                    table.ClearSelection();
                    connection.Close();
                    Good(query);
                }
            }
            catch (MySqlException error) { Bad(error.ToString(), query, longError); }
        }

        /// <summary>
        /// Sends an SQL request to the MySQL database using the current connection string and handles success or failure.
        /// </summary>
        /// <param name="query">The SQL query to be executed.</param>
        /// /// <param name="longError">If false then will show only first strike of error else all the text of an error</param>
        public void SendRequest(string query, bool longError = false)
        {
            query = query.Replace('\n', ' ');
            if (!string.IsNullOrWhiteSpace(query))
            {
                try
                {
                    MySqlConnection connection = new MySqlConnection(ConnectionString.Get());
                    connection.Open();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                    Good(query);
                }
                catch (MySqlException error) { Bad(error.ToString(), query, longError); }
            }
            else
                Bad("Empty query", query);
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
        /// <param name="query">Contains name of database connect to.</param>
        /// <param name="longError">If false then will show only first strike of error else all the text of an error</param>
        public void ConnectDatabase(string query, bool longError = false)
        {
            query = query.Replace('\n', ' ');

            string database = "";
            byte words = 0;
            foreach (string word in query.Split())
            {
                if (!string.IsNullOrEmpty(word))
                {
                    words++;
                    if (words == 2)
                    {
                        database = word;
                        break;
                    }
                }
            }
            var oldconnstr = new MyConnectionString(
                ConnectionString.Server,
                ConnectionString.User,
                ConnectionString.Password,
                ConnectionString.Database
                );
            try
            {
                ConnectionString.Database = database;
                TestConnection(ConnectionString.Get());
                Good("use " + database);
            }
            catch (MySqlException error)
            {
                ConnectionString = oldconnstr;
                Bad(error.ToString(), "use " + database, longError);
            }
        }
    }
}

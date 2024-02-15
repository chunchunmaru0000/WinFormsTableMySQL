using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace TableMySQL
{
    public static class TableHandlerMySQL
    {
        public static void Good(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, string text)
        {
            richTextBox.SelectionColor = Color.Green;
            richTextBox.AppendText("success: " + text + "\n");
            richTextBox.Focus();
            mainTextBox.Focus();
        }

        public static void Bad(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, string text, string code = "none code")
        {
            richTextBox.SelectionColor = Color.Red;
            richTextBox.AppendText($"error code: {code}\n{text}\n");
            richTextBox.Focus();
            mainTextBox.Focus();
        }

        public static void sql_read_in_table(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, 
                                             ref DataGridView table, DataGridViewAutoSizeColumnMode mode,
                                             ref string connectionString ,string query = "show databases")
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
            catch (Exception error) { Bad(ref richTextBox, ref mainTextBox , error.ToString(), query); }
        }
    }

    public static class OtherHandlerMySQL
    {
        static void send_request(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, ref string connectionString, ref string query)
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
            catch (Exception error) { TableHandlerMySQL.Bad(ref richTextBox, ref mainTextBox, error.ToString(), query); }
        }

        public static void test_connection(ref string connectionStr)
        {
            MySqlConnection connection = new MySqlConnection(connectionStr);
            connection.Open();
            connection.Close();
        }

        public static void connect_database(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, string connectionString, string database)
        {
            string oldconnstr = connectionString;
            try
            {
                connectionString = $"server=localhost;user id=root;password=root;database={database};";
                test_connection(ref connectionString);
                TableHandlerMySQL.Good(ref richTextBox, ref mainTextBox, "use " + database);
            }
            catch (Exception error)
            {
                connectionString = oldconnstr;
                TableHandlerMySQL.Bad(ref richTextBox, ref mainTextBox, error.ToString(), "use " + database);
            }
        }
    }

    public class ThisHandlerMySQL
    {
        private RichTextBox richTextBox;
        private RichTextBox mainTextBox;
        private string connectionString;

        public ThisHandlerMySQL(ref RichTextBox richTextBox, ref RichTextBox mainTextBox, ref string connectionString)
        {
            this.richTextBox = richTextBox;
            this.mainTextBox = mainTextBox;
            this.connectionString = connectionString;
        }

        public void Good(string text)
        {
            richTextBox.SelectionColor = Color.Green;
            richTextBox.AppendText("success: " + text + "\n");
            richTextBox.Focus();
            mainTextBox.Focus();
        }

        public void Bad(string text, string code = "none code")
        {
            richTextBox.SelectionColor = Color.Red;
            richTextBox.AppendText($"error code: {code}\n{text}\n");
            richTextBox.Focus();
            mainTextBox.Focus();
        }

        public void sql_read_in_table(ref DataGridView table, DataGridViewAutoSizeColumnMode mode, string query = "show databases")
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
                    Good(query);
                }
            }
            catch (Exception error) { Bad(error.ToString(), query); }
        }

        public void send_request(ref string query)
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
                Good(query);
            }
            catch (Exception error) { Bad(error.ToString(), query); }
        }

        public void test_connection(ref string connectionStr)
        {
            MySqlConnection connection = new MySqlConnection(connectionStr);
            connection.Open();
            connection.Close();
        }

        public void connect_database(string database)
        {
            string oldconnstr = connectionString;
            try
            {
                connectionString = $"server=localhost;user id=root;password=root;database={database};";
                test_connection(ref connectionString);
                Good("use " + database);
            }
            catch (Exception error)
            {
                connectionString = oldconnstr;
                Bad(error.ToString(), "use " + database);
            }
        }
    }
}

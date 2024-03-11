using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
//using System.Data.SqlServerCe;
using MySql.Data.MySqlClient;
using System.Data.OleDb;
using System.IO;
using System.Net.Http.Headers;
using System.Xml;
using System.Data;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Signers;
using Microsoft.Identity.Client;
#pragma warning disable CS8602, CS8600, CS8601, CS8604, CS8605, CS8625, CS0414
namespace Seamlex.Utilities
{
    /// <summary>
    /// multi-provider database access
    /// </summary>
    public class OmniDb
    {

        private string currentprovider = "";
        private string currentconnectionstring = "";

        private string currentoutputmode = "Default";
        private string currentoutputprovider = "Default";
        private string currentoutputconnectionstring = "";

        private string currentquery = "";
        private string currenttablenames = "";
        private string currentquerymethod = "Default";

        private string currentverbosity = "Default";
        private string lasterror = "";
        private string lastscalarresult = "";
        private int rowsaffected = -1;
        private System.Data.DataSet lastresult = new System.Data.DataSet();

        private bool sourcereadonly = true;

        public string CurrentOutputMode
        {
            get
            { 
                return currentoutputmode;
            }
            set
            { 
                string checktarget = value.Trim().ToLower();
                switch (checktarget) 
                {
                    case "s":
                    case "source":
                        currentoutputmode = "Source";
                    break;

                    case "o":
                    case "output":
                        currentoutputmode = "Output";
                    break;

                    case "n":
                    case "none":
                        currentoutputmode = "None";
                    break;

                    case "":
                    case "d":
                    case "default":
                        currentoutputmode = "Default";
                    break;

                default:
                    currentoutputmode = "Default";
                    break;
                }
            }
        }

        public string CurrentProvider
        {
            get
            { 
                return currentprovider;
            }
            set
            { 
                currentprovider = this.CheckProvider(value);
            }
        }

        public string CurrentOutputProvider
        {
            get
            { 
                return currentoutputprovider;
            }
            set
            { 
                currentoutputprovider = this.CheckProvider(value);
            }
        }

        public string CheckProvider(string checkname)
        {
            string output = "Default";
            string checkprovider = checkname.Trim().ToLower();
            switch (checkprovider) 
            {
                case "my":
                case "mydb":
                case "mysql":
                case "mysql.data.mysqlclient":
                    output = "MySql.Data.MySqlClient";
                break;

                case "sl":
                case "sldb":
                case "sqlite":
                case "sqllite":
                case "microsoft.data.sqlite":
                case "microsoft.data.sqllite":
                    output = "Microsoft.Data.Sqlite";
                break;

                case "ss":
                case "ms":
                case "ssdb":
                case "sqldb":
                case "msdb":
                case "mssql":
                case "sqlclient":
                case "sqlserver":
                case "microsoft.data.sqlclient":
                case "system.data.sqlclient":
                    output = "Microsoft.Data.SqlClient";
                break;

                case "od":
                case "odbc":
                case "system.data.odbc":
                    output = "System.Data.Odbc";
                break;

                case "xl":
                case "xlsx":
                case ".xlsx":
                case "excel":
                case "microsoft.excel":
                    output = "Microsoft.Excel";
                break;

                case "cv":
                case "csv":
                case "text.csv":
                case ".csv":
                case "comma":
                case "comma.separated":
                case "commaseparated":
                case "comma-separated":
                    output = "CSV";
                break;

                case "tx":
                case "txt":
                case ".txt":
                case "text":
                case "raw":
                case "raw.text":
                case "raw-text":
                case "rawtext":
                    output = "Text";
                break;

                case "xm":
                case "xml":
                case ".xml":
                case "text.xml":
                    output = "XML";
                break;

                case "js":
                case "json":
                case ".json":
                case "text.json":
                    output = "JSON";
                break;

                case "":
                case "d":
                case "default":
                    output = "Default";
                break;

            default:
                output = "Default";
                break;
            }
            return output;
        }

        public string CurrentConnectionString
        {
            get { return currentconnectionstring; }   // get method
            set { currentconnectionstring = value; }  // set method
        }

        public string CurrentQuery
        {
            get { return currentquery; }
            set { currentquery = value; }
        }
        public string CurrentTableNames
        {
            get { return currenttablenames; }
            set { currenttablenames = value; }
        }

        public string CurrentQueryMode
        {
            get
            {
                return currentquerymethod;
            }
            set
            {

// Scalar is used when your query returns a single value. If it returns more, then the result is the first column of the first row. An example might be SELECT @@IDENTITY AS 'Identity'
// Reader is used for any result set with multiple rows/columns (e.g., SELECT col1, col2 from sometable).
// NonQuery is typically used for SQL statements without results (e.g., UPDATE, INSERT, etc.).

                string checkquerymode = value.Trim().ToLower();
                switch (checkquerymode) 
                {
                    case "":
                    case "d":
                    case "default":
                        currentquerymethod = "Default";
                    break;

                    case "n":
                    case "nq":
                    case "nonquery":
                        currentquerymethod = "NonQuery";
                    break;

                    case "r":
                    case "re":
                    case "read":
                    case "reader":
                        currentquerymethod = "Reader";
                    break;

                    case "s":
                    case "sc":
                    case "scalar":
                        currentquerymethod = "Scalar";
                    break;

                    case "i":
                    case "in":
                    case "info":
                        currentquerymethod = "Info";
                    break;

                    case "t":
                    case "te":
                    case "test":
                        currentquerymethod = "Test";
                    break;

                    default:
                        currentquerymethod = "Default";
                    break;
                }
            }
        }

        public string CurrentVerbosity
        {
            get
            {
                return currentverbosity;
            }
            set
            {

// Scalar is used when your query returns a single value. If it returns more, then the result is the first column of the first row. An example might be SELECT @@IDENTITY AS 'Identity'
// Reader is used for any result set with multiple rows/columns (e.g., SELECT col1, col2 from sometable).
// NonQuery is typically used for SQL statements without results (e.g., UPDATE, INSERT, etc.).

                string checkverbosity = value.Trim().ToLower();
                switch (checkverbosity) 
                {
                    case "":
                    case "d":
                    case "default":
                        currentverbosity = "Default";
                    break;

                    case "0":
                    case "n":
                    case "none":
                        currentverbosity = "None";
                    break;

                    case "1":
                    case "f":
                    case "full":
                        currentverbosity = "Full";
                    break;

                    case "e":
                    case "error":
                    case "errors":
                        currentverbosity = "Errors";
                    break;

                    default:
                        currentverbosity = "Default";
                    break;
                }
            }
        }

        public string LastError
        {
            get { return lasterror; }
            set { lasterror = value; }
        }

        public string LastScalarResult
        {
            get { return lastscalarresult; }
            set { lastscalarresult = value; }
        }
        
        public int RowsAffected
        {
            get { return rowsaffected; }
            set { rowsaffected = value; }
        }
        
        public System.Data.DataSet LastResult 
        {
            get { 
                if(lastresult==null)
                    lastresult = new System.Data.DataSet();
                return lastresult; }
            set { lastresult = value; }
        }


        // #region ValidateGenDbParameters

        // /// <summary>
        // /// 2.1.3 SNJW this constructs an appropriate null query for the respective provider.
        // /// It then tries to connect, runs this query, then closes that connection.
        // /// </summary>
        // /// <returns>string "Test successful" or an error message</returns>
        // public string TestConnection()
        // {
        //     return this.TestConnection(this.currentconnectionstring, this.currentprovider);
        // }

        // #region ValidateGenDbParameters


        #region TestGenDbConnection

        /// <summary>
        /// 2.1.3 SNJW this constructs an appropriate null query for the respective provider.
        /// It then tries to connect, runs this query, then closes that connection.
        /// </summary>
        /// <returns>string "Test successful" or an error message</returns>
        public string TestConnection()
        {
            return this.TestConnection(this.currentconnectionstring, this.currentprovider);
        }
        public string TestConnection(string connectionstring, string provider)
        {
            // 2.1.0 SNJW add MySql
            // 1.6.7 do this differently for SQL CE
            string output = "Cannot find a provider named ";
            if (provider == "")
            {
                output += "{blank}";
            }
            else
            {
                output += provider;
            }

            // try for each
            if (provider == "Microsoft.Data.SqlClient")
            {
                output = this.TestSqlServerConnection(connectionstring);
            }
            else if (provider == "MySql.Data.MySqlClient")
            {
                output = this.TestMySqlConnection(connectionstring);
            }
            else if (provider == "System.Data.SqlServerCe")
            {
                output = "SqlServerCe is not implemented";
                // output = this.TestSqlServerCompactConnection(connectionstring);
            }
            else if (provider == "System.Data.OleDb"
                || provider == "OleDb")
            {
                output = "OleDb is not implemented";
            }
            else if (provider == "System.Data.Odbc")
            {
                output = this.TestOdbcConnection(connectionstring);
            }
            else if (provider == "Microsoft.Data.Sqlite")
            {
                output = this.TestOdbcConnection(connectionstring);
            }

            return output;

        }




        // private string TestSqlServerCompactConnection(string connectionstring)
        // {
        //     string output = "Test successful";
        //     string testcommand = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE 1 < 0";
        //     bool failnow = false;


        //     try
        //     {

        //         System.Data.SqlServerCe.SqlCeConnection testconn = new System.Data.SqlServerCe.SqlCeConnection(connectionstring);
        //         try
        //         {
        //             System.Data.SqlServerCe.SqlCeCommand testcmd = new
        //                 System.Data.SqlServerCe.SqlCeCommand(testcommand, testconn);
        //         }
        //         catch
        //         {
        //             failnow = true;
        //             output = "Test command could not be created";
        //         }

        //         //               System.Data.SqlClient.SqlCommand("select suburbName FROM suburb", testconn);
        //     }
        //     catch
        //     {
        //         failnow = true;
        //         output = "Connection string is invalid";
        //     }

        //     if (failnow == true)
        //         return output;


        //     System.Data.SqlServerCe.SqlCeConnection conn = new System.Data.SqlServerCe.SqlCeConnection(connectionstring);
        //     System.Data.SqlServerCe.SqlCeCommand cmd = new
        //         System.Data.SqlServerCe.SqlCeCommand(testcommand, conn);

        //     try
        //     {
        //         cmd.Connection.Open();
        //     }
        //     catch
        //     {
        //         failnow = true;
        //         output = "Cannot connect to this database";
        //     }

        //     try
        //     {
        //         cmd.Connection.Close();
        //     }
        //     catch
        //     {
        //     }
        //     return output;
        // }
        private string TestSqlServerConnection(string connectionstring)
        {
            string output = "Test successful";
            string testcommand = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE 1 < 0";
            bool failnow = false;


            try
            {

                Microsoft.Data.SqlClient.SqlConnection testconn = new Microsoft.Data.SqlClient.SqlConnection(connectionstring);
                try
                {
                    Microsoft.Data.SqlClient.SqlCommand testcmd = new
                        Microsoft.Data.SqlClient.SqlCommand(testcommand, testconn);
                }
                catch
                {
                    failnow = true;
                    output = "Test command could not be created";
                }

                //               Microsoft.Data.SqlClient.SqlCommand("select suburbName FROM suburb", testconn);
            }
            catch
            {
                failnow = true;
                output = "Connection string is invalid";
            }

            if (failnow == true)
                return output;


            Microsoft.Data.SqlClient.SqlConnection conn = new Microsoft.Data.SqlClient.SqlConnection(connectionstring);
            Microsoft.Data.SqlClient.SqlCommand cmd = new
                Microsoft.Data.SqlClient.SqlCommand(testcommand, conn);

            try
            {
                cmd.Connection.Open();
            }
            catch
            {
                failnow = true;
                output = "Cannot connect to this database";
            }

            try
            {
                cmd.Connection.Close();
            }
            catch
            {
            }
            return output;
        }
        private string TestOdbcConnection(string connectionstring)
        {
            string output = "Test successful";

            string testcommand = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE 1 < 0";
            // 2.1.0 SNJW use MSysObjects for access
            if (connectionstring.Contains("Microsoft Access Driver"))
                testcommand = "SELECT * FROM MSysObjects WHERE 1 < 0";
            bool failnow = false;


            try
            {

                System.Data.Odbc.OdbcConnection testconn = new System.Data.Odbc.OdbcConnection(connectionstring);
                try
                {
                    System.Data.Odbc.OdbcCommand testcmd = new
                       System.Data.Odbc.OdbcCommand(testcommand, testconn);
                }
                catch
                {
                    failnow = true;
                    output = "Test command could not be created";
                }

                //               System.Data.SqlClient.SqlCommand("select suburbName FROM suburb", testconn);
            }
            catch
            {
                failnow = true;
                output = "Connection string is invalid";
            }

            if (failnow == true)
                return output;


            System.Data.Odbc.OdbcConnection conn = new System.Data.Odbc.OdbcConnection(connectionstring);
            System.Data.Odbc.OdbcCommand cmd = new
                System.Data.Odbc.OdbcCommand(testcommand, conn);

            try
            {
                cmd.Connection.Open();
            }
            catch
            {
                failnow = true;
                output = "Cannot connect to this database";
            }

            try
            {
                cmd.Connection.Close();
            }
            catch
            {
            }
            return output;
        }
        private string TestMySqlConnection(string connectionstring)
        {
            string output = "Test successful";
            string testcommand = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE 1 < 0";
            bool failnow = false;
            try
            {

                MySql.Data.MySqlClient.MySqlConnection testconn = new MySql.Data.MySqlClient.MySqlConnection(connectionstring);
                try
                {
                    MySql.Data.MySqlClient.MySqlCommand testcmd = new
                       MySql.Data.MySqlClient.MySqlCommand(testcommand, testconn);
                }
                catch
                {
                    failnow = true;
                    output = "Test command could not be created";
                }

                //               System.Data.SqlClient.SqlCommand("select suburbName FROM suburb", testconn);
            }
            catch
            {
                failnow = true;
                output = "Connection string is invalid";
            }

            if (failnow == true)
                return output;


            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connectionstring);
            MySql.Data.MySqlClient.MySqlCommand cmd = new
                MySql.Data.MySqlClient.MySqlCommand(testcommand, conn);

            try
            {
                cmd.Connection.Open();
            }
            catch
            {
                failnow = true;
                output = "Cannot connect to this database";
            }

            try
            {
                cmd.Connection.Close();
            }
            catch
            {
            }
            return output;
        }

        #endregion TestGenDbConnection

        #region QueryExecuteGenDb

        /// <summary>
        /// 2.1.3 SNJW this performs an SQL query and plops the result into a DataSet attached to this object
        /// </summary>
        /// <param name="query"></param>
        /// <returns>int rowsaffected
        /// If creating a connection fails, output = -2, will not continue
        /// If creating the command fails, output = -3, will not continue execution of query and will try and close if appropriate
        /// If opening the connection fails, output = -4, will not continue
        /// If executing the query fails, output = -5, will continue to try and close
        /// If executing the query succeeds, the number of rows affected is stored to output and to this.rowsaffected
        /// If the connection cannot be closed, output = -6 but this.rowsaffected remains populated
        /// </returns>



        //SqlDataAdapter da = new SqlDataAdapter();
        //SqlCommand cmd = conn.CreateCommand();
        //cmd.CommandText = SQL;
        //da.SelectCommand = cmd;
        //DataSet ds = new DataSet();

        //conn.Open();
        //da.Fill(ds);
        //conn.Close();

        public int QueryExecute()
        {
            return this.QueryExecute(this.currentconnectionstring, this.currentprovider, this.currentquery, this.currentquerymethod, this.currentoutputmode, this.currentoutputconnectionstring, this.currentoutputprovider);
        }

        public int QueryExecute(string connectionstring, string providertype, string query, string querymode = "Default", string outputmode = "Default", string outputconnectionstring = "", string outputprovider = "Default")
        {
            this.LastError="";
            // 1.6.8 SNJW this is 10608 for 1.6.8 and stored in semaphore
            int output = 0;

            // try for each
            if (providertype == "Default" || providertype == "" || providertype == "d")
            {
                providertype = this.GuessProvider(connectionstring);
            }
            if (querymode == "Default" || providertype == "" || providertype == "d")
            {
                querymode = this.GuessQueryMethod(query);
            }


            // try for each
            if (providertype == "Microsoft.Data.SqlClient")
            {
                if (querymode.Contains("Scalar"))
                {
                    output = this.QueryExecuteScalarSqlServer(connectionstring, query);
                }
                else if (querymode.Contains("NonQuery"))
                {
                    output = this.QueryExecuteNonQuerySqlServer(connectionstring, query);
                }
                else if (querymode.Contains("Reader"))
                {
                    output = this.QueryExecuteReaderSqlServer(connectionstring, query);
                }

            }
            else if (providertype == "MySql.Data.MySqlClient")
            {
                if (querymode.Contains("Scalar"))
                {
                    output = this.QueryExecuteScalarMySql(connectionstring, query);
                }
                else if (querymode.Contains("NonQuery"))
                {
                    output = this.QueryExecuteNonQueryMySql(connectionstring, query);
                }
                else if (querymode.Contains("Reader"))
                {
                    output = this.QueryExecuteReaderMySql(connectionstring, query);
                }
            }
            else if (providertype == "Microsoft.Data.Sqlite")
            {
                if (querymode.Contains("Scalar"))
                {
                    output = this.QueryExecuteScalarSqlLite(connectionstring, query);
                }
                else if (querymode.Contains("NonQuery"))
                {
                    output = this.QueryExecuteNonQuerySqlLite(connectionstring, query);
                }
                else if (querymode.Contains("Reader"))
                {
                    output = this.QueryExecuteReaderSqlLite(connectionstring, query);
                }
            }
            // else if (providertype == "System.Data.SqlServerCe")
            // {
            //     if (querymode.Contains("Scalar"))
            //     {
            //         output = this.QueryExecuteScalarSqlServerCompact(connectionstring, query);
            //     }
            //     else if (querymode.Contains("NonQuery"))
            //     {
            //         output = this.QueryExecuteNonQuerySqlServerCompact(connectionstring, query);
            //     }
            //     else if (querymode.Contains("Reader"))
            //     {
            //         output = this.QueryExecuteReaderSqlServerCompact(connectionstring, query);
            //     }
            // }
            else if (providertype == "System.Data.Odbc")
            {
                if (querymode.Contains("Scalar"))
                {
                    output = this.QueryExecuteScalarOdbc(connectionstring, query);
                }
                else if (querymode.Contains("NonQuery"))
                {
                    output = this.QueryExecuteNonQueryOdbc(connectionstring, query);
                }
                else if (querymode.Contains("Reader"))
                {
                    output = this.QueryExecuteReaderOdbc(connectionstring, query);
                }
            }
            else if (providertype == "Microsoft.Excel")
            {
                // override the same XLSX unless 
                if(outputmode == "Default")
                {

                }
                output = this.QueryExecuteExcel(connectionstring, query, querymode, outputconnectionstring, outputprovider);
            }
            else if (providertype == "Text.CSV")
            {
                if (querymode.Contains("Scalar"))
                {
                    output = this.QueryExecuteScalarCsv(connectionstring, query);
                }
                else if (querymode.Contains("NonQuery"))
                {
                    output = this.QueryExecuteNonQueryCsv(connectionstring, query);
                }
                else if (querymode.Contains("Reader"))
                {
                    output = this.QueryExecuteReaderCsv(connectionstring, query);
                }
            }            
            return output;


        }


#region sqlserver

        private int QueryExecuteReaderSqlServer(string connectionstring, string query)
        {
            // 2.1.3 SNJW this populates the attached DataSet via SqlAdapter

            //SqlCommand cmd = conn.CreateCommand();
            //cmd.CommandText = SQL;
            //da.SelectCommand = cmd;
            //DataSet ds = new DataSet();

            //conn.Open();
            //da.Fill(ds);
            //conn.Close();

            int output = -1;
            string testcommand = query;
            //bool failnow = false;

            try
            {

                Microsoft.Data.SqlClient.SqlConnection testconn = new Microsoft.Data.SqlClient.SqlConnection(connectionstring);
                try
                {
                    Microsoft.Data.SqlClient.SqlCommand testcmd = new
                        Microsoft.Data.SqlClient.SqlCommand(testcommand, testconn);
                }
                catch
                {
                    output = -3;
                }
            }
            catch
            {
                output = -2;
            }

            if (output == -2 || output == -3)
                return output;


            Microsoft.Data.SqlClient.SqlDataAdapter da = new Microsoft.Data.SqlClient.SqlDataAdapter();
            Microsoft.Data.SqlClient.SqlConnection conn = new Microsoft.Data.SqlClient.SqlConnection(connectionstring);
            Microsoft.Data.SqlClient.SqlCommand cmd = new
                Microsoft.Data.SqlClient.SqlCommand(testcommand, conn);

            try
            {
                cmd.Connection.Open();
            }
            catch
            {
                output = -4;
            }

            try
            {
                this.lastresult.Clear();
                da.SelectCommand = cmd;
                output = da.Fill(this.lastresult);
                this.rowsaffected = output;
            }
            catch
            {
                output = -5;
            }

            try
            {
                cmd.Connection.Close();
            }
            catch
            {
                output = -6;
            }

            this.SetTableNames();
            return output;
        }

        private int QueryExecuteScalarSqlServer(string connectionstring, string query)
        {
            int output = -1;
            string testcommand = query;
            //bool failnow = false;
            this.rowsaffected = 0;


            try
            {
                Microsoft.Data.Sqlite.SqliteConnection testconn = new Microsoft.Data.Sqlite.SqliteConnection(connectionstring);
                try
                {
                    Microsoft.Data.Sqlite.SqliteCommand testcmd = new
                        Microsoft.Data.Sqlite.SqliteCommand(testcommand, testconn);
                }
                catch
                {
                    output = -3;
                }
            }
            catch
            {
                output = -2;
            }

            if (output == -2)
                return output;


            Microsoft.Data.Sqlite.SqliteConnection conn = new Microsoft.Data.Sqlite.SqliteConnection(connectionstring);
            Microsoft.Data.Sqlite.SqliteCommand cmd = new
                Microsoft.Data.Sqlite.SqliteCommand(testcommand, conn);


            try
            {
                object result = cmd.ExecuteScalar();
                output = (int)result;
                this.rowsaffected = output;
            }
            catch
            {
                output = -5;
            }



            try
            {
                cmd.Connection.Close();
            }
            catch
            {
                output = -6;
            }

            return output;
        }




        private int QueryExecuteNonQuerySqlServer(string connectionstring, string query)
        {
            int output = -1;
            string testcommand = query;
            //bool failnow = false;

            try
            {

                Microsoft.Data.SqlClient.SqlConnection testconn = new Microsoft.Data.SqlClient.SqlConnection(connectionstring);
                try
                {
                    Microsoft.Data.SqlClient.SqlCommand testcmd = new
                        Microsoft.Data.SqlClient.SqlCommand(testcommand, testconn);
                }
                catch
                {
                    output = -3;
                }
            }
            catch
            {
                output = -2;
            }

            if (output == -2)
                return output;


            Microsoft.Data.SqlClient.SqlConnection conn = new Microsoft.Data.SqlClient.SqlConnection(connectionstring);
            Microsoft.Data.SqlClient.SqlCommand cmd = new
                Microsoft.Data.SqlClient.SqlCommand(testcommand, conn);

            try
            {
                cmd.Connection.Open();
            }
            catch
            {
                output = -4;
            }

            try
            {
                output = cmd.ExecuteNonQuery();
            }
            catch
            {
                output = -5;
            }



            try
            {
                cmd.Connection.Close();
            }
            catch
            {
                output = -6;
            }
            return output;
        }



#endregion sqlserver

#region mysql

        private int QueryExecuteReaderMySql(string connectionstring, string query)
        {
            int output = -1;
            string testcommand = query;
            //bool failnow = false;


            try
            {

                MySql.Data.MySqlClient.MySqlConnection testconn = new MySql.Data.MySqlClient.MySqlConnection(connectionstring);
                try
                {
                    MySql.Data.MySqlClient.MySqlCommand testcmd = new
                        MySql.Data.MySqlClient.MySqlCommand(testcommand, testconn);
                }
                catch
                {
                    output = -3;
                }
            }
            catch
            {
                output = -2;
            }

            if (output == -2 || output == -3)
                return output;


            MySql.Data.MySqlClient.MySqlDataAdapter da = new MySql.Data.MySqlClient.MySqlDataAdapter();
            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connectionstring);
            MySql.Data.MySqlClient.MySqlCommand cmd = new
                MySql.Data.MySqlClient.MySqlCommand(testcommand, conn);

            try
            {
                cmd.Connection.Open();
            }
            catch
            {
                output = -4;
            }

            try
            {
                this.lastresult.Clear();
                da.SelectCommand = cmd;
                output = da.Fill(this.lastresult);
                this.rowsaffected = output;
            }
            catch
            {
                output = -5;
            }

            try
            {
                cmd.Connection.Close();
            }
            catch
            {
                output = -6;
            }

            this.SetTableNames();
            return output;
        }

        private int QueryExecuteScalarMySql(string connectionstring, string query)
        {
            int output = -1;
            string testcommand = query;
            //bool failnow = false;
            this.rowsaffected = 0;


            try
            {

                MySql.Data.MySqlClient.MySqlConnection testconn = new MySql.Data.MySqlClient.MySqlConnection(connectionstring);
                try
                {
                    MySql.Data.MySqlClient.MySqlCommand testcmd = new
                        MySql.Data.MySqlClient.MySqlCommand(testcommand, testconn);
                }
                catch
                {
                    output = -3;
                }
            }
            catch
            {
                output = -2;
            }

            if (output == -2 || output == -3)
                return output;


            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connectionstring);
            MySql.Data.MySqlClient.MySqlCommand cmd = new
                MySql.Data.MySqlClient.MySqlCommand(testcommand, conn);

            try
            {
                cmd.Connection.Open();
            }
            catch
            {
                output = -4;
            }

            try
            {
                object result = cmd.ExecuteScalar();
                output = (int)result;
                this.rowsaffected = output;
            }
            catch
            {
                output = -5;
            }



            try
            {
                cmd.Connection.Close();
            }
            catch
            {
                output = -6;
            }
            return output;
        }

        private int QueryExecuteNonQueryMySql(string connectionstring, string query)
        {
            int output = -1;
            string testcommand = query;
            //bool failnow = false;


            try
            {

                MySql.Data.MySqlClient.MySqlConnection testconn = new MySql.Data.MySqlClient.MySqlConnection(connectionstring);
                try
                {
                    MySql.Data.MySqlClient.MySqlCommand testcmd = new
                        MySql.Data.MySqlClient.MySqlCommand(testcommand, testconn);
                }
                catch
                {
                    output = -3;
                }
            }
            catch
            {
                output = -2;
            }

            if (output == -2 || output == -3)
                return output;


            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connectionstring);
            MySql.Data.MySqlClient.MySqlCommand cmd = new
                MySql.Data.MySqlClient.MySqlCommand(testcommand, conn);

            try
            {
                cmd.Connection.Open();
            }
            catch
            {
                output = -4;
            }

            try
            {
                output = cmd.ExecuteNonQuery();
            }
            catch
            {
                output = -5;
            }



            try
            {
                cmd.Connection.Close();
            }
            catch
            {
                output = -6;
            }
            return output;
        }


#endregion mysql

#region odbc

        private int QueryExecuteReaderOdbc(string connectionstring, string query)
        {
            int output = -1;
            string testcommand = query;
            //bool failnow = false;

            try
            {

                System.Data.Odbc.OdbcConnection testconn = new System.Data.Odbc.OdbcConnection(connectionstring);
                try
                {
                    System.Data.Odbc.OdbcCommand testcmd = new
                        System.Data.Odbc.OdbcCommand(testcommand, testconn);
                }
                catch
                {
                    output = -3;
                }
            }
            catch
            {
                output = -2;
            }

            if (output == -2 || output == -3)
                return output;


            System.Data.Odbc.OdbcDataAdapter da = new System.Data.Odbc.OdbcDataAdapter();
            System.Data.Odbc.OdbcConnection conn = new System.Data.Odbc.OdbcConnection(connectionstring);
            System.Data.Odbc.OdbcCommand cmd = new
                System.Data.Odbc.OdbcCommand(testcommand, conn);

            try
            {
                cmd.Connection.Open();
            }
            catch
            {
                output = -4;
            }

            try
            {
                this.lastresult.Clear();
                da.SelectCommand = cmd;
                output = da.Fill(this.lastresult);
                this.rowsaffected = output;
            }
            catch
            {
                output = -5;
            }

            try
            {
                cmd.Connection.Close();
            }
            catch
            {
                output = -6;
            }

            this.SetTableNames();
            return output;
        }

        private int QueryExecuteScalarOdbc(string connectionstring, string query)
        {
            int output = -1;
            string testcommand = query;
            //bool failnow = false;
            this.rowsaffected = 0;

            try
            {

                System.Data.Odbc.OdbcConnection testconn = new System.Data.Odbc.OdbcConnection(connectionstring);
                try
                {
                    System.Data.Odbc.OdbcCommand testcmd = new
                        System.Data.Odbc.OdbcCommand(testcommand, testconn);
                }
                catch
                {
                    output = -3;
                }
            }
            catch
            {
                output = -2;
            }

            if (output == -2 || output == -3)
                return output;


            System.Data.Odbc.OdbcConnection conn = new System.Data.Odbc.OdbcConnection(connectionstring);
            System.Data.Odbc.OdbcCommand cmd = new
                System.Data.Odbc.OdbcCommand(testcommand, conn);

            try
            {
                cmd.Connection.Open();
            }
            catch
            {
                output = -4;
            }

            try
            {
                object result = cmd.ExecuteScalar();
                output = (int)result;
                this.rowsaffected = output;
            }
            catch
            {
                output = -5;
            }

            try
            {
                cmd.Connection.Close();
            }
            catch
            {
                output = -6;
            }
            return output;
        }

        private int QueryExecuteNonQueryOdbc(string connectionstring, string query)
        {
            int output = -1;
            string testcommand = query;
            //bool failnow = false;


            try
            {

                System.Data.Odbc.OdbcConnection testconn = new System.Data.Odbc.OdbcConnection(connectionstring);
                try
                {
                    System.Data.Odbc.OdbcCommand testcmd = new
                        System.Data.Odbc.OdbcCommand(testcommand, testconn);
                }
                catch
                {
                    output = -3;
                }
            }
            catch
            {
                output = -2;
            }

            if (output == -2 || output == -3)
                return output;


            System.Data.Odbc.OdbcConnection conn = new System.Data.Odbc.OdbcConnection(connectionstring);
            System.Data.Odbc.OdbcCommand cmd = new
                System.Data.Odbc.OdbcCommand(testcommand, conn);

            try
            {
                cmd.Connection.Open();
            }
            catch
            {
                output = -4;
            }

            try
            {
                output = cmd.ExecuteNonQuery();
            }
            catch
            {
                output = -5;
            }



            try
            {
                cmd.Connection.Close();
            }
            catch
            {
                output = -6;
            }
            return output;
        }

#endregion odbc

#region sqlite

        private int QueryExecuteNonQuerySqlLite(string connectionstring, string query)
        {
            int output = -1;

            // get the file name if there is one - add this in if not
            string filename = connectionstring;
            if(connectionstring.ToLower().StartsWith("data source="))
            {
                filename = connectionstring.Split('=',StringSplitOptions.None)[1].TrimEnd(';');
            }
            // Data Source=
            // if the database file does not exist and the query is either blank or create table then create it
            bool fileexists = File.Exists(filename);
            if(!fileexists)
            {
                if(query == "")
                {
                    try
                    {
                        Microsoft.Data.Sqlite.SqliteConnection conn = new("Data Source="+filename+";");
                        conn.Open();
                        conn.Close();
                    }
                    catch
                    {
                        this.LastError=$"SQLite file '{filename}' cannot be created.";
                        return -2;
                    }
                }
            }

            try
            {

                Microsoft.Data.Sqlite.SqliteConnection conn = new($"Data Source={filename};");
                conn.Open();

                if(query != "")
                {
                    Microsoft.Data.Sqlite.SqliteCommand cmd = new(query, conn);

                    // System.Data.DataSet ds = new();
                    // ds.Tables.Add(new System.Data.DataTable());
                    // ds.EnforceConstraints = false;
                    //.EnforceConstraint
                    // SqLiteDataAdapter da = new();

                    output = cmd.ExecuteNonQuery();

                }


                conn.Close();
                // this.LastResult=ds;
                // this.SetTableNames();
                // output = ds.Tables[0].Rows.Count;
                // return output;                

            }
            catch
            {
                this.LastError=$"Cannot execute SQLite query '{query}' on '{filename}'.";
                output = -2;
            }

            if (output == -2 || output == -3)
            {
                if(!fileexists)
                {
                    try
                    {
                        GC.Collect();
                        File.Delete(filename);
                    }
                    catch
                    {
//                        this.LastError=$"Error initialising file '{filename}'.";
                    }
                }
            }
            return output;
        }


        private void SaveDataSetToSQLite(DataSet dataSet, string sqliteDbPath)
        {
            // Create or open the SQLite database
            var connectionString = $"Data Source={sqliteDbPath}";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                foreach (DataTable table in dataSet.Tables)
                {
                    // Create table
                    CreateSQLiteTable(table, connection);

                    // Insert data
                    InsertDataIntoSQLite(table, connection);
                }
            }
        }

        public void CreateSQLiteTable(DataTable table, SqliteConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"DROP TABLE IF EXISTS [{table.TableName}];";
                command.ExecuteNonQuery();

                command.CommandText = $"CREATE TABLE [{table.TableName}] (";

                foreach (DataColumn column in table.Columns)
                {
                    command.CommandText += $"[{column.ColumnName}] {GetSQLiteTypeFrom(column.DataType)},";
                }

                command.CommandText = command.CommandText.TrimEnd(',') + ");";
                command.ExecuteNonQuery();
            }
        }

        public void InsertDataIntoSQLite(DataTable table, SqliteConnection connection)
        {
            using (var transaction = connection.BeginTransaction())
            {
                foreach (DataRow row in table.Rows)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;

                        var columnNames = string.Join(", ", table.Columns.Cast<DataColumn>().Select(c => $"[{c.ColumnName}]"));
                        var parameterNames = string.Join(", ", table.Columns.Cast<DataColumn>().Select(c => $"@{c.ColumnName}"));
                        command.CommandText = $"INSERT INTO [{table.TableName}] ({columnNames}) VALUES ({parameterNames})";

                        foreach (DataColumn column in table.Columns)
                        {
                            command.Parameters.AddWithValue($"@{column.ColumnName}", row[column]);
                        }

                        command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }

        private string GetSQLiteTypeFrom(Type netType)
        {
            // Simplified type mapping, extend according to your needs
            if (netType == typeof(int))
                return "INTEGER";
            else if (netType == typeof(string))
                return "TEXT";
            else if (netType == typeof(byte[]))
                return "BLOB";
            else if (netType == typeof(bool))
                return "BOOLEAN";
            else if (netType == typeof(DateTime))
                return "DATETIME";
            else if (netType == typeof(double))
                return "REAL";
            else if (netType == typeof(decimal))
                return "DECIMAL";
            else
                return "TEXT"; // Default, for simplification
        }

        private int QueryExecuteReaderSqlLite(string connectionstring, string query)
        {

            // get the file name if there is one - add this in if not
            string filename = connectionstring;
            if(connectionstring.ToLower().StartsWith("data source="))
                filename = connectionstring.Split('=',StringSplitOptions.None)[1].TrimEnd(';');

            int output = -1;
            System.Data.DataSet ds = new();
            string error = "";
            try
            {
                Microsoft.Data.Sqlite.SqliteConnection conn = new($"Data Source={filename};");
                conn.Open();
                Microsoft.Data.Sqlite.SqliteCommand cmd = new(query, conn);

                ds.Tables.Add(new System.Data.DataTable());
                ds.EnforceConstraints = false;
                //.EnforceConstraint
                // SqLiteDataAdapter da = new();


                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    do
                    {
                        //dt.BeginLoadData();
                        ds.Tables[0].Load(dr);
                        //dt.EndLoadData();
                        //ds.Tables.Add(dt);
                    }
                    while (!dr.IsClosed && dr.NextResult());

                }
                conn.Close();
                this.LastResult=ds;
                this.SetTableNames();

            }
            catch(Exception e)
            {
                // "SQLite Error 14: 'unable to open database file'."
                error = e.Message;
                if(error.Contains("'unable to open database file'"))
                    error = error.Replace("'unable to open database file'","unable to open database file '"+ filename + "'");
                
            }
            if(error != "")
            {
                this.LastError=error;
                return -2;
            }

            output = ds.Tables[0].Rows.Count;
            return output;
        }

        private int QueryExecuteScalarSqlLite(string connectionstring, string query)
        {
            int output = -1;
            string filename = connectionstring;
            if(connectionstring.ToLower().StartsWith("data source="))
                filename = connectionstring.Split('=',StringSplitOptions.None)[1].TrimEnd(';');
            string error = "";

            Microsoft.Data.Sqlite.SqliteConnection conn;
            try
            {

                conn = new($"Data Source={filename};");
                conn.Open();

                Microsoft.Data.Sqlite.SqliteCommand cmd = new(query, conn);

                // System.Data.DataSet ds = new();
                // ds.Tables.Add(new System.Data.DataTable());
                // ds.EnforceConstraints = false;
                //.EnforceConstraint
                // SqLiteDataAdapter da = new();
                object? result = cmd.ExecuteScalar();
                output = Int32.Parse("0" + new string((result ?? "0").ToString().Where(char.IsDigit).ToArray()));
                this.LastScalarResult = (result ?? "").ToString();
                conn.Close();
                // this.LastResult=ds;
                // this.SetTableNames();
                // output = ds.Tables[0].Rows.Count;
                // return output;

            }
            catch(Exception e)
            {
                // "SQLite Error 14: 'unable to open database file'."
                error = e.Message;
                if(error.Contains("'unable to open database file'"))
                    error = error.Replace("'unable to open database file'","unable to open database file '"+ filename + "'");
                output = -2;
            }

            if(error != "")
            {
                this.LastError=error;
            }

            return output;
        }

#endregion sqlite

#region excel


        private string GetExcelSheetName(string excelfile)
        {
            string[] parts = excelfile.Split('!',StringSplitOptions.None);
            if(parts.Length==1)
                return "";
            return parts[parts.Length-1].Trim();
        }
        private string GetExcelFileName(string excelfile)
        {
            string[] parts = excelfile.Split('!',StringSplitOptions.None);
            return parts[0].Trim();
        }


        // to do any query in Excel, we first copy the XLSX to a Sqllite database
        // then execure the query, then save that to the lastresult
        private int QueryExecuteExcel(string connectionstring, string query, string querymode = "", string outputfile = "", string outputprovider = "")
        {
            // for Excel files, the connection string is a file potentiannly 
            string sourcefile = this.GetExcelFileName(connectionstring);
            string sourcesheet = this.GetExcelSheetName(connectionstring);

            if(sourcefile=="")
            {
                this.LastError=$"No Excel file has been specified.";
                return -1;
            }


            int output = -1;

            // this displays the last error (if any)
            var xlsx = new Seamlex.Utilities.ExcelToData();

// query, querymethod, outputmode, outputconnectionstring, outputprovider
            if(!File.Exists(sourcefile))
            {

                // if the database file does not exist and the query is either blank or create table then create it
                if(query == "" || IsCreateTableQuery(query))
                {
                    try
                    {
                        xlsx.ToExcelFile(new DataSet(),sourcefile);
                    }
                    catch
                    {
                        this.LastError=$"Excel file '{sourcefile}' cannot be created.";
                        return -2;
                    }
                }
                else
                {
                    this.LastError=$"Excel file '{sourcefile}' does not exist.";
                    return -2;
                }
            }

            // this fills a .NET datatable from an XLSX file
            DataSet xlsxds = xlsx.ToDataSet(connectionstring);

            // if there is a single sheet, remove others
            if(sourcesheet!="")
            {
                for (int i = xlsxds.Tables.Count; i <= 0; i--)
                {
                    if(xlsxds.Tables[i].TableName.Trim().ToLower() != sourcesheet.Trim().ToLower())
                        xlsxds.Tables.RemoveAt(i);
                }
            }

            Microsoft.Data.Sqlite.SqliteConnection conn = new("Data Source=:memory:");
            conn.Open();

            foreach (DataTable table in xlsxds.Tables)
            {
                // 2024-03-10 SNJW there can be XSLX files with no data - let this continue
                if(table.Columns.Count==0)
                    continue;
                    
                StringBuilder columnDefinitions = new StringBuilder();
                foreach (DataColumn column in table.Columns)
                {
                    if (columnDefinitions.Length > 0)
                        columnDefinitions.Append(", ");
                    columnDefinitions.Append(column.ColumnName + " " + GetSQLiteTypeFrom(column.DataType));
                }

                string createTableCommandText = $"CREATE TABLE IF NOT EXISTS {table.TableName} ({columnDefinitions})";
                using (var sqliteCommand = new SqliteCommand(createTableCommandText, conn))
                {
                    output = sqliteCommand.ExecuteNonQuery();
                }

                string columnNames = string.Join(", ", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                string paramNames = string.Join(", ", table.Columns.Cast<DataColumn>().Select(c => "@" + c.ColumnName));
                string insertCommandText = $"INSERT INTO {table.TableName} ({columnNames}) VALUES ({paramNames})";

                using (var transaction = conn.BeginTransaction())
                {
                    foreach (DataRow row in table.Rows)
                    {
                        using (var insertCommand = new SqliteCommand(insertCommandText, conn, transaction))
                        {
                            foreach (DataColumn column in table.Columns)
                            {
                                insertCommand.Parameters.AddWithValue("@" + column.ColumnName, row[column]);
                            }
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }

            string tablename = this.GuessTableName(query);
            Microsoft.Data.Sqlite.SqliteCommand cmd = new(query, conn);
            System.Data.DataTable dt = new();


             //.EnforceConstraint
            // SqLiteDataAdapter da = new();

            // TO DO --> INSERT UPDATE DELETE
            if(querymode == "Reader")
            {
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    do
                    {
                        //dt.BeginLoadData();
                        dt.Load(dr);
                        //dt.EndLoadData();
                        //ds.Tables.Add(dt);
                    }
                    while (!dr.IsClosed && dr.NextResult());
                }
                if(tablename!= "")
                    dt.TableName = tablename;
                output = dt.Rows.Count;
            }
            else if(querymode == "Scalar" || querymode == "NonQuery")
            {
                using (cmd)
                {
                    output = cmd.ExecuteNonQuery();
                }

                if(tablename!= "")
                {
                    Microsoft.Data.Sqlite.SqliteCommand reloadcmd = new($"SELECT * FROM {tablename};", conn);
                    using (SqliteDataReader dr = reloadcmd.ExecuteReader())
                    {
                        do
                        {
                            //dt.BeginLoadData();
                            dt.Load(dr);
                            //dt.EndLoadData();
                            //ds.Tables.Add(dt);
                        }
                        while (!dr.IsClosed && dr.NextResult());
                    }
                    dt.TableName = tablename;
                }
            }

            System.Data.DataSet ds = new();
            ds.EnforceConstraints = false;

// need to build a 'full' dataset for non-queries
// but only a single table for select commands
            if(querymode == "NonQuery")
            {
                // need to see whether this exists already in the output


                if(tablename == "")
                {
                    tablename = "Table"+ds.Tables.Count.ToString();
                }
                dt.TableName = tablename;
                bool tableadded = false;
                foreach (DataTable sourcetable in xlsxds.Tables)
                {
                    if(sourcetable.TableName.Trim().ToLower() == tablename.Trim().ToLower() && !tableadded)
                    {
                        ds.Tables.Add(dt);
                        tableadded = true;
                    }
                    else
                    {
                        var newTable = sourcetable.Clone(); // Clone structure, not data
                        foreach (DataRow row in sourcetable.Rows)
                        {
                            newTable.ImportRow(row);
                        }

                        // Add the new table to the destination DataSet
                        newTable.TableName = sourcetable.TableName; // Ensure the table name is carried over
                        ds.Tables.Add(newTable);
                    }
                }
                if(!tableadded)
                    ds.Tables.Add(dt);
            }

            if(querymode=="Scalar")
            {
                string scalarResult = "";
                if(lastresult.Tables.Count>0)
                    if(lastresult.Tables[0].Rows.Count>0)
                        if(lastresult.Tables[0].Rows[0].ItemArray.Length>0)
                            scalarResult = (lastresult.Tables[0].Rows[0].ItemArray[0] ?? "").ToString();
                this.LastScalarResult = scalarResult;
                ds.Tables.Add(dt);
            }

            if(querymode=="Reader")
            {
                ds.Tables.Add(dt);
            }

            conn.Close();
            this.LastResult=null;
            GC.Collect();
            this.LastResult=ds;

            // after running the query, we need to work out whether we save this excel over the top of the source file
            // (which is dangerous - but that is how databases work!)
            // or save it into a new file


            // with Excel/CSV files, by default we will NOT overwrite with update/delete/insert, and save to lastresult
            // with databases, by default we WILL overwrite with update/delete/insert, and NOT save to lastresult
            // however, this is passed-into this method so don't do any thinking here
            if(outputfile != "" && ( outputprovider == "Microsoft.Excel" || outputprovider == "Default" || outputprovider == "Text" || outputprovider == "XML" ) )
            {

                // currenttarget = "Source"; // overwrite the source file
                // currenttarget = "Output"; // create new destination file
                // currenttarget = "None"; // no overwriting
                // currenttarget = "Default"; // overwrite to a destination
                if(File.Exists(outputfile))
                {
                    System.IO.File.Delete(outputfile);
                    GC.Collect();
                }

                if(outputprovider == "Microsoft.Excel" || outputprovider == "Default")
                {
                    xlsx.ToExcelFile(LastResult, outputfile);
                }
                else if(outputprovider == "CSV")
                {
                    xlsx.ToCsvFile(LastResult, outputfile);
                }
                else if(outputprovider == "Text")
                {
                    if(querymode=="Scalar")
                    {
                        System.IO.File.WriteAllText(outputfile,this.lastscalarresult);
                    }
                    else if(querymode=="NonQuery")
                    {
                        System.IO.File.WriteAllText(outputfile,this.rowsaffected.ToString());
                    }
                    else
                    {
                        System.IO.File.WriteAllText(outputfile,lastresult.ToString());
                    }
                }
                else if(outputprovider == "XML")
                {
                    System.IO.File.WriteAllText(outputfile,lastresult.ToString());
                }
                GC.Collect();
            }

            this.SetTableNames();
            this.RowsAffected = output;
            return output;
        }


        // private int QueryExecuteQueryExcel(string connectionstring, string query, bool isscalar = false)
        // {

        //     /utput


        //     string[] excelFiles = connectionstring.Split(new char[]{'|'});
        //     string inputXlsx = excelFiles[0].Trim();
        //     string outputXlsx = inputXlsx;
        //     if(excelFiles.Length>1)
        //         outputXlsx = excelFiles[1].Trim();

        //     //int output = -1;
        //     string sqlLiteInMemory = "Data Source=:memory:";

        //     // this displays the last error (if any)
        //     var xlsx = new Seamlex.Utilities.ExcelToData();

        //     // this creates an Excel file from a .NET List<T>
        //     DataSet xlsxds = xlsx.ToDataSet(inputXlsx);
        //     Microsoft.Data.Sqlite.SqliteConnection conn = new(sqlLiteInMemory);
        //     conn.Open();

        //     foreach (DataTable table in xlsxds.Tables)
        //     {
        //         StringBuilder columnDefinitions = new StringBuilder();
        //         foreach (DataColumn column in table.Columns)
        //         {
        //             if (columnDefinitions.Length > 0)
        //                 columnDefinitions.Append(", ");
        //             columnDefinitions.Append(column.ColumnName + " " + GetSQLiteTypeFrom(column.DataType));
        //         }

        //         string createTableCommandText = $"CREATE TABLE IF NOT EXISTS {table.TableName} ({columnDefinitions})";
        //         using (var sqliteCommand = new SqliteCommand(createTableCommandText, conn))
        //         {
        //             sqliteCommand.ExecuteNonQuery();
        //         }

        //         string columnNames = string.Join(", ", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
        //         string paramNames = string.Join(", ", table.Columns.Cast<DataColumn>().Select(c => "@" + c.ColumnName));
        //         string insertCommandText = $"INSERT INTO {table.TableName} ({columnNames}) VALUES ({paramNames})";

        //         using (var transaction = conn.BeginTransaction())
        //         {
        //             foreach (DataRow row in table.Rows)
        //             {
        //                 using (var insertCommand = new SqliteCommand(insertCommandText, conn, transaction))
        //                 {
        //                     foreach (DataColumn column in table.Columns)
        //                     {
        //                         insertCommand.Parameters.AddWithValue("@" + column.ColumnName, row[column]);
        //                     }
        //                     insertCommand.ExecuteNonQuery();
        //                 }
        //             }
        //             transaction.Commit();
        //         }
        //     }

        //     // run the query and then save this back to the XLSX
        //     // note that there is no 'undo ' feature here!!
        //     Microsoft.Data.Sqlite.SqliteCommand cmd = new(query, conn);
        //     // System.Data.DataSet ds = new();
        //     // ds.Tables.Add(new System.Data.DataTable());
        //     // ds.EnforceConstraints = false;
        //     int rowsAffected = cmd.ExecuteNonQuery();
        //     DataSet ds = new();
        //     int tableIndex = 0;
        //     foreach (DataTable table in xlsxds.Tables)
        //     {
        //         cmd.CommandText = $"SELECT * FROM {table.TableName};";

        //         //  
        //         using (SqliteDataReader dr = cmd.ExecuteReader())
        //         {
        //             do
        //             {
        //                 DataTable dt = new();
        //                 dt.Load(dr);
        //                 dt.TableName = table.TableName;
        //                 ds.Tables.Add(dt);
        //                 //dt.EndLoadData();
        //                 //ds.Tables.Add(dt);
        //             }
        //             while (!dr.IsClosed && dr.NextResult());
        //         }
        //         tableIndex++;
        //     }

        //     conn.Close();
        //     this.LastResult=ds;
        //     if(isscalar)
        //     {
        //         string scalarResult = "";
        //         if(lastresult.Tables.Count>0)
        //             if(lastresult.Tables[0].Rows.Count>0)
        //                 if(lastresult.Tables[0].Rows[0].ItemArray.Length>0)
        //                     scalarResult = (lastresult.Tables[0].Rows[0].ItemArray[0] ?? "").ToString();
        //         this.LastScalarResult = scalarResult;
        //     }

        //     // with Excel/CSV files, by default we will NOT overwrite with update/delete/insert, and save to lastresult
        //     // with databases, by default we WILL overwrite with update/delete/insert, and NOT save to lastresult
        //     if(this.CurrentTarget == "Default" )
        //     {

        //         // currenttarget = "Source";
        //         // currenttarget = "Output";
        //         // currenttarget = "None";
        //         // currenttarget = "Default";
        //     }




        //     // now set this back to the XLSX
        //     return rowsAffected;



        //     // int output = -1;

        //     // try
        //     // {

        //     //     Microsoft.Data.Sqlite.SqliteConnection conn = new(connectionstring);
        //     //     conn.Open();
        //     //     Microsoft.Data.Sqlite.SqliteCommand cmd = new(query, conn);

        //     //     // System.Data.DataSet ds = new();
        //     //     // ds.Tables.Add(new System.Data.DataTable());
        //     //     // ds.EnforceConstraints = false;
        //     //     //.EnforceConstraint
        //     //     // SqLiteDataAdapter da = new();

        //     //     output = cmd.ExecuteNonQuery();

        //     //     conn.Close();
        //     //     // this.LastResult=ds;
        //     //     // this.SetTableNames();
        //     //     // output = ds.Tables[0].Rows.Count;
        //     //     // return output;                

        //     // }
        //     // catch
        //     // {
        //     //     output = -2;
        //     // }

        //     // if (output == -2 || output == -3)
        //     //     return output;


        //     // return output;
        // }

#endregion excel

#region csv

        private int QueryExecuteReaderCsv(string connectionstring, string query)
        {
            int output = -1;

            // for XLSX, the 

            Microsoft.Data.Sqlite.SqliteConnection conn = new(connectionstring);
            conn.Open();
            Microsoft.Data.Sqlite.SqliteCommand cmd = new(query, conn);

            System.Data.DataSet ds = new();
            ds.Tables.Add(new System.Data.DataTable());
            ds.EnforceConstraints = false;
             //.EnforceConstraint
            // SqLiteDataAdapter da = new();

            using (SqliteDataReader dr = cmd.ExecuteReader())
            {
                do
                {
                    //dt.BeginLoadData();
                    ds.Tables[0].Load(dr);
                    //dt.EndLoadData();
                    //ds.Tables.Add(dt);
                }
                while (!dr.IsClosed && dr.NextResult());

            }

            conn.Close();
            this.LastResult=ds;
            this.SetTableNames();
            output = ds.Tables[0].Rows.Count;
            return output;
        }

        private int QueryExecuteScalarCsv(string connectionstring, string query)
        {
            // this needs completing - will not work as intended
            int output = -1;
            Microsoft.Data.Sqlite.SqliteConnection conn;
            try
            {

                conn = new(connectionstring);
                conn.Open();

                Microsoft.Data.Sqlite.SqliteCommand cmd = new(query, conn);

                // System.Data.DataSet ds = new();
                // ds.Tables.Add(new System.Data.DataTable());
                // ds.EnforceConstraints = false;
                //.EnforceConstraint
                // SqLiteDataAdapter da = new();
                object? result = cmd.ExecuteScalar();
                output = Int32.Parse("0" + new string((result ?? "0").ToString().Where(char.IsDigit).ToArray()));
                conn.Close();
                // this.LastResult=ds;
                // this.SetTableNames();
                // output = ds.Tables[0].Rows.Count;
                // return output;


            }
            catch
            {
                output = -2;
            }

            if (output == -2 || output == -3)
                return output;

            return output;
        }

        private int QueryExecuteNonQueryCsv(string connectionstring, string query)
        {
            // this needs completing - will not work as intended
            int output = -1;
            try
            {

                Microsoft.Data.Sqlite.SqliteConnection conn = new(connectionstring);
                conn.Open();
                Microsoft.Data.Sqlite.SqliteCommand cmd = new(query, conn);

                // System.Data.DataSet ds = new();
                // ds.Tables.Add(new System.Data.DataTable());
                // ds.EnforceConstraints = false;
                //.EnforceConstraint
                // SqLiteDataAdapter da = new();

                output = cmd.ExecuteNonQuery();

                conn.Close();
                // this.LastResult=ds;
                // this.SetTableNames();
                // output = ds.Tables[0].Rows.Count;
                // return output;                

            }
            catch
            {
                output = -2;
            }

            if (output == -2 || output == -3)
                return output;


            return output;
        }



        public void SaveDataSetToCsv(DataSet dataSet, string mainFilePath)
        {
            if (dataSet == null || dataSet.Tables.Count == 0)
            {
                return;
            }

            bool isDirectory = Directory.Exists(mainFilePath);
            for (int i = 0; i < dataSet.Tables.Count; i++)
            {
                DataTable table = dataSet.Tables[i];

                StringBuilder csvContent = new StringBuilder();

                // Writing column headers
                string[] columnNames = table.Columns.Cast<DataColumn>()
                                        .Select(column => $"\"{column.ColumnName.Replace("\"", "\"\"")}\"")
                                        .ToArray();
                csvContent.AppendLine(string.Join(",", columnNames));

                // Writing data rows
                foreach (DataRow row in table.Rows)
                {
                    string[] rowValues = row.ItemArray.Select(field =>
                    {
                        string fieldAsString = field.ToString();
                        // Encapsulate the field in double quotes and escape internal quotes
                        return $"\"{fieldAsString.Replace("\"", "\"\"")}\"";
                    }).ToArray();
                    csvContent.AppendLine(string.Join(",", rowValues));
                }

                // Write the CSV content to a file
                string outputPath = mainFilePath;
                if(isDirectory)
                {
                    outputPath +=  $"{table.TableName}.csv";
                }
                else if(dataSet.Tables.Count > 1)
                {
                    if(Path.GetExtension(mainFilePath).ToLower() == ".csv")
                    {
                        outputPath = Path.GetFileNameWithoutExtension(mainFilePath) +  $"{table.TableName}.csv";
                    }
                    else
                    {
                        outputPath = mainFilePath +  $".{table.TableName}.csv";
                    }
                }

                File.WriteAllText(outputPath, csvContent.ToString());
                
            }
        }



#endregion csv


        private void SetTableNames()
        {
            this.SetTableNames(this.lastresult, this.currenttablenames);
        }
        private void SetTableNames(System.Data.DataSet source, string namelist)
        {
            // 2.1.3 SNJW by default, the dataset will be "Table, Table1, Table2 ..."
            // This just takes the object tablename property, scans it for a delimiter (I don't care what), splits
            // it up, then goes through the attached DataSet and updates the names there appropirately

            // 2.1.4 SNJW this does not work - needs rethinking
            // This should be simple: get the existing list of names in the dataset
            // then, check whether any already exist in "namelist" - if so, remove this from namelist and skip it when re-naming
            if(namelist=="")
                return;

            // this is intentionally case-sensitive
            List<string> existing = new List<string>();
            foreach (System.Data.DataTable thistable in source.Tables)
            {
                existing.Add(thistable.TableName);
            }

            string[] updates = namelist.Split(new char[] { ';', ',' });

            List<string> allupdates = updates.ToList();
            int sourcecount = source.Tables.Count;
            int updatecount = 0;
            for (int currenttablecount = 0; currenttablecount < source.Tables.Count; currenttablecount++)
            {
                // if this table already exists in update somewhere, skip it
                string currenttablename = source.Tables[currenttablecount].TableName;
                if (allupdates.Count(x => x.Equals(currenttablename)) == 0)
                {
                    if(allupdates[updatecount] != "")
                        source.Tables[currenttablecount].TableName = allupdates[updatecount];
                    updatecount++;
                }
            }
        }

        public string GuessQueryMethod(string sqlQuery)
        {
            string checkSql = (sqlQuery.Trim().ToLower() + new string(' ',20));
            if(checkSql.Substring(0,7) == "select ")
                return "Reader";
            return "NonQuery";
        }

        public bool IsCreateTableQuery(string sqlQuery)
        {
            string[] parts = sqlQuery.Trim().Split(' ',StringSplitOptions.None);
            if(parts.Length < 3)
                return false;
            return parts[0].ToUpper() == "CREATE" && parts[1].ToUpper() == "TABLE";
        }

        public string GuessTableName(string sqlQuery)
        {
            string tablename = "";
            string[] parts = sqlQuery.Trim().Split(' ',StringSplitOptions.None);
            if(parts.Length < 3)
                return "";
            if(parts[0].ToUpper() == "SELECT")
            {
                // if there is a FROM somewhere then the next item will hopefully be the table-name
                int fromindex = -1;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if(parts[i].ToUpper()=="FROM")
                    {
                        fromindex = i;
                        break;
                    }
                }

                if(fromindex>0)
                {
                    tablename = parts[fromindex+1];

                    // now check if this is a join - if so, do not return it
                    if(parts.Length > fromindex+2)
                    {
                        if(parts[fromindex+1] == ",")
                        {
                            tablename = "";
                        }
                    }
                    else if(tablename.EndsWith(','))
                    {
                        tablename = "";
                    }
                    else
                    {
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if(parts[i].ToUpper() == "JOIN" && i < parts.Length - 3)
                            {
                                if(parts[i+2].ToUpper() == "ON" || parts[i+2].ToUpper() == "WHERE" )
                                {
                                    tablename = "";
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if(parts[0].ToUpper() == "INSERT" || parts[1].ToUpper() == "INTO")
            {
                tablename = parts[2].Split('(',StringSplitOptions.None)[0].Trim();
                // account for brackets
                // "AspNetUsers(Id,"

            }
            else if(parts[0].ToUpper() == "CREATE" || parts[1].ToUpper() == "TABLE")
            {
                tablename = parts[2].Split('(',StringSplitOptions.None)[0].Trim();
                // account for brackets
                // "AspNetUsers(Id,"
            }
            else if(parts[0].ToUpper() == "UPDATE" )
            {
                tablename = parts[1];
            }
            else if(parts[0].ToUpper() == "DELETE" || parts[1].ToUpper() == "FROM")
            {
                tablename = parts[2];
            }
            return tablename.TrimEnd(';').TrimEnd(' ');
        }

        public string GuessProvider(string connString)
        {
            if(connString.ToLower().Trim().StartsWith("data source=") && connString.ToLower().TrimEnd(';').EndsWith(".db"))
                return "Microsoft.Data.Sqlite";
            if(connString.ToLower().Trim().TrimEnd(';').EndsWith(".db"))
                return "Microsoft.Data.Sqlite";
            if(connString.ToLower().TrimEnd(';').EndsWith(".xlsx"))
                return "Microsoft.Excel";
            if(connString.ToLower().TrimEnd(';').EndsWith(".csv"))
                return "CSV";
            if(connString.ToLower().TrimEnd(';').EndsWith(".txt"))
                return "Text";
            if(connString.ToLower().TrimEnd(';').EndsWith(".xml"))
                return "XML";
            if(connString.ToLower().TrimEnd(';').EndsWith(".json"))
                return "JSON";
            return "MySql.Data.MySqlClient";
        }

        public string GetPipeTable(DataTable dataTable, int maxRows = -1)
        {
            StringBuilder output = new();
            if (dataTable == null || dataTable.Columns.Count == 0)
                return "";
            
            if(maxRows > dataTable.Rows.Count || maxRows > (16^16) || maxRows < 0)
                maxRows = System.Math.Min(dataTable.Rows.Count,maxRows);

            // Calculate the maximum width for each column
            int[] columnWidths = new int[dataTable.Columns.Count];
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                columnWidths[i] = dataTable.Columns[i].ColumnName.Trim().Length; // Start with column name length
                if(maxRows > 0)
                {
                    for (int j = 0; j < maxRows; j++)
                    {
                        DataRow row = dataTable.Rows[j];

                        // Update the column width if any value is longer than the current max
                        int length = row[i].ToString().Trim().Length;
                        if (length > columnWidths[i])
                        {
                            columnWidths[i] = length;
                        }
                    }
                }
            }

            // Iterate through each row of the DataTable
            StringBuilder rowString = new StringBuilder();

            // ... but start with the column names

            // Iterate through each column of the row
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                // Pad the column value to ensure fixed width and append to the row string
                rowString.Append(dataTable.Columns[i].ColumnName.Trim().PadRight(columnWidths[i]));

                // If it's not the last column, append the pipe delimiter
                if (i < dataTable.Columns.Count - 1)
                {
                    rowString.Append(" | "); // Add spaces around the pipe for better readability
                }
            }
            // add this to the output
            output.AppendLine(rowString.ToString());
            rowString.Clear();

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                rowString.Append(new String('-', columnWidths[i] + 1));

                if(i < dataTable.Columns.Count - 1)
                    rowString.Append("+-");
            }
            output.AppendLine(rowString.ToString());
            rowString.Clear();

            // output.AppendLine(new String('-', columnWidths.Sum() + ( ( dataTable.Columns.Count - 1 ) * 3 ) ));

            for (int j = 0; j < maxRows; j++)
            {
                DataRow row = dataTable.Rows[j];
                // Iterate through each column of the row
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    // Pad the column value to ensure fixed width and append to the row string
                    string columnValue = row[i].ToString().PadRight(columnWidths[i]);
                    rowString.Append(columnValue);

                    // If it's not the last column, append the pipe delimiter
                    if (i < dataTable.Columns.Count - 1)
                    {
                        rowString.Append(" | "); // Add spaces around the pipe for better readability
                    }
                }
                // add this to the output
                output.AppendLine(rowString.ToString());
                rowString.Clear();
            }
            return output.ToString();
        }

        public int DataSetToSqliteFile(DataSet ds, string filename)
        {
            int output = -1;

            if(System.IO.File.Exists(filename))
            {
                try
                {
                    System.IO.File.Delete(filename);
                    GC.Collect();
                }
                catch
                {
                    output = -2;
                    this.LastError = $"Cannot delete existing '{filename}' file.";
                }
                if(output == -2)
                    return output;
            }

            try
            {
                this.SaveDataSetToSQLite(ds,filename);
                GC.Collect();
                output = 1;
            }
            catch
            {
                output = -2;
                this.LastError = $"Cannot create Sqlite file '{filename}'.";
            }
            return output;

        }
        public int DataSetToJsonFile(DataSet ds, string filename)
        {
            int output = -1;
            if(System.IO.File.Exists(filename))
            {
                try
                {
                    System.IO.File.Delete(filename);
                    GC.Collect();
                    output = 1;
                }
                catch
                {
                    output = -2;
                    this.LastError = $"Cannot delete existing '{filename}' file.";
                }
                if(output == -2)
                    return output;
            }

            try
            {
                // jsonds = new Newtonsoft.Json();


                // System.IO.File.WriteAllText(filename,Newtonsoft.Json.JsonConverter() ds.ToString());
                GC.Collect();
            }
            catch
            {
                output = -2;
                this.LastError = $"Cannot fill '{filename}' with dataset.";
            }
            return output;
        }
        public int DataSetToXmlFile(DataSet ds, string filename)
        {
            int output = -1;
            if(System.IO.File.Exists(filename))
            {
                try
                {
                    System.IO.File.Delete(filename);
                    GC.Collect();
                    output = 1;
                }
                catch
                {
                    output = -2;
                    this.LastError = $"Cannot delete existing '{filename}' file.";
                }
                if(output == -2)
                    return output;
            }

            try
            {
                ds.WriteXml(filename);
                GC.Collect();
            }
            catch
            {
                output = -2;
                this.LastError = $"Cannot fill '{filename}' with dataset.";
            }
            return output;
        }
        public int DataSetToCsvFile(DataSet ds, string filename)
        {
            int output = -1;
            var xslxds = new Seamlex.Utilities.ExcelToData();
            try
            {
                xslxds.ToCsvFile(ds,filename);
                output = 1;
            }
            catch
            {
                output = -2;
                this.LastError = $"Cannot create CSV file '{filename}'.";
            }
            return output;
        }
        public int DataSetToTextFile(DataSet ds, string filename)
        {
            int output = -1;
            if(System.IO.File.Exists(filename))
            {
                try
                {
                    System.IO.File.Delete(filename);
                    GC.Collect();
                }
                catch
                {
                    output = -2;
                    this.LastError = $"Cannot overwrite '{filename}' with new text file.";
                }
                if(output == -2)
                    return output;
            }

            try
            {
                ds.WriteXml(filename);
                GC.Collect();
                output = 1;
            }
            catch
            {
                output = -2;
                this.LastError = $"Cannot fill '{filename}' with dataset.";
            }
            return output;
        }
        public int ValueToTextFile(string textvalue, string filename)
        {
            int output = -1;
            if(System.IO.File.Exists(filename))
            {
                try
                {
                    System.IO.File.Delete(filename);
                    GC.Collect();
                }
                catch
                {
                    output = -2;
                    this.LastError = $"Cannot overwrite '{filename}' with new text file.";
                }
                if(output == -2)
                    return output;
            }

            try
            {
                File.WriteAllText(filename,textvalue);
                GC.Collect();
                output = 1;
            }
            catch
            {
                output = -2;
                this.LastError = $"Cannot fill '{filename}' with dataset.";
            }
            return output;
        }

        public int DataSetToExcelFile(DataSet ds, string filename)
        {
            int output = -1;
            var xslxds = new Seamlex.Utilities.ExcelToData();
            try
            {
                xslxds.ToExcelFile(ds,filename);
                output = 1;
            }
            catch
            {
                output = -2;
                this.LastError = $"Cannot create XLSX file '{filename}'.";
            }
            return output;                
        }



    #endregion QueryExecuteGenDb

    }

}

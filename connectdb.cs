/* 2018-02-05 SNJW fix the universal data loader later

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlServerCe;
using MySql.Data.MySqlClient;
using System.Data.OleDb;
using System.IO;



namespace gd
{
    /// <summary>
    /// 2.1.3 SNJW this class is the universal data-access module.
    /// The NonQuery just returns the success/fail code (if negative) or rows affected (if >= 0).
    /// The Scalar does the same but also sets the string lastscalarresult.
    /// The Reader does likewise with the code, and sets the lastresult DataSet.
    /// </summary>
    public class connectdb
    {

        public string currentprovider = "";
        public string currentconnectionstring = "";
        public string currentquery = "";
        public string currenttablenames = "";
        public string currentquerymode = "NonQuery";
        public string lastscalarresult = "";
        public int rowsaffected = -1;
        public System.Data.DataSet lastresult = new System.Data.DataSet();

        public bool OpenConnection()
        {
            // assume the connection-strings are already set
            return true;
        }

//        public bool CreateDatabase(string connectionstring, string providertype, string dbname, string query)
//        {
//            // 2.1.0 SNJW this is a generic db-creation tool
//            // 2.1.3 SNJW this sets the appropriate properties on the object, then runs a query
//            int result = -1;
//            bool output = false;

//            // try for each
//            if (providertype == "System.Data.SqlClient")
//            {
//                // if query is blank, do this
//                if(query=="")
//                {
//                    StringBuilder sb = new StringBuilder();
//                    sb.AppendLine("USE master");
//                    sb.AppendLine("GO");
//                    sb.AppendLine("IF EXISTS(SELECT name FROM sys.databases WHERE name = '" + dbname +@"')");
//                    sb.AppendLine("    DROP DATABASE " + dbname +"");
//                    sb.AppendLine("GO");
//                    sb.AppendLine("CREATE DATABASE " + dbname +"");
//                    sb.AppendLine("GO");
//                    sb.AppendLine("USE " + dbname +"");
//                    sb.AppendLine("GO");
//                    query = sb.ToString();
//                }
//                this.currentconnectionstring=connectionstring

//                result = this.QueryExecute(connectionstring, providertype, query);
//            }
//            else if (providertype == "MySql.Data.MySqlClient")
//            {
//                // if query is blank, do the generic MySQL db creation
//                if(query=="")
//                {
//                    StringBuilder sb = new StringBuilder();
//                    sb.AppendLine("CREATE DATABASE IF NOT EXISTS " + dbname +"");
//                    query = sb.ToString();
//                }

//                result = this.QueryExecute(connectionstring, providertype, query);
//            }
//            else if (providertype == "System.Data.SqlServerCe.4.0")
//            {
//                // if query is blank, do the generic SQLCE creation
//                // we need to create the physical file first
//          //      try
//         //       {

//                  if (System.IO.File.Exists(dbname))
//                  {
//                    System.IO.File.Delete(dbname);
//                  }

//                  SqlCeEngine en = new SqlCeEngine(connectionstring);
//                  en.CreateDatabase();
//                  en.Dispose();
//          //      }
//           //     catch
//           //     {
//           //     }
                
//                // note that dbname will be a full path
//                if(query=="")
//                {
//                    StringBuilder sb = new StringBuilder();
//                    sb.AppendLine("CREATE DATABASE '" + System.IO.Path.GetFileNameWithoutExtension(dbname) +@"'");
//                    query = sb.ToString();
//                }

//                result = this.QueryExecute(connectionstring, providertype, query);
//            }
//            else if (providertype == "System.Data.Odbc")
//            {
//                // TO DO - access, excel, vfp, text
////                output = this.TestOdbcConnection(connectionstring);
//            }

//            output = (result >0);

//            return output;



//        }





        #region GetDatabaseVersion
        
        public string GetVersion()
        {
            return "20100";
        }

        public int VersionAsInteger(string versionstring)
        {
            string[] allparts = versionstring.Split('.');
            int major = System.Int32.Parse(allparts[0]);
            int minor = System.Int32.Parse(allparts[1]);
            int revision = System.Int32.Parse(allparts[2]);

            return (major * 10000) + (minor * 100) + (revision);
        }
        public string VersionAsString(int versionint)
        {
            string allparts = versionint.ToString();
            string major = allparts.Substring(0, allparts.Length - 4);
            string minor = allparts.Substring(allparts.Length - 4, 2);
            string revision = allparts.Substring(allparts.Length - 2, 2);

            return major + "." + minor + "." + revision;
        }

        public int GetDatabaseVersion(string connectionstring, string providertype)
        {
            // 1.6.8 SNJW this is 10608 for 1.6.8 and stored in semaphore
            int output = 10000;

            // try for each
            if (providertype == "System.Data.SqlClient")
            {
                output = this.GetDatabaseVersionSqlServer(connectionstring);
            }
            else if (providertype == "MySql.Data.MySqlClient")
            {
                output = this.GetDatabaseVersionMySql(connectionstring);
            }
            else if (providertype == "System.Data.SqlServerCe.4.0")
            {
                output = this.GetDatabaseVersionSqlServerCompact(connectionstring);
            }
            else if (providertype == "System.Data.Odbc")
            {
                output = -1;
            }
            return output;
        }

        private int GetDatabaseVersionSqlServer(string connectionstring)
        {
            return this.QueryExecuteScalarSqlServer(connectionstring,"SELECT semaphoreValue FROM semaphore WHERE semaphoreField = 'DATABASEVERSION'");
        }

        private int GetDatabaseVersionSqlServerCompact(string connectionstring)
        {
            return this.QueryExecuteScalarSqlServerCompact(connectionstring,"SELECT semaphoreValue FROM semaphore WHERE semaphoreField = 'DATABASEVERSION'");
        }

        private int GetDatabaseVersionOdbc(string connectionstring)
        {
            return this.QueryExecuteScalarOdbc(connectionstring, "SELECT semaphoreValue FROM semaphore WHERE semaphoreField = 'DATABASEVERSION'");
        }

        private int GetDatabaseVersionMySql(string connectionstring)
        {
            return this.QueryExecuteScalarMySql(connectionstring,"SELECT semaphoreValue FROM semaphore WHERE semaphoreField = 'DATABASEVERSION'");
        }

        #endregion GetDatabaseVersion


        #region TestGenDbConnection

        /// <summary>
        /// 2.1.3 SNJW this constructs an appropriate null query for the respective provider.
        /// It then trys to connect, runs this query, then closes that connection.
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
            if (provider == "System.Data.SqlClient")
            {
                output = this.TestSqlServerConnection(connectionstring);
            }
            else if (provider == "MySql.Data.MySqlClient")
            {
                output = this.TestMySqlConnection(connectionstring);
            }
            else if (provider == "System.Data.SqlServerCe.4.0")
            {
                output = this.TestSqlServerCompactConnection(connectionstring);
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

            return output;

        }
        private string TestSqlServerCompactConnection(string connectionstring)
        {
            string output = "Test successful";
            string testcommand = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE 1 < 0";
            bool failnow = false;


            try
            {

                System.Data.SqlServerCe.SqlCeConnection testconn = new System.Data.SqlServerCe.SqlCeConnection(connectionstring);
                try
                {
                    System.Data.SqlServerCe.SqlCeCommand testcmd = new
                        System.Data.SqlServerCe.SqlCeCommand(testcommand, testconn);
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


            System.Data.SqlServerCe.SqlCeConnection conn = new System.Data.SqlServerCe.SqlCeConnection(connectionstring);
            System.Data.SqlServerCe.SqlCeCommand cmd = new
                System.Data.SqlServerCe.SqlCeCommand(testcommand, conn);

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
        private string TestSqlServerConnection(string connectionstring)
        {
                string output = "Test successful";
                string testcommand = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE 1 < 0";
                bool failnow = false;


                 try
                 {

                    System.Data.SqlClient.SqlConnection testconn = new System.Data.SqlClient.SqlConnection(connectionstring);
                     try
                     {
                        System.Data.SqlClient.SqlCommand testcmd = new
                            System.Data.SqlClient.SqlCommand(testcommand, testconn);
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


                System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connectionstring);
                System.Data.SqlClient.SqlCommand cmd = new
                    System.Data.SqlClient.SqlCommand(testcommand, conn);

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
            if(connectionstring.Contains("Microsoft Access Driver"))
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
            return this.QueryExecute(this.currentconnectionstring,this.currentprovider, this.currentquery, this.currentquerymode);
        }

        public int QueryExecute(string connectionstring, string providertype, string query, string querymode)
        {
            // 1.6.8 SNJW this is 10608 for 1.6.8 and stored in semaphore
            int output = 0;

            // try for each
            if (providertype == "System.Data.SqlClient")
            {
                if (querymode.Contains("Scalar"))
                {
                    output = this.QueryExecuteScalarSqlServer(connectionstring,query);
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
            else if (providertype == "System.Data.SqlServerCe")
            {
                if (querymode.Contains("Scalar"))
                {
                    output = this.QueryExecuteScalarSqlServerCompact(connectionstring, query);
                }
                else if (querymode.Contains("NonQuery"))
                {
                    output = this.QueryExecuteNonQuerySqlServerCompact(connectionstring, query);
                }
                else if (querymode.Contains("Reader"))
                {
                    output = this.QueryExecuteReaderSqlServerCompact(connectionstring, query);
                }
            }
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

                System.Data.SqlClient.SqlConnection testconn = new System.Data.SqlClient.SqlConnection(connectionstring);
                try
                {
                    System.Data.SqlClient.SqlCommand testcmd = new
                        System.Data.SqlClient.SqlCommand(testcommand, testconn);
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

            if(output == -2)
                return output;


            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connectionstring);
            System.Data.SqlClient.SqlCommand cmd = new
                System.Data.SqlClient.SqlCommand(testcommand, conn);


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

        private int QueryExecuteScalarSqlServerCompact(string connectionstring, string query)
        {
            int output = -1;
            string testcommand = query;
            //bool failnow = false;
            this.rowsaffected = 0;


            try
            {       

                System.Data.SqlServerCe.SqlCeConnection testconn = new System.Data.SqlServerCe.SqlCeConnection(connectionstring);
                try
                {
                    System.Data.SqlServerCe.SqlCeCommand testcmd = new
                        System.Data.SqlServerCe.SqlCeCommand(testcommand, testconn);
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


            System.Data.SqlServerCe.SqlCeConnection conn = new System.Data.SqlServerCe.SqlCeConnection(connectionstring);
            System.Data.SqlServerCe.SqlCeCommand cmd = new
                System.Data.SqlServerCe.SqlCeCommand(testcommand, conn);

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

        private int QueryExecuteNonQuerySqlServer(string connectionstring, string query)
        {
            int output = -1;
            string testcommand = query;
            //bool failnow = false;

            try
            {

                System.Data.SqlClient.SqlConnection testconn = new System.Data.SqlClient.SqlConnection(connectionstring);
                try
                {
                    System.Data.SqlClient.SqlCommand testcmd = new
                        System.Data.SqlClient.SqlCommand(testcommand, testconn);
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

            if(output == -2)
                return output;


            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connectionstring);
            System.Data.SqlClient.SqlCommand cmd = new
                System.Data.SqlClient.SqlCommand(testcommand, conn);

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

        private int QueryExecuteNonQuerySqlServerCompact(string connectionstring, string query)
        {
            int output = -1;
            string testcommand = query;
            //bool failnow = false;


            try
            {

                System.Data.SqlServerCe.SqlCeConnection testconn = new System.Data.SqlServerCe.SqlCeConnection(connectionstring);
                try
                {
                    System.Data.SqlServerCe.SqlCeCommand testcmd = new
                        System.Data.SqlServerCe.SqlCeCommand(testcommand, testconn);
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


            System.Data.SqlServerCe.SqlCeConnection conn = new System.Data.SqlServerCe.SqlCeConnection(connectionstring);
            System.Data.SqlServerCe.SqlCeCommand cmd = new
                System.Data.SqlServerCe.SqlCeCommand(testcommand, conn);

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

                System.Data.SqlClient.SqlConnection testconn = new System.Data.SqlClient.SqlConnection(connectionstring);
                try
                {
                    System.Data.SqlClient.SqlCommand testcmd = new
                        System.Data.SqlClient.SqlCommand(testcommand, testconn);
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


            System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter();
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connectionstring);
            System.Data.SqlClient.SqlCommand cmd = new
                System.Data.SqlClient.SqlCommand(testcommand, conn);

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

        private int QueryExecuteReaderSqlServerCompact(string connectionstring, string query)
        {
            int output = -1;
            string testcommand = query;
            //bool failnow = false;


            try
            {

                System.Data.SqlServerCe.SqlCeConnection testconn = new System.Data.SqlServerCe.SqlCeConnection(connectionstring);
                try
                {
                    System.Data.SqlServerCe.SqlCeCommand testcmd = new
                        System.Data.SqlServerCe.SqlCeCommand(testcommand, testconn);
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


            System.Data.SqlServerCe.SqlCeDataAdapter da = new System.Data.SqlServerCe.SqlCeDataAdapter();
            System.Data.SqlServerCe.SqlCeConnection conn = new System.Data.SqlServerCe.SqlCeConnection(connectionstring);
            System.Data.SqlServerCe.SqlCeCommand cmd = new
                System.Data.SqlServerCe.SqlCeCommand(testcommand, conn);

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

        //public void AppendQuery(string query, string tablename)
        //{
        //    // 2.1.3 SNJW this adds a query to the local SQL command depending on the provider
        //    // try for each
        //    string providertype = this.currentprovider;
        //    string existingquery = this.currentquery.Trim();

        //    if (providertype == "System.Data.SqlClient")
        //    {
        //        if(existingquery.Length == 0)

        //    }
        //    else if (providertype == "MySql.Data.MySqlClient")
        //    {

        //        else if (providertype == "System.Data.SqlServerCe")
        //    {

        //    }
        //    else if (providertype == "System.Data.Odbc")
        //    {

        //    }


        //}
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

            // this is intentionally case-sensitive
            List<string> existing = new List<string>();
            foreach(System.Data.DataTable thistable in source.Tables)
            {
                existing.Add(thistable.TableName);
            }

            string[] updates = namelist.Split(new char[]{';',','});

            List<string> allupdates = updates.ToList();
            int sourcecount = source.Tables.Count;
            int updatecount = 0;
            for(int currenttablecount = 0; currenttablecount<source.Tables.Count; currenttablecount++)
            {
                // if this table already exists in update somewhere, skip it
                string currenttablename = source.Tables[currenttablecount].TableName;
                if(allupdates.Count(x => x.Equals(currenttablename)) == 0)
                {
                    source.Tables[currenttablecount].TableName = allupdates[updatecount];
                    updatecount++;
                }

            }
 
        }


    }





        #endregion QueryExecuteGenDb

}


*/

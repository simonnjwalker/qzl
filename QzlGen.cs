
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Features;
using Seamlex.Utilities;
#pragma warning disable CS8602, CS8600, CS0219
namespace Seamlex.Utilities
{
    /// <summary>
    /// Manages and read csgen-specific parameters
    /// </summary>
    public class QzlGen
    {
        public string lastmessage="";
        public QzlGenInfo mv = new();

#region setup-generation

        public bool SetFullModel(List<ParameterSetting> ps)
        {
            // the QzlGen fullmodel is all information passed-in
            var output = new QzlGenInfo();

            // add properties onto the output
            foreach(var prop in output.GetType().GetProperties())
            {
                var checkvalue = ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--"+prop.Name)));
                if(checkvalue != null)
                {
                    if(prop.PropertyType.FullName.Equals("System.Boolean"))
                    {
                        if(checkvalue.nextinput.Trim().ToLower().StartsWith('t') ||
                            checkvalue.nextinput.Trim().ToLower().StartsWith('1') ||
                            checkvalue.nextinput.Trim().ToLower().StartsWith('y') ||
                            checkvalue.paratype.Equals(ParameterType.Switch))
                        {
                            prop.SetValue(output, true);
                        }
                        else
                        {
                            prop.SetValue(output, false);
                        }
                    }
                    else
                    {
                        prop.SetValue(output, checkvalue.nextinput);
                    }

                    if(output.category=="")
                        output.category = checkvalue.category.Trim().ToLower();
                }
            }

            output.verbosity = ToProper(output.verbosity);
            mv = output;
            return true;
        }
        public bool Action()
        {
            return this.Action(this.mv);
        }

        public bool Action(QzlGenInfo source)
        {
            // this checks whether the content of parameters are correct
            if(!this.ValidateSource(source))
                return false;

            if(source.category.ToLower()=="sql")
            {
                return this.SqlAction(this.mv);
            }
            else if(source.category.ToLower()=="text")
            {
                return this.TextAction(this.mv);
            }
            else if(source.category.ToLower()=="http")
            {
                return this.HttpAction(this.mv);
            }
            else if(source.category.ToLower()=="json")
            {
                return this.JsonAction(this.mv);
            }
            return false;
        }
        public bool ValidateSource(QzlGenInfo source)
        {
            // if(source.category=="")
            // {
            //     this.lastmessage = "No category of output has been entered.";
            //     return false;
            // }


            // if(source.genfields.Count==0)
            // {
            //     this.lastmessage = $"Creating a {source.categoryname} requires at least one field to be entered.";
            //     return false;
            // }
            // if(source.vname=="" && (source.category=="vm" || source.category=="view" || source.category=="facade" )) // || source.category=="controller" ))
            // {
            //     this.lastmessage = $"Creating a {source.categoryname} requires a ViewModel name.";
            //     return false;
            // }
            // if(source.mname=="" && ( source.category=="model" ))
            // {
            //     this.lastmessage = $"Creating a {source.categoryname} requires a Model name.";
            //     return false;
            // }
            // if(source.cname=="" && (source.category=="controller" ))
            // {
            //     this.lastmessage = $"Creating a {source.categoryname} requires a Controller action name.";
            //     return false;
            // }

            return true;
        }
        public bool JsonAction(QzlGenInfo source)
        {
            return true;
        }
        public bool TextAction(QzlGenInfo source)
        {
            return true;
        }
        public bool HttpAction(QzlGenInfo source)
        {
            return true;
        }

        public bool SqlAction(QzlGenInfo source)
        {

            // TO DO - wire the console up to the parameters
            /*

            category [string]:"reader"
            categoryname [string]:""
            connection [string]:"Data Source=app.db"
            dqchar [string]:""
            indent [int]:4
            lastmessage [string]:""
            output [string]:""
            provider [string]:"sqllite"
            query [string]:"SELECT * FROM AspNetUsers;"
            source [string]:""
            verbosity [string]:""

            */

            // if there is a .SQL file to be executed, this is done command-by-command
            List<string> consoleItems = new();
            if(source.source != "" && Path.GetExtension(source.source).ToLower() == ".sql" )
            {
                // open this and try to get the line-by-line SQL commands
                string fileText = "";
                string errorText = "";
                string errorFullText = "";
                if(System.IO.File.Exists(source.source))
                {
                    try
                    {
                        fileText = System.IO.File.ReadAllText(source.source);
                    }
                    catch(Exception e)
                    {
                        errorText = $"Cannot open file '{source.source}'";
                        errorFullText = e.Message;
                    }
                }
                else
                {
                    errorText = $"File '{source.source}' does not exist";
                }

                if(fileText =="")
                    errorText = $"File '{source.source}' is empty";

                if(errorText =="")
                {
                    // try to split the 
                    var sqlCommands = this.SplitSqlCommands(fileText);
                    foreach(string sqlText in sqlCommands)
                    {
                        if(sqlText.Trim() != "")
                        {
                            QzlGenInfo thisSource = 
                                new QzlGenInfo()
                                {
                                    output = source.output,
                                    format = source.format,
                                    source = "",
                                    provider = source.provider,
                                    query = sqlText,
                                    connection = source.connection,
                                    method = source.method,
                                    verbosity = source.verbosity,
                                    dqchar = source.dqchar
                                };

                            QzlGen qzlGen = new();
                            qzlGen.SqlAction(thisSource);
                            consoleItems.Add(qzlGen.lastmessage);
                        }
                    }
                    foreach(string consoleMessage in consoleItems)
                        this.lastmessage += Environment.NewLine + consoleMessage;
                }
                else
                {
                    if( source.verbosity == "Default" || source.verbosity == "Full" )
                    {
                        this.lastmessage = errorText.ToString();
                        if(source.verbosity == "Full" && errorFullText !="")
                            this.lastmessage += Environment.NewLine + errorFullText;
                    }
                }
                return true;
            }

            if(!source.noheuristic)
                this.ApplyHeuristics(source);

            OmniDb sqldb = new();
            sqldb.CurrentConnectionString = source.connection;
            sqldb.CurrentProvider  = source.provider;
            sqldb.CurrentQuery = source.query;
            sqldb.CurrentQueryMode = source.method;
            sqldb.CurrentVerbosity = source.verbosity;
            sqldb.CurrentOutputProvider = source.format;

            string connectionstring = sqldb.CurrentConnectionString;
            string query = sqldb.CurrentQuery;
            string querymode = sqldb.CurrentQueryMode;
            string verbosity = sqldb.CurrentVerbosity;
            string provider = sqldb.CurrentProvider;
            string outputfile = source.output;
            string outputformat = source.format;






            int result = -1;
            try
            {
                result = sqldb.QueryExecute();
            }
            catch(Exception e)
            {
                if(sqldb.LastError=="")
                    sqldb.LastError = e.Message;
                result = -2;
            }
            
            if(sqldb.LastError != "")
            {
                // the Excel query uses the SQL Lite engine - tweak the error message accordingly
                if(provider == "Microsoft.Excel")
                {
                    this.lastmessage = sqldb.LastError.Replace("SQLite Error","Excel SQL Error");
                }
                else
                {
                    this.lastmessage = sqldb.LastError;
                }
                return false;
            }


            // create an output file
            if(outputfile != "")
            {
                // sqldb.DataSetToExcelFile(sqldb.LastResult,outputfile);
                try
                {
                    if(outputformat == "Microsoft.Data.Sqlite")
                    {
                        sqldb.DataSetToSqliteFile(sqldb.LastResult,outputfile);
                    }
                    else if(outputformat == "Microsoft.Excel")
                    {
                        sqldb.DataSetToExcelFile(sqldb.LastResult,outputfile);
                    }
                    else if(outputformat == "CSV")
                    {
                        sqldb.DataSetToCsvFile(sqldb.LastResult,outputfile);
                    }
                    else if(outputformat == "JSON")
                    {
                        sqldb.DataSetToJsonFile(sqldb.LastResult,outputfile);
                    }
                    else if(outputformat == "XML")
                    {
                        sqldb.DataSetToXmlFile(sqldb.LastResult,outputfile);
                    }
                    else if(outputformat == "Text")
                    {
                        if(querymode == "Scalar")
                        {
                            sqldb.ValueToTextFile(sqldb.LastScalarResult,outputfile);
                        }
                        else
                        {
                            sqldb.DataSetToTextFile(sqldb.LastResult,outputfile);
                        }
                    }
                }
                catch
                {
                    sqldb.LastError = $"Could not create {outputformat.Replace("Data.","").Replace("Microsoft.","")} file '{outputfile}.";
                }

                if(sqldb.LastError != "")
                {
                    this.lastmessage = sqldb.LastError;
                    return false;
                }
            }

            if(result >= -1)
                result = sqldb.RowsAffected;


            // console output
            StringBuilder console = new();
            if(verbosity == "Full" )
                console.AppendLine(sqldb.CurrentQuery);


            if(querymode == "Reader" && ( verbosity == "Default" || verbosity == "Full" ) )
            {
                int maxLines = 10;
                if(verbosity == "Full")
                    maxLines = (2^24)-1;

                for (int i = 0; i < sqldb.LastResult.Tables.Count; i++)
                {
                    if(i>0)
                        console.AppendLine($" ");

                    // 
                    if(sqldb.CurrentVerbosity == "Full")
                    {
                        console.AppendLine(new String('-', sqldb.LastResult.Tables[i].TableName.Length));
                        console.AppendLine($"{sqldb.LastResult.Tables[i].TableName}");
                        console.AppendLine(new String('-', sqldb.LastResult.Tables[i].TableName.Length));
                    }

                    console.Append(sqldb.GetPipeTable(sqldb.LastResult.Tables[i],maxLines ));
                }
            }

            if(querymode == "Scalar" && ( verbosity == "Default" || verbosity == "Full" ) )
            {
                console.Append(sqldb.LastScalarResult);
            }

            if(querymode == "NonQuery" && ( verbosity == "Default" || verbosity == "Full" ) )
            {
                if(result < -1 && verbosity == "Full")
                    console.Append("The operation did not complete successfully.  ");

                if(result >= -1 && verbosity == "Full")
                    console.Append("The operation completed successfully.  ");
                
            }

            if(result < -1 && ( verbosity == "Default" || verbosity == "Full" ) )
            {
                string explanation = sqldb.GetExplanation(result);
                console.AppendLine($"qzl result code: {result}");
                console.AppendLine(explanation);
            }

            if(result >= -1 && ( verbosity == "Default" || verbosity == "Full" ) )
                console.AppendLine($"Rows affected: {result}");

            if(verbosity == "Full" && sqldb.LastDbError != "")
            {
                console.AppendLine($" ");
                console.AppendLine($"Last DB error:");
                console.AppendLine(sqldb.LastDbError);
            }


/*

none      No console output.
errors    Error messages only.
default   NonQuery: 'Rows affected: n'
        Scalar:   'n'
        Reader:   'Rows affected: n'
                    'Table1'
                    'Column1[|Column2][|Column3][..]' {max 200 chars}
                    '[Row1Item1[|Row1Item2][|Row1Item3][..]] {max 200 chars}
                    '[Row2Item1[|Row2Item2][|Row2Item3][..]] {max 200 chars}
                    '{max 20 rows}
full      NonQuery: 'Rows affected: n'
        Scalar:   'n'
        Reader:   'Rows affected: n'
                    'Table1'
                    'Column1[|Column2][|Column3][..]'
                    '[Row1Item1[|Row1Item2][|Row1Item3][..]]
                    '[Row2Item1[|Row2Item2][|Row2Item3][..]]"

*/
            this.lastmessage = console.ToString();
        

            return true;
        }

        private void ApplyHeuristics(QzlGenInfo source)
        {
            // use the following rules

            // 1 - if there is a query + provider + connection, no guesses are made
            if(source.query != "" && source.provider != "" && source.connection != "")
                return;


            // 2 - if there is no provider but either connection or source
            //     then guess based on the file extension or connection-string
            if(     (
                        source.provider == "" || source.provider == "Default"
                    )
                    &&
                    (
                        !(source.connection == "" || source.connection == "Default")
                        ||
                        !(source.source == "" || source.source == "Default")
                    )
            )
            {
                // if the file exists, use it
                string fileCheck = source.connection;
                string fileExt = Path.GetExtension(source.connection).ToLower();
                string workingDir = System.Environment.CurrentDirectory;
                bool isFile = false;

                // guess the provider from file extension
                if(fileExt == ".txt" || fileExt == ".accdb"  || fileExt == ".mdb"  || fileExt == ".db" || fileExt == ".csv" || fileExt == ".xslx" )
                    isFile = true;

                if(!isFile)
                {
                    // check the 'source' parameter 
                    fileCheck = source.source;
                    fileExt = Path.GetExtension(source.source).ToLower();
                    if(fileExt == ".txt" || fileExt == ".accdb"  || fileExt == ".mdb"  || fileExt == ".db" || fileExt == ".csv" || fileExt == ".xslx" )
                        isFile = true;
                }

                if(isFile)
                {
                    // determine whether this exists
                    isFile = File.Exists(fileCheck);
                    if(!isFile)
                    {
                        isFile = File.Exists(Path.Combine(workingDir,fileCheck));
                        if(isFile)
                            fileCheck = Path.Combine(workingDir,fileCheck);
                    }
                }

                if(isFile)
                {
                    // this exists, create a 'real' connection based on the extension type
                    if(fileExt == ".txt" )
                    {
                        source.provider = "Text";
                        source.connection = fileCheck;
                        source.source = "";
                    }
                    else if(fileExt == ".csv" )
                    {
                        source.provider = "CSV";
                        source.connection = fileCheck;
                        source.source = "";
                    }
                    else if(fileExt == ".xlsx")
                    {
                        source.provider = "Microsoft.Excel";
                        source.connection = fileCheck;
                        source.source = "";
                    }
                    else if(fileExt == ".accdb" || fileExt == ".mdb")
                    {
                        source.provider = "Microsoft.Access";
                        source.connection = @"Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq="+fileCheck+";Exclusive=1;Uid=admin;Pwd=;";;
                        source.source = "";
                    }
                    else if(fileExt == ".db")
                    {
                        source.provider = "Microsoft.Data.Sqlite";
                        source.connection = @$"Data Source="+fileCheck+";";
                        source.source = "";
                    }
                }
            }
        


            // 3 - if there no connection and no source
            //     then we can check the provider
            //     3a - if the provider is non-empty and file-based we can determine the file type
            //     3b - if the provider is empty and file-based we can determine the file type
            //     3c - then we check the current directory for files either of the specified type or any type 

            if(
                (source.connection == "" || source.connection == "Default")
                &&
                (source.source == "" || source.source == "Default")
            )
            {

                string currentDirectory = Directory.GetCurrentDirectory();

                // If the list of extensions is empty, use default types
                List<string> uniqueFiles = GetFilesWithUniqueExtensions(currentDirectory);

                List<string> extensions = new();
                if(source.provider == "" || source.provider == "Default")
                {
                    // the order here is the preference for files
                    // that is, if there is a .db file then this will be used first
                    extensions = new List<string> { ".db", ".accdb", ".mdb", ".xlsx", ".csv", ".txt" };
                }
                else
                {
                    if(source.provider == "Text")
                    {
                        extensions.Add(".txt");
                    }
                    else if(source.provider == "CSV")
                    {
                        extensions.Add(".csv");
                    }
                    else if(source.provider == "Microsoft.Excel")
                    {
                        extensions.Add(".xlsx");
                    }
                    else if(source.provider == "Microsoft.Access")
                    {
                        extensions.Add(".accdb");
                        extensions.Add(".mdb");
                    }
                    else if(source.provider == "Microsoft.Data.Sqlite")
                    {
                        extensions.Add(".db");
                    }
                }

                string fileName = "";
                string providerName = "";
                foreach(string extension in extensions)
                {
                    foreach(string uniqueFile in uniqueFiles)
                    {
                        if(Path.GetExtension(uniqueFile) == extension)
                        {
                            fileName = uniqueFile;
                            providerName = GetProviderFromExtension(extension);
                            break;
                        }
                    }
                    if(fileName != "")
                        break;
                }

                if(fileName != "")
                {
                    source.provider = providerName;
                    source.source = fileName;
                }
            }



            // 4 - if there is a query + provider + source but no connection, then;
            //     4a - if the provider is file-based and source file exists then create a valid connection
            //     4b - otherwise, set connection = source and source = ""
            if(    !(source.query == "" || source.query == "Default")
                && !(source.provider == "" || source.provider == "Default" )
                &&  (source.connection == "" || source.connection == "Default")
                && !(source.source == "" || source.source == "Default") )
            {
                if(source.provider == "Microsoft.Data.Sqlite"
                    || source.provider == "Microsoft.Access"
                    || source.provider == "Microsoft.Excel"
                    || source.provider == "CSV"
                    || source.provider == "XML"
                    || source.provider == "JSON"
                    || source.provider == "Text")
                {
                    string fileCheck = source.source;
                    if(!File.Exists(fileCheck))
                        fileCheck =Path.Combine(System.Environment.CurrentDirectory,fileCheck);

                    // this exists, create a 'real' connection based on the extension type
                    if(source.provider == "Text" )
                    {
                        source.connection = fileCheck;
                    }
                    else if(source.provider == "CSV" )
                    {
                        source.connection = fileCheck;
                    }
                    else if(source.provider == "Microsoft.Excel" )
                    {
                        source.connection = fileCheck;
                    }
                    else if(source.provider == "Microsoft.Access" )
                    {

                        source.connection = @"Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq="+fileCheck+";Exclusive=1;Uid=admin;Pwd=;";;
                    }
                    else if(source.provider == "Microsoft.Data.Sqlite" )
                    {
                        source.connection = @$"Data Source="+fileCheck+";";
                    }
                }
                else
                {
                    // the source can be just a URL: change it to the relevant connection-string
                    source.connection = source.source;
                }
            }

                // MySql.Data.MySqlClient
                // Microsoft.Data.Sqlite
                // Microsoft.Data.SqlClient
                // System.Data.Odbc
                // Microsoft.Access
                // Microsoft.Excel
                // CSV
                // Text
                // XML
                // JSON


//            string outputfile = source.output;
//            string outputformat = source.format;

            // if(querymode == "Default")
            //     querymode = sqldb.GuessQueryMethod(sqldb.CurrentQuery);
            // if(provider == "Default")
            //     provider = sqldb.GuessProvider(sqldb.CurrentConnectionString);




            // 5 - if there is no provider, check the file extension
            if(source.provider == "" || source.provider == "Default" )
            {
                source.provider = GetProviderFromExtension(Path.GetExtension(source.connection.TrimEnd(';')));
            }


            // 6 - if there is no output specified then;
            //     6a - if the input is Excel and no output is specified
            //          then overwrite the source file ONLY if a nonquery and is either Default or specified
            string querymode = GuessQueryMethod(source.query);
            if(source.provider == "Microsoft.Excel" && querymode == "NonQuery"  && source.output == "" )
            {
                source.output = source.connection;
                source.format = "Microsoft.Excel";
            }


            // 7 - if there is an output file specified but not a format, guess the format
            if(source.format == "" && source.output != "" )
            {
                source.format = GuessOutputFormat(source.output);
            }





            // // if the input is Excel and no output is specified
            // // then overwrite the source file ONLY if a nonquery and is either Default or specified
            // if(provider == "Microsoft.Excel" && querymode == "NonQuery" && outputfile == "" && outputformat != "None")
            //     outputfile = source.connection;

            // if(outputformat == "" && outputfile == "")
            //     outputformat = "None";
            // if(outputformat == "" || outputformat == "Default")
            //     outputformat = this.GuessOutputFormat(outputfile);




        }




        #endregion setup-generation


        #region general-helper-methods

        public string GetShortType(string longtype)
        {
            if(longtype == "System.String")
                return "string";
            if(longtype == "System.Int32")
                return "int";
            if(longtype == "System.Boolean")
                return "bool";
            if(longtype == "System.Byte")
                return "byte[]";
            return longtype;
        } 



        /// <summary>Convert text to proper case.  That is, capitalise the first letter in each word and convert the remainder to lower case.
        /// </summary>
        /// <param name="text">The text to convert to proper case.</param>
        public string ToProper(string text)
        {
            if((text ?? "") == "")
                return "";
            if(text.Length == 1)
                return text.ToUpper();
            if(!text.Contains(' '))
                return text[0].ToString().ToUpper() + text.Substring(1,text.Length-1).ToLower();
            string output = "";
            foreach(string part in text.Split(' ',StringSplitOptions.None))
                output += (ToProper(part) + ' ');
            // return output.Substring(0,output.Length-1);
            return output[..^1];
        }

        public string GuessOutputFormat(string filename)
        {
            if(filename.ToLower().TrimEnd(';').EndsWith(".db"))
                return "Microsoft.Data.Sqlite";
            if(filename.ToLower().TrimEnd(';').EndsWith(".xlsx"))
                return "Microsoft.Excel";
            if(filename.ToLower().TrimEnd(';').EndsWith(".csv"))
                return "CSV";
            if(filename.ToLower().TrimEnd(';').EndsWith(".json"))
                return "JSON";
            if(filename.ToLower().TrimEnd(';').EndsWith(".txt"))
                return "Text";
            return "XML";
        }

//         public string[] GetSqlSplit(string sqlText)
//         {
// //            String input = "select * from table1 where col1 = 'abc;de'; select * from table2;";
//             var regex = new System.Text.RegularExpressions.Regex(@"(?<=;)\s*(?=(?:[^\""'`]*[\""'`]){0,1}[^\""'`]*$)");

//             // Regex.Split(sql, @"(?<=;)\s*(?=(?:[^\""'`]*[\""'`]){0,1}[^\""'`]*$)");

//             return regex.Split(sqlText);
      
//         }

        public List<string> SplitSqlCommands(string sqlContent)
        {
            List<string> sqlCommands = new List<string>();
            StringBuilder currentCommand = new StringBuilder();
            bool inSingleLineComment = false;
            bool inMultiLineComment = false;
            bool inString = false;
            char stringDelimiter = '\0';

            for (int i = 0; i < sqlContent.Length; i++)
            {
                char currentChar = sqlContent[i];
                char nextChar = i < sqlContent.Length - 1 ? sqlContent[i + 1] : '\0';

                if (inSingleLineComment)
                {
                    if (currentChar == '\n')
                    {
                        inSingleLineComment = false;
                    }
                }
                else if (inMultiLineComment)
                {
                    if (currentChar == '*' && nextChar == '/')
                    {
                        inMultiLineComment = false;
                        i++; // Skip the '/'
                    }
                }
                else if (inString)
                {
                    if (currentChar == stringDelimiter && sqlContent[i - 1] != '\\')
                    {
                        inString = false;
                    }
                }
                else
                {
                    if (currentChar == '-' && nextChar == '-')
                    {
                        inSingleLineComment = true;
                        i++; // Skip the second '-'
                    }
                    else if (currentChar == '/' && nextChar == '*')
                    {
                        inMultiLineComment = true;
                        i++; // Skip the '*'
                    }
                    else if (currentChar == '\'' || currentChar == '\"')
                    {
                        inString = true;
                        stringDelimiter = currentChar;
                    }
                    else if (currentChar == ';')
                    {
                        sqlCommands.Add(currentCommand.ToString().Trim());
                        currentCommand.Clear();
                        continue;
                    }
                }

                if (!inSingleLineComment && !inMultiLineComment)
                {
                    currentCommand.Append(currentChar);
                }
            }

            // Add any remaining command after the last semicolon
            if (currentCommand.Length > 0)
            {
                sqlCommands.Add(currentCommand.ToString().Trim());
            }

            return sqlCommands;
        }

        public List<string> GetFilesWithUniqueExtensions(string directoryPath)
        {
            // Get all files in the specified directory
            var allFiles = Directory.GetFiles(directoryPath);

            // Dictionary to count occurrences of each file extension
            Dictionary<string, int> extensionCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Populate the dictionary with file extension counts
            foreach (var file in allFiles)
            {
                string extension = Path.GetExtension(file);
                if (extensionCount.ContainsKey(extension))
                {
                    extensionCount[extension]++;
                }
                else
                {
                    extensionCount[extension] = 1;
                }
            }

            // Filter files that have unique (exactly one) file extension
            var uniqueFiles = allFiles.Where(file => extensionCount[Path.GetExtension(file)] == 1).ToList();

            return uniqueFiles;
        }

        private static string GetProviderFromExtension(string extension)
        {
            string provider = "";
            if(extension == ".txt")
            {
                provider = "Text";
            }
            else if(extension == ".csv")
            {
                provider = "CSV";
            }
            else if(extension == ".xlsx")
            {
                provider = "Microsoft.Excel";
            }
            else if(extension == ".accdb" || extension == ".mdb")
            {
                provider = "Microsoft.Access";
            }
            else if(extension == ".db")
            {
                provider = "Microsoft.Data.Sqlite";
            }
            return provider;
        }

        private static string GuessQueryMethod(string sqlQuery)
        {
            string checkSql = (sqlQuery.Trim().ToLower() + new string(' ',20));
            if(checkSql.Substring(0,7) == "select ")
                return "Reader";
            return "NonQuery";
        }

#endregion general-helper-methods
    }


}
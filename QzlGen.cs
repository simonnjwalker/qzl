
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            if(source.source != "")
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

            if(querymode == "Default")
                querymode = sqldb.GuessQueryMethod(sqldb.CurrentQuery);
            if(provider == "Default")
                provider = sqldb.GuessProvider(sqldb.CurrentConnectionString);


            // if the input is Excel and no output is specified
            // then overwrite the source file ONLY if a nonquery and is either Default or specified
            if(provider == "Microsoft.Excel" && querymode == "NonQuery" && outputfile == "" && outputformat != "None")
                outputfile = source.connection;

            if(outputformat == "" && outputfile == "")
                outputformat = "None";
            if(outputformat == "" || outputformat == "Default")
                outputformat = this.GuessOutputFormat(outputfile);

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




            // try
            // {
            //     System.IO.File.WriteAllText(destfile,text);
            // }
            // catch
            // {
            //     success = false;
            // }
            // if(!success)
            // {
            //     this.lastmessage = ($"Could not write to file '{destfile}'.");
            //     return false;
            // }


                    // helptext.Add( "   -p|--provider     Provider name.");
                    // helptext.Add( "   -c|--connection   Connection string.");
                    // helptext.Add( "   -q|--query        SQL query.");
                    // helptext.Add($"   -s|--source       Full path to source file.");
                    // helptext.Add($"   -o|--output       Full path to output file.");
                    // helptext.Add( "   -d|--dqchar       Character to replace with '\"' in query/connection string.");
                    // helptext.Add( "   -v|--verbosity    Level of output verbosity.");
                    // load.Add(new ParameterSetting(){
                    //         category = category,
                    //         setting = "--provider",
                    //         synonym = "-p",
                    //         description = "Provider Name",
                    //         helptext = new List<string>(){
                    //             $"Usage: qzl {category} -p providername",
                    //             "",
                    //             "Specify the name of the SQL provider.  This must be one of the following:",
                    //             "",
                    //             "ms mssql|SqlServer",
                    //             "my mysql|MySql",
                    //             "or oracle|Oracle",
                    //             "od odbc|ODBC",
                    //             "sl sqllite|SQLlite",
                    //             "pg postgre|PostgreSQL",
                    //             "xl xlsx|XLSX|Excel",
                    //             "cv csv|CSV",
                    //             "",
                    //         },
                    //         paratype = ParameterType.Input,
                    //         nextparatype = ParameterType.Text
                    //     });
                    // load.Add(new ParameterSetting(){
                    //         category = category,
                    //         setting = "--connection",
                    //         synonym = "-c",
                    //         description = "Connection string",
                    //         helptext = new List<string>(){
                    //             $"Usage: qzl {category} -c connectionstring",
                    //             "",
                    //             "Specify the connection string.",
                    //             "",
                    //             "If the string has double-quotes, replace these with '|' characters.",
                    //             "or the character specified with the -d|--double option",
                    //         },
                    //         paratype = ParameterType.Input,
                    //         nextparatype = ParameterType.Text
                    //     });
                    // load.Add(new ParameterSetting(){
                    //         category = category,
                    //         setting = "--query",
                    //         synonym = "-q",
                    //         description = "SQL query",
                    //         helptext = new List<string>(){
                    //             $"Usage: qzl {category} -q query",
                    //             "",
                    //             "Specify the SQL query.",
                    //             "",
                    //             "If the query has double-quotes, replace these with '|' characters.",
                    //             "or the character specified with the -d|--double option",
                    //         },
                    //         paratype = ParameterType.Input,
                    //         nextparatype = ParameterType.Text
                    //     });

                    // load.Add(new ParameterSetting(){
                    //         category = category,
                    //         setting = "--source",
                    //         synonym = "-s",
                    //         description = "SQL query source file",
                    //         helptext = new List<string>(){
                    //             $"Usage: csgen {category} -s sourcefilename",
                    //             "",
                    //             "Specify the full path to a file containing SQL queries.",
                    //             "",
                    //             "If the -q|--query option is passed in then -s|--source is ignored.",
                    //             "",
                    //             "If no -q|--query option is passed then the current directory.",
                    //         },
                    //         paratype = ParameterType.Input,
                    //         nextparatype = ParameterType.File
                    //     });

                    // load.Add(new ParameterSetting(){
                    //         category = category,
                    //         setting = "--output",
                    //         synonym = "-o",
                    //         description = "Output File",
                    //         helptext = new List<string>(){
                    //             $"Usage: qzl {category} -o outputfilename",
                    //             "",
                    //             "Specify the name of the file to be created and optionally the full path.",
                    //             "",
                    //             "If no full path is specified then the current directory is used.",
                    //             "This must be a valid filename and will be overwritten without notification.",
                    //             $"If not specified, the {category} name is used with '.{fileextension}' appended.",
                    //             $"If neither are specified, the file '{defaultfile}' is created in the current directory."

                    //         },
                    //         paratype = ParameterType.Input,
                    //         nextparatype = ParameterType.File
                    //     });
                    // load.Add(new ParameterSetting(){
                    //         category = category,
                    //         setting = "--dqchar",
                    //         synonym = "-d",
                    //         description = "Replacement for \" character",
                    //         helptext = new List<string>(){
                    //             $"Usage: qzl {category} -d character",
                    //             "",
                    //             "Specify the character in parameters to be replaced with a '\"' character.",
                    //             "",
                    //             "By default all '|' characters are replaced with '\"' characters.",
                    //             "Specify -d \"\" to have no replacements.",
                    //         },
                    //         paratype = ParameterType.Input,
                    //         nextparatype = ParameterType.Any
                    //     });
                    // load.Add(new ParameterSetting(){
                    //         category = category,
                    //         setting = "--verbosity",
                    //         synonym = "-v",
                    //         description = "Verbosity level",
                    //         helptext = new List<string>(){
                    //             $"Usage: qzl {category} -v verbositylevel",
                    //             "",
                    //             "Specify the level of information outputed to the console.",
                    //             "(none|default|full)",
                    //             "",
                    //             "The following are valid values for verbositylevel are:",
                    //             "",
                    //             "  none      No console output.",
                    //             "  default   NonQuery: 'Rows affected: n'",
                    //             "            Scalar:   'n'",
                    //             "            Reader:   'Rows affected: n'",
                    //             "                      'Table1'",
                    //             "                      'Column1[|Column2][|Column3][..]' {max 200 chars}",
                    //             "                      '[Row1Item1[|Row1Item2][|Row1Item3][..]] {max 200 chars}",
                    //             "                      '[Row2Item1[|Row2Item2][|Row2Item3][..]] {max 200 chars}",
                    //             "                      '{max 20 rows}",
                    //             "  full      NonQuery: 'Rows affected: n'",
                    //             "            Scalar:   'n'",
                    //             "            Reader:   'Rows affected: n'",
                    //             "                      'Table1'",
                    //             "                      'Column1[|Column2][|Column3][..]'",
                    //             "                      '[Row1Item1[|Row1Item2][|Row1Item3][..]]",
                    //             "                      '[Row2Item1[|Row2Item2][|Row2Item3][..]]"
                    //         },
                    //         paratype = ParameterType.Input,
                    //         nextparatype = ParameterType.Any
                    //     });





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

#endregion general-helper-methods
    }
}
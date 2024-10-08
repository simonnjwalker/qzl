
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Seamlex.Utilities;
#pragma warning disable CS8602, CS8600, CS0219
namespace Seamlex.Utilities
{
    /// <summary>
    /// Manages parameters
    /// </summary>
    public class QzlParameters
    {
        public List<ParameterSetting> ps = new List<ParameterSetting>();
        public string appname = "qzl";
        public void SetParameterInfo()
        {
            ps.Clear();
            ps.AddRange(this.SetParameterInfoByCategory("--help"));
            ps.AddRange(this.SetParameterInfoByCategory("sql"));
            ps.AddRange(this.SetParameterInfoByCategory("http"));
            ps.AddRange(this.SetParameterInfoByCategory("text"));
            ps.AddRange(this.SetParameterInfoByCategory("json"));
            return;
        }
        private List<ParameterSetting> SetParameterInfoByCategory(string category)
        {
            List<ParameterSetting> output = new List<ParameterSetting>();

            if(category == "--help")
            {
                output.Add(new ParameterSetting(){
                    category = "--help",
                    setting = "--help",
                    synonym = "-h",
                    description = this.appname + " Help Information",
                    isactive = false,
                    input = "",
                    nextisactive = false,
                    nextinput = "",
                    required  = false,
                    helptext = new List<string>(){
                        "Usage: qzl [method] [options]",
                        "",
                        "Execute queries from the command-line.",
                        "",
                        "Method:",
                        "  sql               Performs a SQL query.",
                        "  http              Performs an HTTP query.",
                        "  text              Performs operations on a text file.",
                        "  json              Performs operations on a JSON file.",
                        "",
                        "Options:",
                        "  -h|--help         Display help for each method."
                    },
                    paratype = ParameterType.Switch,
                    paraintmin = 0,
                    paraintmax = 65535,
                    nextparatype = ParameterType.Any,
                    nextparaintmin = 0,
                    nextparaintmax = 65535
                });


            }
            if(category == "sql" || category == "http" || category == "text" || category == "json" )
            {
                string article = "a";
                if(category=="sql" || category=="http")
                    article = "an";

                string categoryname = ToProper(category);
                string defaultfile = "newfile.sql";
                string fileextension = "sql";

                ParameterSetting help = new ParameterSetting(){
                    category = category,
                    setting = "--help",
                    synonym = "-h",
                    description = $"{categoryname} Method Help",
                    restriction = "",
                    input = "",
                    nextinput = "",
                    required  = false,
                    paratype = ParameterType.Switch,
                    paraintmin = 0,
                    paraintmax = 65535,
                    nextparatype = ParameterType.Any,
                    nextparaintmin = 0,
                    nextparaintmax = 65535
                };

                List<ParameterSetting> load = new List<ParameterSetting>();

                var helptext = new List<string>(){
                        $"Usage: qzl {category} [options]",
                        "",
                        $"Executes {article} {categoryname} Method.",
                        "",
                        "Options:"};
                
                if(category == "sql")
                {
                    helptext.Add( "  -p|--provider      Provider name.");
                    helptext.Add( "  -c|--connection    Connection string or file.");
                    helptext.Add( "  -q|--query         SQL query.");
                    helptext.Add( "  -m|--method        SQL method (reader/scalar/nonquery).");
                    helptext.Add($"  -s|--source        Full path to a SQL query in a file.");
                    helptext.Add($"  -f|--format        Format of output (XML/SQL/XLSX).");
                    helptext.Add($"  -o|--output        Full path to output file.");
                    // helptext.Add($"  -t|--type          Output type.");
                    helptext.Add( "  -v|--verbosity     Level of information displayed in console.");
                    helptext.Add( " -dq|--dqchar        Character to replace with '\"' in query/connection string.");
                    helptext.Add( " -nh|--noheuristic   Suppress defaulting missing/imcomplete parameters.");
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--provider",
                            synonym = "-p",
                            description = "Provider Name",
                            helptext = new List<string>(){
                                $"Usage: qzl {category} -p providername",
                                "",
                                "Specify the name of the provider/input type.  This must be one of the following:",
                                "",
                                "   ms mssql|SqlServer",
                                "   my mysql|MySql",
                                "   ma access|Access",
                                // "   or oracle|Oracle",
                                "   od odbc|ODBC",
                                "   sl sqlite|SQLite",
                                // "   pg pgsql|PostgreSQL",
                                "   xl xlsx|XLSX|Excel",
                                "   cv csv|CSV",
                                "   xm xml|XML",
                                "",
                                "If no provider is specific, this will be inferred from the connection string.",
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.Text
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--connection",
                            synonym = "-c",
                            description = "Connection string/source",
                            helptext = new List<string>(){
                                $"Usage: qzl {category} -c connectionstring",
                                "",
                                "Specify the connection string or file.",
                                "",
                                "If the string has double-quotes, replace these with '|' characters.",
                                "",
                                "or the character specified with the -dq|--dqchar option",
                                "",
                                "For file formats (XLSX,XML,CSV,JSON), this is either:",
                                "   1 - the full source path and file name; or,",
                                "   2 - the source URL.",
                                "",
                                "If there is a '!' character, this can delimit specific sheets/tables.",
                                "",
                                "For example:",
                                "qsl sql -c c:\\myfile.xlsx!Sheet1",
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.Text
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--query",
                            synonym = "-q",
                            description = "SQL query",
                            helptext = new List<string>(){
                                $"Usage: qzl {category} -q query",
                                "",
                                "Specify the SQL query.",
                                "",
                                "If the query has double-quotes, replace these with '|' characters",
                                "or the character specified with the -d|--double option.",
                            },
                            required  = true,
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.Text
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--method",
                            synonym = "-m",
                            description = "SQL method",
                            helptext = new List<string>(){
                                $"Usage: qzl {category} -m method",
                                "",
                                "Specify the the SQL query method.  This must be one of the following:",
                                "",
                                "   reader|r",
                                "   nonquery|n",
                                "   scalar|s",
                                "",
                                "If no provider is specified, this defaults to:",
                                "",
                                "   SELECT -> reader",
                                "   INSERT -> nonquery",
                                "   DELETE -> nonquery",
                                "   UPDATE -> nonquery",
                                "   CREATE -> nonquery",
                                "",
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.Text
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--source",
                            synonym = "-s",
                            description = "SQL data source file",
                            helptext = new List<string>(){
                                $"Usage: qzl {category} -s sourcefilename",
                                "",
                                "Specify the full path to a .sql file or a file data source.",
                                "",
                                "If the -q|--query option is passed and this is a .sql file then ",
                                "the value in -s|--source is ignored.",
                                "",
                                "If no -q|--query option is passed then the current directory.",
                                "",
                                "Alternatively, if this is a data file and no -c|connection property",
                                "is passed in and the -nh|noheuristic parameter is not set then the",
                                "application will use this file as a data source.",
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.File
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--format",
                            synonym = "-f",
                            description = "Output Format",
                            helptext = new List<string>(){
                                $"Usage: qzl {category} -f outputformat",
                                "",
                                "Specify the type of output for the query.",
                                "",
                                "   x|xml      XML file",
                                "   t|txt|text Text file",
                                "   e|xl|xlsx  Excel XLSX file",
                                "   c|csv      CSV file",
                                "   s|sql      SQL file with CREATE/INSERT commands.",
                                "   j|json     JSON file.",
                                "",
                                "If no output type is specified is inferred from -o|--output file type.",
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.Text
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--output",
                            synonym = "-o",
                            description = "Output File",
                            helptext = new List<string>(){
                                $"Usage: qzl {category} -o outputfilename",
                                "",
                                "Specify the name of the file to be created and optionally the full path.",
                                "",
                                "If no full path is specified then the current directory is used.",
                                "This must be a valid filename and will be overwritten without notification.",
                                "",
                                "If no output type is specified with -f|--format then this is inferred.",
                                "",
                                "If the method is 'reader' then this is a data file.",
                                "If the method is 'scalar' then this is a file with a single value.",
                                "If the method is 'nonquery' then no file is created."
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.File
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--verbosity",
                            synonym = "-v",
                            description = "Verbosity level",
                            helptext = new List<string>(){
                                $"Usage: qzl {category} -v verbositylevel",
                                "",
                                "Specify the level of information outputed to the console.",
                                "(none|default|full)",
                                "",
                                "The following are valid values for verbositylevel are:",
                                "",
                                "  none      No console output.",
                                "  errors    Error messages only.",
                                "  default   NonQuery: 'Rows affected: n'",
                                "            Scalar:   'n'",
                                "            Reader:   'Rows affected: n'",
                                "                      'Table1'",
                                "                      'Column1[|Column2][|Column3][..]' {max 200 chars}",
                                "                      '[Row1Item1[|Row1Item2][|Row1Item3][..]] {max 200 chars}",
                                "                      '[Row2Item1[|Row2Item2][|Row2Item3][..]] {max 200 chars}",
                                "                      '{max 20 rows}",
                                "  full      NonQuery: 'Rows affected: n'",
                                "            Scalar:   'n'",
                                "            Reader:   'Rows affected: n'",
                                "                      'Table1'",
                                "                      'Column1[|Column2][|Column3][..]'",
                                "                      '[Row1Item1[|Row1Item2][|Row1Item3][..]]",
                                "                      '[Row2Item1[|Row2Item2][|Row2Item3][..]]"
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.Any
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--noheuristic",
                            synonym = "-nh",
                            description = "Disable defaulting parameters",
                            helptext = new List<string>(){
                                $"Usage: qzl {category} -nh",
                                "",
                                "By default, the following actions will be taken by the application:",
                                "",
                                "If -c|--connection and -q|--query + -p|--provider are all set",
                                "then no heuristics are used",
                                "",
                                "If there is no provider but either connection or source",
                                "then guess provider and set the connection make source blank",
                                "",
                                "If there is no connection and no source:",
                                "then check the files in the current directory",
                                "and if there is only one of the specified provider ",
                                "however if not provider, use file+provider in this order",
                                ".db -> .accdb -> .mdb -> .xlsx -> .csv -> .txt",
                                "",
                                "If there is a query + provider + source but no connection:",
                                "if a file-based provider and source exists then create a connection",
                                "otherwise connection = source and source = ''",
                                "",
                                "If there is no output specified then:",
                                "if the input is Excel then overwrite the source file ",
                                "only if a nonquery and is either Default or specified",
                                "",
                                "If there is an output file specified but not a format, guess the format"

                            },
                            paratype = ParameterType.Switch
                        });

                    // TO DO: have the connection string be retrieves from userssecrets and/or config files 
                    // https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding/?tabs=dotnet-core-cli

                }

                helptext.Add("  -h|--help          Display help.");
                help.helptext.AddRange(helptext);
                output.Add(help);
                output.AddRange(load);
            }

            return output;
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
    }
}
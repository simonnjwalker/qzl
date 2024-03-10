
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#pragma warning disable CS8602, CS8600
namespace Seamlex.Utilities
{
    /// <summary>
    /// Manages parameters
    /// </summary>
    
    public class ParameterHander
    {
        public List<ParameterSetting> ps = new List<ParameterSetting>();
        public string lastmessage = "";
        public string appname = "csgen";

        public string reservedcs = ",abstract,as,base,bool,break,byte,case,catch,char,checked,class,const,continue,decimal,default,delegate,do,double,else,enum,event,explicit,extern,false,finally,fixed,float,for,foreach,goto,if,implicit,in,int,interface,internal,is,lock,long,namespace,new,null,object,operator,out,override,params,private,protected,public,readonly,ref,return,sbyte,sealed,short,sizeof,stackalloc,static,string,struct,switch,this,throw,true,try,typeof,uint,ulong,unchecked,unsafe,ushort,using,virtual,void,volatile,while,";

        public string fieldtypescs = ",guid,byte,int,int32,string,bool,date,datetime,char,decimal,double,float,long,object,sbyte,short,uint,ulong,ushort,";

        public string FixFieldTypeCs(string fieldtypecs)
        {
            string output = "System.String";
            string checktype = (fieldtypecs.ToLower()).Replace("system.","");
            if(!this.fieldtypescs.Contains(','+checktype+','))
                return output;
            if(checktype == "int" || checktype == "int32" )
                return "System.Int32";
            if(checktype == "date" || checktype == "datetime" )
                return "System.DateTime";
            if(checktype == "bool" || checktype == "boolean" )
                return "System.Boolean";
            if(checktype == "decimal" || checktype == "float" || checktype == "double" )
                return "System.Double";
            if(checktype == "byte")
                return "System.Byte";
            if(checktype == "guid")
                return "System.Guid";
            return output;
        }

        public bool SetParameters(List<string> parameters)
        {
            //this.LoadParameterInfo();

            // we will need to check the first parameter
            // if empty, 
            string category = "";
            string setting = "";
            bool ishelp = false;
            if(parameters.Count==0)
            {
                category = "--help";
                setting = "--help";
                ishelp = true;
            }
            else if( parameters[0].Trim().ToLower() == "--help" || parameters[0].Trim().ToLower() == "-h"  || parameters[0].Trim().ToLower() == "" )
            {
                category = "--help";
                setting = "--help";
                ishelp = true;
            }
            else
            {
                category = parameters[0].Trim().ToLower();
                if(parameters.Count==1)
                {
                    // we validate the category below
                    // setting = "--help";
                    // ishelp = true;
                }
                else if(parameters.Count==2)
                {
                    // if the second parameter is help then do the same as above
                    if(parameters[1].ToLower().Trim().Equals("--help") || parameters[1].ToLower().Trim().Equals("-h"))
                    {
                        setting = "--help";
                        ishelp = true;
                    }
                }
                else
                {
                    // if the final parameter is help then get help on the first parameter after the category (if this is a match)
                    if(parameters[parameters.Count-1].ToLower().Trim().Equals("--help") || parameters[parameters.Count-1].ToLower().Trim().Equals("-h"))
                    {
                        string checkparameter = parameters[1].ToLower().Trim();
                        var checksetting = ps.Find(x => x.category.Equals(category) && (x.setting.Equals(checkparameter) || x.synonym.Equals(checkparameter)));
                        if(checksetting != null)
                        {
                            setting = checksetting.setting;
                            ishelp = true;
                        }
                    }
                }
            }

            if(!ishelp)
            {
                // check if there is a valid category entered
                var checkcategory = ps.Find(x => x.category.Equals(category) && x.setting.Equals("--help"));
                if(checkcategory == null)
                {
                    // this category does not exist
                    // error 
                    this.lastmessage = $"Category '{category}' does not exist.  Enter {this.appname} --help for a list of categories.";
                    return false;
                }

                // if this is just a single category with nothing else, get the help for it
                if(parameters.Count==1)
                {
                    ishelp = true;
                    setting = "--help";
                }
            }

            // if this is a help request, set only the minimal properties and exit
            if(ishelp)
            {
                var checkhelp = ps.Find(x => x.category.Equals(category) && (x.setting.Equals(setting) || x.synonym.Equals(setting)));
                if(checkhelp == null)
                {
                    this.lastmessage = $"No help is available for category '{category}' and option '{setting}'.";
                    return false;
                }
                checkhelp.isactive = true;
                checkhelp.ishelprequest = true;
                return true;
            }

            // go through each parameter, try to determine what each is, validate its form, and set the relevant properties on the ps object
            bool skipnext = false;
            for(int i = 0; i < parameters.Count; i++)
            {
                // if the first one, just find the category help and set active
                if(skipnext)
                {
                    // do nothing here
                    skipnext = false;
                }
                else if(i==0)
                {
                    var categorysetting = ps.Find(x => x.category.Equals(category) && (x.setting.Equals("--help")));
                    if(categorysetting==null)
                    {
                        this.lastmessage = $"Cannot locate category '{category}' in parameters.";
                        return false;
                    }
                    categorysetting.isactive = true;
                }
                else
                {
                    // if a parameter is passed-in, if it's not --help|-h (which is analysed and finalised above) and/or the category 
                    // (again which is determined above) then it is basically one of three things:
                    // 1 a recognised command (like -f|--filename)
                    // 2 is something following a recognised command that isn't a switch
                    // 3 is in a specific order in the parameter listing (an ordinal command)

                    setting = parameters[i].Trim().ToLower();
                    var checksetting = ps.Find(x => x.category.Equals(category) && (x.setting.Equals(setting) || x.synonym.Equals(setting)));
                    bool isordinal = false;
                    bool issetting = false;
                    bool isswitch = false;
                    ParameterSetting? currentsetting; 
                    if(checksetting==null)
                    {
                        // check the ordinal - this might be okay if in a specific place in the parameters
                        var ordinalsetting = ps.Find(x => x.category.Equals(category) && (x.ordinal.Equals(i+1)));
                        if(ordinalsetting==null)
                        {
                            this.lastmessage = $"Parameter '{parameters[i]}' is incorrect.  Enter {this.appname} {category} --help for more information.";
                            return false;
                        }
                        isordinal = true;
                        currentsetting = ordinalsetting;
                    }
                    else
                    {
                        issetting = true;
                        currentsetting = checksetting;
                    }
                    isswitch = (currentsetting.paratype == ParameterType.Switch);
                    // if this is an ordinal, we validate it directly
                    // if this is a switch, we do not validate at all
                    // if this is a setting, we validate the next parameter using the 'nextxxx' properties on the current setting object
                    string validatetext = "";
                    if(isswitch)
                    {
                        validatetext = "";
                    }
                    else if(isordinal)
                    {
                        validatetext = this.ValidateParameter(parameters[i],checksetting!.description,checksetting!.paratype,checksetting!.paraintmin,checksetting!.paraintmax,checksetting!.paraseparator);
                    }
                    else if(issetting)
                    {
                        // try to grab the next parameter
                        if(i == (parameters.Count - 1))
                        {
                            this.lastmessage = $"Setting '{setting}' must be followed by an additional parameter.  Enter {this.appname} {category} {setting} --help for more information.";
                            return false;
                        }
                        validatetext = this.ValidateParameter(parameters[i+1],checksetting!.description,checksetting!.nextparatype,checksetting!.nextparaintmin,checksetting!.nextparaintmax,checksetting!.nextparaseparator);
                    }

                    if(validatetext != "")
                    {
                        this.lastmessage = validatetext;
                        return false;
                    }

                    // now set the relevant properties of the parameter object
                    if(isswitch)
                    {
                        checksetting!.isactive = true;
                        checksetting!.input = parameters[i];
                        checksetting!.nextisactive = false;
                        checksetting!.nextinput = "";
                    }
                    else if(isordinal)
                    {
                        checksetting!.isactive = true;
                        checksetting!.input = parameters[i];
                        checksetting!.nextisactive = false;
                        checksetting!.nextinput = "";
                    }
                    else if(issetting)
                    {
                        checksetting!.isactive = true;
                        checksetting!.input = parameters[i];
                        checksetting!.nextisactive = true;
                        checksetting!.nextinput = parameters[i+1];
                        skipnext = true;
                    }
                }
            }
            return true;
        }

        public string ValidateParameter(string parameter, string description, ParameterType paratype, int paraintmin = 0, int paraintmax = 0, string paraseparator = "")
        {
            if(paraseparator != "" && parameter.Contains(paraseparator))
            {
                string[] fields = parameter.Split(paraseparator,StringSplitOptions.None);
                string errormessage = "";
                foreach(var field in fields)
                {
                    errormessage = this.ValidateParameter(field,description,paratype,paraintmin,paraintmax);
                    if(errormessage != "")
                        return errormessage;
                }
                return "";
            }

            string output = "";
            if(paratype == ParameterType.Any)
            {

            }
            else if(paratype == ParameterType.Switch)
            {

            }
            else if(paratype == ParameterType.Input)
            {

            }
            else if(paratype == ParameterType.Text)
            {

            }
            else if(paratype == ParameterType.File)
            {

            }
            else if(paratype == ParameterType.Integer)
            {
                if(parameter=="")
                    return $"{description} parameter must be a number between {paraintmin} and {paraintmax}.";
                if(parameter.Contains(' '))
                    return $"{description} parameter value '{parameter}' cannot contain spaces.";
                if(parameter.StartsWith('-') && paraintmin >= 0 )
                    return $"{description} parameter value '{parameter}' cannot be negative.";
                if(parameter.Contains('.'))
                    return $"{description} parameter value '{parameter}' must be an integer.";
                string checkparameter = new String(parameter.Where(Char.IsDigit).ToArray());
                if(parameter != checkparameter)
                    return $"{description} parameter value '{parameter}' must only contain digits from 0-9.";
                if(checkparameter.Length >= Int32.MaxValue.ToString().Length)
                    return $"{description} parameter value '{parameter}' is too many digits.";
                int parameternum = Int32.Parse(checkparameter);
                if(parameternum < paraintmin)
                    return $"{description} parameter value '{parameter}' is less than {paraintmin}.";
                if(parameternum > paraintmax)
                    return $"{description} parameter value '{parameter}' is more than {paraintmax}.";
            }
            else if(paratype == ParameterType.HtmlFieldName)
            {
                if(parameter=="")
                    return $"{description} parameter cannot be blank.";
                if(parameter.Contains(' '))
                    return $"{description} parameter value '{parameter}' cannot contain spaces.";
                if(parameter.StartsWith('_'))
                    return $"{description} parameter value '{parameter}' cannot start with an underscore.";
                if(parameter.EndsWith('_'))
                    return $"{description} parameter value '{parameter}' cannot end with an underscore.";
                if(Char.IsDigit(parameter[0]))
                    return $"{description} parameter value '{parameter}' cannot start with a digit.";
                if(parameter.Contains('.'))
                    return $"{description} parameter value '{parameter}' cannot contain the period character.";
                string nounderscores = parameter.Replace("_","");
                string checkparameter = new String(nounderscores.Where(Char.IsLetterOrDigit).ToArray());
                if(nounderscores != checkparameter)
                    return $"{description} parameter value '{parameter}' must only contain letters, digits, and underscores.";
            }
            else if(paratype == ParameterType.CsFieldName || paratype == ParameterType.CsNameSpace)
            {
                if(parameter=="")
                {
                    if(paratype == ParameterType.CsFieldName)
                    {
                        return $"{description} parameter cannot be blank.";
                    }
                    return "";
                }
                if(parameter.Contains(' '))
                    return $"{description} parameter value '{parameter}' cannot contain spaces.";
                // if(parameter.StartsWith('_'))
                //     return $"{description} parameter value '{parameter}' cannot start with an underscore.";
                if(parameter.EndsWith('_'))
                    return $"{description} parameter value '{parameter}' cannot end with an underscore.";
                if(Char.IsDigit(parameter[0]))
                    return $"{description} parameter value '{parameter}' cannot start with a digit.";
                if(parameter.Contains('.'))
                {
                    if(paratype == ParameterType.CsFieldName)
                        return $"{description} parameter value '{parameter}' cannot contain the period character.";
                    if(parameter.Contains(".."))
                        return $"{description} parameter value '{parameter}' cannot contain consecutive period characters.";
                    if(parameter.StartsWith("."))
                        return $"{description} parameter value '{parameter}' cannot start with a period character.";
                    if(parameter.EndsWith("."))
                        return $"{description} parameter value '{parameter}' cannot end with a period character.";

                    string[] classnames = parameter.Split('.',StringSplitOptions.None);
                    foreach(var classname in classnames)
                    {
                        string checktext = this.ValidateParameter(classname, description, ParameterType.CsClassName, paraintmin, paraintmax, paraseparator);
                        if(checktext != "")
                            return checktext;
                    }
                }
                    
                string nounderscores = parameter.Replace("_","");
                string checkunderscores = new String(nounderscores.Where(Char.IsLetterOrDigit).ToArray());
                if(nounderscores != checkunderscores && paratype == ParameterType.CsFieldName)
                {
                    return $"{description} parameter value '{parameter}' must only contain letters, digits, and underscores.";
                }
                string noperiods = parameter.Replace(".","");
                string checknamespace = new String(noperiods.Where(Char.IsLetterOrDigit).ToArray());
                if(noperiods != checknamespace && paratype == ParameterType.CsNameSpace)
                {
                    return $"{description} parameter value '{parameter}' must only contain letters, digits, and periods.";
                }


                if(this.reservedcs.Contains(','+parameter+','))
                    return $"The value '{parameter}' is a reserved word in C# and cannot be used.";

            }

            return output;
        }

        public bool IsHelpRequested()
        {
            var checkhelp = ps.Find(x => x.ishelprequest.Equals(true));
            return (checkhelp != null);
        }
        public List<string> GetHelp()
        {
            var checkhelp = ps.Find(x => x.ishelprequest.Equals(true));
            if(checkhelp == null)
                return new List<string>();
            return checkhelp!.helptext;
        }



        private List<ParameterSetting> LegacyGetParameterInfoByCategory(string category)
        {
            // throw new Exception();
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
                        "Usage: csgen [command] [options]",
                        "",
                        "Creates and edits text files to scaffold C# applications from the command-line.",
                        "",
                        "Command:",
                        "  replace           Search+replace in a text file.",
                        "  replacechar       Search+replace using CHR() numbers in a text file.",
                        "  replacen          Search+replace the nth item found in a text file.",
                        "  replacedq         Search in a text file for double-quotes.",
                        "  replacewithdq     Replace in a text file with double-quotes.",
                        "  insert            Insert text at a specified line in a text file.",
                        "  model             Create a .cs Model class.",
                        "  vm                Create a .cs ViewModel class.",
                        "  view              Create a .cshtml View file.",
                        "  controller        Create a .cs Controller class.",
                        "  facade            Create a .cs Facade class.",
                        "",
                        "Options:",
                        "  -h|--help         Display help for each command."
                    },
                    paratype = ParameterType.Switch,
                    paraintmin = 0,
                    paraintmax = 65535,
                    nextparatype = ParameterType.Any,
                    nextparaintmin = 0,
                    nextparaintmax = 65535
                });


            }
            if(category == "vm" || category == "view" || category == "model" || category == "controller" || category == "facade")
            {
                string categoryname = "ViewModel";
                string defaultfile = "newfile.cs";
                string fileextension = "cs";
                if(category != "vm")
                    categoryname = category[0].ToString().ToUpper() + category.Substring(1,category.Length-1);
                if(category == "view")
                    fileextension = "cshtml";
                defaultfile = "new"+categoryname.ToLower()+'.'+fileextension;

                ParameterSetting help = new ParameterSetting(){
                    category = category,
                    setting = "--help",
                    synonym = "-h",
                    description = $"{categoryname} Creation Help",
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
                        $"Usage: csgen {category} [options]",
                        "",
                        $"Creates a {categoryname} file.",
                        "",
                        "Options:"};
                
                if(category != "model")
                {
                    helptext.Add("  -vn|--vname       ViewModel name.");
                    helptext.Add("  -vm|--vnamespace  ViewModel namespace.");
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--vname",
                            synonym = "-vn",
                            description = "ViewModel Name",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -vn viewmodelname",
                                "",
                                "Specify the name of the ViewModel.  This must be both valid in both HTML5 and C#.",
                                "If no name is specified, the filename (without '.cs') is used.  If neither are supplied, the ViewModel is named 'newviewmodel'.",
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.CsClassName
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--vnamespace",
                            synonym = "-vm",
                            description = "ViewModel Namespace",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -vm viewmodelnamespace",
                                "",
                                "Specify the name of the ViewModel namespace.  This must be valid in C#.",
                                "This parameter is option.  If no name is specified, the ViewModel class is created with no namespace.",
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.CsNameSpace
                        });

                }
                if(category == "view" || category == "controller" )
                {
                    helptext.Add("  -wn|--wname       View name.");
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--wname",
                            synonym = "-wn",
                            description = "View Name",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -wm viewname",
                                "",
                                "Specify the name of the View.  This is the text appearing in the <h3> tags inside the view.  This can be any text.",
                                "If not supplied, there will be no heading in the View."
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.Any
                        });

                }
                if(category == "model" || category == "controller" || category == "facade" )
                {
                    helptext.Add("  -mn|--mname       Model name.");
                    helptext.Add("  -mm|--mnamespace  Model namespace.");
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--mname",
                            synonym = "-mn",
                            description = "Model Name",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -mn modelname",
                                "",
                                "Specify the name of the Model.",
                                "If not supplied, the name will be 'newmodel'."
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.CsClassName
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--mnamespace",
                            synonym = "-mm",
                            description = "Model Namespace",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -mm modelnamespace",
                                "",
                                "Specify the name of the Model namespace.",
                                "If not supplied, no namespace will be used."
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.CsNameSpace
                        });

                }

                helptext.Add($"  -o|--output       Full path to output .{fileextension} file.");
                load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--output",
                        synonym = "-o",
                        description = "Output File",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -o outputfilename",
                            "",
                            "Specify the name of the file to be created and optionally the full path.",
                            "",
                            "If no full path is specified then the current directory is used.",
                            "This must be a valid filename and will be overwritten without notification.",
                            $"If not specified, the {category} name is used with '.{fileextension}' appended.",
                            $"If neither are specified, the file '{defaultfile}' is created in the current directory."

                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.File
                    });
                helptext.Add($"  -s|--source       Loads field properties from a CSV file.");
                load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--source",
                        synonym = "-s",
                        description = "Model/ViewModel Field Information Source File",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -s sourcefilename",
                            "",
                            "Specify the name of a CSV file containing fields.",
                            "",
                            "If no full path is specified then the current directory is checked.",
                            "",
                            "The order that fields are loaded is the natural order of the rows in the source file.",
                            "If the -f|--fieldnames option is also used, those fields will be added after any obtained through the -s|--source     load.",
                            "",
                            "Each item in the header row aligns with the relavent parameter:",
                            "vfnames,vftypes,vfsizes,vfdescs,vfreqs,vfcaps,mfnames,mftypes,wfclass,wftype, wfdclass, wficlass",
                            "",
                            "The CSV can optionally have vname or mname fields to filter records.",
                            "Only records that have a matching vname/mname to the --vname or --mname parameter will be used."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.File
                    });



                if(category == "model" || category == "controller" || category == "facade" )
                    helptext.Add($"                    Field properties are filtered by the 'mname' field matching the --mname property.");
                if(category == "vm" || category == "view" )
                    helptext.Add($"                    Field properties are filtered by the 'vname' field matching the --vname property.");
                if(category != "model" )
                {
                    helptext.Add("  -vf|--vfnames     Comma-separated list of ViewModel field names in order.");
                    helptext.Add("                    Syntax is vfname1[,vfname2][,...].");
                    helptext.Add("  -vt|--vftypes     Comma-separated list of ViewModel field types in order.");
                    helptext.Add("                    Syntax is vftype1[,vftype2][,...].");
                    helptext.Add("  -vz|--vfsizes     Comma-separated list of ViewModel field sizes in order.");
                    helptext.Add("                    Syntax is vfsize1[,vfsize2][,...].");
                    helptext.Add("  -vc|--vfdescs     Comma-separated list of ViewModel field descriptions in order.");
                    helptext.Add("                    Syntax is vfdesc1[,vfdesc2][,...].");
                    helptext.Add("  -vq|--vfreqs      Comma-separated list of ViewModel field required text in order.");
                    helptext.Add("                    Syntax is vfreq1[,vfreqc2][,...].");
                    helptext.Add("  -va|--vfcaps      Comma-separated list of ViewModel field captions in order.");
                    helptext.Add("                    Syntax is vfcap1[,vfcap2][,...].");

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vfnames",
                        synonym = "-vf",
                        description = "ViewModel Field Names",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vf fieldnames",
                            "",
                            "Specify a comma-separated list of fieldnames.",
                            "",
                            "The format of this is:",
                            "vfname1[,vfname2][,...]",
                            "",
                            "If the -s|--source     option is also used, fields will be loaded from that file first.",
                            "",
                            "Each name must be valid as a CSS/HTML name."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.HtmlFieldName,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vftypes",
                        synonym = "-vt",
                        description = "ViewModel Field Types",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vf fieldtypes",
                            "",
                            "Specify a comma-separated list of supported DataTypes associated with fields.",
                            "",
                            "The format of this is:",
                            "vftype1[,vftype2][,...]",
                            "",
                            "If the -s|--source     option is also used, fields will be loaded from that file first.",
                            "",
                            "If not specified,unknown, or unsupported this will default to string."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vfsizes",
                        synonym = "-vz",
                        description = "ViewModel Field Sizes",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vz fieldsizes",
                            "",
                            "Specify a comma-separated list of field sizes.",
                            "",
                            "The format of this is:",
                            "vfsize1[,vfsiz2][,...]",
                            "",
                            "Where the field size is set to zero then this is ignored."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Integer,
                        nextparaseparator = ",",
                        nextparaintmin = 0
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vfdescs",
                        synonym = "-vc",
                        description = "ViewModel Field Descriptions",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vc fielddescs",
                            "",
                            "Specify a comma-separated list of supported DataTypes associated with fields.",
                            "",
                            "The format of this is:",
                            "vfdesc1[,vfdesc2][,...]"
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vfreqs",
                        synonym = "-vq",
                        description = "ViewModel Field Required Text",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vq fieldreqtext",
                            "",
                            "Specify a comma-separated list of validation text to appear.",
                            "",
                            "The format of this is:",
                            "vfreq1[,vfreq2][,...]"
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vfcaps",
                        synonym = "-va",
                        description = "ViewModel Field Captions",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -va fieldcaps",
                            "",
                            "Specify a comma-separated list of label text associated with fields.",
                            "",
                            "The format of this is:",
                            "vfcap1[,vfcap2][,...]"
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                }
                if(category == "model" || category == "controller" || category == "facade" )
                {
                    helptext.Add("  -mf|--mfnames     Comma-separated list of Model field names in order.");
                    helptext.Add("                    Syntax is mfname1[,mfname2][,...].");
                    helptext.Add("  -mt|--mftypes     Comma-separated list of Model field types in order.");
                    helptext.Add("                    Syntax is mftype1[,mftype2][,...].");


                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--mfnames",
                        synonym = "-mf",
                        description = "Model Field Names",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -mf fieldnames",
                            "",
                            "Specify a comma-separated list of Model field names.",
                            "",
                            "The format of this is:",
                            "mfname1[,mfname2][,...]"
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsFieldName,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--mftypes",
                        synonym = "-mt",
                        description = "Model Field Types",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -mt fieldtypes",
                            "",
                            "Specify a comma-separated list of Model field types.",
                            "",
                            "The format of this is:",
                            "mftype1[,mftype2][,...]",
                            "",
                            "Field types that are empty, not C# data types, or not language shortcuts default to System.String",
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });



                }
                if(category == "view"  )
                {
                    helptext.Add("  -we|--wfclasses   Comma-separated list of View form field CSS classes in order.");
                    helptext.Add("                    Syntax is wfclass1[,wfclass2][,...].");
                    helptext.Add("  -wy|--wftypes     Comma-separated list of View form field HTML types in order.");
                    helptext.Add("                    Syntax is wftype1[,wftype2][,...].");
                    helptext.Add("  -wd|--wfdclasses  Comma-separated list of colon-delimited CSS classes in order to wrap a form field in <div> tags.");
                    helptext.Add("                    Syntax is wfdclass1a:wfdclass1b[:wfdclass1c][,wfdclass2a][,...].");
                    helptext.Add("  -wi|--wficlasses  Comma-separated list of a CSS class for an <i> tag that follows a form field.");
                    helptext.Add("                    Syntax is wficlass1[,wficlass2][,...].");

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wfclasses",
                        synonym = "-we",
                        description = "View Form Field CSS Classes",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -we cssclasses",
                            "",
                            "Specify a comma-separated list of View form field CSS classes in order.",
                            "",
                            "The format of this is:",
                            "wfclass1[,wfclass2][,...]",
                            "",
                            "This is the class of the field itself and contain spaces.",
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wftypes",
                        synonym = "-wy",
                        description = "View Form Field HTML Types",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wy fieldtypes",
                            "",
                            "Specify a comma-separated list of View form field HTML types in order.",
                            "",
                            "The format of this is:",
                            "wftype1[,wftype2][,...]",
                            "",
                            "This defaults to 'input' if missing or unrecognised."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wfdclasses",
                        synonym = "-wd",
                        description = "View Form Field <div> wrapper CSS Classes",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wd cssclasses",
                            "",
                            "Specify a comma-separated list of colon-delimited CSS classes in order to wrap a form field in <div> tags.",
                            "",
                            "The format of this is:",
                            "wfdclass1a:wfdclass1b[:wfdclass1c][,wfdclass2a][,...]",
                            "",
                            "If there are colons, the particular form object is wrapped in nested <div> tags.",
                            "Otherwise you can simply have spaces between class names.",
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wficlasses",
                        synonym = "-wi",
                        description = "View Form Field <i> wrapper CSS Classes",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wi cssclasses",
                            "",
                            "Specify a comma-separated list of CSS classes for an <i> tag that follows a form field.",
                            "",
                            "The format of this is:",
                            "wficlass1[,wficlass2][,...]"
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });


                }


                if(category == "model" || category == "controller" || category == "facade" )
                {
                    helptext.Add("  -fp|--mpkey       Specifies the primary key field in the Model.");
                    helptext.Add("  -ff|--mfkey       Specifies the foreign key field in the Model.");
                    helptext.Add("  -ft|--mftable     Specifies the parent of the Model.");

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--mpkey",
                        synonym = "-fp",
                        description = "Model Primary Key Field",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -fp primarykey",
                            "",
                            "Specify the primary key field in the Model."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--mfkey",
                        synonym = "-ff",
                        description = "Model Parent Foreign Key Field",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -ff foreignkey",
                            "",
                            "Specify the foreign key field in the Model."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--mftable",
                        synonym = "-ft",
                        description = "Model Parent Table",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -ft parenttable",
                            "",
                            "Specify the parent of this Model."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });
                }
                if(category == "model" || category == "controller" || category == "facade" )
                {

                    helptext.Add("  -fu|--vuserkey    Specifies the userid field in the ViewModel.");
                    helptext.Add("  -fm|--vmessage    Specifies a field in the ViewModel to relay messages.");
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vuserkey",
                        synonym = "-fu",
                        description = "ViewModel UserId Field",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -fu useridfield",
                            "",
                            "Specify a field that the ViewModel uses to check and pass UserId details."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vmessage",
                        synonym = "-fm",
                        description = "ViewModel Message Field",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -fm messagefield",
                            "",
                            "Specify a field that the ViewModel uses to pass messages."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });
                }


                helptext.Add("  -h|--help         Display help.");
                help.helptext.AddRange(helptext);
                output.Add(help);
                output.AddRange(load);
            }

            return output;
        }

        private void LegacyLoadParameterInfo()
        {
            ps.Clear();
            ps.AddRange(this.LegacyGetParameterInfoByCategory("--help"));
            ps.AddRange(this.LegacyGetParameterInfoByCategory("vm"));
            ps.AddRange(this.LegacyGetParameterInfoByCategory("view"));
            ps.AddRange(this.LegacyGetParameterInfoByCategory("facade"));
            ps.AddRange(this.LegacyGetParameterInfoByCategory("controller"));
            ps.AddRange(this.LegacyGetParameterInfoByCategory("model"));
            if(ps.ToString()!="?")
                return;


           ps.Add(new ParameterSetting(){
                category = "vm",
                setting = "--help",
                synonym = "-h",
                description = "ViewModel Creation Help",
                restriction = "",
                input = "",
                nextinput = "",
                required  = false,
                helptext = new List<string>(){
                    "Usage: csgen vm [options]",
                    "",
                    "Creates a ViewModel file.",
                    "",
                    "Options:",
                    "  -n|--name         ViewModel name.",
                    "  -p|--namespace    ViewModel namespace.",
                    "  -o|--output       Full path to output .cs file.",
                    "  -s|--source       Loads field properties from a CSV file.",
                    "                    The header row must contain 'vmfieldname','vmfieldtype','vmfieldsize','vmfielddesc''vmfieldreq','vmfieldcap' to be used.",
                    "  -vf|--fieldnames  Comma-separated list of ViewModel field names in order.",
                    "                    Syntax is vmfieldname1[,vmfieldname2][,...].",
                    "  -vt|--fieldtypes  Comma-separated list of ViewModel field types in order.",
                    "                    Syntax is vmfieldtype1[,vmfieldtype2][,...].",
                    "  -vz|--fieldsizes  Comma-separated list of ViewModel field sizes in order.",
                    "                    Syntax is vmfieldsize1[,vmfieldsize2][,...].",
                    "  -vc|--fielddescs  Comma-separated list of ViewModel field descriptions in order.",
                    "                    Syntax is vmfielddesc1[,vmfielddesc2][,...].",
                    "  -vq|--fieldreqs   Comma-separated list of ViewModel field required text in order.",
                    "                    Syntax is vmfieldreq1[,vmfieldreqc2][,...].",
                    "  -va|--fieldcaps   Comma-separated list of ViewModel field captions in order.",
                    "                    Syntax is vmfieldcap1[,vmfieldcap2][,...].",
                    "  -k|--key          Specifies the primary key field.",
                    "  -h|--help         Display help."
                },
                paratype = ParameterType.Switch,
                paraintmin = 0,
                paraintmax = 65535,
                nextparatype = ParameterType.Any,
                nextparaintmin = 0,
                nextparaintmax = 65535
            });

           ps.Add(new ParameterSetting(){
                category = "vm",
                setting = "--name",
                synonym = "-n",
                description = "ViewModel Model Name",
                helptext = new List<string>(){
                    "Usage: csgen vm -n viewmodelname",
                    "",
                    "Specify the name of the ViewModel.  This must be both valid in both HTML5 and C#.",
                    "If no name is specified, the filename (without '.cs') is used.  If neither are supplied, the ViewModel is named 'newviewmodel'.",
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.CsClassName
            });

           ps.Add(new ParameterSetting(){
                category = "vm",
                setting = "--namespace",
                synonym = "-p",
                description = "ViewModel Model Namespace",
                helptext = new List<string>(){
                    "Usage: csgen vm -p viewmodelnamespace",
                    "",
                    "Specify the name of the ViewModel.  This must be valid in C#.",
                    "This parameter is option.  If no name is specified, the ViewModel class is created with no namespace.",
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.CsClassName
            });

           ps.Add(new ParameterSetting(){
                category = "vm",
                setting = "--output",
                synonym = "-o",
                description = "ViewModel Output File",
                helptext = new List<string>(){
                    "Usage: csgen vm -o outputfilename",
                    "",
                    "Specify the name of the file to be created.",
                    "",
                    "If no full path is specified then the current directory is used.",
                    "This must be a valid filename and will be overwritten without notification.",
                    "If not specified, the model name is used with '.cs' appended.  If neither are specified, the file 'newviewmodel.cs' is created in the current directory.",
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.File
            });

           ps.Add(new ParameterSetting(){
                category = "vm",
                setting = "--source",
                synonym = "-s",
                description = "ViewModel Field Information Source File",
                helptext = new List<string>(){
                    "Usage: csgen vm -s sourcefilename",
                    "",
                    "Specify the name of a CSV file containing fields.",
                    "",
                    "If no full path is specified then the current directory is checked.",
                    "If the header row contains 'vmfieldname','vmfieldtype','vmfieldsize','vmfielddesc','vmfieldcap','vmfieldreq' then these will be used.",
                    "",
                    "The CSV can optionally have a 'vmname' field to filter records.",
                    "Only records that have a matching 'vmname' the same as the specified model will be used.",
                    "The order that fields are loaded is the natural order of the rows in the source file.",
                    "If the -f|--fieldnames option is also used, those fields will be added after any obtained through the -s|--source     load."
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.File
            });

           ps.Add(new ParameterSetting(){
                category = "vm",
                setting = "--fieldnames",
                synonym = "-vf",
                description = "ViewModel Field Names",
                helptext = new List<string>(){
                    "Usage: csgen vm -vf fieldnames",
                    "",
                    "Specify a comma-separated list of fieldnames.",
                    "",
                    "The format of this is:",
                    "vmfieldname1[,vmfieldname2][,...]",
                    "",
                    "If the -s|--source     option is also used, fields will be loaded from that file first and ones specified with -f are appended.",
                    "Each name must be valid as a CSS/HTML name."
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.HtmlFieldName,
                nextparaseparator = ","
            });

           ps.Add(new ParameterSetting(){
                category = "vm",
                setting = "--fieldtypes",
                synonym = "-vt",
                description = "ViewModel Field Types",
                helptext = new List<string>(){
                    "Usage: csgen vm -vt fieldtypes",
                    "",
                    "Specify a comma-separated list of C# types associated with fields.",
                    "",
                    "The format of this is:",
                    "vmfieldtype1[,vmfieldtype2][,...]",
                    "",
                    "If not specified, the default is string."
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.CsClassName,
                nextparaseparator = ","
            });

           ps.Add(new ParameterSetting(){
                category = "vm",
                setting = "--fieldsizes",
                synonym = "-vz",
                description = "ViewModel Field Sizes",
                helptext = new List<string>(){
                    "Usage: csgen vm -vz fieldsizes",
                    "",
                    "Specify a comma-separated list of field sizes.",
                    "",
                    "The format of this is:",
                    "vmfieldsize1[,vmfieldsize2][,...]",
                    "",
                    "Where the field size is set to zero then this is ignored."
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.Integer,
                nextparaseparator = ",",
                nextparaintmin = 0
            });


           ps.Add(new ParameterSetting(){
                category = "vm",
                setting = "--fielddescs",
                synonym = "-vd",
                description = "ViewModel Field Descriptions",
                helptext = new List<string>(){
                    "Usage: csgen vm -vt fielddescs",
                    "",
                    "Specify a comma-separated list of field descriptions.",
                    "",
                    "The format of this is:",
                    "vmfielddesc1[,vmfielddesc2][,...]"
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.CsClassName,
                nextparaseparator = ","
            });

           ps.Add(new ParameterSetting(){
                category = "vm",
                setting = "--fieldcaps",
                synonym = "-va",
                description = "ViewModel Field Captions",
                helptext = new List<string>(){
                    "Usage: csgen vm -va fieldcaps",
                    "",
                    "Specify a comma-separated list of the caption text that will appear in the View.",
                    "",
                    "The format of this is:",
                    "vmfieldcap1[,vmfieldcap2][,...]"
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.CsClassName,
                nextparaseparator = ","
            });

           ps.Add(new ParameterSetting(){
                category = "vm",
                setting = "--fieldreqs",
                synonym = "-vq",
                description = "ViewModel Field Required Text",
                helptext = new List<string>(){
                    "Usage: csgen vm -vq fieldreqs",
                    "",
                    "Specify a comma-separated list of text that appears when required.",
                    "Note that if blank or missing, the field is not required.",
                    "",
                    "The format of this is:",
                    "vmfieldreq1[,vmfieldreq2][,...]"
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.CsClassName,
                nextparaseparator = ","
            });

           ps.Add(new ParameterSetting(){
                category = "vm",
                setting = "--key",
                synonym = "-k",
                description = "ViewModel Primary Key",
                helptext = new List<string>(){
                    "Usage: csgen vm -k pkeyfieldname",
                    "",
                    "Specify the name of the primary key field.",
                    "",
                    "If not specified, no pkey will be set."
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.CsClassName
            });




           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--help",
                synonym = "-h",
                description = "View Creation Help",
                restriction = "",
                input = "",
                nextinput = "",
                required  = false,
                helptext = new List<string>(){
                    "Usage: csgen view [options]",
                    "",
                    "Creates a View file.",
                    "",
                    "Options:",
                    "  -v|--viewname     View name.",
                    "  -n|--name         ViewModel name.",
                    "  -p|--namespace    ViewModel namespace.",
                    "  -l|--layout       Layout cshtml file.",
                    "  -o|--output       Full path to output .cshtml file.",
                    "  -s|--source       Loads field properties from a CSV file.",
                    "                    The header row must contain 'vmfieldname','vmfieldtype','vmfieldsize','vmfielddesc''vmfieldreq','vmfieldcap' to be used.",
                    "                    The header row can also contain 'viewfclass','viewftype','viewfdclass','viewficlass','viewfrows' to be used.",
                    "  -vf|--fieldnames  Comma-separated list of ViewModel field names in order.",
                    "                    Syntax is vmfieldname1[,vmfieldname2][,...].",
                    "  -vt|--fieldtypes  Comma-separated list of ViewModel field types in order.",
                    "                    Syntax is vmfieldtype1[,vmfieldtype2][,...].",
                    "  -vz|--fieldsizes  Comma-separated list of ViewModel field sizes in order.",
                    "                    Syntax is vmfieldsize1[,vmfieldsize2][,...].",
                    "  -vc|--fielddescs  Comma-separated list of ViewModel field descriptions in order.",
                    "                    Syntax is vmfielddesc1[,vmfielddesc2][,...].",
                    "  -vq|--fieldreqs   Comma-separated list of ViewModel field required text in order.",
                    "                    Syntax is vmfieldreq1[,vmfieldreqc2][,...].",
                    "  -va|--fieldcaps   Comma-separated list of ViewModel field captions in order.",
                    "                    Syntax is vmfieldcap1[,vmfieldcap2][,...].",
                    "  -we|--viewfclass  Comma-separated list of View form field CSS classes in order.",
                    "                    Syntax is viewfclass1[,viewfclass2][,...].",
                    "  -wy|--viewftype   Comma-separated list of View form field HTML types in order.",
                    "                    Syntax is viewftype1[,viewftype2][,...].",
                    "  -wd|--viewfdclass Comma-separated list of colon-delimited CSS classes in order to wrap a form field in <div> tags.",
                    "                    Syntax is viewfdclass1a:viewfdclass1b[:viewfdclass1c][,viewfdclass2a][,...].",
                    "  -wi|--viewficlass Comma-separated list of a CSS class for an <i> tag that follows a form field.",
                    "                    Syntax is viewficlass1[,viewficlass2][,...].",
                    "  -wr|--viewfrows   Comma-separated list of <textarea> row size.",
                    "                    Syntax is viewfrows1[,viewfrows2][,...].",
                    "  -fk|--key         Specifies the primary key field.",
                    "  -fg|--fkey        Specifies the foreign key field.",
                    "  -fu|--user        Specifies the user key field.",
                    "  -fu|--message     Specifies the messaging field.",
                    "  -wst|--submit     Type of Submit object on the form.",
                    "  -wsa|--subaction  Specifies the Submit action.",
                    "  -wsd|--subdclass  Colon-delimited CSS classes for the Submit object.",
                    "  -wsi|--subiclass  CSS class for an embedded <i> tag for the Submit object.",
                    "  -wrt|--return     Type of Return object on the form.",
                    "  -wra|--retaction  Specifies the Return action.",
                    "  -wrd|--retdclass  Colon-delimited CSS classes for the Return object.",
                    "  -wri|--reticlass  CSS class for an embedded <i> tag for the Return object.",
                    "  -wfa|--formaction Specifies the Form action.",
                    "  -wfc|--formclass  Colon-delimited CSS classes wrapping the Form object.",
                    "  -wfs|--formsub    Colon-delimited CSS classes wrapping all objects inside the Form object.",
                    "  -wpc|--pageclass  Specifies the CSS class wrapping the Info and Form sections.",
                    "  -wic|--infoclass  Colon-delimited CSS classes wrapping the Info section above form fields.",
                    "  -wih|--infohclass CSS class of the heading in the Info section.",
                    "  -wit|--infotext   Text for the information section.",
                    "  -wlf|--layfiles   Colon-separated list of Layout cshtml files associated with --laynames.",
                    "  -wln|--laynames   Colon-separated list of @section names.",
                    "  -h|--help         Display help."
                },
                paratype = ParameterType.Switch,
                paraintmin = 0,
                paraintmax = 65535,
                nextparatype = ParameterType.Any,
                nextparaintmin = 0,
                nextparaintmax = 65535
            });

           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--viewname",
                synonym = "-v",
                description = "View Name",
                helptext = new List<string>(){
                    "Usage: csgen view -n viewname",
                    "",
                    "Specify the name of the View.  This is the text appearing in the <h3> tags inside the view. xModel that will be the base of this View.  This can be any text.",
                    "If not supplied, there will be no heading in the View.",
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.Any
            });

           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--name",
                synonym = "-n",
                description = "ViewModel Name",
                helptext = new List<string>(){
                    "Usage: csgen view -n viewmodelname",
                    "",
                    "Specify the name of the ViewModel that will be the base of this View.  This must be both valid in both HTML5 and C#.",
                    "If no name is specified, the filename (without '.cshtml') is used.  If neither are supplied, the ViewModel is named 'newviewmodel'.",
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.CsClassName
            });

           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--namespace",
                synonym = "-p",
                description = "ViewModel Namespace",
                helptext = new List<string>(){
                    "Usage: csgen view -p viewmodelnamespace",
                    "",
                    "Specify the name of the namespace of the associatedViewModel.  This must be a valid in C# name.",
                    "This parameter is option.  If no name is specified, the ViewModel class used in the View has no namespace.",
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.CsClassName
            });

           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--output",
                synonym = "-o",
                description = "View Output File",
                helptext = new List<string>(){
                    "Usage: csgen view -o outputfilename",
                    "",
                    "Specify the name of the file to be created.",
                    "",
                    "If no full path is specified then the current directory is used.",
                    "This must be a valid filename and will be overwritten without notification.",
                    "If not specified, the view name is used with spaces removed and '.cshtml' appended.  If neither are specified, the file 'newview.cshtml' is created in the current directory.",
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.File
            });

           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--source",
                synonym = "-s",
                description = "View Field Information Source File",
                helptext = new List<string>(){
                    "Usage: csgen view -s sourcefilename",
                    "",
                    "Specify the name of a CSV file containing fields.",
                    "",
                    "This is a superset of the fields in csgen vm -s.",
                    "",
                    "If no full path is specified then the current directory is checked.",
                    "The ViewModel fields are: 'vmfieldname','vmfieldtype','vmfieldsize','vmfielddesc''vmfieldreq','vmfieldcap' to be used.",
                    "Additional View fields are: 'viewfclass','viewftype','viewfdclass','viewficlass','viewfrows'.",
                    "",
                    "The CSV can optionally have a 'vmname' field to filter records.",
                    "Only records that have a matching 'vmname' the same as the specified model will be used.",
                    "The order that fields are loaded is the natural order of the rows in the source file.",
                    "If the -vf|--fieldnames option is also used, those fields will be added after any obtained through the -s|--source     load."
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.File
            });

           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--fieldnames",
                synonym = "-vf",
                description = "View Field Names",
                helptext = new List<string>(){
                    "Usage: csgen view -vf fieldnames",
                    "",
                    "Specify a comma-separated list of ViewModel fieldnames used in the View.",
                    "",
                    "The format of this is:",
                    "vmfieldname1[,vmfieldname2][,...]",
                    "",
                    "If the -s|--source     option is also used, fields will be loaded from that file first and ones specified with -f are appended.",
                    "Each name must be valid as a CSS/HTML name."
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.HtmlFieldName,
                nextparaseparator = ","
            });

           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--fieldtypes",
                synonym = "-vt",
                description = "View Field Types",
                helptext = new List<string>(){
                    "Usage: csgen view -vt fieldtypes",
                    "",
                    "Specify a comma-separated list of C# types associated with fields.",
                    "",
                    "The format of this is:",
                    "vmfieldtype1[,vmfieldtype2][,...]",
                    "",
                    "If not specified, the default is string."
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.CsClassName,
                nextparaseparator = ","
            });

           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--fieldsizes",
                synonym = "-vz",
                description = "View Field Sizes",
                helptext = new List<string>(){
                    "Usage: csgen view -vz fieldsizes",
                    "",
                    "Specify a comma-separated list of field sizes.",
                    "",
                    "The format of this is:",
                    "vmfieldsize1[,vmfieldsize2][,...]",
                    "",
                    "Where the field size is set to zero then this is ignored."
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.Integer,
                nextparaseparator = ",",
                nextparaintmin = 0
            });


           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--fielddescs",
                synonym = "-vd",
                description = "View Field Descriptions",
                helptext = new List<string>(){
                    "Usage: csgen view -vd fielddescs",
                    "",
                    "Specify a comma-separated list of field descriptions.",
                    "",
                    "The format of this is:",
                    "vmfielddesc1[,vmfielddesc2][,...]"
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.Any,
                nextparaseparator = ","
            });

           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--fieldcaps",
                synonym = "-va",
                description = "View Field Captions",
                helptext = new List<string>(){
                    "Usage: csgen view -va fieldcaps",
                    "",
                    "Specify a comma-separated list of the caption text that will appear in the View.",
                    "",
                    "The format of this is:",
                    "vmfieldcap1[,vmfieldcap2][,...]"
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.Any,
                nextparaseparator = ","
            });

           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--fieldreqs",
                synonym = "-vq",
                description = "View Field Required Text",
                helptext = new List<string>(){
                    "Usage: csgen view -vq fieldreqs",
                    "",
                    "Specify a comma-separated list of text that appears when required.",
                    "Note that if blank or missing, the field is not required.",
                    "",
                    "The format of this is:",
                    "vmfieldreq1[,vmfieldreq2][,...]"
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.Any,
                nextparaseparator = ","
            });


           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--fieldtypes",
                synonym = "-vt",
                description = "View Field Types",
                helptext = new List<string>(){
                    "Usage: csgen view -vt fieldtypes",
                    "",
                    "Specify a comma-separated list of C# types associated with fields.",
                    "",
                    "The format of this is:",
                    "vmfieldtype1[,vmfieldtype2][,...]",
                    "",
                    "If not specified, the default is string."
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.CsClassName,
                nextparaseparator = ","
            });




           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--viewftype",
                synonym = "-wy",
                description = "View Field Types",
                helptext = new List<string>(){
                    "Usage: csgen view -wy viewfieldtypes",
                    "",
                    "Comma-separated list of View form field HTML types in order.",
                    "",
                    "The format of this is:",
                    "viewftype1[,viewftype2][,...]]"
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.HtmlFieldType,
                nextparaseparator = ","
            });

           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--viewfdclass",
                synonym = "-wd",
                description = "View Field Classes",
                helptext = new List<string>(){
                    "Usage: csgen view -wd cssclasses",
                    "",
                    "Specify a comma-separated list of colon-delimited CSS classes that wrap the associated form field in <div> tags.",
                    "",
                    "The format of this is:",
                    "viewfdclass1a:viewfdclass1b[:viewfdclass1c][,viewfdclass2a][,...]",
                    "",
                    "Each class can have spaces.  Separate divs are created with colons.",
                    "",
                    "For example:",
                    "<div class=\"row justify-content-center\">",
                    "    <div class=\"col-xxl-5 col-xl-5 col-lg-7 col-md-10\">",
                    "        <div class=\"section-title text-center mb-50\">",
                    "",
                    "The format of this is:",
                    "row justify-content-center:col-xxl-5 col-xl-5 col-lg-7 col-md-10:section-title text-center mb-50"
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.Any,
                nextparaseparator = ","
            });

           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--fieldreqs",
                synonym = "-wq",
                description = "View Field Required Text",
                helptext = new List<string>(){
                    "Usage: csgen view -wq fieldreqs",
                    "",
                    "Specify a comma-separated list of text that appears when required.",
                    "Note that if blank or missing, the field is not required.",
                    "",
                    "The format of this is:",
                    "vmfieldreq1[,vmfieldreq2][,...]"
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.Any,
                nextparaseparator = ","
            });


           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--fieldtypes",
                synonym = "-wt",
                description = "View Field Types",
                helptext = new List<string>(){
                    "Usage: csgen view -wt fieldtypes",
                    "",
                    "Specify a comma-separated list of C# types associated with fields.",
                    "",
                    "The format of this is:",
                    "vmfieldtype1[,vmfieldtype2][,...]",
                    "",
                    "If not specified, the default is string."
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.CsClassName,
                nextparaseparator = ","
            });




           ps.Add(new ParameterSetting(){
                category = "view",
                setting = "--key",
                synonym = "-k",
                description = "View Primary Key",
                helptext = new List<string>(){
                    "Usage: csgen vm -k pkeyfieldname",
                    "",
                    "Specify the name of the primary key field.",
                    "",
                    "If not specified, no pkey will be set."
                },
                paratype = ParameterType.Input,
                nextparatype = ParameterType.CsClassName
            });

        }

    }
    public class ParameterSetting
    {
        public string category = "";
        public string setting = "";
        public string synonym = "";
        public string description = "";
        public string restriction = "";
        public string input = "";
        public bool isactive = false;
        public bool ishelprequest = false;
        public string nextinput = "";
        public bool nextisactive = false;
        public bool required  = false;
        public int ordinal = 0;
        public List<string> helptext = new List<string>();
        public ParameterType paratype = ParameterType.Any; // Switch, Input, Any, File, Integer, Text, CsFieldName, CsFieldInfo, CsClassName, HtmlFieldInfo, HtmlFieldName
        public int paraintmin = 0; 
        public int paraintmax = 65535;
        public string paraseparator = ""; 
        public ParameterType nextparatype = ParameterType.Any; // Switch, Input, Any, File, Integer, Text, CsFieldName, CsFieldInfo, CsClassName, HtmlFieldInfo, HtmlFieldName
        public int nextparaintmin = 0; 
        public int nextparaintmax = 65535; 
        public string nextparaseparator = ""; 
    }

    public enum ParameterType
    {
        Any,
        Switch,
        Input,
        File,
        Integer,
        Text,
        CsFieldName,
        CsFieldInfo,
        CsClassName,
        CsNameSpace,
        CssName,
        HtmlFieldName,
        HtmlFieldType
    }
}
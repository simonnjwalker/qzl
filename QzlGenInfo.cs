
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Seamlex.Utilities;
#pragma warning disable CS8602, CS8600, IDE1006
namespace Seamlex.Utilities
{
    /// <summary>
    /// Manages and read csgen-specific parameters
    /// </summary>
    public class QzlGenInfo
    {
        public string lastmessage="";

        // These properties are set in code:
        public string category {get;set;} = "";
        // public string categoryname {get;set;} = "";
        // public int indent {get;set;} = 4;

        // these are set using System.Reflection:
        public string output {get;set;} = "";  //  Full path to output file.
        public string format {get;set;} = "";  //  Output format style.
        public string source {get;set;} = "";  // Loads queries from a file.
        public string provider {get;set;} = "";  // SQL Provider.
        public string query {get;set;} = "";  // SQL query
        public string connection {get;set;} = "";  // connection string
        public string method {get;set;} = "";  // SQL query method
        public string verbosity {get;set;} = "";  // console output verbosity

        // 1.0.7 SNJW create a level for this
        public int verblevel {get;set;} = -1;  // console output verbosity
        public string dqchar {get;set;} = "";
        public bool noheuristic {get;set;} = false;

    }
}
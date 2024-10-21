
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
    /// Manages and read parameters passed-in
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
        public int verblevel {get;set;} = -1;  // console output verbosity level

        // 1.0.8 SNJW create layout option for vertical (keypair) output to the console
        public string layout {get;set;} = "";  // console output layout style

        public string dqchar {get;set;} = "";

        public bool noheuristic {get;set;} = false;

       // 1.0.9 SNJW added for http (qzl net)
         public string url {get;set;} = "";
        public string headerstyle {get;set;} = "";
        public string requestmethod {get;set;} = "";

        public int maxrows {get;set;} = -1;  // console output maximum rows shown


    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Seamlex.Utilities;
#pragma warning disable CS8602, CS8600, CS0219
namespace Seamlex.Utilities
{

    // TO DO need to output the SQL queries for excel to a separate file

    public class Program
    {
        /// <summary>
        /// Execute queries from the command-line.
        /// 
        /// Usage: qzl [operation] [options]
        /// 
        /// Method:
        ///   sql               Performs a SQL query.
        ///   http              Performs an HTTP query.
        ///   text              Performs operations on a text file.
        ///   json              Performs operations on a JSON file.
        /// 
        /// Options:
        ///   -h|--help         Display help for each method.
        /// 
        /// Run 'qzl [operation] --help' for more information on a command.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            QzlMain cg = new();
            // test
            bool testaccess = false;
            bool testxlsx = false;
            bool testreader = false;
            bool testscalar = false;
            bool testnoquery = false;
            bool testinfo = false;

             string source = @"C:\snjw\code\shared\qzl\nullsource2.db";
             string dboutput = @"C:\snjw\code\qzl\app.output.xlsx";

// qzl sql -c "test.db" -q "SELECT * FROM Users;" -o "users.csv"

/*
dotnet publish --configuration Release -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:CopyOutputSymbolsToPublishDirectory=false --self-contained

cmdadmin

xcopy c:\snjw\code\qzl\bin\Release\net7.0\win-x64\publish\qzl.exe c:\windows\system32

*/

            if(testaccess)
            {
                // qzl sql -c "c:\temp\f.xlsx" -q "CREATE TABLE Users (Id TEXT);"
                cg.parameters.Clear();
                cg.parameters.Add("sql");
                cg.parameters.Add("-c");
                cg.parameters.Add(@"C:\Users\seaml\OneDrive\learning\ED5985\AT3\testnames.accdb");

                cg.parameters.Add("--source");
                cg.parameters.Add(@"C:\Users\seaml\OneDrive\learning\ED5985\AT3\testnames.txt");


                // cg.parameters.Add("-q");
//                cg.parameters.Add(@"SELECT * FROM Student;");
//                cg.parameters.Add(@"UPDATE Student SET FirstName ='Emma' WHERE UCASE(FirstName) = 'EMMA++';");
                // cg.parameters.Add("-v");
                // cg.parameters.Add("full");
                // cg.parameters.Add("-o");
                // cg.parameters.Add(@"C:\Users\seaml\OneDrive\learning\ED5985\AT3\student.csv");

                // qzl sql -c "test.db" -q "SELECT COUNT(*) AS usrcount FROM Users WHERE Level=1 GROUP BY Level;"  -m Scalar
                // cg.parameters.Add("-q");
                // cg.parameters.Add("CREATE TABLE Users (Id TEXT);");
                cg.Run();
                return;
            }




            if(testxlsx)
            {
                // qzl sql -c "c:\temp\f.xlsx" -q "CREATE TABLE Users (Id TEXT);"
                cg.parameters.Clear();
                cg.parameters.Add("sql");
                cg.parameters.Add("-c");
                cg.parameters.Add(@"C:\snjw\code\shared\qzl\test.db");
                cg.parameters.Add("-q");
                cg.parameters.Add(@"SELECT * FROM Users;");
                cg.parameters.Add("-o");
                cg.parameters.Add(@"C:\snjw\code\shared\qzl\users.csv");

                // qzl sql -c "test.db" -q "SELECT COUNT(*) AS usrcount FROM Users WHERE Level=1 GROUP BY Level;"  -m Scalar
                // cg.parameters.Add("-q");
                // cg.parameters.Add("CREATE TABLE Users (Id TEXT);");
                cg.Run();
                return;
            }


            // if(testxlsx)
            // {
            //     // qzl sql -c "c:\temp\f.xlsx" -q "CREATE TABLE Users (Id TEXT);"
            //     cg.parameters.Clear();
            //     cg.parameters.Add("sql");
            //     cg.parameters.Add("-c");
            //     cg.parameters.Add(@"C:\snjw\code\shared\qzl\test.db");
            //     cg.parameters.Add("-q");
            //     cg.parameters.Add(@"SELECT COUNT(*) AS usrcount FROM Users WHERE Level=1 GROUP BY Level;");
            //     cg.parameters.Add("-m");
            //     cg.parameters.Add("Scalar");
            //     cg.parameters.Add("-o");
            //     cg.parameters.Add(@"C:\snjw\code\shared\qzl\scalar.txt");

            //     // qzl sql -c "test.db" -q "SELECT COUNT(*) AS usrcount FROM Users WHERE Level=1 GROUP BY Level;"  -m Scalar
            //     // cg.parameters.Add("-q");
            //     // cg.parameters.Add("CREATE TABLE Users (Id TEXT);");
            //     cg.Run();
            //     return;
            // }



            // if(testxlsx)
            // {
            //     // qzl sql -c "c:\temp\f.xlsx" -q "CREATE TABLE Users (Id TEXT);"
            //     sql -c "Data Source=c:\temp\test.db;"
            //     cg.parameters.Clear();
            //     cg.parameters.Add("sql");
            //     cg.parameters.Add("-c");
            //     cg.parameters.Add(@"c:\temp\f.xlsx");
            //     cg.parameters.Add("-q");
            //     cg.parameters.Add("CREATE TABLE Users (Id TEXT);");
            //     cg.Run();
            //     return;
            // }

            // if(testxlsx)
            // {


            //     // qzl sql -c "c:\temp\f.xlsx" -q "CREATE TABLE Users (Id TEXT);"

            //     // string output = @"c:\SNJW\code\xp\Areas\Election\Controllers\ElectionController2.cs";
            //     cg.parameters.Clear();
            //     cg.parameters.Add("sql");
            //     // cg.parameters.Add("--provider");
            //     // cg.parameters.Add($"xl");
                
            //     cg.parameters.Add("--connection");
            //     cg.parameters.Add(dbsource);
            //     //cg.parameters.Add($"Data Source={dbsource};");
            //     // cg.parameters.Add("--output");
            //     // cg.parameters.Add(dboutput); //$"Data Source={filename};"
            //     cg.parameters.Add("--query");
            //     //cg.parameters.Add("CREATE TABLE Customers (Id TEXT, Name TEXT, Code TEXT)");
                
            //     //cg.parameters.Add("SELECT name, sql FROM sqlite_master WHERE type='table' ORDER BY name;");
            //     //cg.parameters.Add(" SELECT * FROM AspNetUsers;");
            //     //cg.parameters.Add("DELETE FROM Users WHERE Name LIKE 'A%';");
            //     //cg.parameters.Add("DELETE FROM AspNetUsers WHERE Id = '1007';");
            //     //cg.parameters.Add("SELECT Id, Email, phoneNumber, username FROM AspNetUsers WHERE Id > '1000';");
            //     cg.parameters.Add("INSERT INTO AspNetUsers(Id, Email, phoneNumber, username) VALUES ('1007','g@h.com','0472881881','Dave');");
                
                
            //     cg.parameters.Add("--verbosity");
            //     cg.parameters.Add("full");
            //     cg.Run();
            //     return;
            // }



            if(testreader)
            {

                // string output = @"c:\SNJW\code\xp\Areas\Election\Controllers\ElectionController2.cs";
                cg.parameters.Clear();
                cg.parameters.Add("sql");
                cg.parameters.Add("--provider");
                cg.parameters.Add($"sqllite");
                cg.parameters.Add("--connection");
                cg.parameters.Add(@"Data Source=C:\snjw\code\qzl\app.db");
                cg.parameters.Add("--query");
                cg.parameters.Add("SELECT * FROM AspNetUsers;");
                cg.parameters.Add("--method");
                cg.parameters.Add("reader");
                cg.Run();

                // INSERT INTO AspNetUsers (Id,AccessFailedCount,EmailConfirmed,LockoutEnabled,PhoneNumberConfirmed,TwoFactorEnabled,UserName) SELECT MAX( Id ) + 1,0,1,0,1,1,'New User' FROM AspNetUsers ;

// INSERT INTO AspNetUsers (Id,AccessFailedCount,EmailConfirmed,LockoutEnabled,PhoneNumberConfirmed,TwoFactorEnabled,UserName) VALUES('1003',0,1,0,1,1,'User3');

                // System.Diagnostics.Process.Start("notepad.exe",output);
                return;
            }

            if(testscalar)
            {
                // string output = @"c:\SNJW\code\xp\Areas\Election\Controllers\ElectionController2.cs";
                cg.parameters.Clear();
                cg.parameters.Add("scalar");
                cg.parameters.Add("--cname");

                cg.Run();
                // if(System.IO.File.Exists(outputfile))
                //     System.Diagnostics.Process.Start("notepad.exe",outputfile);
                return;                

            }

            // 2022-07-27 SNJW made parameters generic
            // If this is one of the 'original' items then use the old code 


            if(args.Length==0)
                args = new string[]{"--help"};
            if(args[0] == "")
                args = new string[]{"--help"};

            // do new stuff...
            cg.SetParameters(args);
            cg.Run();

            // if(!cg.Run())
            //     Console.WriteLine("csgen.exe did not complete.");
                // return;


            // if(args.Length==0)
            // {
            //     cg.NoParameters();
            // }
            // else if(args.Length==1)
            // {
            //     csgenold.OneParameter(args[0].ToString());
            // }
            // else
            // {
            //     csgenold.SetParameters(args);
            //     csgenold.Run();
            //     // Console.WriteLine("");
            //     // Console.WriteLine("csgen.exe completed");
            // }
        
        }

        public void Message(string message) {Console.WriteLine(message);}
    }

}
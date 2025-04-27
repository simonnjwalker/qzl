
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Seamlex.Utilities;
#pragma warning disable CS8602, CS8600, CS0219
namespace Seamlex.Utilities
{


/*

git add .
git commit -m "1.0.x mmasge"
git push

dotnet publish --configuration Release -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:CopyOutputSymbolsToPublishDirectory=false --self-contained

cmdadmin

xcopy c:\snjw\code\qzl\bin\Release\net7.0\win-x64\publish\qzl.exe c:\windows\system32

*/


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
        ///   pdf               Performs operations on a PDF file.
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
            bool testpdf = false;
            bool testheader = false;
            bool testmaxrows = false;
            bool testxml = false;
            bool testget = false;
            bool testpost = false;
            bool testverbosity = false;
            bool testaccess = false;
            bool testxlsx = false;
            bool testreader = false;
            bool testscalar = false;
            bool testnoquery = false;
            bool testinfo = false;

            string source = @"C:\snjw\code\shared\qzl\nullsource2.db";
            string dboutput = @"C:\snjw\code\qzl\app.output.xlsx";



            if(testpdf)
            {


                // qzl sql -c "c:\temp\f.xlsx" -q "CREATE TABLE Users (Id TEXT);"
                cg.parameters.Clear();
                cg.parameters.Add("pdf");
                cg.parameters.Add("-s");
                cg.parameters.Add(@"AGLC4-with-Bookmarks-1.pdf");
//                 cg.parameters.Add("-o");
//                 cg.parameters.Add(@"C:\Users\seaml\OneDrive - Department of Education\2025\LEG12\inquiry\output");
  



// qzl pdf -s AGLC4-with-Bookmarks-1.pdf -o c:\temp



                // // qzl sql -c "c:\temp\f.xlsx" -q "CREATE TABLE Users (Id TEXT);"
                // cg.parameters.Clear();
                // cg.parameters.Add("pdf");
                // cg.parameters.Add("-s");
                // cg.parameters.Add(@"C:\Users\seaml\OneDrive - Department of Education\2025\LEG12\inquiry\sources\*.*");
                // cg.parameters.Add("-o");
                // cg.parameters.Add(@"C:\Users\seaml\OneDrive - Department of Education\2025\LEG12\inquiry\output");
                // cg.parameters.Add("-m");
                // cg.parameters.Add(@"5");

                cg.Run();
                return;
            }


// qzl sql -q "SELECT * FROM QCEStudentT;" -o c:\temp\QCEStudentT.xml


/*

qzl sql -q "SELECT * FROM AchievementResultsT;" -c C:\snjw\code\pshs\ar23.accdb  -o c:\temp\AchievementResultsT.xml -v 5 -la v

qzl sql -q "SELECT * FROM StudentT;" -c C:\snjw\code\pshs\ar23.accdb  -o c:\temp\StudentT.xml -v 5 -la v

qzl sql -q "SELECT * FROM QCEClassT;" -c C:\snjw\code\pshs\ar23.accdb  -o c:\temp\QCEClassT.xml -v 5 -la v

qzl sql -q "SELECT * FROM QCEStudentT;" -c C:\snjw\code\pshs\ar23.accdb  -o c:\temp\QCEStudentT.xml -v 5 -la v

qzl sql -q "SELECT [QCELinkClassT].[EQID], [QCELinkClassT].[Class_Name], [QCELinkClassT].[11T1] AS x11T1, [QCELinkClassT].[11T2] AS x11T2, [QCELinkClassT].[11T3] AS x11T3, [QCELinkClassT].[11T4] AS x11T4, [QCELinkClassT].[12T1] AS x12T1, [QCELinkClassT].[12T2] AS x12T2, [QCELinkClassT].[12T3] AS x12T3, [QCELinkClassT].[12T4] AS x12T4, [Type], OnTrack, PotentialPoints FROM QCELinkClassT;" -c C:\snjw\code\pshs\ar23.accdb -o c:\temp\QCELinkClassT.xml -v 5 -la v

qzl sql -q "SELECT * FROM QCENoteT;" -c C:\snjw\code\pshs\ar23.accdb -o c:\temp\QCENoteT.xml -v 5 -la v

*/



            if(testheader)
            {

// qzl net -u "http://localhost:5005/api/v1/upload/xml/StudentT/087474676dbc4cc48014274ac6aeb6c2" -rm POST -o StudentT.xml -v 5 -mr 0

//    c:\snjw\code\qzl\bin\Release\net7.0\win-x64\publish\qzl.exe net -u "http://localhost:5005/api/v1/upload/xml/StudentT/087474676dbc4cc48014274ac6aeb6c2" -rm POST -o StudentT.xml -v 5 -mr 0



// helptext.Add( "  -u|--url           Remote location.");
// helptext.Add($" -hs|--headerstyle   Style of header (Chrome/Edge/FireFox).");
// helptext.Add($" -rm|--requestmethod Request method (GET/POST).");
// helptext.Add($"  -s|--source        Full path to upload file.");
// helptext.Add($"  -o|--output        Full path to output file.");
// helptext.Add( "  -v|--verbosity     Level of information displayed in console.");
// "Unable to track an instance of type 'QCEStudentT' because it does not have a primary key. Only entity types with a primary key may be tracked."

  
                // qzl sql -c "c:\temp\f.xlsx" -q "CREATE TABLE Users (Id TEXT);"
                cg.parameters.Clear();
                cg.parameters.Add("net");
                cg.parameters.Add("-u");
                cg.parameters.Add(@"http://localhost:5005/api/v1/upload/xml/StudentT/087474676dbc4cc48014274ac6aeb6c2");
                cg.parameters.Add("-rm");
                cg.parameters.Add(@"POST");
                cg.parameters.Add("-s");
                cg.parameters.Add(@"C:\snjw\code\pshs\QCEStudentT.xml");
                cg.parameters.Add("-v");
                cg.parameters.Add(@"5");

                cg.Run();
                return;
            }


            if(testmaxrows)
            {
                // qzl sql -c "c:\temp\f.xlsx" -q "CREATE TABLE Users (Id TEXT);"
                cg.parameters.Clear();
                cg.parameters.Add("sql");
                cg.parameters.Add("-q");
                cg.parameters.Add(@"SELECT * FROM QCEStudentT");
                cg.parameters.Add("-c");
                cg.parameters.Add(@"C:\snjw\code\pshs\ar23.accdb");
                cg.parameters.Add("-mr");
                cg.parameters.Add(@"3");
                cg.parameters.Add("-la");
                cg.parameters.Add(@"v");

                cg.Run();
                return;
            }



            if(testxml)
            {
                System.IO.File.Delete(@"c:\temp\QCELinkClassT.xml");
                cg.parameters.Clear();
                cg.parameters.Add("sql");
                cg.parameters.Add("-q");
                cg.parameters.Add(@"SELECT * FROM QCELinkClassT;");
                cg.parameters.Add("-c");
                cg.parameters.Add(@"C:\snjw\code\pshs\ar23.accdb");
                cg.parameters.Add("-o");
                cg.parameters.Add(@"c:\temp\QCENoteT.xml");
                cg.Run();
                return;
            }




            if(testpost)
            {

// qzl net -u "http://localhost:5005/api/v1/upload/xml/StudentT/087474676dbc4cc48014274ac6aeb6c2" -rm POST -o StudentT.xml -v 5 -mr 0


// helptext.Add( "  -u|--url           Remote location.");
// helptext.Add($" -hs|--headerstyle   Style of header (Chrome/Edge/FireFox).");
// helptext.Add($" -rm|--requestmethod Request method (GET/POST).");
// helptext.Add($"  -s|--source        Full path to upload file.");
// helptext.Add($"  -o|--output        Full path to output file.");
// helptext.Add( "  -v|--verbosity     Level of information displayed in console.");
// "Unable to track an instance of type 'QCEStudentT' because it does not have a primary key. Only entity types with a primary key may be tracked."


                // qzl sql -c "c:\temp\f.xlsx" -q "CREATE TABLE Users (Id TEXT);"
                cg.parameters.Clear();
                cg.parameters.Add("net");
                cg.parameters.Add("-u");
                cg.parameters.Add(@"http://localhost:5005/api/v1/upload/xml/achievementresultsts/087474676dbc4cc48014274ac6aeb6c2");
                cg.parameters.Add("-rm");
                cg.parameters.Add(@"POST");
                cg.parameters.Add("-s");
                cg.parameters.Add(@"c:\temp\QCEStudentT.xml");
                cg.parameters.Add("-v");
                cg.parameters.Add(@"3");

                cg.Run();
                return;
            }




            if(testget)
            {

// helptext.Add( "  -u|--url           Remote location.");
// helptext.Add($" -hs|--headerstyle   Style of header (Chrome/Edge/FireFox).");
// helptext.Add($" -rm|--requestmethod Request method (GET/POST).");
// helptext.Add($"  -s|--source        Full path to upload file.");
// helptext.Add($"  -o|--output        Full path to output file.");
// helptext.Add( "  -v|--verbosity     Level of information displayed in console.");
// http://localhost:5005/api/v1/upload/xml/achievementresultsts/087474676dbc4cc48014274ac6aeb6c2

                // qzl sql -c "c:\temp\f.xlsx" -q "CREATE TABLE Users (Id TEXT);"
                cg.parameters.Clear();
                cg.parameters.Add("net");
                cg.parameters.Add("-u");
                cg.parameters.Add(@"https://anapioficeandfire.com/api/characters/583");
                cg.parameters.Add("-rm");
                cg.parameters.Add(@"GET");
                cg.parameters.Add("-o");
                cg.parameters.Add(@"C:\temp\texoutput.txt");
                cg.parameters.Add("-v");
                cg.parameters.Add(@"5");
                cg.parameters.Add("-hs");
                cg.parameters.Add(@"c");

                cg.Run();
                return;
            }



            if(testverbosity)
            {
                // qzl sql -c "c:\temp\f.xlsx" -q "CREATE TABLE Users (Id TEXT);"
                System.IO.File.Exists("testdb1.db");
                    System.IO.File.Delete("testdb1.db");
                cg.parameters.Clear();
                cg.parameters.Add("sql");
                cg.parameters.Add("-q");
                cg.parameters.Add(@"CREATE TABLE test1(testfield1 INT);");
                cg.parameters.Add("-c");
                cg.parameters.Add(@"testdb1.db");
                cg.parameters.Add("-v");
                cg.parameters.Add(@"all");

                cg.Run();
                return;
            }



            if(testaccess)
            {

                // qzl sql -c "testnames.accdb" -q "create table staff(firstname VARCHAR, lastname VARCHAR, code VARCHAR)" -v full
                if(System.IO.File.Exists(@"C:\snjw\code\qzl\bin\Debug\net7.0\testnames.accdb"))
                    System.IO.File.Delete(@"C:\snjw\code\qzl\bin\Debug\net7.0\testnames.accdb");                
                System.IO.File.Copy(@"C:\Users\seaml\OneDrive\learning\ED5985\AT3\testnames.accdb", @"C:\snjw\code\qzl\bin\Debug\net7.0\testnames.accdb");                


                // qzl sql -c "c:\temp\f.xlsx" -q "CREATE TABLE Users (Id TEXT);"
                cg.parameters.Clear();
                cg.parameters.Add("sql");
                cg.parameters.Add("-q");
                
                cg.parameters.Add(@"SELECT * FROM Student;");
                cg.parameters.Add("-c");
                cg.parameters.Add("testnames.accdb");
                cg.parameters.Add("-v");
                cg.parameters.Add(@"5");

                cg.parameters.Add(@"--layout");
//                cg.parameters.Add(@"Vertical");
                cg.parameters.Add(@"v");

/*


                cg.parameters.Add(@"INSERT INTO staff(lastname,firstname,code) VALUES('Abraham','Asha',1);");

GRANT SELECT ON MSysObjects TO Admin;

                cg.parameters.Add(@"SELECT QCEStudentT.EQID, QCEStudentT.Year_Level, QCEStudentT.QCERisk, QCEStudentT.Preferred_Last_Name, QCEStudentT.Preferred_First_Name, QCEStudentT.Last_Name, QCEStudentT.First_Name, QCEStudentT.Enrolment_Status,  QCEStudentT.MonitoredBy FROM QCEStudentT WHERE EQID='7709685374K'");

SELECT MSysObjects.Name AS table_name FROM MSysObjects WHERE (((Left([Name],1))<>'~') AND ((Left([Name],4))<>'MSys') AND ((MSysObjects.Type) In (1,4,6)) AND ((MSysObjects.Flags)=0))order by MSysObjects.Name 
*/
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
                cg.parameters.Add(@"C:\temp\source.xlsx");
                cg.parameters.Add("-q");
                cg.parameters.Add(@"CREATE TABLE Users (Id TEXT, Name TEXT, Code TEXT);");
                // cg.parameters.Add("-o");
                // cg.parameters.Add(@"C:\snjw\code\shared\qzl\users.csv");

//c:\snjw\code\qzl>qzl sql -c "C:\temp\source.xlsx" -q "CREATE TABLE Users (Id TEXT, Name TEXT, Code TEXT);"
//Rows affected: 0

//qzl sql -c "C:\temp\source.xlsx" -q "INSERT INTO Users (Id, Name, Code) VALUES (1, 'Simon', 'SWAL007');"
//SQLite Error 1: 'no such table: Users'.



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
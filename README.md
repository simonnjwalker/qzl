# qzl
Execute queries from the command-line.

## Description
A multi-usage tool for ETL (Extraction-Transformation-Load) purposes where multiple database providers and/or XLSX/CSV files need to be created or edited.  

## Project Details
This tool has been designed and built to be used in scripting.  It builds to an executable file and this can be done on-the-fly using the commands below.

## Usage
When installed, the syntax for use is as follows:

    Usage: qzl [operation] [options]
    
    Operation:
      sql               Performs a SQL query.
      http              Performs an HTTP query.
      text              Performs operations on a text file.
      json              Performs operations on a JSON file.
    
    Options:
      -h|--help         Display help for each method.
    
    Run 'qzl [operation] --help' for more information on a command.

## Installation
This is an executable file that can be compiled into a shared folder with the syntax below.  Change to your directories.
    
    @echo ***************************************** 
    @echo Install qzl on win-x64 
    @echo ***************************************** 
    @echo Installing qzl in c:\SNJW\code\shared\qzl 
    cd c:\SNJW\code\shared 
    rd qzl /S /Q 
    rd qzlbuild /S /Q 
    md qzl 
    md qzlbuild
    cd qzlbuild
    git clone https://github.com/simonnjwalker/qzl.git 
    cd c:\SNJW\code\shared\qzlbuild\qzl 
    dotnet publish -r win-x64 -p:PublishSingleFile=True --self-contained false --use-current-runtime true 
    xcopy c:\snjw\code\shared\qzlbuild\qzl\bin\Debug\net7.0\win-x64\publish\*.* c:\SNJW\code\shared\qzl /Y /H /R /I
    cd c:\snjw\code\shared 
    rd qzlbuild /S /Q 
    cd c:\snjw\code\shared\qzl 
    
    @echo Finished installing qzl 
    @echo To test this out, try these commands: 
    qzl sql -c "test.db" -q "CREATE TABLE Users(Id TEXT, Name TEXT, Code TEXT, Level INTEGER);"
    qzl sql -c "test.db" -q "INSERT INTO Users(Id, Name, Code, Level) VALUES('1','Amy','AWB',1);" 
    qzl sql -c "test.db" -q "INSERT INTO Users(Id, Name, Code, Level) VALUES('2','Brad','BZ',2);" 
    qzl sql -c "test.db" -q "INSERT INTO Users(Id, Name, Code, Level) VALUES('3','Ciri','CW',1);" 
    qzl sql -c "test.db" -q "INSERT INTO Users(Id, Name, Code, Level) VALUES('4','Dave','DG',1);" 
    qzl sql -c "test.db" -q "INSERT INTO Users(Id, Name, Code, Level) VALUES('5','Elle','EM',3);" 
    qzl sql -c "test.db" -q "INSERT INTO Users(Id, Name, Code, Level) VALUES('6','Fred','FF',1);" 
    qzl sql -c "test.db" -q "SELECT * FROM Users;" -o "test.xlsx" 
    qzl sql -c "test.db" -q "SELECT COUNT(*) AS usrcount FROM Users WHERE Level=1 GROUP BY Level;" -o "result.txt" -m Scalar
    qzl sql -c "test.db" -q "SELECT * FROM Users;" -o "users.csv"
    
## Sample code

### Create a new Excel file
To create a new Excel file with a single sheet and headers:

    qzl sql -c "C:\temp\source.xlsx" -q "CREATE TABLE Users (Id TEXT, Name TEXT, Code TEXT);"

### Create a new SQLite file
To create a new SQLite database with a single table:

    qzl sql -c "C:\temp\source.db" -q "CREATE TABLE Users (Id TEXT, Name TEXT, Code TEXT);"

Note that the fully-formed connection-string can alternatively be passed:

    qzl sql -c "Data Source=C:\temp\source.db;" -q "CREATE TABLE Users (Id TEXT, Name TEXT, Code TEXT);"

A blank database could also be created by omitting the query:

    qzl sql -c "C:\temp\source.db"

### Insert data into an Excel document
To insert rows of data into an Excel file line-by-line:

    qzl sql -c "C:\temp\source.xlsx" -q "INSERT INTO Users (Id, Name, Code) VALUES (1, 'Simon', 'SWAL007');"

    qzl sql -c "C:\temp\source.xlsx" -q "INSERT INTO Users (Id, Name, Code) VALUES (2, 'Karen', 'KING001');"

    qzl sql -c "C:\temp\source.xlsx" -q "INSERT INTO Users (Id, Name, Code) VALUES (3, 'Devon', 'DBON004');"

    qzl sql -c "C:\temp\source.xlsx" -q "INSERT INTO Users (Id, Name, Code) VALUES (4, 'Steve', 'SGRA001');"

### Update data in an Excel document
To change rows of data in an Excel file:

    qzl sql -c "C:\temp\source.xlsx" -q "UPDATE Users SET Name = 'Devin' WHERE Id = 3;"

### Delete data in an Excel document
To remove rows of data in an Excel file:

    qzl sql -c "C:\temp\source.xlsx" -q "DELETE FROM Users WHERE Id > 3;"

### Save into a separate Excel file
To output to a new file, specify this in the "--output" field:

    qzl sql -c "C:\temp\source.xlsx" -q "SELECT * FROM Users WHERE Id <= 3;" --output "C:\temp\users.xlsx"
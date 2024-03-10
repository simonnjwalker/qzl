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
    @echo Install qzl
    @echo *****************************************
    @echo Installing qzl in c:\SNJW\code\shared
    cd c:\SNJW\code\shared
    del qzl.* /Q /F
    rd qzl /S /Q
    git clone https://github.com/simonnjwalker/qzl.git
    cd c:\SNJW\code\shared\qzl
    dotnet build
    cd..
    xcopy c:\SNJW\code\shared\qzl\bin\Debug\net7.0\qzl.* c:\SNJW\code\shared /Y /H /R
    rd qzl /S /Q
    @echo Finished installing qzl


## Sample code

### Create a new Excel file
To create a new Excel file with a single sheet and headers:
    qzl sql -c "C:\temp\source.xlsx" -q "CREATE TABLE Users (Id TEXT, Name TEXT, Code TEXT);"

### Insert data into an Excel document
To insert rows of data into this Excel file:
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
To output to a new file, specifiy this in the "--output" field:
    qzl sql -c "C:\temp\source.xlsx" -q "SELECT * FROM Users WHERE Id <= 3;" --output "C:\temp\users.xlsx"
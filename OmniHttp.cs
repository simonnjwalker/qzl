using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
//using System.Data.SqlServerCe;
using MySql.Data.MySqlClient;
using System.Data.OleDb;
using System.IO;
using System.Net.Http.Headers;
using System.Xml;
using System.Data;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Signers;
using Microsoft.Identity.Client;
#pragma warning disable CS8602, CS8600, CS8601, CS8604, CS8605, CS8625, CS0414
namespace Seamlex.Utilities
{
    /// <summary>
    /// flexible http tools
    /// </summary>
    public class OmniHttp
    {

        private string currenturl = "";
        private string currenthttpmethod = "GET";
        private string currentverbosity = "Default";
        private string lasterror = "";
        private string lasthttperror = "";
        private int rowsaffected = -1;

//        private System.Net.Http.HttpResponseMessage lastresult;

        private string lastresult = "";
        private string fileformfield = "file";

        public string CurrentURL
        {
            get { return currenturl; }
            set { currenturl = value; }
        }

        public string LastError
        {
            get { return lasterror; }
            set { lasterror = value; }
        }

        public string LastHttpError
        {
            get { return lasthttperror; }
            set { lasthttperror = value; }
        }

        public string LastResult 
        {
            get { 
                if(lastresult==null)
                    lastresult = "";
                return lastresult; }
            set { lastresult = value; }
        }

        public string FileFormField 
        {
            get { 
                if(fileformfield==null)
                    fileformfield = "";
                return fileformfield; }
            set { fileformfield = value; }
        }




        public string CurrentMethod
        {
            get
            {
                return currenthttpmethod;
            }
            set
            {

                string checkhttpmethod = value.Trim().ToLower();
                switch (checkhttpmethod) 
                {
                    case "":
                    case "g":
                    case "get":
                    case "default":
                        currenthttpmethod = "GET";
                    break;

                    case "p":
                    case "post":
                    case "file":
                        currenthttpmethod = "POST";
                    break;
                    default:
                        currenthttpmethod = "GET";
                    break;
                }
            }
        }

        public string CurrentVerbosity
        {
            get
            {
                return currentverbosity;
            }
            set
            {
                string verbosityText = value.Trim().ToLower();
                switch (verbosityText)
                {
                    case "none":
                    case "lowest":
                    case "nothing":
                    case "n":
                    case "0":
                    case "nil":
                        verbosityText = "None";
                        break;
                    case "minimum":
                    case "min":
                    case "m":
                    case "1":
                        verbosityText = "Minimum";
                        break;
                    case "low":
                    case "l":
                    case "2":
                        verbosityText = "Low";
                        break;
                    case "default":
                    case "def":
                    case "standard":
                    case "std":
                    case "d":
                    case "s":
                    case "3":
                        verbosityText = "Default";
                        break;
                    case "high":
                    case "higher":
                    case "more":
                    case "h":
                    case "4":
                        verbosityText = "High";
                        break;
                    case "full":
                    case "all":
                    case "everything":
                    case "highest":
                    case "f":
                    case "a":
                    case "5":
                    case "6":
                    case "7":
                    case "8":
                    case "9":
                        verbosityText = "Full";
                        break;
                    default:
                        verbosityText = "Default";
                        break;
                }
                currentverbosity = verbosityText;
            }
        }


        private static readonly HttpClient _httpClient = new HttpClient();


//httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
        public bool SetHeaders(string headerStyle)
        {
            
            if(headerStyle == "Chrome") //  Chrome on Windows.
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
            }
            else if(headerStyle == "Firefox") // Mozilla Firefox on Windows 10
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:118.0) Gecko/20100101 Firefox/118.0");
            }
            else if(headerStyle == "Edge") // Microsoft Edge on Windows 11
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36 Edg/115.0.0.0");
            }
            else if(headerStyle == "Safari") // 4. Safari on macOS (Big Sur)
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 11_2_3) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0.3 Safari/605.1.15");
            }
            else if(headerStyle == "ChromeMacOS") // Google Chrome on macOS (Monterey)
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 12_0_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.5790.110 Safari/537.36");
            }
            else if(headerStyle == "ChromeAndroid") // Google Chrome on Android
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 11; SM-G998B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.5790.110 Mobile Safari/537.36");
            }
            else if(headerStyle == "SafariiOS") // Safari on iOS
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15A372 Safari/604.1");
            }
            else if(headerStyle == "Opera") // Opera on Windows 10
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.5790.110 Safari/537.36 OPR/80.0.4170.63");
            }
            else if(headerStyle == "ChromeLinux") // Google Chrome on Linux (Ubuntu)
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.5790.110 Safari/537.36");
            }


            return true;
        }


        public bool Post(string uploadUrl, string filePath = "")
        {
            return this.PostAsync(uploadUrl, filePath).Result;
        }

        public async Task<bool> PostAsync(string uploadUrl, string filePath = "")
        {
            try
            {
                if (filePath!="" && !File.Exists(filePath))
                {
                    this.lasterror = $"Could not open file '{filePath}'.";
                    return false;
                }

                using (var form = new MultipartFormDataContent())
                {
                    if (filePath!="")
                    {
                        var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
                        form.Add(fileContent, this.fileformfield, Path.GetFileName(filePath));
                    }

                    var response = await _httpClient.PostAsync(uploadUrl, form);
                    this.LastResult = await response.Content.ReadAsStringAsync();

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                this.LastError = $"Could not complete upload: '{ex.Message}'.";
                return false;
            }
        }

        public bool Get(string targetUrl)
        {
            return this.GetAsync(targetUrl).Result;
        }

        public async Task<bool> GetAsync(string targetUrl)
        {
            try
            {
                using (var form = new MultipartFormDataContent())
                {
                    var response = await _httpClient.GetAsync(targetUrl);
                    // this.LastResult = response.Content.ToString();
                    this.LastResult = await response.Content.ReadAsStringAsync();
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                this.LastError = $"Could not complete download: '{ex.Message}'.";
                return false;
            }
        }






        internal string GetExplanation(int result)
        {

        /// If creating a connection fails, output = -2, will not continue
        /// If creating the command fails, output = -3, will not continue execution of query and will try and close if appropriate
        /// If opening the connection fails, output = -4, will not continue
        /// If executing the query fails, output = -5, will continue to try and close
        /// If executing the query succeeds, the number of rows affected is stored to output and to this.rowsaffected
        /// If the connection cannot be closed, output = -6 but this.rowsaffected remains populated

            if(result > 1)
            {
                return "The operation completed and returned data";
            }
            else if(result == 1 || result == 0)
            {
                return "The operation completed normally";
            }
            else if(result == -1)
            {
                return "The operation could not complete due to an initial connection or access issue";
            }
            else if(result == -2)
            {
                return "The operation could not complete due to a connection or access issue";
            }
            else if(result == -3)
            {
                return "The operation could not create the command or query";
            }
            else if(result == -4)
            {
                return "The operation could not open a database connection";
            }
            else if(result == -5)
            {
                return "The operation could not execute the query";
            }
            return "";
        }

    }

}

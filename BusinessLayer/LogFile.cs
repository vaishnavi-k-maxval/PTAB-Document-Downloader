using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace BusinessLayer
{
    public class LogFile
    {
        public static void WriteToFile(string content)
        {
            try
            {

                //DateTime date = Convert.ToDateTime(serverDt);
                //serverDt = date.ToString("MM/dd/yyyy");

                StreamWriter log;
                string serverDt;
                string filepath;

                serverDt = DateTime.Now.ToString("MM/dd/yyyy");
                serverDt = serverDt.Replace('/', '-');
                serverDt = serverDt.Replace(':', ' ');
                string Path = System.AppDomain.CurrentDomain.BaseDirectory + "\\Log" ;


                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }

                filepath = Path + "\\PTABDocumentDownloadLog_" + serverDt + ".txt";

                if (!File.Exists(filepath))
                {
                    log = new StreamWriter(filepath);
                }
                else
                {
                    log = File.AppendText(filepath);
                }

                log.WriteLine(DateTime.Now);
                log.WriteLine(content);
                log.WriteLine();
                // Close the stream:
                log.Close();
            }
            catch (Exception)
            {
                //throw;
            }

        }
    }
}

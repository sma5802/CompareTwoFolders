using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;

namespace CompareTwoFolders
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                LogMessageToFile("started...");
                RemoveAllnewStuff();
                GetOldStuffBack();
                LogMessageToFile("ended...");
            }
            catch(Exception e)
            {
                LogMessageToFile("Exception: " + e.Message);
            }
        }

        static void RemoveAllnewStuff()
        {
            string folder1 = ConfigurationManager.AppSettings["folder1"];
            string folder2 = ConfigurationManager.AppSettings["folder2"];
            DirectoryInfo dirFolder2 = new DirectoryInfo(folder2);
            DateTime createDT = dirFolder2.CreationTime;
            DateTime lastModifiedDT = dirFolder2.LastWriteTime;
            if (lastModifiedDT == null)
                lastModifiedDT = createDT;
            DirectoryInfo dirFolder1 = new DirectoryInfo(folder1);
            FileInfo [] files = dirFolder1.GetFiles("*.*", SearchOption.AllDirectories);
            //Remove all files that been newly created.
            for(int iCount = 0; iCount < files.Length; iCount ++)
            {
                if (files[iCount].CreationTime > lastModifiedDT || files[iCount].LastWriteTime > lastModifiedDT)
                {
                    files[iCount].Delete();
                    LogMessageToFile("Removed file: " + files[iCount].FullName);
                }
            }
            //Remove all directories that been newly created.
            DirectoryInfo[] dirs = dirFolder1.GetDirectories("*.*", SearchOption.AllDirectories);
            for (int iCount = 0; iCount < dirs.Length; iCount++)
            {
                if (dirs[iCount].CreationTime > lastModifiedDT && dirs[iCount].GetFiles("*.*").Length == 0 && dirs[iCount].GetDirectories("*.*").Length == 0)
                {
                   
                    dirs[iCount].Delete();
                    LogMessageToFile("Removed folder: " + dirs[iCount].FullName);
                }
            }
        }

        static void GetOldStuffBack()
        {
            string folder1 = ConfigurationManager.AppSettings["folder1"];
            string folder2 = ConfigurationManager.AppSettings["folder2"];
            DirectoryInfo dirFolder2 = new DirectoryInfo(folder2);
            DateTime lastModifiedDT = dirFolder2.CreationTime;

            DirectoryInfo dirFolder1 = new DirectoryInfo(folder1);
            FileInfo[] files1 = dirFolder1.GetFiles("*.*", SearchOption.AllDirectories);
            FileInfo[] files2 = dirFolder2.GetFiles("*.*", SearchOption.AllDirectories);
            var fileNames2 = from f2 in files2
                             select f2.FullName.Replace(folder2, folder1);
            var fileNames1 = from f1 in files1
                             select f1.FullName;


            IEnumerable<string> filesNeedCopied = fileNames2.Except(fileNames1); 
            foreach(string fi in filesNeedCopied)
            {
                string path2 = fi.Replace(folder1, folder2);
                File.Copy(path2, fi);
                LogMessageToFile("Copied file: " + path2);
            }
        }

        static void LogMessageToFile(string msg)
        {
            System.IO.StreamWriter sw = System.IO.File.AppendText(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\log" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
            try
            {
                string logLine = System.String.Format(
                    "{0:G}: {1}.", System.DateTime.Now, msg);
                sw.WriteLine(logLine);
            }
            finally
            {
                sw.Close();
            }
        }
    }
}

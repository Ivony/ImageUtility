using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;

namespace SkuFileTidier
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("程序已经启动...");
            string sourcedirpath = ConfigurationManager.AppSettings["sourceDirectory"];
            string targetdirpath = ConfigurationManager.AppSettings["targetDirectory"];
            if (CheckDirectoryExist(sourcedirpath, targetdirpath))
            {
                Console.WriteLine("程序可能会运行较长时间，请耐心等待...");

                ParseSku parse = new ParseSku();
                parse.GetSkuFiles(sourcedirpath);
                List<SkuFile> skufiles = parse.SkuFiles;
                List<string> filepaths = parse.FilePath;

                string strNow = DateTime.Now.ToString("yyyyMMddhhmmss");
                string unMovedFileLog = Path.Combine(sourcedirpath, string.Format("UnMatchedFiles_{0}.txt", strNow));
                string MovedFileLog = Path.Combine(sourcedirpath, string.Format("MovedFiles_{0}.txt", strNow));
                string ExistFileIgnore = Path.Combine(sourcedirpath, string.Format("IgnoreFiles_{0}.txt", strNow));

                FileStream fsUnMoved = File.Create(unMovedFileLog);
                FileStream fsMoved = File.Create(MovedFileLog);
                FileStream fsExistFileIgnore = File.Create(ExistFileIgnore);

                foreach (string item in filepaths)
                {
                    byte[] buffer = System.Text.Encoding.Default.GetBytes(item + Environment.NewLine);
                    fsUnMoved.Write(buffer, 0, buffer.Length);
                }

                foreach (SkuFile item in skufiles)
                {
                    string prefixFullPath = Path.Combine(targetdirpath, item.Prefix);
                    string skuFullPath = Path.Combine(targetdirpath, item.SkuPath);
                    string fileFullPath = Path.Combine(targetdirpath, item.FileTargetPath);
                    if (!Directory.Exists(prefixFullPath))
                    {
                        Directory.CreateDirectory(prefixFullPath);
                    }
                    if (!Directory.Exists(skuFullPath))
                    {
                        Directory.CreateDirectory(skuFullPath);
                    }
                    if (!File.Exists(fileFullPath))
                    {
                        File.Copy(item.SourcefullPath, fileFullPath);
                        Console.WriteLine("文件已经从{0}移动到{1}", item.SourcefullPath, fileFullPath);
                        byte[] buff = System.Text.Encoding.Default.GetBytes(fileFullPath + Environment.NewLine);
                        fsMoved.Write(buff, 0, buff.Length);
                    }
                    else
                    {
                        string msg = string.Format("目标文件夹{0}中已经存在{1}", item.SkuPath, Path.GetFileName(item.SourcefullPath));
                        Console.WriteLine(msg);
                        byte[] buff = System.Text.Encoding.Default.GetBytes(msg + Environment.NewLine);
                        fsExistFileIgnore.Write(buff, 0, buff.Length);
                    }
                }

                fsUnMoved.Flush();
                fsUnMoved.Close();

                fsMoved.Flush();
                fsMoved.Close();

                fsExistFileIgnore.Flush();
                fsExistFileIgnore.Close();

                Console.WriteLine("详细日志信息，请查看目录{0}", sourcedirpath);
            }
            Console.WriteLine("此次执行完成.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        private static void FileTidier()
        {
            Console.WriteLine("程序已经启动...");
            string sourcedirpath = ConfigurationManager.AppSettings["sourceDirectory"];
            string targetdirpath = ConfigurationManager.AppSettings["targetDirectory"];
            if (CheckDirectoryExist(sourcedirpath, targetdirpath))
            {
                Console.WriteLine("程序可能会运行较长时间，请耐心等待...");

                ParseSku parse = new ParseSku();
                parse.GetSkuFiles(sourcedirpath);
                List<SkuFile> skufiles = parse.SkuFiles;
                List<string> filepaths = parse.FilePath;

                string strNow = DateTime.Now.ToString("yyyyMMddhhmmss");
                string unMovedFileLog = Path.Combine(sourcedirpath, string.Format("UnMatchedFiles_{0}.txt", strNow));
                string MovedFileLog = Path.Combine(sourcedirpath, string.Format("MovedFiles_{0}.txt", strNow));
                string ExistFileIgnore = Path.Combine(sourcedirpath, string.Format("IgnoreFiles_{0}.txt", strNow));

                FileStream fsUnMoved = File.Create(unMovedFileLog);
                FileStream fsMoved = File.Create(MovedFileLog);
                FileStream fsExistFileIgnore = File.Create(ExistFileIgnore);

                foreach (string item in filepaths)
                {
                    byte[] buffer = System.Text.Encoding.Default.GetBytes(item + Environment.NewLine);
                    fsUnMoved.Write(buffer, 0, buffer.Length);
                }

                foreach (SkuFile item in skufiles)
                {
                    string prefixFullPath = Path.Combine(targetdirpath, item.Prefix);
                    string skuFullPath = Path.Combine(targetdirpath, item.SkuPath);
                    string fileFullPath = Path.Combine(targetdirpath, item.FileTargetPath);
                    if (!Directory.Exists(prefixFullPath))
                    {
                        Directory.CreateDirectory(prefixFullPath);
                    }
                    if (!Directory.Exists(skuFullPath))
                    {
                        Directory.CreateDirectory(skuFullPath);
                    }
                    if (!File.Exists(fileFullPath))
                    {
                        File.Move(item.SourcefullPath, fileFullPath);
                        Console.WriteLine("文件已经从{0}移动到{1}", item.SourcefullPath, fileFullPath);
                        byte[] buff = System.Text.Encoding.Default.GetBytes(fileFullPath + Environment.NewLine);
                        fsMoved.Write(buff, 0, buff.Length);
                    }
                    else
                    {
                        string msg = string.Format("目标文件夹{0}中已经存在{1}", item.SkuPath, Path.GetFileName(item.SourcefullPath));
                        Console.WriteLine(msg);
                        byte[] buff = System.Text.Encoding.Default.GetBytes(msg + Environment.NewLine);
                        fsExistFileIgnore.Write(buff, 0, buff.Length);
                    }
                }

                fsUnMoved.Flush();
                fsUnMoved.Close();

                fsMoved.Flush();
                fsMoved.Close();

                fsExistFileIgnore.Flush();
                fsExistFileIgnore.Close();

                Console.WriteLine("详细日志信息，请查看目录{0}", sourcedirpath);
            }
            Console.WriteLine("此次执行完成.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        private static bool CheckDirectoryExist(string source, string target)
        {
            bool result = true;
            if(Directory.Exists(source)==false)
            {
                Console.WriteLine("您指定的源目录{0}不存在。程序无法继续运行！", source);
                result  = false;
            }
            if (Directory.Exists(target) == false)
            {
                Directory.CreateDirectory(target);                  
            }
            return result;
        }

        private static void Exit()
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine(); 
        }
    }
}

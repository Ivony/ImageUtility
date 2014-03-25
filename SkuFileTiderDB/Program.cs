using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;

namespace SkuFileTiderDB
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourcedirpath = ConfigurationManager.AppSettings["sourceDirectory"];
            string targetdirpath = ConfigurationManager.AppSettings["targetDirectory"];

            if (!Directory.Exists(sourcedirpath))
            {
                Console.WriteLine("您指定的源目录:{0}不存在", sourcedirpath);
                return;
            }
            string logFilename = DateTime.Now.ToString("yyyyMMddhhmmss")+".txt";
            LogWrtie main = new LogWrtie(Path.Combine(sourcedirpath, logFilename));
            string msg = string.Empty;

            if (!Directory.Exists(targetdirpath))
            {
                msg = string.Format("已为您创建目标文件夹:{0}", targetdirpath);
                Console.WriteLine(msg);
                main.Write(msg);
            }           

            //Get Data From Database
            List<SourceSKU> sourceSkus = GetSourceSKU();
            msg = string.Format("从数据库中共获取{0}条数据", sourceSkus.Count.ToString());
            Console.WriteLine(msg);
            main.Write(msg);
            
            //Parse Data to SKU image File Path
            List<ParseSourceSku> parseSourceSkus = GetParseSouceSku(sourcedirpath, targetdirpath,sourceSkus);
           msg = string.Format("从数据中解析出{0}个图片", parseSourceSkus.Count.ToString());
            Console.WriteLine(msg);
            main.Write(msg);
                        
            //Move SKU image File
            main.Write("开始移动文件");
            Move(parseSourceSkus,ref main);
            main.Write("移动文件结束");
                       
            msg = "此次执行完成";
            Console.WriteLine(msg);
            main.Write(msg);
            main.Close();

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        public static List<SourceSKU> GetSourceSKU()
        {
            List<SourceSKU> sourceskus = new List<SourceSKU>();     
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ProductImageDB"].ConnectionString))
            {
                string sql = "select SKU,ImagePath from dbo.ProductImages";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Connection.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    SourceSKU sourcesku = new SourceSKU(sdr["sku"].ToString(), sdr["ImagePath"].ToString());
                    sourceskus.Add(sourcesku);
                }
            }
            return sourceskus;
        }

        public static List<ParseSourceSku> GetParseSouceSku(string sourcepath,string targetpath,List<SourceSKU> sourceskus)
        {
            List<ParseSourceSku> parseSourceSkus = new List<ParseSourceSku>();
            foreach (SourceSKU item in sourceskus)
            {
                ParseSourceSku pss = new ParseSourceSku(sourcepath, targetpath, item);
                parseSourceSkus.Add(pss);
            }
            return parseSourceSkus;
        }

        public static void Move(List<ParseSourceSku> parseSourceSkus, ref LogWrtie movelog)
        {
            foreach (ParseSourceSku item in parseSourceSkus)
            {
                if (!Directory.Exists(item.SkuPrefixFolderFullPath))
                {
                    Directory.CreateDirectory(item.SkuPrefixFolderFullPath);
                }
                if (!Directory.Exists(item.SkuSubFolderFullPath))
                {
                    Directory.CreateDirectory(item.SkuSubFolderFullPath);
                }
                File.Copy(item.SourceFullPath, item.TargetFullPath);
                string msg = string.Format("文件已从{0}复制到{1}",item.SourceFullPath,item.TargetFullPath);
                movelog.Write(msg);
                Console.WriteLine(msg);
            }
        }
    }

    public class LogWrtie
    {
        public FileStream fileStream;
        public LogWrtie(string fileName)
        {
            this.fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        }
        public void Write(string message)
        {
            byte[] msg = System.Text.Encoding.Default.GetBytes(message+Environment.NewLine);
            fileStream.Write(msg, 0, msg.Length);
            this.fileStream.Flush();
        }
        public void Close()
        {
            this.fileStream.Close();
        }
    }

    public class ParseSourceSku
    {
        public ParseSourceSku(string sourceFolder, string targetFolder, SourceSKU sku)
        {
            this.SourceFullPath = Path.Combine(sourceFolder, sku.ImagePath);
            string prefix = sku.SKU.Substring(0, 2);
            string skubody = sku.SKU.Substring(2, sku.SKU.Length - 3);
            this.SkuPrefixFolderFullPath = Path.Combine(targetFolder, prefix);
            this.SkuSubFolderFullPath = Path.Combine(this.SkuPrefixFolderFullPath, skubody);

            int lastslash = sku.ImagePath.LastIndexOf(@"\");
            this.TargetFullPath = Path.Combine(SkuSubFolderFullPath, sku.ImagePath.Substring(lastslash + 1, sku.ImagePath.Length - lastslash - 1));
        }

        public string SourceFullPath;
        public string TargetFullPath;
        public string SkuPrefixFolderFullPath;
        public string SkuSubFolderFullPath;
    }

    public class SourceSKU
    {
        public SourceSKU(string sku,string imagePath)
        {
            this._SKU = sku;
            this._imagePath = imagePath;
        }
        private string _SKU;
        private string _imagePath;

        public string ImagePath
        {
            get { return _imagePath; }
            set { _imagePath = value; }
        }

        public string SKU
        {
            get { return _SKU; }
            set { _SKU = value; }
        }
    }
}

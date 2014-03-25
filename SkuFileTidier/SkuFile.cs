using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace SkuFileTidier
{
    public class ParseSku
    {
        private List<string> _filePath;

        public List<string> FilePath
        {
            get { return _filePath; } 
        }
        private List<SkuFile> _skuFiles;

        public List<SkuFile> SkuFiles
        {
            get { return _skuFiles; } 
        }

        public ParseSku()
        {
            this._filePath = new List<string>();
            this._skuFiles = new List<SkuFile>();
        }

        private   Regex filenameRegex = new Regex(@"(?<sku>^[A-Z0-9]{6,7})(-[0-9]+)?\.JPG", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public  void  GetSkuFiles(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath)) throw new ArgumentNullException(directoryPath);

            var files = new DirectoryInfo(directoryPath).EnumerateFiles("*.jpg", SearchOption.AllDirectories);

            foreach (string item in files.Select( file => file.FullName))
            {
                SkuFile skufile = GetSkuFile(item);
                if (skufile == null)
                {
                    _filePath.Add(item);
                    Console.WriteLine("文件名：{0}，不符合SKU规则，忽略此文件", Path.GetFileName(item));
                }
                else
                {
                    _skuFiles.Add(skufile);
                }
            }
        }

        public   SkuFile GetSkuFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(filePath);
            var filename = Path.GetFileName(filePath);
            var match = filenameRegex.Match(filename);
            if (!match.Success)
                return null;
            else
            {
                var sku = match.Groups["sku"].Value;
                SkuFile skufile = new SkuFile(sku, filePath);
                return skufile;
            }
        }
    }

    public class SkuFile
    {
        public SkuFile(string skuFileName, string sourceFilePath)
        {
            if (string.IsNullOrEmpty(skuFileName)) throw new ArgumentNullException("skuFileName");
            if (string.IsNullOrEmpty(sourceFilePath)) throw new ArgumentNullException("sourceFilePath");
            _sourcefullPath = sourceFilePath;
            SourcefullPath = _sourcefullPath;
            Sku = skuFileName;
        }

        private string _sourcefullPath;

        public string SourcefullPath
        {
            get { return _sourcefullPath; }
            set
            {
                _sourcefullPath = value;
                _fileName = Path.GetFileName(_sourcefullPath);
            }
        }
        private string _prefix;
        private string _sku;
        private string _skuBody;
        private string _skuPath;
        private string _fileName;
        private string _fileTargetPath;

        public string FileTargetPath
        {
            get { return _fileTargetPath; } 
        }

      

        public string Sku
        {
            get { return _sku; }
            set
            {
                _sku = value;
                _prefix = _sku.Substring(0, 2);
                _skuBody = _sku.Substring(2, _sku.Length - 3);
                _skuPath = Path.Combine(_prefix, _skuBody);
                _fileTargetPath = Path.Combine(_prefix, _skuBody, _fileName);
            }
        }

        public string Prefix
        {
            get { return _prefix; }
        } 

        public string SkuBody
        {
            get { return _skuBody; } 
        } 

        public string SkuPath
        {
            get { return _skuPath; }
        }
        public string FileName
        {
            get { return _fileName; }
        }
    }
        
}

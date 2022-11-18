using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using static System.Console;

namespace LSPImgs
{
    class Program
    {
        static void Main(string[] args)
        {
            string imgSouces = "https://dopaminegirl.com/api/posts/ranking?offset=50";
            string folderURI = @"D:\LspImgs\";
            string jsonUrl = CreateGetHttpRequest(imgSouces);
            int downloadCount = 0;
            //获取的jsonURL进行遍历
            JArray jArray = JArray.Parse(jsonUrl);
            List<string> fileList = GetAllFileNames(folderURI);
            List<string> md5List = new List<string>();
            foreach (var item in fileList)
            {
                md5List.Add(GetMD5HashFromFile(folderURI + item));
            }
            foreach (var jsonItem in jArray)
            {
                JObject job = (JObject)jsonItem;
                string url = job["url"].ToString();
                Random random = new Random();
                string imgFileName = SaveImageFromWeb(url, folderURI, random.Next().ToString());
                downloadCount += 1;
                WriteLine($"第{downloadCount}张，已下载完成，下载路径为:{folderURI + imgFileName}");
                string MD5 = GetMD5HashFromFile(folderURI+imgFileName);
                //如果当前下载的图片已存在则删除（不保存）当前下载的图片
                if (md5List.Contains(MD5))
                {
                    WriteLine("发现重复文件！Delete中...");
                    WriteLine("路径为：" + folderURI + imgFileName);
                    File.Delete(folderURI + imgFileName);
                    WriteLine("已删除!");
                }
            }
            WriteLine("下载完成！回车以关闭！下载数量：" + downloadCount);
            ReadLine();
        }

        public static string CreateGetHttpRequest(string souces)
        {
            HttpWebResponse response = CreateGetHttpResponse(souces);
            //获取流
            Stream streamResponse = response.GetResponseStream();
            //使用UTF8解码
            StreamReader streanReader = new StreamReader(streamResponse, Encoding.UTF8);

            string retString = streanReader.ReadToEnd();

            return retString;
        }

        public static HttpWebResponse CreateGetHttpResponse(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "text/html;chartset=UTF-8";
            request.UserAgent = "Mozilla / 5.0(Windows NT 10.0; Win64; x64; rv: 48.0) Gecko / 20100101 Firefox / 48.0"; //火狐用户代理
            request.Method = "GET";
            return (HttpWebResponse)request.GetResponse();
        }

        ////保存文件
        //public static void SaveFile(string content)
        //{
        //    string dirPath = @"D:\test";
        //    Random random = new Random();
        //    string filePath = dirPath +"\\"+ random.Next().ToString() + @"getRequest.jpg";
        //    //先创建保存的路径
        //    if (!Directory.Exists(dirPath))
        //        Directory.CreateDirectory(dirPath);
        //    if (!Directory.Exists(filePath))
        //        using (File.Create(filePath)) ;

        //    //创建文件流
        //    FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        //    //创建写入器
        //    StreamWriter sr = new StreamWriter(fs);
        //    sr.Write(content);
        //    sr.Close();
        //    fs.Close();
        //}

        /// <summary>
        /// 保存web图片到本地
        /// </summary>
        /// <param name="imgUrl">web图片路径</param>
        /// <param name="path">保存路径</param>
        /// <param name="fileName">保存文件名</param>
        /// <returns></returns>
        public static string SaveImageFromWeb(string imgUrl, string path, string fileName)
        {
            if (path.Equals(""))
                throw new Exception("未指定保存文件的路径");
            string imgName = imgUrl.ToString().Substring(imgUrl.ToString().LastIndexOf("/") + 1);
            string defaultType = ".jpg";
            string[] imgTypes = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            string imgType = imgUrl.ToString().Substring(imgUrl.ToString().LastIndexOf("."));
            string imgPath = "";
            foreach (string it in imgTypes)
            {
                if (imgType.ToLower().Equals(it))
                    break;
                if (it.Equals(".bmp"))
                    imgType = defaultType;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imgUrl);
            request.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
            request.Timeout = 20000;

            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();

            if (response.ContentType.ToLower().StartsWith("image/"))
            {
                byte[] arrayByte = new byte[1024];
                int imgLong = (int)response.ContentLength;
                int l = 0;

                if (fileName == "")
                    fileName = imgName;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                    FileStream fso = new FileStream(path + fileName + imgType, FileMode.Create);
                while (l < imgLong)
                {
                    int i = stream.Read(arrayByte, 0, 1024);
                    fso.Write(arrayByte, 0, i);
                    l += i;
                }

                fso.Close();
                stream.Close();
                response.Close();
                imgPath = fileName + imgType;
                return imgPath;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="file">文件绝对路径</param>
        /// <returns>MD5值</returns>
        public static string GetMD5HashFromFile(string file)
        {
            try
            {
                FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fileStream);
                fileStream.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("获取文件MD5值error:" + ex.Message);
            }
        }

        /// <summary>
        /// 获取当前目录所有文件名
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        static List<string> GetAllFileNames(string path, string pattern = "*")
        {
            List<FileInfo> folder = new DirectoryInfo(path).GetFiles(pattern).ToList();

            return folder.Select(x => x.Name).ToList();
        }
    }
}

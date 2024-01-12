using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections; 

namespace ConfigDll
{
    /// <summary>
    /// 文本文件操作类
    /// </summary>
    public class TxtOperation
    {
        /// <summary>
        /// 读取文本文件到字符串
        /// </summary>
        /// <param name="sFullFilename"></param>
        /// <returns></returns>
        public static string ReadTxtFileToString(string sFullFilename)
        {
            //如果文件不存在则退出
            if (!File.Exists(sFullFilename))
            {
                return "";
            }
            //如果文件不是txt格式则退出
            string fileExtName = sFullFilename.Substring(sFullFilename.LastIndexOf(".") + 1, 3);
            if(fileExtName.ToLower().Trim() !="txt")
            {
                return "";
            }
            //创建读取数据流
            StreamReader readFileStream = new StreamReader(sFullFilename, Encoding.Default);
            //读取全部文本
            string result = readFileStream.ReadToEnd();
            //关闭文本文件
            readFileStream.Close();
            //返回
            return result;
        }

        /// <summary>
        /// 写入字符串到文本文件
        /// </summary>
        /// <param name="sFullFilename">完整文件名</param>
        /// <param name="sInfo">写入的信息</param>
        /// <returns></returns>
        public static bool WriteStringToTxtFile(string sFullFilename, string sInfo)
        {
            bool isSucceed = false;
            //如果文件不是txt格式则退出
            string fileExtName = sFullFilename.Substring(sFullFilename.LastIndexOf(".") + 1, 3);
            if (fileExtName.ToLower().Trim() != "txt")
            {
                return false;
            }
            //创建写入数据流
            StreamWriter writeFileStream = new StreamWriter(sFullFilename, true, Encoding.Default);
            writeFileStream.Write(sInfo);
            //关闭文本文件
            writeFileStream.Close();
            isSucceed = true;
            //返回
            return isSucceed;
        }

        /// <summary>
        /// 读取文本文件每一行到字符串数组
        /// </summary>
        /// <param name="sFullFilename"></param>
        /// <returns></returns>
        public static string[] ReadTxtFileToStringSet(string sFullFilename)
        {
            //如果文件不存在则退出
            if (!File.Exists(sFullFilename))
            {
                return null;
            }
            //如果文件不是txt格式则退出
            string fileExtName = sFullFilename.Substring(sFullFilename.LastIndexOf(".") + 1, 3);
            if (fileExtName.ToLower().Trim() != "txt")
            {
                return null;
            }

            ArrayList sArray = new ArrayList(500);
            // 创建StreamReader读取文件. 使用'using'自动关闭文件
            using (StreamReader sr = new StreamReader(sFullFilename, Encoding.Default)) 
            {
                string line;
                // 循环读取文件
                while ((line = sr.ReadLine()) != null)
                {
                    sArray.Add(line);
                }
            }

            string[] sResult = new string[sArray.Count];
            for (int i = 0; i < sArray.Count; ++i)
            {
                sResult[i] = Convert.ToString(sArray[i]);
            }
            //返回
            return sResult;
        }

        /// <summary>
        /// 读取SQL文件每一行到字符串数组
        /// </summary>
        /// <param name="sFullFilename"></param>
        /// <returns></returns>
        public static string[] ReadSqlFileToSQLStringSet(string sFullFilename)
        {
            //如果文件不存在则退出
            if (!File.Exists(sFullFilename))
            {
                return null;
            }
            //如果文件不是txt格式则退出
            string fileExtName = sFullFilename.Substring(sFullFilename.LastIndexOf(".") + 1, 3);
            if (fileExtName.ToLower().Trim() != "sql")
            {
                return null;
            }

            ArrayList sArray = new ArrayList(500);
            // 创建StreamReader读取文件. 使用'using'自动关闭文件
            using (StreamReader sr = new StreamReader(sFullFilename, Encoding.Default))
            {
                string line = "";
                string sqlLine = "";
                // 循环读取文件
                while ((line = sr.ReadLine()) != null)
                {
                    //不是注释
                    if ((line != "") && (line.Substring(0, 1) != "-"))
                    {
                        sqlLine = sqlLine + line;
                    }
                    else if (sqlLine != "")
                    {
                        sArray.Add(sqlLine);
                        sqlLine = "";
                    }
                }
            }

            string[] sResult = new string[sArray.Count];
            for (int i = 0; i < sArray.Count; ++i)
            {
                sResult[i] = Convert.ToString(sArray[i]);
            }
            //返回
            return sResult;
        }
    }
}

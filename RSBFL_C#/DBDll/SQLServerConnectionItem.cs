using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DBDll
{
    /// <summary>
    /// 连接字符串操作
    /// </summary>
    public class SQLServerConnectionItem
    {
        /// <summary>
        /// Provider
        /// </summary>
        protected string m_sProvider = String.Empty;
        /// <summary>
        /// 获取或设置Provider
        /// </summary>
        public string Provider
        {
            get { return m_sProvider; }
            set { m_sProvider = value; }
        }

        /// <summary>
        /// DataSource
        /// </summary>
        protected string m_sDataSource = String.Empty;
        /// <summary>
        /// 获取或设置DataSource
        /// </summary>
        public string DataSource
        {
            get { return m_sDataSource; }
            set { m_sDataSource = value; }
        }

        /// <summary>
        /// InitialCatalog
        /// </summary>
        protected string m_sInitialCatalog = String.Empty;
        /// <summary>
        /// 获取或设置InitialCatalog
        /// </summary>
        public string InitialCatalog
        {
            get { return m_sInitialCatalog; }
            set { m_sInitialCatalog = value; }
        }

        /// <summary>
        /// UserID
        /// </summary>
        protected string m_sUserID = String.Empty;
        /// <summary>
        /// 获取或设置UserID
        /// </summary>
        public string UserID
        {
            get { return m_sUserID; }
            set { m_sUserID = value; }
        }

        /// <summary>
        /// Password
        /// </summary>
        protected string m_sPassword = String.Empty;
        /// <summary>
        /// 获取或设置Password
        /// </summary>
        public string Password
        {
            get { return m_sPassword; }
            set { m_sPassword = value; }
        }

        /// <summary>
        /// PersistSecurityInfo
        /// </summary>
        protected string m_sPersistSecurityInfo = String.Empty;
        /// <summary>
        /// 获取或设置PersistSecurityInfo
        /// </summary>
        public string PersistSecurityInfo
        {
            get { return m_sPersistSecurityInfo; }
            set { m_sPersistSecurityInfo = value; }
        }

        /// <summary>
        /// 构造函数：创建数据库默认连接
        /// </summary>
        public SQLServerConnectionItem()
        {
        }

        /// <summary>
        /// 通过UDL文件中的连接字符串
        /// </summary>
        /// <param name="sUDLFullFileName"></param>
        /// <returns></returns>
        private static string ReadFullConnectionStringFromUDL(string sUDLFullFileName)
        {
            string sConnectionString = String.Empty;

            //如果文件不存在则退出
            if (!File.Exists(sUDLFullFileName))
            {
                return sConnectionString;
            }
            //如果文件不是udl格式则退出
            string fileExtName = sUDLFullFileName.Substring(sUDLFullFileName.LastIndexOf(".") + 1, 3);
            if (fileExtName.ToLower().Trim().ToLower() != "udl")
            {
                return sConnectionString;
            }
            //创建读取数据流
            StreamReader readFileStream = new StreamReader(sUDLFullFileName, Encoding.Default);
            //读取文本
            while (!readFileStream.EndOfStream)
            {
                sConnectionString = readFileStream.ReadLine();
            }
            //关闭文本文件
            readFileStream.Close();

            return sConnectionString;
        }

        /// <summary>
        /// 通过UDL文件构造连接完整字符串
        /// </summary>
        /// <param name="sUDLFullFileName"></param>
        /// <returns></returns>
        public static string ConstructFullConnectionStringFromUDL(string sUDLFullFileName)
        {
            return ReadFullConnectionStringFromUDL(sUDLFullFileName);
        }

        /// <summary>
        /// 通过UDL文件构造SQLServer可用的连接字符串
        /// </summary>
        /// <param name="sUDLFullFileName"></param>
        /// <returns></returns>
        public static string ConstructUsableConnectionStringFromUDL(string sUDLFullFileName)
        {
            string sConnectionString = ReadFullConnectionStringFromUDL(sUDLFullFileName);

            if (String.Empty == sConnectionString)
            {
                return String.Empty;
            }

            //删除Provider
            sConnectionString = sConnectionString.Remove(0, sConnectionString.IndexOf(';') + 1);

            return sConnectionString;
        }

        /// <summary>
        /// 通过连接字符串项构造SQLServer可用的连接字符串
        /// </summary>
        /// <param name="mConnectionItem">连接字符串项</param>
        /// <returns></returns>
        public static string ConstructUsableConnectionStringFromConnectionItem(SQLServerConnectionItem mConnectionItem)
        {
            string sConnectionString = String.Empty;

            //构造字符串
            if (mConnectionItem.DataSource != String.Empty)
            {
                sConnectionString = sConnectionString + "Data Source=" + mConnectionItem.DataSource + ";";
            }
            if (mConnectionItem.InitialCatalog != String.Empty)
            {
                sConnectionString = sConnectionString + "Initial Catalog=" + mConnectionItem.InitialCatalog + ";";
            }
            if (mConnectionItem.UserID != String.Empty)
            {
                sConnectionString = sConnectionString + "User ID=" + mConnectionItem.UserID + ";";
            }
            if (mConnectionItem.Password != String.Empty)
            {
                sConnectionString = sConnectionString + "Password=" + mConnectionItem.Password + ";";
            }
            sConnectionString = sConnectionString + "Persist Security Info=" + "true";

            return sConnectionString;
        }

        /// <summary>
        /// 通过连接字符串项构造UDL源文件
        /// </summary>
        /// <param name="mConnectionItem">连接字符串项</param>
        /// <returns></returns>
        public static string ConstructUDLFromConnectionItem(SQLServerConnectionItem mConnectionItem)
        {
            string sConnectionString = String.Empty;
            //构造字符串
            sConnectionString = sConnectionString + "Provider=SQLOLEDB.1;";
            if (mConnectionItem.Password != String.Empty)
            {
                sConnectionString = sConnectionString + "Password=" + mConnectionItem.Password + ";";
            }
            sConnectionString = sConnectionString + "Persist Security Info=" + "true;";
            if (mConnectionItem.UserID != String.Empty)
            {
                sConnectionString = sConnectionString + "User ID=" + mConnectionItem.UserID + ";";
            }
            if (mConnectionItem.InitialCatalog != String.Empty)
            {
                sConnectionString = sConnectionString + "Initial Catalog=" + mConnectionItem.InitialCatalog + ";";
            }
            if (mConnectionItem.DataSource != String.Empty)
            {
                sConnectionString = sConnectionString + "Data Source=" + mConnectionItem.DataSource;
            }
 
            string sUDLString = "[oledb]" + "\r\n" + "; Everything after this line is an OLE DB initstring" + "\r\n" + sConnectionString;

            return sUDLString;
        }


        /// <summary>
        /// 通过UDL文件获取各项连接参数值
        /// </summary>
        /// <param name="sUDLFullFileName"></param>
        /// <returns></returns>
        public static SQLServerConnectionItem GetFullConnectionItemFromUDL(string sUDLFullFileName)
        {
            string sConnectionString = ReadFullConnectionStringFromUDL(sUDLFullFileName);

            if (String.Empty == sConnectionString)
            {
                return null;
            }

            //获取各项值
            SQLServerConnectionItem mSQLServerConnectionItem = GetFullConnectionItemFromConnectionString(sConnectionString);

            return mSQLServerConnectionItem;
        }

        /// <summary>
        /// 通过连接字符串获取各项连接参数值
        /// </summary>
        /// <param name="sConnectionString"></param>
        /// <returns></returns>
        public static SQLServerConnectionItem GetFullConnectionItemFromConnectionString(string sConnectionString)
        {
            SQLServerConnectionItem mSQLServerConnectionItem = new SQLServerConnectionItem();
            string[] sConnectionItemStrings = sConnectionString.Split(';');
            string[] sOneConnectionItemStrings = null;
            for (int i = 0; i < sConnectionItemStrings.Length; ++i)
            {
                sOneConnectionItemStrings = sConnectionItemStrings[i].Split('=');
                switch (sOneConnectionItemStrings[0])
                {
                    case "Provider":
                    {
                        mSQLServerConnectionItem.m_sProvider = sOneConnectionItemStrings[1];
                        break;
                    }
                    case "Data Source":
                    {
                        mSQLServerConnectionItem.m_sDataSource = sOneConnectionItemStrings[1];
                        break;
                    }
                    case "Initial Catalog":
                    {
                        mSQLServerConnectionItem.m_sInitialCatalog = sOneConnectionItemStrings[1];
                        break;
                    }
                    case "User ID":
                    {
                        mSQLServerConnectionItem.m_sUserID = sOneConnectionItemStrings[1];
                        break;
                    }
                    case "Password":
                    {
                        mSQLServerConnectionItem.m_sPassword = sOneConnectionItemStrings[1];
                        break;
                    }
                    case "Persist Security Info":
                    {
                        mSQLServerConnectionItem.m_sPersistSecurityInfo = sOneConnectionItemStrings[1];
                        break;
                    }  
                } 
            }
            //
            return mSQLServerConnectionItem;
        }
    }
}

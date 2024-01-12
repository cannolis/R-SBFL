using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Specialized;

//
using DBDll;

namespace FaultLocalization
{
    /// <summary>
    /// 数据库操作类
    /// </summary>
    public partial class FLDBServer
    {
        /// <summary>
        /// 数据库文件
        /// </summary>
        private static string m_sDbFile = "";

        /// <summary>
        /// SQLServerOperation对象
        /// </summary>
        private static SQLServerOperation m_SQLServerOperation = null;

        /// <summary>
        /// 设置数据库对象
        /// </summary>
        /// <param name="sDbFile"></param>
        public static void SetDBServer(string sDbFile)
        {
            m_sDbFile = sDbFile;
            m_SQLServerOperation = new SQLServerOperation(m_sDbFile);
        }

        /// <summary>
        /// 打开数据库
        /// </summary>
        /// <returns>是否成功打开</returns>
        public static bool OpenConnection()
        {
            if (m_SQLServerOperation.SQLServerOpenConnection())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 关闭数据库
        /// </summary>
        public static void CloseConnection()
        {
            m_SQLServerOperation.SQLServerCloseConnection();
        }

        // 获取已有的实验包名称
        public static List<string> GetSuitesNameInDB()
        {
            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT 实验包 FROM 实验对象数据表 GROUP BY 实验包";
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return null;
            }

            List<string> result = new List<string>();
            for (int i = 0; i < mDataSet.Tables[0].Rows.Count; i++)
            {
                result.Add(mDataSet.Tables[0].Rows[i]["实验包"].ToString());
            }
            return result;
        }

        // 获取实验包下已有的实验程序名
        public static List<string> GetProgramNameofSuite(string suiteName)
        {
            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT 目标程序 FROM 实验对象数据表"
                          + " WHERE 实验包=" + "'" + suiteName + "'"
                          + " GROUP BY 目标程序";
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return null;
            }

            List<string> result = new List<string>();
            for (int i = 0; i < mDataSet.Tables[0].Rows.Count; i++)
            {
                result.Add(mDataSet.Tables[0].Rows[i]["目标程序"].ToString());
            }
            return result;
        }

        // 获取缺陷版本名称
        public static List<string> GetVersionNameofProgram(string suiteName, string programName)
        {
            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT 缺陷版本 FROM 实验对象数据表"
                         + " WHERE 实验包=" + "'" + suiteName + "'"
                         + " AND 目标程序=" + "'" + programName + "'"
                         + " GROUP BY 缺陷版本";
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return null;
            }

            List<string> result = new List<string>();
            for (int i = 0; i < mDataSet.Tables[0].Rows.Count; i++)
            {
                result.Add(mDataSet.Tables[0].Rows[i]["缺陷版本"].ToString());
            }
            return result;
        }

        // 获取程序的缺陷名称
        public static List<string> GetFaultNamesofProgram(string suiteName, string programName)
        {
            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT 缺陷名称 FROM 缺陷描述表"
                         + " WHERE 实验包=" + "'" + suiteName + "'"
                         + " AND 目标程序=" + "'" + programName + "'"
                         + " GROUP BY 缺陷名称";
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return null;
            }

            List<string> result = new List<string>();
            for (int i = 0; i < mDataSet.Tables[0].Rows.Count; i++)
            {
                result.Add(mDataSet.Tables[0].Rows[i]["缺陷名称"].ToString());
            }
            return result;
        }

        // 获取表(tableName)的列(rowName)中最大编号
        public static int GetMaxIndexOfIn(string rowName, string tableName)
        {
            DataSet mDataSet = null;            //数据源
            string strSQL = "";                 //SQL语句

            //读取坐标系编号
            strSQL = "SELECT MAX(" + rowName + ") FROM " + tableName;
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            //未读取数据
            if ((null == mDataSet) || (0 == mDataSet.Tables[0].Rows.Count))
            {
                return -1;
            }

            int result;
            if (int.TryParse(mDataSet.Tables[0].Rows[0][0].ToString(), out result))
            {
                return result;
            }
            else
            {
                return -1;
            }

        }

        public static DataSet ReadDataToDataSet(string strSQL)
        {
            return m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);
        }
    }
}

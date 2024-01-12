using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Specialized;
//
using FaultLocalization;
//
using DBDll;

namespace FaultLocalization
{
    /// <summary>
    /// 数据库操作类.缺陷设置表
    /// </summary>
    public partial class FLDBServer
    {
        /// <summary>
        /// 获取一个实验对象的缺陷设置
        /// </summary>
        /// <param name="ID">实验对象ID</param>
        /// <returns>缺陷语句</returns>
        public static List<FLStaFault> ReadFaultVersionSettings(int ID)
        {
            DataSet mDataSet = null;
            //读取缺陷版本
            string strSQL = "SELECT 缺陷编号 FROM 缺陷设置表"
                         + " WHERE ID=" + ID.ToString();
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
                return null;

            List<FLStaFault> result = new List<FLStaFault>();
            for (int i = 0; i < mDataSet.Tables[0].Rows.Count; i++)
                result.Add(ReadStaFault(Convert.ToInt32(mDataSet.Tables[0].Rows[i]["缺陷编号"].ToString())));

            return result;
        }

        #region [Obsolete!!!]读取一个缺陷版本的缺陷设置
        // 读取一个缺陷版本的缺陷设置
        public static FLStaFaultVersionSetInfo ReadFaultVersionSettings(string suiteName, string programName, string versionName)
        {
            DataSet mDataSet = null;

            int ID = GetIDofVersion(suiteName, programName, versionName);
            //读取缺陷版本
            string strSQL = "SELECT 缺陷编号 FROM 缺陷设置表"
                         + " WHERE ID=" + ID.ToString();
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return null;
            }

            
            List<FLStaFault> temp= new List<FLStaFault>();
            for (int i = 0; i < mDataSet.Tables[0].Rows.Count; i++)
            {
                temp.Add(ReadStaFault(Convert.ToInt32(mDataSet.Tables[0].Rows[i]["缺陷编号"].ToString())));
            }

            FLStaFaultVersionSetInfo result = new FLStaFaultVersionSetInfo(suiteName, programName, versionName, temp);

            return result;
        }
        #endregion

        // 读取与某缺陷相关的缺陷版本号
        public static int[] ReadFaultVersionIDWithFault(string suiteName, string programName, string faultName)
        {
            int faultIndex = GetIDofFault(suiteName, programName, faultName);

            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT ID FROM 缺陷设置表"
                         + " WHERE 缺陷编号=" + faultIndex.ToString();
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return null;
            }

            int[] result = new int[mDataSet.Tables[0].Rows.Count];
            // 依次记录
            for (int IDIndex = 0; IDIndex < mDataSet.Tables[0].Rows.Count; IDIndex++)
            {
                result[IDIndex] = Convert.ToInt32(mDataSet.Tables[0].Rows[IDIndex]["ID"].ToString());
            }

            return result;
        }

        // 写入数据库一个缺陷版本的设置
        public static void InsertFaultVersionSetting(string suiteName, string programName, string versionName, List<string> faultList)
        {
            int ID = GetIDofVersion(suiteName, programName, versionName);

            for (int i = 0; i < faultList.Count; i++)
            {
                int faultIndex = GetIDofFault(suiteName, programName, faultList[i]);
                if (-1 != faultIndex)
                {
                    string sSQLString = "INSERT INTO 缺陷设置表(ID, 缺陷编号)" + "VALUES("
                                             + ID.ToString() + ","
                                             + faultIndex.ToString() + ")";
                    m_SQLServerOperation.SQLServerExecuteSQLString(sSQLString);
                }
                else
                {
                    throw new Exception("没有缺陷 - " + suiteName + "-" + programName + "-" + faultList[i] + " - 的描述");
                }
            }
        }



    }
}
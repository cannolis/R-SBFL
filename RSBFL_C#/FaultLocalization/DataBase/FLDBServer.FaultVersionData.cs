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
    /// 数据库操作类.实验对象数据表
    /// </summary>
    public partial class FLDBServer
    {
        // 获取缺陷版本ID
        public static int GetIDofVersion(string suiteName, string programName, string versionName)
        {
            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT ID FROM 实验对象数据表"
                         + " WHERE 实验包=" + "'" + suiteName + "'"
                         + " AND 目标程序=" + "'" + programName + "'"
                         + " AND 缺陷版本=" + "'" + versionName + "'";
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return -1;
            }

            int result;
            if (int.TryParse(mDataSet.Tables[0].Rows[0]["ID"].ToString(), out result))
            {
                return result;
            }
            else
            {
                return -1;
            }
        }

        // 根据缺陷数量获取缺陷版本ID列表
        public static int[] GetIDsofVersionByNumFault(string suiteName, string programName, int iNumFault)
        {
            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT ID FROM 实验对象数据表"
                         + " WHERE 实验包=" + "'" + suiteName + "'"
                         + " AND 目标程序=" + "'" + programName + "'"
                         + " AND 缺陷数量=" + iNumFault.ToString();
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return new int[0];
            }

            int[] result = new int[mDataSet.Tables[0].Rows.Count];
            for (int i = 0; i < result.Length; i++)
            {
                int item = -1;
                if (int.TryParse(mDataSet.Tables[0].Rows[i]["ID"].ToString(), out item))
                {
                    result[i] = item;
                }
                else
                {
                    result[i] = -1;
                }
            }

            return result;
        }

        // 获取缺陷版本名称
        public static FLStaFaultVersionName GetVersionNameByID(int ID)
        {
            FLStaFaultVersionName result = new FLStaFaultVersionName();
            result.suiteName = string.Empty;
            result.programName = string.Empty;
            result.versionName = string.Empty;

            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT 实验包,目标程序, 缺陷版本 FROM 实验对象数据表"
                         + " WHERE ID=" + ID.ToString();
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return result;
            }

            result.suiteName = mDataSet.Tables[0].Rows[0]["实验包"].ToString();
            result.programName = mDataSet.Tables[0].Rows[0]["目标程序"].ToString();
            result.versionName = mDataSet.Tables[0].Rows[0]["缺陷版本"].ToString();

            return result;
        }

        // 读取一个缺陷版本的测试信息
        public static FLStaFaultVersionCovInfo ReadFaultVersionData(string suiteName, string programName, string versionName)
        {
            FLStaFaultVersionCovInfo mFaultVersion = null;

            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT * FROM 实验对象数据表"
                         + " WHERE 实验包=" + "'" + suiteName + "'"
                         + " AND 目标程序=" + "'" + programName + "'"
                         + " AND 缺陷版本=" + "'" + versionName + "'";
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);            

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return null;
            }

            mFaultVersion = new FLStaFaultVersionCovInfo();
            mFaultVersion.NumRuns = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["测试用例总数"].ToString());
            mFaultVersion.NumSucRuns = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["成例数"].ToString());
            mFaultVersion.NumFalRuns = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["失例数"].ToString());
            mFaultVersion.NumStatements = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["插桩语句数"].ToString());
            mFaultVersion.NumFaults = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["缺陷数量"].ToString());  //李成龙添加

            return mFaultVersion;
        }

        // 向数据库中写入一个缺陷版本的记录
        public static void InsertFaultVersionData(string suiteName, string programName, string versionName, int iNumFault,FLStaFaultVersionCovInfo faultVersion)
        {
            try
            {
                if (-1 == GetIDofVersion(suiteName, programName, versionName))
                {
                    int ID = GetMaxIndexOfIn("ID", "实验对象数据表") + 1;

                    string sSQLString = "INSERT INTO 实验对象数据表(ID, 实验包,目标程序, 缺陷版本, 缺陷数量,测试用例总数, 成例数, 失例数, 插桩语句数)" + "VALUES("
                                     + ID.ToString() + ","
                                     + "'" + suiteName + "',"
                                     + "'" + programName + "',"
                                     + "'" + versionName + "',"
                                     + Convert.ToInt16(iNumFault).ToString() + ","
                                     + faultVersion.NumRuns.ToString() + ","
                                     + faultVersion.NumSucRuns.ToString() + ","
                                     + faultVersion.NumFalRuns.ToString() + ","
                                     + faultVersion.NumStatements.ToString() + ")";
                    m_SQLServerOperation.SQLServerExecuteSQLString(sSQLString);

                    // 矩阵数据的存放 remark
                }
                else
                {
                    UpdateFaultVersionData(suiteName, programName, versionName, iNumFault, faultVersion);
                }
            }
            catch (Exception e)
            {

            }
        }

        // 删除缺陷版本
        public static void DeleFaultVersionData(string suiteName, string programName, string versionName)
        {
            try
            {
                string strSQL = "DELETE FROM 实验对象数据表"
                              + " WHERE 实验包=" + "'" + suiteName + "'"
                              + " AND 目标程序=" + "'" + programName + "'"
                              + " AND 缺陷版本=" + "'" + versionName + "'";
                m_SQLServerOperation.SQLServerExecuteSQLString(strSQL);
            }
            catch (Exception e)
            {

            }
        }

        // 更新缺陷版本
        public static void UpdateFaultVersionData(string suiteName, string programName, string versionName, int iNumFault, FLStaFaultVersionCovInfo faultVersion)
        {
            try
            {
                string sSQLString = "UPDATE 实验对象数据表 SET "
                              + "缺陷数量=" + Convert.ToInt16(iNumFault).ToString() + ","
                              + "测试用例总数=" + faultVersion.NumRuns.ToString() + ","
                              + "成例数=" + faultVersion.NumSucRuns.ToString() + ","
                              + "失例数=" + faultVersion.NumFalRuns.ToString() + ","
                              + "插桩语句数=" + faultVersion.NumStatements.ToString()
                              + " WHERE 实验包=" + "'" + suiteName + "'"
                              + " AND 目标程序=" + "'" + programName + "'"
                              + " AND 缺陷版本=" + "'" + versionName + "'";

                m_SQLServerOperation.SQLServerExecuteSQLString(sSQLString);
            }
            catch (Exception e)
            {

            }
        }

    }
}

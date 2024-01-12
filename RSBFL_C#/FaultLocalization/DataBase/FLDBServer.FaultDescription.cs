/*************************************************************************
 * 
 *      SPDBServer.FaultyStatement
 * 
 *      通过删除所有缺陷语句然后添加所有缺陷语句来更新
 * 
 * ***********************************************************************/
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
    /// 数据库操作类.缺陷描述表
    /// </summary>
    public partial class FLDBServer
    {

        // 获取一个缺陷
        public static FLStaFault ReadStaFault(int faultIndex)
        {
            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT * FROM 缺陷描述表"
                         + " WHERE 缺陷编号=" + faultIndex.ToString();
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return null;
            }

            FLStaFault result = new FLStaFault();
            result.FaultName = mDataSet.Tables[0].Rows[0]["缺陷名称"].ToString();
            result.Description = mDataSet.Tables[0].Rows[0]["描述"].ToString();
            result.FaultyStatements = GetFaultyStatements(faultIndex);

            return result;
        }

        // 向数据库中写入一个缺陷
        public static void InsertFaultof(string suiteName, string programName, FLStaFault fault)
        {
            int ID = GetIDofFault(suiteName, programName, fault.FaultName);
            // 如果没有
            if (-1 == ID)
            {
                ID = GetMaxIndexOfIn("缺陷编号", "缺陷描述表") + 1;
                string sSQLString = "INSERT INTO 缺陷描述表(缺陷编号, 实验包, 目标程序, 缺陷名称, 描述)" + "VALUES("
                      + ID.ToString() + ","
                      + "'" + suiteName + "',"
                      + "'" + programName + "',"
                      + "'" + fault.FaultName + "',"
                      + "'" + fault.Description + "')";
                m_SQLServerOperation.SQLServerExecuteSQLString(sSQLString);

                // 写缺陷语句
                for (int i = 0; i < fault.FaultyStatements.Count; i++)
                {
                    InsertFaultyStatement(ID, fault.FaultyStatements[i]);
                }
            }
            else
            {
                UpdateFault(ID ,suiteName, programName, fault);
            }
        }

        // 删除一个缺陷
        public static void DeleFault(string suiteName, string programName, FLStaFault fault)
        {
            int ID = GetIDofFault(suiteName, programName, fault.FaultName);
            DeleFault(ID);
        }

        // 删除一个缺陷
        public static void DeleFault(int ID)
        {
            try
            {
                string strSQL = "DELETE FROM 缺陷描述表"
                              + " WHERE 缺陷编号=" + ID.ToString();
                m_SQLServerOperation.SQLServerExecuteSQLString(strSQL);
            }
            catch (Exception e)
            {

            }
        }

        // 更新缺陷版本的缺陷语句
        public static void UpdateFault(int ID, string suiteName, string programName, FLStaFault fault)
        {
            try
            {
                string sSQLString = "UPDATE 缺陷描述表 SET "
                                  + "实验包=" + "'" + suiteName + "',"
                                  + "目标程序=" + "'" + programName + "',"
                                  + "缺陷名称=" + "'" + fault.FaultName + "',"
                                  + "描述=" + "'" + fault.Description + "'"
                                  + " WHERE 缺陷编号=" + ID.ToString();
                m_SQLServerOperation.SQLServerExecuteSQLString(sSQLString);

                // 删除所有缺陷语句
                DeleFaulyStatementsof(ID);
                // 写缺陷语句
                for (int i = 0; i < fault.FaultyStatements.Count; i++)
                {
                    InsertFaultyStatement(ID, fault.FaultyStatements[i]);
                }
            }
            catch (Exception e)
            {

            }
        }

        // 获取缺陷编号
        public static int GetIDofFault(string suiteName, string programName, string faultName)
        {
            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT 缺陷编号 FROM 缺陷描述表"
                         + " WHERE 实验包=" + "'" + suiteName + "'"
                         + " AND 目标程序=" + "'" + programName + "'"
                         + " AND 缺陷名称=" + "'" + faultName + "'";
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return -1;
            }

            int result;
            if (int.TryParse(mDataSet.Tables[0].Rows[0]["缺陷编号"].ToString(), out result))
            {
                return result;
            }
            else
            {
                return -1;
            }
        }

    }
}

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
    /// 数据库操作类.缺陷桩点对照表
    /// </summary>
    public partial class FLDBServer
    {
        // 获取缺陷的缺陷语句
        public static List<FLStatement> GetFaultyStatements(int ID)
        {
            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT 缺陷语句行 FROM 缺陷桩点对照表"
                         + " WHERE 缺陷编号=" + ID.ToString();
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return null;
            }

            List<FLStatement> result = new List<FLStatement>();
            for (int i = 0; i < mDataSet.Tables[0].Rows.Count; i++)
            {
                FLStatement temp = new FLStatement();
                temp.LineNumber = Convert.ToInt32(mDataSet.Tables[0].Rows[i]["缺陷语句行"].ToString());
                result.Add(temp);
            }

            return result;
        }

        // 向数据库中的编号为ID的缺陷写入一个缺陷语句faultyStatement
        public static void InsertFaultyStatement(int ID, FLStatement faultyStatement)
        {
            string sSQLString = "INSERT INTO 缺陷桩点对照表(缺陷编号, 缺陷语句行)" + "VALUES("
                  + ID.ToString() + ","
                  + faultyStatement.LineNumber.ToString() + ")";
            m_SQLServerOperation.SQLServerExecuteSQLString(sSQLString);
        }

        // 删除缺陷版本的所有语句
        public static void DeleFaulyStatementsof(int ID)
        {
            string strSQL = "DELETE FROM 缺陷桩点对照表"
                + " WHERE 缺陷编号=" + ID.ToString();
            m_SQLServerOperation.SQLServerExecuteSQLString(strSQL);
        }

        // 删除缺陷版本中的指定语句
        public static void DeleFaultyStatement(int ID, FLStatement faultyStatement)
        {
            try
            {
                string strSQL = "DELETE FROM 缺陷桩点对照表"
                              + " WHERE 缺陷编号=" + ID.ToString()
                              + " AND 缺陷语句行=" + faultyStatement.LineNumber.ToString();
                m_SQLServerOperation.SQLServerExecuteSQLString(strSQL);
            }
            catch (Exception e)
            {

            }
        }

        // 更新缺陷版本的缺陷语句
        public static void UpdateFaultyStatement(int ID, FLStatement faultyStatement)
        {
            try
            {
                DeleFaultyStatement(ID, faultyStatement);
                InsertFaultyStatement(ID, faultyStatement);
            }
            catch (Exception e)
            {

            }
        }

    }
}

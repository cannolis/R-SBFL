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
    /// 数据库操作类.实验结果表
    /// </summary>
    public partial class FLDBServer
    {
        /// <summary>
        /// 读取一个缺陷版本的定位效果
        /// </summary>
        /// <param name="suiteName">实验包</param>
        /// <param name="programName">目标程序</param>
        /// <param name="versionName">版本</param>
        /// <returns>缺陷版本</returns>
        public static FLStaLocationEffort ReadLocationEffortofVersion(int ID, string method, string description)
        {
            FLStaLocationEffort result = null;

            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT 实验描述, 最优排位, 平均排位, 最次排位, 绝对排位, 最优排位下的expense, 平均排位下的expense, 最次排位下的expense, 绝对排位下的expense FROM 实验结果表"
                         + " WHERE ID=" + ID.ToString()
                         + " AND 算法=" + "'" + method + "'"
                         + " AND 实验描述=" + "'" + description + "'";
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return null;
            }

            result = new FLStaLocationEffort();
            result.AlgorithmName = method;
            result.ExperimentDiscription = description;
            result.BestSort = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["最优排位"].ToString());
            result.WorstSort = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["最次排位"].ToString());
            result.AveSort = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["平均排位"].ToString());
            result.AbsSort = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["绝对排位"].ToString());

            result.LeastExpense = Convert.ToDouble(mDataSet.Tables[0].Rows[0]["最优排位下的expense"].ToString());
            result.MostExpense = Convert.ToDouble(mDataSet.Tables[0].Rows[0]["最次排位下的expense"].ToString());
            result.AveExpense = Convert.ToDouble(mDataSet.Tables[0].Rows[0]["平均排位下的expense"].ToString());
            result.AbsExpense = Convert.ToDouble(mDataSet.Tables[0].Rows[0]["绝对排位下的expense"].ToString());

            return result;
        }

        /// <summary>
        /// 向数据库中写入一个缺陷版本的定位效果
        /// </summary>
        /// <param name="faultVersion">缺陷版本</param>
        public static void InsertLocationEffortofVersion(int ID, FLStaLocationEffort locationEffort)
        {
            FLStaLocationEffort temp = ReadLocationEffortofVersion(ID, locationEffort.AlgorithmName, locationEffort.ExperimentDiscription);

            if (null == temp)
            {
                string sSQLString = "INSERT INTO 实验结果表(ID,算法,实验描述,最优排位,平均排位,最次排位,绝对排位,最优排位下的expense,平均排位下的expense,最次排位下的expense,绝对排位下的expense)" + "VALUES("
                                 + ID.ToString() + ","
                                 + "'" + locationEffort.AlgorithmName + "',"
                                 + "'" + locationEffort.ExperimentDiscription + "',"
                                 + locationEffort.BestSort.ToString() + ","
                                 + locationEffort.AveSort.ToString() + ","
                                 + locationEffort.WorstSort.ToString() + ","
                                 + locationEffort.AbsSort.ToString() + ","
                                 + locationEffort.LeastExpense.ToString() + ","
                                 + locationEffort.AveExpense.ToString() + ","
                                 + locationEffort.MostExpense.ToString() + ","
                                 + locationEffort.AbsExpense.ToString() + ")";
                //string testing = "INSERT INTO 实验结果表(ID,算法,实验描述,最优排位,平均排位,最次排位,绝对排位,最优排位下的expense,平均排位下的expense,最次排位下的expense,绝对排位下的expense)VALUES(4058,'Op1','变更用例0.2000_NumSExpSort.0',1482,2607,3731,2883,0.397212543554007,0.698740284106138,1,0.772715089788261)";
                m_SQLServerOperation.SQLServerExecuteSQLString(sSQLString);
            }
            else
            {
                UpdateLocationEffortofVersion(ID, locationEffort);
            }
        }

        // 删除一个缺陷版本的所有结果
        public static void DeleLocationEffortofVersion(int ID, string description)
        {
            try
            {
                string strSQL = "DELETE FROM 实验结果表"
                              + " WHERE ID=" + ID.ToString()
                              + " AND 实验描述=" + "'" + description + "'";
                m_SQLServerOperation.SQLServerExecuteSQLString(strSQL);
            }
            catch (Exception e)
            {

            }
        }

        // 删除一个缺陷版本在某算法和策略下的定位效果
        public static void DeleLocationEffortofVersion(int ID, string algorithmName, string description)
        {
            try
            {
                string strSQL = "DELETE FROM 实验结果表"
                              + " WHERE ID=" + ID.ToString()
                              + " AND 算法=" + "'" + algorithmName + "'"
                              + " AND 实验描述=" + "'" + description + "'";
                m_SQLServerOperation.SQLServerExecuteSQLString(strSQL);
            }
            catch (Exception e)
            {

            }
        }

        // 删除一个与某个缺陷相关的缺陷版本的实验结果
        public static void DeleLocationEffortofFault(int faultIndex, string description)
        {
            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT ID FROM 缺陷设置表"
                         + " WHERE 缺陷编号=" + faultIndex.ToString();
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return;
            }

            // 依次删除
            for (int IDIndex = 0; IDIndex < mDataSet.Tables[0].Rows.Count; IDIndex++)
            {
                int ID = Convert.ToInt32(mDataSet.Tables[0].Rows[IDIndex]["ID"].ToString());
                DeleLocationEffortofVersion(ID, description);
            }
        }

        // 删除一个与某缺陷相关的缺陷版本的实验结果
        public static void DeleLocationEffortofFault(int faultIndex, string algorithmName, string description)
        {
            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT ID FROM 缺陷设置表"
                         + " WHERE 缺陷编号=" + faultIndex.ToString();
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return;
            }

            // 依次删除
            for (int IDIndex = 0; IDIndex < mDataSet.Tables[0].Rows.Count; IDIndex++)
            {
                int ID = Convert.ToInt32(mDataSet.Tables[0].Rows[IDIndex]["ID"].ToString());
                DeleLocationEffortofVersion(ID, algorithmName, description);
            }
        }

        /// <summary>
        /// 更新一个缺陷版本的定位效果
        /// </summary>
        /// <param name="faultVersion">缺陷版本</param>
        public static void UpdateLocationEffortofVersion(int ID, FLStaLocationEffort locationEffort)
        {
            try
            {
                DeleLocationEffortofVersion(ID, locationEffort.AlgorithmName,locationEffort.ExperimentDiscription);
                InsertLocationEffortofVersion(ID, locationEffort);
            }
            catch (Exception e)
            {

            }
        }

    }
}

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
    /// 数据库操作类.随机试验统计结果表
    /// </summary>
    public partial class FLDBServer
    {
        /// <summary>
        /// 读取一个缺陷版本的统计结果
        /// </summary>
        /// <param name="suiteName">实验包</param>
        /// <param name="programName">目标程序</param>
        /// <param name="versionName">版本</param>
        /// <returns>缺陷版本</returns>
        public static FLStaLocationEffortStatic ReadStatisticLocationEffortofVersion(int ID, string method, string description)
        {
            FLStaLocationEffortStatic result = null;

            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT 最优排位下的expense, 平均排位下的expense, 最次排位下的expense, 绝对排位下的expense,"
                         + "最优排位下的expense方差, 平均排位下的expense方差, 最次排位下的expense方差, 绝对排位下的expense方差"
                         + " FROM 随机试验统计结果表"
                         + " WHERE ID=" + ID.ToString()
                         + " AND 算法=" + "'" + method + "'"
                         + " AND 实验描述=" + "'" + description + "'";
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return null;
            }

            result = new FLStaLocationEffortStatic();
            result.AlgorithmName = method;
            result.ExperimentDiscription = description;

            result.LeastExpense = Convert.ToDouble(mDataSet.Tables[0].Rows[0]["最优排位下的expense方差"].ToString());
            result.MostExpense = Convert.ToDouble(mDataSet.Tables[0].Rows[0]["最次排位下的expense方差"].ToString());
            result.AveExpense = Convert.ToDouble(mDataSet.Tables[0].Rows[0]["平均排位下的expense方差"].ToString());
            result.AbsExpense = Convert.ToDouble(mDataSet.Tables[0].Rows[0]["绝对排位下的expense方差"].ToString());

            result.LeastExpenseVariance = Convert.ToDouble(mDataSet.Tables[0].Rows[0]["最优排位下的expense方差"].ToString());
            result.MostExpenseVariance = Convert.ToDouble(mDataSet.Tables[0].Rows[0]["最次排位下的expense方差"].ToString());
            result.AveExpenseVariance = Convert.ToDouble(mDataSet.Tables[0].Rows[0]["平均排位下的expense方差"].ToString());
            result.AbsExpenseVariance = Convert.ToDouble(mDataSet.Tables[0].Rows[0]["绝对排位下的expense方差"].ToString());


            return result;
        }

        /// <summary>
        /// 向数据库中写入一个缺陷版本的定位效果
        /// </summary>
        /// <param name="faultVersion">缺陷版本</param>
        public static void InsertStatisticLocationEffortofVersion(int ID, FLStaLocationEffortStatic locationEffort)
        {
            FLStaLocationEffortStatic temp = ReadStatisticLocationEffortofVersion(ID, locationEffort.AlgorithmName, locationEffort.ExperimentDiscription);

            if (null == temp)
            {
                string sSQLString = "INSERT INTO 随机试验统计结果表(ID,算法, 实验描述,最优排位下的expense,平均排位下的expense,最次排位下的expense, 绝对排位下的expense,最优排位下的expense方差, 平均排位下的expense方差, 最次排位下的expense方差, 绝对排位下的expense方差)" + "VALUES("
                                 + ID.ToString() + ","
                                 + "'" + locationEffort.AlgorithmName + "',"
                                 + "'" + locationEffort.ExperimentDiscription + "',"
                                 + locationEffort.LeastExpense.ToString() + ","
                                 + locationEffort.AveExpense.ToString() + ","
                                 + locationEffort.MostExpense.ToString() + ","
                                 + locationEffort.AbsExpense.ToString() + ","
                                 + locationEffort.LeastExpenseVariance.ToString() + ","
                                 + locationEffort.AveExpenseVariance.ToString() + ","
                                 + locationEffort.MostExpenseVariance.ToString() + ","
                                 + locationEffort.AbsExpenseVariance.ToString() + ")";

                m_SQLServerOperation.SQLServerExecuteSQLString(sSQLString);
            }
            else
            {
                UpdateStatisticLocationEffortofVersion(ID, locationEffort);
            }
        }

        // 删除一个缺陷版本的所有结果
        public static void DeleStatisticLocationEffortofVersion(int ID, string description)
        {
            try
            {
                string strSQL = "DELETE FROM 随机试验统计结果表"
                              + " WHERE ID=" + ID.ToString()
                              + " AND 实验描述=" + "'" + description + "'";
                m_SQLServerOperation.SQLServerExecuteSQLString(strSQL);
            }
            catch (Exception e)
            {

            }
        }

        // 删除一个缺陷版本在某算法和策略下的定位效果
        public static void DeleStatisticLocationEffortofVersion(int ID, string algorithmName, string description)
        {
            try
            {
                string strSQL = "DELETE FROM 随机试验统计结果表"
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
        public static void DeleStatisticLocationEffortofFault(int faultIndex, string description)
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
        public static void DeleStatisticLocationEffortofFault(int faultIndex, string algorithmName, string description)
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
        public static void UpdateStatisticLocationEffortofVersion(int ID, FLStaLocationEffortStatic locationEffort)
        {
            try
            {
                DeleStatisticLocationEffortofVersion(ID, locationEffort.AlgorithmName, locationEffort.ExperimentDiscription);
                InsertStatisticLocationEffortofVersion(ID, locationEffort);
            }
            catch (Exception e)
            {

            }
        }

    }
}

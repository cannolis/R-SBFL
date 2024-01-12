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
    /// 数据库操作类.测试用例分组表
    /// </summary>
    public partial class FLDBServer
    {
        /// <summary>
        /// 读取一个缺陷版本在某个设置下的用例拆分
        /// </summary>
        /// <param name="ID">缺陷版本</param>
        /// <param name="cfg">分组策略描述</param>
        /// <param name="itimes">实验次数</param>
        /// <returns>result[0]:suc result[1]:fal</returns>
        public static List<int[]>[] ReadTestCaseDivinfo(int ID, FLConfigure cfg, int itimes)  //李成龙改  不变更用例的测试用例分组
        {
            List<int[]>[] result = null;

            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT 分组数量,成例分组记录,失例分组记录 FROM 测试用例分组表"
                         + " WHERE ID=" + ID.ToString()
                         + " AND 分组策略描述=" + "'" + cfg.ClassRatioDivideStrategy + "'"
                         + " AND 类别比例=" + cfg.ClassRatio.ToString()
                         + " AND 实验次数=" + itimes.ToString();
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
            {
                return null;
            }

            int num = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["分组数量"].ToString());
            result = new List<int[]>[2];
            result[0] = new List<int[]>();
            result[1] = new List<int[]>();
            string sucStr = mDataSet.Tables[0].Rows[0]["成例分组记录"].ToString();
            string falStr = mDataSet.Tables[0].Rows[0]["失例分组记录"].ToString();

            string[] sucStrLists = sucStr.Split(';');
            string[] falStrLists = falStr.Split(';');
            for (int i = 0; i < num; i++)
            {
                string[] sucStrList = sucStrLists[i].Split(',');
                string[] falStrList = falStrLists[i].Split(',');

                int[] sucIntList = new int[sucStrList.Length];
                int[] falIntList = new int[falStrList.Length];

                for (int j = 0; j < sucStrList.Length; j++)
                {
                    sucIntList[j] = Convert.ToInt32(sucStrList[j]);
                }
                result[0].Add(sucIntList);

                for (int j = 0; j < falStrList.Length; j++)
                {
                    falIntList[j] = Convert.ToInt32(falStrList[j]);
                }
                result[1].Add(falIntList);
            }

            return result;
        }

        /// <summary>
        /// 向数据库中写入一个缺陷版本的定位效果
        /// </summary>
        /// <param name="faultVersion">缺陷版本</param>
        public static void InsertTestCaseDivinfo(int ID, FLConfigure cfg, int itimes, List<FLRunsGroupInfo> groups)
        {
            List<int[]>[] temp = ReadTestCaseDivinfo(ID, cfg, itimes);
            if (null != temp)
                UpdateTestCaseDivinfo(ID, cfg, itimes, groups);
            else  // 李成龙改
            {
                int num = groups.Count;
                StringBuilder sucLists = new StringBuilder();
                StringBuilder falLists = new StringBuilder();
                for (int i = 0; i < num; i++)
                {
                    int[] sucCaseIDs = groups[i].SucCaseIDs;
                    sucLists.Append(sucCaseIDs[0].ToString());
                    for (int j = 1; j < sucCaseIDs.Length; j++)
                        sucLists.Append("," + sucCaseIDs[j].ToString());
                    sucLists.Append(";");

                    int[] falCaseIDs = groups[i].FalCaseIDs;
                    falLists.Append(falCaseIDs[0].ToString());
                    for (int j = 1; j < falCaseIDs.Length; j++)
                        falLists.Append("," + falCaseIDs[j].ToString());
                    falLists.Append(";");
                }
                string sSQLString = "INSERT INTO 测试用例分组表(ID,分组策略描述, 类别比例, 实验次数,分组数量, 成例分组记录, 失例分组记录)" + "VALUES("
                                    + ID.ToString() + ","
                                    + "'" + cfg.ClassRatioDivideStrategy + "',"
                                    + cfg.ClassRatio.ToString() + ","
                                    + itimes.ToString() + ","
                                    + num.ToString() + ","
                                    + "'" + sucLists.ToString() + "',"
                                    + "'" + falLists.ToString() + "')";

                m_SQLServerOperation.SQLServerExecuteSQLString(sSQLString);
            }
        }

        public static void DeleTestCaseDivinfo(int ID, FLConfigure cfg, int itimes)
        {
            try
            {
                string strSQL = "DELETE FROM 测试用例分组表"
                              + " WHERE ID=" + ID.ToString()
                              + " AND 分组策略描述=" + "'" + cfg.ClassRatioDivideStrategy + "'"
                              + " AND 类别比例=" + cfg.ClassRatio.ToString()
                              + " AND 实验次数=" + itimes.ToString();
                m_SQLServerOperation.SQLServerExecuteSQLString(strSQL);
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// 更新一个缺陷版本的定位效果
        /// </summary>
        /// <param name="faultVersion">缺陷版本</param>
        public static void UpdateTestCaseDivinfo(int ID, FLConfigure cfg, int itimes, List<FLRunsGroupInfo> groups)
        {
            try
            {
                DeleTestCaseDivinfo(ID, cfg, itimes);
                InsertTestCaseDivinfo(ID, cfg, itimes, groups);
            }
            catch (Exception e)
            {

            }
        }
    }
}

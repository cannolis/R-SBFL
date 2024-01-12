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
    /// 数据库操作类.随机选取用例表
    /// </summary>
    public partial class FLDBServer
    {
        /// <summary>
        /// 从数据库中获取随机选取用例信息
        /// </summary>
        /// <param name="ID">缺陷版本</param>
        /// <param name="description">用例选取策略描述</param>
        /// <param name="itimes">实验次数</param>
        /// <returns>result[0]:suc result[1]:fal</returns>
        public static List<int>[] ReadTestCaseSelectInfo(int ID, string description, int itimes)
        {
            List<int>[] result = null;

            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT 选取用例总数, 选取成例数, 选取失例数, 成例选取记录, 失例选取记录 FROM 随机选取用例表"
                         + " WHERE ID=" + ID.ToString()
                         + " AND 用例选取策略描述=" + "'" + description + "'"
                         + " AND 实验次数=" + itimes.ToString();
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
                return null;

            int allNum = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["选取用例总数"].ToString());
            int sucNum = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["选取成例数"].ToString());
            int falNum = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["选取失例数"].ToString());
            result = new List<int>[2];
            result[0] = new List<int>();
            result[1] = new List<int>();
            string sucStr = mDataSet.Tables[0].Rows[0]["成例选取记录"].ToString();
            string falStr = mDataSet.Tables[0].Rows[0]["失例选取记录"].ToString();

            string[] sucStrList = sucStr.Split(',');
            string[] falStrList = falStr.Split(',');

            for (int i = 0; i < sucStrList.Length; i++)
                result[0].Add(Convert.ToInt32(sucStrList[i]));
            for (int j = 0; j < falStrList.Length; j++)
                result[1].Add(Convert.ToInt32(falStrList[j]));

            return result;
        }

        /// <summary>
        /// 从数据库中获取随机变更后类别信息的拆分结果
        /// </summary>
        /// <param name="ID">缺陷版本</param>
        /// <param name="cfg">实验配置</param>
        /// <returns>result[0]:suc result[1]:fal</returns>
        public static List<int[]>[] ReadTestCaseSelectDivInfo(int ID, FLConfigure cfg, int stimes, int dtimes)
        {
            List<int[]>[] result = null;

            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT 分组数量,成例分组记录,失例分组记录 FROM 随机选取用例_集成实验分组表"
                         + " WHERE ID=" + ID.ToString()
                         + " AND 选取用例选取策略描述=" + "'" + cfg.TestCaseSelectStrategy + "'"
                         + " AND 选取用例实验次数=" + stimes.ToString()
                         + " AND 分组策略描述=" + "'" + cfg.ClassRatioDivideStrategy + "'"
                         + " AND 类别比例=" + cfg.ClassRatio.ToString()
                         + " AND 实验次数=" + dtimes.ToString();
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
        /// 向数据库中插入一次用例选取信息
        /// </summary>
        /// <param name="ID">缺陷版本</param>
        /// <param name="description">用例选取策略描述</param>
        /// <param name="itimes">实验次数</param>
        /// <param name="group">用例组</param>
        public static void InsertTestCaseSelectInfo(int ID, string description, int itimes, FLRunsGroupInfo group)
        {
            List<int>[] temp = ReadTestCaseSelectInfo(ID, description, itimes);
            if (null != temp)
                UpdateTestCasSelectInfo(ID, description, itimes, group);
            else
            {
                StringBuilder sucList = new StringBuilder();
                StringBuilder falList = new StringBuilder();

                int[] sucCaseIDs = group.SucCaseIDs;
                sucList.Append(sucCaseIDs[0].ToString());
                for (int i = 1; i < sucCaseIDs.Length; i++)
                    sucList.Append("," + sucCaseIDs[i].ToString());

                int[] falCaseIDs = group.FalCaseIDs;
                falList.Append(falCaseIDs[0].ToString());
                for (int i = 1; i < falCaseIDs.Length; i++)
                    falList.Append("," + falCaseIDs[i].ToString());

                string sSQLString = "INSERT INTO 随机选取用例表(ID,用例选取策略描述, 实验次数, 选取用例总数, 选取成例数, 选取失例数, 成例选取记录, 失例选取记录)" + "VALUES("
                                 + ID.ToString() + ","
                                 + "'" + description + "',"
                                 + itimes.ToString() + ","
                                 + (sucCaseIDs.Length + falCaseIDs.Length).ToString() + ","
                                 + sucCaseIDs.Length.ToString() + ','
                                 + falCaseIDs.Length.ToString() + ','
                                 + "'" + sucList.ToString() + "',"
                                 + "'" + falList.ToString() + "')";
                m_SQLServerOperation.SQLServerExecuteSQLString(sSQLString);
            }
        }

        /// <summary>
        /// 向数据库中插入一次随机变更用例类别以及用例分组信息
        /// </summary>
        /// <param name="ID">缺陷版本</param>
        /// <param name="cfg">实验配置</param>
        /// <param name="itimes">分组实验次数</param>
        /// <param name="groups">分组</param>
        public static void InsertTestCaseSelectDivInfo(int ID, FLConfigure cfg, int stimes, int dtimes, List<FLRunsGroupInfo> groups)
        {
            List<int[]>[] temp = ReadTestCaseSelectDivInfo(ID, cfg, stimes, dtimes);
            if (null != temp)
                UpdateTestCasSelectDivInfo(ID, cfg, stimes, dtimes, groups);
            else
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
                string sSQLString = "INSERT INTO 随机选取用例_集成实验分组表(ID, 选取用例选取策略描述, 选取用例实验次数, 分组策略描述, 类别比例, 实验次数, 分组数量, 成例分组记录, 失例分组记录)" + "VALUES("
                     + ID.ToString() + ","
                     + "'" + cfg.TestCaseSelectStrategy + "',"
                     + stimes.ToString() + ","
                     + "'" + cfg.ClassRatioDivideStrategy + "',"
                     + cfg.ClassRatio.ToString() + ","
                     + dtimes.ToString() + ","
                     + num.ToString() + ","
                     + "'" + sucLists.ToString() + "',"
                     + "'" + falLists.ToString() + "')";

                m_SQLServerOperation.SQLServerExecuteSQLString(sSQLString);

            }
        }
        
        /// <summary>
        /// 从数据库中删除一次用例选取信息
        /// </summary>
        /// <param name="ID">缺陷版本</param>
        /// <param name="description">用例选取策略描述</param>
        /// <param name="itimes">实验次数</param>
        public static void DeleTestCaseSelectInfo(int ID, string description, int itimes)
        {
            try
            {
                string strSQL = "DELETE FROM 随机选取用例表"
                              + " WHERE ID=" + ID.ToString()
                              + " AND 用例选取策略描述=" + "'" + description + "'"
                              + " AND 实验次数=" + itimes.ToString();
                m_SQLServerOperation.SQLServerExecuteSQLString(strSQL);
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// 从数据库中删除一次用例选取以及分组信息
        /// </summary>
        /// <param name="ID">缺陷版本</param>
        /// <param name="cfg">实验配置</param>
        /// <param name="itimes">分组实验次数</param>
        public static void DeleTestCaseSelectDivinfo(int ID, FLConfigure cfg, int stimes, int dtimes)
        {
            try
            {
                string strSQL = "DELETE FROM 随机选取用例_集成实验分组表"
                         + " WHERE ID=" + ID.ToString()
                         + " AND 选取用例选取策略描述=" + "'" + cfg.TestCaseSelectStrategy + "'"
                         + " AND 选取用例实验次数=" + stimes.ToString()
                         + " AND 分组策略描述=" + "'" + cfg.ClassRatioDivideStrategy + "'"
                         + " AND 类别比例=" + cfg.ClassRatio.ToString()
                         + " AND 实验次数=" + dtimes.ToString();
                m_SQLServerOperation.SQLServerExecuteSQLString(strSQL);
            }
            catch (Exception e)
            {

            }
        }
        
        /// <summary>
        /// 更新数据库中一次用例选取信息
        /// </summary>
        /// <param name="ID">缺陷版本</param>
        /// <param name="description">用例选取策略描述</param>
        /// <param name="itimes">实验次数</param>
        /// <param name="group">用例组</param>
        private static void UpdateTestCasSelectInfo(int ID, string description, int itimes, FLRunsGroupInfo group)
        {
            try
            {
                DeleTestCaseSelectInfo(ID, description, itimes);
                InsertTestCaseSelectInfo(ID, description, itimes, group);
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// 更新数据库中删除一次用例选取以及分组信息
        /// </summary>
        /// <param name="ID">缺陷版本</param>
        /// <param name="cfg">实验配置</param>
        /// <param name="itimes">分组实验次数</param>
        /// <param name="groups">分组</param>
        private static void UpdateTestCasSelectDivInfo(int ID, FLConfigure cfg, int stimes, int dtimes, List<FLRunsGroupInfo> groups)
        {
            try
            {
                DeleTestCaseSelectDivinfo(ID, cfg, stimes, dtimes);
                InsertTestCaseSelectDivInfo(ID, cfg, stimes, dtimes, groups);
            }
            catch (Exception e)
            {

            }
        }

        
    }
}
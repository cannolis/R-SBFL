/*************************************************************************
 * 
 *      FLDBServer.CovMatrixSelect
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
using FaultLocalization.Basic;

namespace FaultLocalization
{
    /// <summary>
    /// 数据库操作类.覆盖矩阵表  //李成龙添加
    /// </summary>
    public partial class FLDBServer
    {
        /// <summary>
        /// 从数据库中获取覆盖矩阵信息
        /// </summary>
        /// <param name="ID">缺陷版本</param>
        /// <param name="description">用例选取策略描述</param>
        /// <param name="itimes">实验次数</param>
        /// <returns>covMatrix</returns>
        public static FLBoolCovMatrix ReadCovMatrixInfo(int ID, string description, int itimes)
        {
            List<bool[]>[] result = null;

            DataSet mDataSet = null;

            //读取缺陷版本
            string strSQL = "SELECT 成例数量, 失例数量, 成例矩阵,失例矩阵 FROM 覆盖矩阵表"
                         + " WHERE ID=" + ID.ToString()
                         + " AND 用例选取策略描述=" + "'" + description + "'"
                         + " AND 实验次数=" + itimes.ToString();
            mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
                return null;

            int sucNum = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["成例数量"].ToString());
            int falNum = Convert.ToInt32(mDataSet.Tables[0].Rows[0]["失例数量"].ToString());
            result = new List<bool[]>[2];
            result[0] = new List<bool[]>();
            result[1] = new List<bool[]>();
            string rawSucStr = mDataSet.Tables[0].Rows[0]["成例矩阵"].ToString();
            string rawFalStr = mDataSet.Tables[0].Rows[0]["失例矩阵"].ToString();

            // Gzip解压
            string sucStr = Gzip.GetDatasetByString(rawSucStr);
            string falStr = Gzip.GetDatasetByString(rawFalStr);

            string[] sucRuns = sucStr.Split(';');
            string[] falRuns = falStr.Split(';');
            if ("" != sucStr)
            {
                for (int i = 0; i < sucRuns.Length - 1; i++)
                {
                    string sucStrList = sucRuns[i];
                    bool[] data = sucStrList.Select(c => c == '1').ToArray();
                    result[0].Add(data);
                }
            }
            if ("" != falStr)
            {
                for (int i = 0; i < falRuns.Length - 1; i++)
                {
                    string falStrList = falRuns[i];
                    bool[] data = falStrList.Select(c => c == '1').ToArray();
                    result[1].Add(data);
                }
            }

            FLBoolCovMatrix covMatrix = new FLBoolCovMatrix(result);
            return covMatrix;
        }

        /// <summary>
        /// 向数据库中写入一个序列化后的覆盖矩阵
        /// </summary>
        /// <param name="ID">缺陷版本</param>
        /// <param name="description">用例选取策略描述</param>
        /// <param name="itimes">实验次数</param>
        /// <param name="covMatrix">覆盖矩阵</param>
        public static void InsertCovMatrixInfo(int ID, string description, int itimes, FLBoolCovMatrix covMatrix)
        {
            FLBoolCovMatrix temp = ReadCovMatrixInfo(ID, description, itimes);
            if (null != temp)
                UpdateCovMatrixInfo(ID, description, itimes, covMatrix);
            else 
            {
                StringBuilder sucList = new StringBuilder();
                StringBuilder falList = new StringBuilder();

                if (0 < covMatrix.NumSucRuns)
                {
                    for (int i = 0; i < covMatrix.NumSucRuns; i++)
                    {
                        bool[] data = covMatrix.SucCoverageMetrix[i];
                        for (int j = 0; j < data.Length; j++)
                        {
                            sucList.Append(Convert.ToInt32(data[j]).ToString());
                        }
                        sucList.Append(";");
                    }
                }

                if (0 < covMatrix.NumFalRuns)
                {
                    for (int i = 0; i < covMatrix.NumFalRuns; i++)
                    {
                        bool[] data = covMatrix.FalCoverageMetrix[i];
                        for (int j = 0; j < data.Length; j++)
                        {
                            falList.Append(Convert.ToInt32(data[j]).ToString());
                        }
                        falList.Append(";");
                    }
                }

                // Gzip压缩字符串
                string sucStr = sucList.ToString();
                string falStr = falList.ToString();
                var rawSucStr = Gzip.GetStringByDataset(sucStr);
                var rawFalStr = Gzip.GetStringByDataset(falStr);

                string sSQLString = "INSERT INTO 覆盖矩阵表(ID,用例选取策略描述,实验次数,成例数量,失例数量,成例矩阵,失例矩阵)" + "VALUES("
                                + ID.ToString() + ","
                                + "'" + description + "',"
                                + itimes.ToString() + ","
                                + covMatrix.NumSucRuns.ToString() + ','
                                + covMatrix.NumFalRuns.ToString() + ','
                                + "'" + rawSucStr + "',"
                                + "'" + rawFalStr + "')";

                m_SQLServerOperation.SQLServerExecuteSQLString(sSQLString);
            }
        }

        /// <summary>
        /// 从数据库中删除一个覆盖矩阵
        /// </summary>
        /// <param name="ID">缺陷版本</param>
        /// <param name="description">用例选取策略描述</param>
        /// <param name="itimes">实验次数</param>
        public static void DeleCovMatrixInfo(int ID, string description, int itimes)
        {
            try
            {
                string strSQL = "DELETE FROM 覆盖矩阵表"
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
        /// 更新数据库中一个覆盖矩阵
        /// </summary>
        /// <param name="ID">缺陷版本</param>
        /// <param name="description">用例选取策略描述</param>
        /// <param name="itimes">实验次数</param>
        /// <param name="covMatrix">覆盖矩阵</param>
        private static void UpdateCovMatrixInfo(int ID, string description, int itimes, FLBoolCovMatrix covMatrix)
        {
            try
            {
                DeleCovMatrixInfo(ID, description, itimes);
                InsertCovMatrixInfo(ID, description, itimes, covMatrix);
            }
            catch (Exception e)
            {

            }
        }

    }
}

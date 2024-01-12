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
//
using Microsoft.Office.Interop.Excel;
using System.Reflection;

namespace FaultLocalization
{
    /// <summary>
    /// 传统方法A与均衡用例组B的对比
    /// </summary>
    public class FLExcelAB
    {
        /// <summary>
        /// Excel对象
        /// </summary>
        private static Microsoft.Office.Interop.Excel.Application m_ExcelObject = null;
        /// <summary>
        /// 工作簿
        /// </summary>
        private static Workbook workbook1 = null;

        /// <summary>
        /// 初始化Excel对象
        /// </summary>
        private static void InitialABCDExcel() // 李成龙改
        {
            // 实例化对象
            m_ExcelObject = new Microsoft.Office.Interop.Excel.Application();
            workbook1 = m_ExcelObject.Workbooks.Add(true);

            m_ExcelObject.Sheets.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);
            m_ExcelObject.Sheets.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);
            m_ExcelObject.Sheets.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);
            m_ExcelObject.Sheets.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);

            ((Worksheet)m_ExcelObject.Sheets[1]).Name = "最优排位下的鲁棒性";
            ((Worksheet)m_ExcelObject.Sheets[2]).Name = "平均排位下的鲁棒性";
            ((Worksheet)m_ExcelObject.Sheets[3]).Name = "最次排位下的鲁棒性";
            ((Worksheet)m_ExcelObject.Sheets[4]).Name = "绝对排位下的鲁棒性";


            m_ExcelObject.Visible = false;
        }


        /// <summary>
        /// 初始化Excel对象
        /// </summary>
        private static void InitialExcel()
        {
            // 实例化对象
            m_ExcelObject = new Microsoft.Office.Interop.Excel.Application();
            workbook1 = m_ExcelObject.Workbooks.Add(true);

            m_ExcelObject.Sheets.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);
            m_ExcelObject.Sheets.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);
            m_ExcelObject.Sheets.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);
            m_ExcelObject.Sheets.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);

            ((Worksheet)m_ExcelObject.Sheets[1]).Name = "最优排位下的expense";
            ((Worksheet)m_ExcelObject.Sheets[2]).Name = "平均排位下的expense";
            ((Worksheet)m_ExcelObject.Sheets[3]).Name = "最次排位下的expense";
            ((Worksheet)m_ExcelObject.Sheets[4]).Name = "绝对排位下的expense";

            m_ExcelObject.Visible = false;
        }


        public static void ABAveAsExcel(List<string> suiteNameList, int iNumFault, List<string> methodList, string description, string resultPath)
        {
            InitialExcel();

            AXAveLabel(methodList);

            AXAve(suiteNameList, iNumFault, methodList, description);

            StringBuilder suiteName = new StringBuilder();
            suiteName.Append(suiteNameList[0]);
            for (int i = 1; i < suiteNameList.Count; i++)
            {
                suiteName.Append(".");
                suiteName.Append(suiteNameList[i]);
            }

            string fileName = resultPath + "\\" + suiteName + "." + iNumFault.ToString() + ".AXAve." + description + ".xlsx";

            CloseExcel(fileName);
        }

        public static void ABAveAsExcel(int iNumFault, List<string> methodList, string description, string resultPath)
        {
            InitialExcel();

            AXAveLabel(methodList);

            AXAveAll(iNumFault, methodList, description);

            string fileName = resultPath + "\\All." + iNumFault.ToString() + ".AXAve." + description + ".xlsx";

            CloseExcel(fileName);
        }
        
        private static void AXAveLabel(List<string> methodList)
        {
            for (int i = 1; i < 5; i++)
            {
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[1, 1] = "ID";
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[1, 2] = "实验包";
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[1, 3] = "目标程序";
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[1, 4] = "缺陷版本";
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[1, 5] = "成例数";
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[1, 6] = "失例数";

                for (int methodIndex = 0; methodIndex < methodList.Count; methodIndex++)
                {
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[1, 7 + methodIndex + 0 * (methodList.Count + 1)] = methodList[methodIndex];
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[1, 7 + methodIndex + 1 * (methodList.Count + 1)] = methodList[methodIndex];
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[1, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                }
            }
        }

        private static void AXAveAll(int iNumFault, List<string> methodList, string description)
        {
            DataSet mDataSetA = null;
            DataSet mDataSetB = null;

            for (int methodIndex = 0; methodIndex < methodList.Count; methodIndex++)
            {
                // 读取A策略的结果
                string strSQLA = "SELECT 实验对象数据表.ID, 实验包,目标程序,缺陷版本,成例数,失例数,最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 实验对象数据表, 实验结果表"
                               + " WHERE 缺陷数量=" + iNumFault.ToString()
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'不集成'"
                               + " AND 实验对象数据表.ID=实验结果表.ID"
                               + " AND 成例数>失例数"
                               + " ORDER BY 实验对象数据表.ID";
                mDataSetA = FLDBServer.ReadDataToDataSet(strSQLA);

                // 未读取数据 || 数据为空
                if ((null == mDataSetA) || 0 == mDataSetA.Tables[0].Rows.Count)
                {
                    return;
                }
                // 对于每个缺陷版本
                for (int versionIndex = 0; versionIndex < mDataSetA.Tables[0].Rows.Count; versionIndex++)
                {
                    // 读取B策略的结果
                    string strSQLB = "SELECT 最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 随机试验统计结果表"
                               + " WHERE ID=" + mDataSetA.Tables[0].Rows[versionIndex]["ID"].ToString()
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'" + description + "'";
                    mDataSetB = FLDBServer.ReadDataToDataSet(strSQLB);
                    // 若是没有写版本信息则写版本信息
                    if (0 == methodIndex)
                    {
                        WriteVersionInfo(versionIndex, mDataSetA);
                    }
                    // 写策略A的结果
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["最优排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["平均排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["最次排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["绝对排位下的expense"].ToString());
                    // 写策略B的结果
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最优排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["平均排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最次排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["绝对排位下的expense"].ToString());
                    // 写AB策略的对比
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                } // endof versionIndex

                #region 写对比的统计结果
                string startCell = CellIndexToName(2, 7 + methodIndex + 2 * (methodList.Count + 1));
                string endCell = CellIndexToName(2 + mDataSetA.Tables[0].Rows.Count - 1, 7 + methodIndex + 2 * (methodList.Count + 1));
                for (int i = 1; i < 5; i++)
                {
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 0, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    // MAX, AVERAGE, MIN
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 1, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MAX(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 2, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=AVERAGE(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 3, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MIN(" + startCell + ":" + endCell + ")";

                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 4, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    // >0 =0 <0
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 5, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"<0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 6, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"=0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 7, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\">0\")";

                }
                #endregion

            }
        }

        public static void WriteABCDintoExcel(FLConfigure cfg, string suiteName, int iNumFault, List<string> methodList, string descriptionB, string descriptionC, string descriptionD, string resultPath) // 李成龙改
        {
            InitialABCDExcel();

            AXAveLabel(methodList);

            ABCDAve(cfg, suiteName, iNumFault, methodList, descriptionB, descriptionC, descriptionD);

            string fileName = resultPath + "\\" + suiteName + "." + iNumFault.ToString() +  "_" + descriptionD +  "(集成前后鲁棒性对比)" + ".xlsx";

            CloseExcel(fileName);
        }

        public static void ABAveAsExcel(FLConfigure cfg, string suiteName, int iNumFault, List<string> methodList, string description, string resultPath)
        {
            InitialExcel();

            AXAveLabel(methodList);

            AXAve(cfg, suiteName, iNumFault, methodList, description);

            string fileName = resultPath + "\\" + suiteName + "." + iNumFault.ToString() +  "(不变更不集成)" + "_对比_" + "(" + description + ")" + ".xlsx";

            CloseExcel(fileName);
        }

        public static void ABAveAsExcel(FLConfigure cfg, string suiteName, int iNumFault, List<string> methodList, string desc1, string desc2, string resultPath)
        {
            InitialExcel();

            AXAveLabel(methodList);

            AveXAve(cfg, suiteName, iNumFault, methodList, desc1, desc2);

            string fileName = resultPath + "\\" + suiteName + "." + iNumFault.ToString() + "_" +  desc2 + "(集成前后准确度对比)" + ".xlsx";  // 李成龙改

            CloseExcel(fileName);
        }

        /// <summary>
        /// 李成龙改 检测版本是否符合要求
        /// </summary>
        /// <param name="mDataSet"></param>
        /// <param name="cfg"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static bool CheckVersion(DataSet mDataSet, FLConfigure cfg, int i)
        {
            int numRuns = Convert.ToInt32(mDataSet.Tables[0].Rows[i]["测试用例总数"]);
            int numSucRuns = Convert.ToInt32(mDataSet.Tables[0].Rows[i]["成例数"]);
            int numFalRuns = Convert.ToInt32(mDataSet.Tables[0].Rows[i]["失例数"]);
            if (numRuns < cfg.MinRuns)
                return true;

            double ratio = Convert.ToDouble(numSucRuns) / Convert.ToDouble(numFalRuns);
            if (cfg.MinClassRatio > 0)
            {
                if (ratio < cfg.MinClassRatio)
                    return true;
            }
            if (cfg.MaxClassRatio > 0)
            {
                if (ratio > cfg.MaxClassRatio)
                    return true;
            }


            if ((cfg.MaxClassChangeRatioInOriginal * numFalRuns < 1) || (cfg.MaxClassChangeRatioInOriginal * numSucRuns < 1)) // 李成龙改 最大变更数量不得低于1个，以免该用例集丢失扰动
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// 对比两个随机试验统计结果
        /// </summary>
        /// <param name="cfg">实验配置</param>
        /// <param name="suiteName">实验包</param>
        /// <param name="iNumFault">缺陷数量</param>
        /// <param name="methodList">算法列表</param>
        /// <param name="desc1">实验配置1描述</param>
        /// <param name="desc2">实验配置2描述</param>
        private static void AveXAve(FLConfigure cfg, string suiteName, int iNumFault, List<string> methodList, string desc1, string desc2)  // 李成龙改
        {
            string strSQL = "SELECT ID,实验包,目标程序,缺陷版本,测试用例总数,成例数,失例数"
                          + " FROM 实验对象数据表"
                          + " WHERE 实验包=" + "'" + suiteName + "'"
                          + " AND 缺陷数量=" + iNumFault.ToString()
                          // + " AND 测试用例总数>" + cfg.MinRuns.ToString()  // 李成龙注释
                          // + " AND 成例数>失例数"      // 李成龙注释
                          + " ORDER BY 实验对象数据表.ID";
            DataSet mDataSet = FLDBServer.ReadDataToDataSet(strSQL);
            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
                return;

            int versionIndex = 0;
            for (int i = 0; i < mDataSet.Tables[0].Rows.Count; i++)
            {
                #region 筛选实验对象
                // 李成龙改
                if (CheckVersion(mDataSet, cfg, i))
                    continue;
                WriteVersionInfo(versionIndex, mDataSet, i);
                #endregion

                // 读取实验结果
                string idStr = mDataSet.Tables[0].Rows[i]["ID"].ToString();
                for (int methodIndex = 0; methodIndex < methodList.Count; methodIndex++)
                {
                    #region 读取A，B策略的结果
                    // 读取A策略的结果
                    string strSQLA = "SELECT 最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 随机试验统计结果表"
                               + " WHERE ID=" + idStr
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'" + desc1 + "'";
                    DataSet mDataSetA = FLDBServer.ReadDataToDataSet(strSQLA);
                    // 未读取数据 || 数据为空
                    if ((null == mDataSetA) || 0 == mDataSetA.Tables[0].Rows.Count)
                        continue;
                    // 读取B策略的结果
                    string strSQLB = "SELECT 最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 随机试验统计结果表"
                               + " WHERE ID=" + idStr
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'" + desc2 + "'";
                    DataSet mDataSetB = FLDBServer.ReadDataToDataSet(strSQLB);
                    // 未读取数据 || 数据为空
                    if ((null == mDataSetB) || 0 == mDataSetB.Tables[0].Rows.Count)
                        continue;
                    #endregion

                    #region 写结果
                    // 写策略A的结果
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["最优排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["平均排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["最次排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["绝对排位下的expense"].ToString());
                    // 写策略B的结果
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最优排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["平均排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最次排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["绝对排位下的expense"].ToString());
                    // 写AB策略的对比
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1));
                    #endregion
                }

                versionIndex++;
            }
            #region 写对比的统计结果
            for (int methodIndex = 0; methodIndex < methodList.Count; methodIndex++)
            {
                string startCell = CellIndexToName(2, 7 + methodIndex + 2 * (methodList.Count + 1));
                string endCell = CellIndexToName(2 + versionIndex - 1, 7 + methodIndex + 2 * (methodList.Count + 1));
                for (int i = 1; i < 5; i++)  // 李成龙改，增加了平均值和差值的写入
                {
                    // MAX, AVERAGE, MIN
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 1, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 2, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MAX(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 3, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=AVERAGE(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 4, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MIN(" + startCell + ":" + endCell + ")";

                    // >0 =0 <0
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 6, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 7, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"<0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 8, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"=0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 9, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\">0\")";

                    // 写均值和差值
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 11, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 12, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=AVERAGE(" + CellIndexToName(2, 7 + methodIndex + 0 * (methodList.Count + 1))
                                                                                                                                    + ":" + CellIndexToName(2 + versionIndex - 1, 7 + methodIndex + 0 * (methodList.Count + 1)) + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 13, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=AVERAGE(" + CellIndexToName(2, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                    + ":" + CellIndexToName(2 + versionIndex - 1, 7 + methodIndex + 1 * (methodList.Count + 1)) + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 14, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex + 12, 7 + methodIndex + 2 * (methodList.Count + 1))
                                                                                                                                    + "-" + CellIndexToName(2 + versionIndex + 13, 7 + methodIndex + 2 * (methodList.Count + 1));

                }
            }
            #endregion
        }

        private static double Above(double a, double b)
        {
            if ((a - b) > 0)
                return a - b;
            else
                return 0;
        
        }

        /// <summary>
        /// 对比四个实验的统计结果
        /// </summary>
        /// <param name="cfg">实验配置</param>
        /// <param name="suiteName">实验包</param>
        /// <param name="iNumFault">缺陷数量</param>
        /// <param name="methodList">算法列表</param>
        /// <param name="description">实验配置描述</param>
        private static void ABCDAve(FLConfigure cfg, string suiteName, int iNumFault, List<string> methodList, string descriptionB, string descriptionC, string descriptionD)   // 李成龙新增
        {
            string strSQL = "SELECT ID,实验包,目标程序,缺陷版本,测试用例总数,成例数,失例数"
              + " FROM 实验对象数据表"
              + " WHERE 实验包=" + "'" + suiteName + "'"
              + " AND 缺陷数量=" + iNumFault.ToString()
              // + " AND 测试用例总数>" + cfg.MinRuns.ToString()  // 李成龙注释
              // + " AND 成例数>失例数"  // 李成龙注释
              + " ORDER BY 实验对象数据表.ID";
            DataSet mDataSet = FLDBServer.ReadDataToDataSet(strSQL);
            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
                return;

            int versionIndex = 0;
            for (int i = 0; i < mDataSet.Tables[0].Rows.Count; i++)
            {
                #region 筛选实验对象
                // 李成龙改
                if (CheckVersion(mDataSet, cfg, i))
                    continue;
                WriteVersionInfo(versionIndex, mDataSet, i);
                #endregion

                // 读取实验结果
                string idStr = mDataSet.Tables[0].Rows[i]["ID"].ToString();
                for (int methodIndex = 0; methodIndex < methodList.Count; methodIndex++)
                {
                    #region 读取A，B, C, D策略的结果
                    // 读取A策略的结果
                    string strSQLA = "SELECT 最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                                   + " FROM 实验结果表"
                                   + " WHERE ID=" + idStr
                                   + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                                   + " AND 实验描述='不集成'";
                    DataSet mDataSetA = FLDBServer.ReadDataToDataSet(strSQLA);
                    // 未读取数据 || 数据为空
                    if ((null == mDataSetA) || 0 == mDataSetA.Tables[0].Rows.Count)
                        continue;
                    // 读取B策略的结果
                    string strSQLB = "SELECT 最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 随机试验统计结果表"
                               + " WHERE ID=" + idStr
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'" + descriptionB + "'";
                    DataSet mDataSetB = FLDBServer.ReadDataToDataSet(strSQLB);
                    // 未读取数据 || 数据为空
                    if ((null == mDataSetB) || 0 == mDataSetB.Tables[0].Rows.Count)
                        continue;
                    // 读取C策略的结果
                    string strSQLC = "SELECT 最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 随机试验统计结果表"
                               + " WHERE ID=" + idStr
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'" + descriptionC + "'";
                    DataSet mDataSetC = FLDBServer.ReadDataToDataSet(strSQLC);
                    // 未读取数据 || 数据为空
                    if ((null == mDataSetC) || 0 == mDataSetC.Tables[0].Rows.Count)
                        continue;
                    // 读取D策略的结果
                    string strSQLD = "SELECT 最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 随机试验统计结果表"
                               + " WHERE ID=" + idStr
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'" + descriptionD + "'";
                    DataSet mDataSetD = FLDBServer.ReadDataToDataSet(strSQLD);
                    // 未读取数据 || 数据为空
                    if ((null == mDataSetD) || 0 == mDataSetD.Tables[0].Rows.Count)
                        continue;
                    #endregion

                    #region 写结果
                    // 写集成前的鲁棒性 1-|C-A|
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = 1.0 - Math.Abs(Convert.ToDouble(mDataSetC.Tables[0].Rows[0]["最优排位下的expense"].ToString()) - Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["最优排位下的expense"].ToString()));
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = 1.0 - Math.Abs(Convert.ToDouble(mDataSetC.Tables[0].Rows[0]["平均排位下的expense"].ToString()) - Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["平均排位下的expense"].ToString()));
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = 1.0 - Math.Abs(Convert.ToDouble(mDataSetC.Tables[0].Rows[0]["最次排位下的expense"].ToString()) - Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["最次排位下的expense"].ToString()));
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = 1.0 - Math.Abs(Convert.ToDouble(mDataSetC.Tables[0].Rows[0]["绝对排位下的expense"].ToString()) - Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["绝对排位下的expense"].ToString()));
                    // 写集成后的鲁棒性 1-|D-B|
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = 1.0 - Math.Abs(Convert.ToDouble(mDataSetD.Tables[0].Rows[0]["最优排位下的expense"].ToString()) - Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最优排位下的expense"].ToString()));
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = 1.0 - Math.Abs(Convert.ToDouble(mDataSetD.Tables[0].Rows[0]["平均排位下的expense"].ToString()) - Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["平均排位下的expense"].ToString()));
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = 1.0 - Math.Abs(Convert.ToDouble(mDataSetD.Tables[0].Rows[0]["最次排位下的expense"].ToString()) - Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最次排位下的expense"].ToString()));
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = 1.0 - Math.Abs(Convert.ToDouble(mDataSetD.Tables[0].Rows[0]["绝对排位下的expense"].ToString()) - Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["绝对排位下的expense"].ToString()));

                    //// 写集成前的鲁棒性 1-abv(C-A)
                    //((Worksheet)m_ExcelObject.Sheets["最优排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = 1.0 - Above(Convert.ToDouble(mDataSetC.Tables[0].Rows[0]["最优排位下的expense"].ToString()), Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["最优排位下的expense"].ToString()));
                    //((Worksheet)m_ExcelObject.Sheets["平均排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = 1.0 - Above(Convert.ToDouble(mDataSetC.Tables[0].Rows[0]["平均排位下的expense"].ToString()), Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["平均排位下的expense"].ToString()));
                    //((Worksheet)m_ExcelObject.Sheets["最次排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = 1.0 - Above(Convert.ToDouble(mDataSetC.Tables[0].Rows[0]["最次排位下的expense"].ToString()), Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["最次排位下的expense"].ToString()));
                    //((Worksheet)m_ExcelObject.Sheets["绝对排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = 1.0 - Above(Convert.ToDouble(mDataSetC.Tables[0].Rows[0]["绝对排位下的expense"].ToString()), Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["绝对排位下的expense"].ToString()));
                    //// 写集成后的鲁棒性 1-abv(D-B)
                    //((Worksheet)m_ExcelObject.Sheets["最优排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = 1.0 - Above(Convert.ToDouble(mDataSetD.Tables[0].Rows[0]["最优排位下的expense"].ToString()), Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最优排位下的expense"].ToString()));
                    //((Worksheet)m_ExcelObject.Sheets["平均排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = 1.0 - Above(Convert.ToDouble(mDataSetD.Tables[0].Rows[0]["平均排位下的expense"].ToString()), Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["平均排位下的expense"].ToString()));
                    //((Worksheet)m_ExcelObject.Sheets["最次排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = 1.0 - Above(Convert.ToDouble(mDataSetD.Tables[0].Rows[0]["最次排位下的expense"].ToString()), Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最次排位下的expense"].ToString()));
                    //((Worksheet)m_ExcelObject.Sheets["绝对排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = 1.0 - Above(Convert.ToDouble(mDataSetD.Tables[0].Rows[0]["绝对排位下的expense"].ToString()), Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["绝对排位下的expense"].ToString()));

                    // 写ABCD策略的对比
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的鲁棒性"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    #endregion
                }

                versionIndex++;
            }
            #region 写对比的统计结果
            for (int methodIndex = 0; methodIndex < methodList.Count; methodIndex++)
            {
                string startCell = CellIndexToName(2, 7 + methodIndex + 2 * (methodList.Count + 1));
                string endCell = CellIndexToName(2 + versionIndex - 1, 7 + methodIndex + 2 * (methodList.Count + 1));
                for (int i = 1; i < 5; i++)
                {

                    // MAX, AVERAGE, MIN
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 1, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 2, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MAX(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 3, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=AVERAGE(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 4, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MIN(" + startCell + ":" + endCell + ")";

                    // >0 =0 <0
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 6, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 7, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"<0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 8, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"=0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 9, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\">0\")";


                    // 写均值和差值
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 11, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 12, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=AVERAGE(" + CellIndexToName(2, 7 + methodIndex + 0 * (methodList.Count + 1))
                                                                                                                                    + ":" + CellIndexToName(2 + versionIndex - 1, 7 + methodIndex + 0 * (methodList.Count + 1)) + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 13, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=AVERAGE(" + CellIndexToName(2, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                    + ":" + CellIndexToName(2 + versionIndex - 1, 7 + methodIndex + 1 * (methodList.Count + 1)) + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 14, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex + 13, 7 + methodIndex + 2 * (methodList.Count + 1))
                                                                                                                                    + "-" + CellIndexToName(2 + versionIndex + 12, 7 + methodIndex + 2 * (methodList.Count + 1));
                }
            }
            #endregion
        }


        /// <summary>
        /// 对比指定实验与不集成实验的结果
        /// </summary>
        /// <param name="cfg">实验配置</param>
        /// <param name="suiteName">实验包</param>
        /// <param name="iNumFault">缺陷数量</param>
        /// <param name="methodList">算法列表</param>
        /// <param name="description">实验配置描述</param>
        private static void AXAve(FLConfigure cfg, string suiteName, int iNumFault, List<string> methodList, string description)
        {
            string strSQL = "SELECT ID,实验包,目标程序,缺陷版本,测试用例总数,成例数,失例数"
              + " FROM 实验对象数据表"
              + " WHERE 实验包=" + "'" + suiteName + "'"
              + " AND 缺陷数量=" + iNumFault.ToString()
              // + " AND 测试用例总数>" + cfg.MinRuns.ToString()  // 李成龙注释
              // + " AND 成例数>失例数"  // 李成龙注释
              + " ORDER BY 实验对象数据表.ID";
            DataSet mDataSet = FLDBServer.ReadDataToDataSet(strSQL);
            // 未读取数据 || 数据为空
            if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
                return;

            int versionIndex = 0;
            for (int i = 0; i < mDataSet.Tables[0].Rows.Count; i++)
            {
                #region 筛选实验对象
                // 李成龙改
                if (CheckVersion(mDataSet, cfg, i))
                    continue;
                WriteVersionInfo(versionIndex, mDataSet, i);
                #endregion

                // 读取实验结果
                string idStr = mDataSet.Tables[0].Rows[i]["ID"].ToString();
                for (int methodIndex = 0; methodIndex < methodList.Count; methodIndex++)
                {
                    #region 读取A，B策略的结果
                    // 读取A策略的结果
                    string strSQLA = "SELECT 最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 实验结果表"
                               + " WHERE ID=" + idStr
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述='不集成'";
                    DataSet mDataSetA = FLDBServer.ReadDataToDataSet(strSQLA);
                    // 未读取数据 || 数据为空
                    if ((null == mDataSetA) || 0 == mDataSetA.Tables[0].Rows.Count)
                        continue;
                    // 读取B策略的结果
                    string strSQLB = "SELECT 最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 随机试验统计结果表"
                               + " WHERE ID=" + idStr
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'" + description + "'";
                    DataSet mDataSetB = FLDBServer.ReadDataToDataSet(strSQLB);
                    // 未读取数据 || 数据为空
                    if ((null == mDataSetB) || 0 == mDataSetB.Tables[0].Rows.Count)
                        continue;
                    #endregion

                    #region 写结果
                    // 写策略A的结果
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["最优排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["平均排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["最次排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[0]["绝对排位下的expense"].ToString());
                    // 写策略B的结果
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最优排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["平均排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最次排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["绝对排位下的expense"].ToString());
                    // 写AB策略的对比
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1));
                    #endregion
                }

                versionIndex++;
            }
            #region 写对比的统计结果
            for (int methodIndex = 0; methodIndex < methodList.Count; methodIndex++)
            {
                string startCell = CellIndexToName(2, 7 + methodIndex + 2 * (methodList.Count + 1));
                string endCell = CellIndexToName(2 + versionIndex - 1, 7 + methodIndex + 2 * (methodList.Count + 1));
                for (int i = 1; i < 5; i++)  // 李成龙改，增加了平均值和差值的写入
                {
                    // MAX, AVERAGE, MIN
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 1, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 2, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MAX(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 3, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=AVERAGE(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 4, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MIN(" + startCell + ":" + endCell + ")";

                    // >0 =0 <0
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 6, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 7, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"<0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 8, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"=0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 9, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\">0\")";


                    // 写均值和差值
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 11, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 12, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=AVERAGE(" + CellIndexToName(2, 7 + methodIndex + 0 * (methodList.Count + 1))
                                                                                                                                    + ":" + CellIndexToName(2 + versionIndex - 1, 7 + methodIndex + 0 * (methodList.Count + 1)) + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 13, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=AVERAGE(" + CellIndexToName(2, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                    + ":" + CellIndexToName(2 + versionIndex - 1, 7 + methodIndex + 1 * (methodList.Count + 1)) + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex + 14, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex + 12, 7 + methodIndex + 2 * (methodList.Count + 1))
                                                                                                                                    + "-" + CellIndexToName(2 + versionIndex + 13, 7 + methodIndex + 2 * (methodList.Count + 1));

                }
            }
            #endregion
        }

        #region obsolete
        /// <summary>
        /// 对比AB两种策略的均值
        /// </summary>
        /// <param name="suiteName">实验包</param>
        /// <param name="iNumFault">缺陷数量</param>
        /// <param name="methodList">试验方法</param>
        /// <param name="description">描述</param>
        /// <param name="resultPath">结果输出路径</param>
        public static void ABAveAsExcel(string suiteName, int iNumFault, List<string> methodList, string description, string resultPath)
        {
            InitialExcel();

            AXAveLabel(methodList);

            AXAve(suiteName, iNumFault, methodList, description);

            string fileName = resultPath + "\\" + suiteName + "." + iNumFault.ToString() + ".AXAve." + description + ".xlsx";

            CloseExcel(fileName);
        }

        /// <summary>
        /// 对比AB两种策略的均值
        /// </summary>
        /// <param name="suiteName">实验包</param>
        /// <param name="iNumFault">缺陷数量</param>
        /// <param name="methodList">试验方法</param>
        /// <param name="desc1">实验描述1</param>
        /// <param name="desc2">实验描述2</param>
        /// <param name="resultPath">结果输出路径</param>
        public static void ABAveAsExcel(string suiteName, int iNumFault, List<string> methodList, string desc1, string desc2, string resultPath)
        {
            InitialExcel();

            AXAveLabel(methodList);

            AveXAve(suiteName, iNumFault, methodList, desc1, desc2);

            string fileName = resultPath + "\\" + suiteName + "." + iNumFault.ToString() + ".AXAve." + desc1 + "." + desc2 + ".xlsx";

            CloseExcel(fileName);
        }

        /// <summary>
        /// 对比指定实验与不集成实验的结果
        /// </summary>
        /// <param name="suiteName">实验包</param>
        /// <param name="iNumFault">缺陷数量</param>
        /// <param name="methodList">算法列表</param>
        /// <param name="description">实验配置描述</param>
        private static void AXAve(string suiteName, int iNumFault, List<string> methodList,string description)
        {
            DataSet mDataSetA = null;
            DataSet mDataSetB = null;

            for (int methodIndex = 0; methodIndex < methodList.Count; methodIndex++)
            {
                // 读取A策略的结果
                string strSQLA = "SELECT 实验对象数据表.ID, 实验包,目标程序,缺陷版本,成例数,失例数,最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 实验对象数据表, 实验结果表"
                               + " WHERE 实验包=" + "'" + suiteName + "'"
                               + " AND 缺陷数量=" + iNumFault.ToString()
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'不集成'"
                               + " AND 实验对象数据表.ID=实验结果表.ID"
                               + " AND 成例数>失例数"
                               + " ORDER BY 实验对象数据表.ID";
                mDataSetA = FLDBServer.ReadDataToDataSet(strSQLA);

                // 未读取数据 || 数据为空
                if ((null == mDataSetA) || 0 == mDataSetA.Tables[0].Rows.Count)
                    return;
                // 对于每个缺陷版本
                for (int versionIndex = 0; versionIndex < mDataSetA.Tables[0].Rows.Count; versionIndex++)
                {

                    // 读取B策略的结果
                    string strSQLB = "SELECT 最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 随机试验统计结果表"
                               + " WHERE ID=" + mDataSetA.Tables[0].Rows[versionIndex]["ID"].ToString()
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'" + description + "'";
                    mDataSetB = FLDBServer.ReadDataToDataSet(strSQLB);
                    // 若是没有写版本信息则写版本信息
                    if (0 == methodIndex)
                    {
                        WriteVersionInfo(versionIndex, mDataSetA);
                    }
                    #region 写结果
                    // 写策略A的结果
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["最优排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["平均排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["最次排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["绝对排位下的expense"].ToString());
                    // 写策略B的结果
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最优排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["平均排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最次排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["绝对排位下的expense"].ToString());
                    // 写AB策略的对比
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    #endregion
                } // endof versionIndex

                #region 写对比的统计结果
                string startCell = CellIndexToName(2, 7 + methodIndex + 2 * (methodList.Count + 1));
                string endCell = CellIndexToName(2 + mDataSetA.Tables[0].Rows.Count - 1, 7 + methodIndex + 2 * (methodList.Count + 1));
                for (int i = 1; i < 5; i++)
                {
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 0, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    // MAX, AVERAGE, MIN
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 1, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MAX(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 2, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=AVERAGE(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 3, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MIN(" + startCell + ":" + endCell + ")";

                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 4, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    // >0 =0 <0
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 5, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"<0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 6, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"=0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 7, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\">0\")";

                }
                #endregion

            }

        }

        /// <summary>
        /// 对比两个随机试验统计结果
        /// </summary>
        /// <param name="suiteName">实验包</param>
        /// <param name="iNumFault">缺陷数量</param>
        /// <param name="methodList">算法列表</param>
        /// <param name="desc1">实验配置1描述</param>
        /// <param name="desc2">实验配置2描述</param>
        private static void AveXAve(string suiteName, int iNumFault, List<string> methodList, string desc1, string desc2)
        {
            DataSet mDataSetA = null;
            DataSet mDataSetB = null;

            for (int methodIndex = 0; methodIndex < methodList.Count; methodIndex++)
            {
                // 读取A策略的结果
                string strSQLA = "SELECT 实验对象数据表.ID, 实验包,目标程序,缺陷版本,成例数,失例数,最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 实验对象数据表, 随机试验统计结果表"
                               + " WHERE 实验包=" + "'" + suiteName + "'"
                               + " AND 缺陷数量=" + iNumFault.ToString()
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'" + desc1 + "'"
                               + " AND 实验对象数据表.ID=随机试验统计结果表.ID"
                               + " AND 成例数>失例数"
                               + " ORDER BY 实验对象数据表.ID";
                mDataSetA = FLDBServer.ReadDataToDataSet(strSQLA);
                // 未读取数据 || 数据为空
                if ((null == mDataSetA) || 0 == mDataSetA.Tables[0].Rows.Count)
                    return;
                // 对于每个缺陷版本
                for (int versionIndex = 0; versionIndex < mDataSetA.Tables[0].Rows.Count; versionIndex++)
                {
                    // 读取B策略的结果
                    string strSQLB = "SELECT 最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 随机试验统计结果表"
                               + " WHERE ID=" + mDataSetA.Tables[0].Rows[versionIndex]["ID"].ToString()
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'" + desc2 + "'";
                    mDataSetB = FLDBServer.ReadDataToDataSet(strSQLB);
                    // 若是没有写版本信息则写版本信息
                    if (0 == methodIndex)
                        WriteVersionInfo(versionIndex, mDataSetA);

                    #region 写结果
                    // 写策略A的结果
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["最优排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["平均排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["最次排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["绝对排位下的expense"].ToString());
                    // 写策略B的结果
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最优排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["平均排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最次排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["绝对排位下的expense"].ToString());
                    // 写AB策略的对比
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    #endregion
                } // endof versionIndex

                #region 写对比的统计结果
                string startCell = CellIndexToName(2, 7 + methodIndex + 2 * (methodList.Count + 1));
                string endCell = CellIndexToName(2 + mDataSetA.Tables[0].Rows.Count - 1, 7 + methodIndex + 2 * (methodList.Count + 1));
                for (int i = 1; i < 5; i++)
                {
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 0, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    // MAX, AVERAGE, MIN
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 1, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MAX(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 2, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=AVERAGE(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 3, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MIN(" + startCell + ":" + endCell + ")";

                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 4, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    // >0 =0 <0
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 5, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"<0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 6, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"=0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 7, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\">0\")";

                }
                #endregion
            }
        }
        #endregion

        private static void AXAve(List<string> suiteNameList, int iNumFault, List<string> methodList, string description)
        {
            DataSet mDataSetA = null;
            DataSet mDataSetB = null;

            StringBuilder suiteName = new StringBuilder();
            suiteName.Append("实验包='" + suiteNameList[0] + "'");
            for (int i = 1; i < suiteNameList.Count; i++)
            {
                suiteName.Append(" OR ");
                suiteName.Append("实验包='" + suiteNameList[i] + "'");
            }

            for (int methodIndex = 0; methodIndex < methodList.Count; methodIndex++)
            {
                
                // 读取A策略的结果
                string strSQLA = "SELECT 实验对象数据表.ID, 实验包,目标程序,缺陷版本,成例数,失例数,最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 实验对象数据表, 实验结果表"
                               + " WHERE (" + suiteName + ")"
                               + " AND 缺陷数量=" + iNumFault.ToString()
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'不集成'"
                               + " AND 实验对象数据表.ID=实验结果表.ID"
                               + " AND 成例数>失例数"
                               + " ORDER BY 实验对象数据表.ID";
                mDataSetA = FLDBServer.ReadDataToDataSet(strSQLA);

                // 未读取数据 || 数据为空
                if ((null == mDataSetA) || 0 == mDataSetA.Tables[0].Rows.Count)
                {
                    return;
                }
                // 对于每个缺陷版本
                for (int versionIndex = 0; versionIndex < mDataSetA.Tables[0].Rows.Count; versionIndex++)
                {
                    // 读取B策略的结果
                    string strSQLB = "SELECT 最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense"
                               + " FROM 随机试验统计结果表"
                               + " WHERE ID=" + mDataSetA.Tables[0].Rows[versionIndex]["ID"].ToString()
                               + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                               + " AND 实验描述=" + "'" + description + "'";
                    mDataSetB = FLDBServer.ReadDataToDataSet(strSQLB);
                    // 若是没有写版本信息则写版本信息
                    if (0 == methodIndex)
                    {
                        WriteVersionInfo(versionIndex, mDataSetA);
                    }
                    // 写策略A的结果
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["最优排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["平均排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["最次排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetA.Tables[0].Rows[versionIndex]["绝对排位下的expense"].ToString());
                    // 写策略B的结果
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最优排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["平均排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["最次排位下的expense"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1)] = Convert.ToDouble(mDataSetB.Tables[0].Rows[0]["绝对排位下的expense"].ToString());
                    // 写AB策略的对比
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 1 * (methodList.Count + 1))
                                                                                                                                                   + "-" + CellIndexToName(2 + versionIndex, 7 + methodIndex + 0 * (methodList.Count + 1));
                } // endof versionIndex

                #region 写对比的统计结果
                string startCell = CellIndexToName(2, 7 + methodIndex + 2 * (methodList.Count + 1));
                string endCell = CellIndexToName(2 + mDataSetA.Tables[0].Rows.Count - 1, 7 + methodIndex + 2 * (methodList.Count + 1));
                for (int i = 1; i < 5; i++)
                {
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 0, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    // MAX, AVERAGE, MIN
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 1, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MAX(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 2, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=AVERAGE(" + startCell + ":" + endCell + ")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 3, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=MIN(" + startCell + ":" + endCell + ")";

                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 4, 7 + methodIndex + 2 * (methodList.Count + 1)] = methodList[methodIndex];
                    // >0 =0 <0
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 5, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"<0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 6, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\"=0\")";
                    ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + mDataSetA.Tables[0].Rows.Count + 7, 7 + methodIndex + 2 * (methodList.Count + 1)] = "=COUNTIF(" + startCell + ":" + endCell + ",\">0\")";

                }
                #endregion

            }

        }


        // 写缺陷版本信息
        private static void WriteVersionInfo(int versionIndex, DataSet mDataSet)
        {
            for (int i = 1; i < 5; i++)
            {
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex, 1] = Convert.ToInt32(mDataSet.Tables[0].Rows[versionIndex]["ID"].ToString());
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex, 2] = mDataSet.Tables[0].Rows[versionIndex]["实验包"].ToString();
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex, 3] = mDataSet.Tables[0].Rows[versionIndex]["目标程序"].ToString();
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex, 4] = mDataSet.Tables[0].Rows[versionIndex]["缺陷版本"].ToString();
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex, 5] = mDataSet.Tables[0].Rows[versionIndex]["成例数"].ToString();
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex, 6] = mDataSet.Tables[0].Rows[versionIndex]["失例数"].ToString();
            }
        }

        private static void WriteVersionInfo(int versionIndex, DataSet mDataSet, int setIndex)
        {
            for (int i = 1; i < 5; i++)
            {
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex, 1] = Convert.ToInt32(mDataSet.Tables[0].Rows[setIndex]["ID"].ToString());
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex, 2] = mDataSet.Tables[0].Rows[setIndex]["实验包"].ToString();
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex, 3] = mDataSet.Tables[0].Rows[setIndex]["目标程序"].ToString();
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex, 4] = mDataSet.Tables[0].Rows[setIndex]["缺陷版本"].ToString();
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex, 5] = mDataSet.Tables[0].Rows[setIndex]["成例数"].ToString();
                ((Worksheet)m_ExcelObject.Sheets[i]).Cells[2 + versionIndex, 6] = mDataSet.Tables[0].Rows[setIndex]["失例数"].ToString();
            }
        }

        /// <summary>
        /// 关闭Excel对象 回收内存
        /// </summary>
        private static void CloseExcel(string fileName)
        {
            string outpath_statistic = fileName;
            m_ExcelObject.Visible = false;
            m_ExcelObject.DisplayAlerts = false;//不显示提示框
            workbook1.Close(true, outpath_statistic, null);
            //关闭

            workbook1 = null;
            m_ExcelObject.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(m_ExcelObject);
            m_ExcelObject = null;
            System.GC.Collect();

        }


        /// <summary>
        /// 将字符串形式的表格名名称转换为表的行和列索引(二维int数组,第1位是行索引,第2位是列索引) - 注意:行和列的数字索引是从1开始而不是0
        /// </summary>
        /// <param name="cellName">表格名 - 如"C3", "AB15"等。</param>
        /// <returns></returns>
        public static int[] CellNameToIndex(string cellName)
        {
            int[] cellIndex = new int[2];
            //行索引
            char[] alphabet = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
            cellIndex[0] = Convert.ToInt32(cellName.ToUpper().TrimStart(alphabet));

            //列索引
            char[] number = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            string mLetter = cellName.TrimEnd(number).ToUpper();
            if (1 == mLetter.Length)
            {
                cellIndex[1] = Convert.ToInt32((short)(Encoding.ASCII.GetBytes(mLetter.Trim())[0])) - 64;
            }
            else if (2 == mLetter.Length)
            {
                cellIndex[1] =
                    ((Convert.ToInt32((short)(Encoding.ASCII.GetBytes(mLetter.Substring(0, 1).Trim()))[0])) - 64) * 26
                    + (Convert.ToInt32((short)(Encoding.ASCII.GetBytes(mLetter.Substring(1, 1).Trim())[0])) - 64);
            }

            return cellIndex;
        }

        /// <summary>
        /// 将表和索引转换为字符串名称 - 注意:行和列的数字索引是从1开始而不是0
        /// </summary>
        /// <param name="rowIndex">行索引 - 从1开始</param>
        /// <param name="columnIndex">列索引 - 从1开始</param>
        /// <returns></returns>
        public static string CellIndexToName(int rowIndex, int columnIndex)
        {
            string cellName = GetColumnNameByIndex(columnIndex);
            cellName = cellName + rowIndex.ToString();
            return cellName;
        }

        /// <summary>
        /// 将column index转化为字母，至多两位 - 注意:数字索引是从1开始而不是0
        /// </summary>
        /// <param name="index">列索引 - 从1开始</param>
        /// <returns></returns>
        public static string GetColumnNameByIndex(int index)
        {
            string[] alphabet = new string[] { "", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y" };
            string columnName = "";
            int temp = index;

            while (temp > 0)
            {
                int temp2 = temp % 26;
                temp = temp / 26;

                if (0 == temp2)
                {
                    columnName = "Z" + columnName;
                    temp = temp - 1;
                }
                else
                {
                    columnName = alphabet[temp2] + columnName;
                }
            }

            return columnName;
        }
    }
}

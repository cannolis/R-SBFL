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
    public partial class FLDBServer
    {
        /// <summary>
        /// Excel对象
        /// </summary>
        private static Microsoft.Office.Interop.Excel.Application m_ExcelObject = null;
        /// <summary>
        /// 工作簿
        /// </summary>
        private static Workbook workbook1 = null;

        // 结果另存为Excel
        public static void SaveAsExcel(string suiteName, int iNumFault, List<string> methodList, string description, string resultPath)
        {
            InitialExcel();

            OutputExcel(suiteName, iNumFault, methodList, description);

            string fileName = resultPath + "\\" + suiteName + "." + iNumFault.ToString() + "." + description + ".xlsx";

            CloseExcel(fileName);
        }

        public static void SaveAllAsExcel(int iNumFault, List<string> methodList, string description, string resultPath)
        {
            InitialExcel();

            OutputAllExcel( iNumFault, methodList, description);

            string fileName = resultPath + "\\All" + "." + iNumFault.ToString() + "." + description + ".xlsx";

            CloseExcel(fileName);
        }

        /// <summary>
        /// 初始化Excel对象
        /// </summary>
        public static void InitialExcel()
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

        /// <summary>
        /// 输出所有版本的excel表
        /// </summary>
        /// <param name="iNumFault"></param>
        /// <param name="methodList"></param>
        /// <param name="description"></param>
        public static void OutputAllExcel(int iNumFault, List<string> methodList, string description)
        {
            DataSet mDataSet = null;

            for (int methodIndex = 0; methodIndex < methodList.Count; methodIndex++)
            {
                //读取缺陷版本
                string strSQL = "SELECT 实验对象数据表.ID, 实验包,目标程序,缺陷版本,最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense FROM 实验对象数据表, 实验结果表"
                             + " WHERE 缺陷数量=" + iNumFault.ToString()
                             + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                             + " AND 实验描述=" + "'" + description + "'"
                             + " AND 实验对象数据表.ID=实验结果表.ID"
                             + " ORDER BY 实验对象数据表.ID";

                mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

                // 未读取数据 || 数据为空
                if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
                {
                    return;
                }

                WriteExcel(methodList, methodIndex, mDataSet);

            }
        }

        /// <summary>
        /// 输出Excel表
        /// </summary>
        /// <param name="suiteName">对象</param>
        /// <param name="iNumFault">缺陷数量</param>
        /// <param name="methodList">方法列表</param>
        /// <param name="description">实验描述</param>
        public static void OutputExcel(string suiteName, int iNumFault, List<string> methodList, string description)
        {
            DataSet mDataSet = null;

            for (int methodIndex = 0; methodIndex < methodList.Count; methodIndex++)
            {
                //读取缺陷版本
                string strSQL = "SELECT 实验对象数据表.ID, 实验包,目标程序,缺陷版本,最优排位下的expense,平均排位下的expense, 最次排位下的expense, 绝对排位下的expense FROM 实验对象数据表, 实验结果表"
                             + " WHERE 实验包=" + "'" + suiteName + "'"
                             + " AND 缺陷数量=" + iNumFault.ToString()
                             + " AND 算法=" + "'" + methodList[methodIndex] + "'"
                             + " AND 实验描述=" + "'" + description + "'"
                             + " AND 实验对象数据表.ID=实验结果表.ID"
                             + " ORDER BY 实验对象数据表.ID";

                mDataSet = m_SQLServerOperation.SqlServerReadDataToDataSet(strSQL);

                // 未读取数据 || 数据为空
                if ((null == mDataSet) || 0 == mDataSet.Tables[0].Rows.Count)
                {
                    return;
                }

                WriteExcel(methodList, methodIndex, mDataSet);

            }
        }


        private static void WriteExcel(List<string> methodList, int methodIndex, DataSet mDataSet)
        {
            if (0 == methodIndex)
            {
                ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[1, 1] = "ID";
                ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[1, 2] = "实验包";
                ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[1, 3] = "目标程序";
                ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[1, 4] = "缺陷版本";

                ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[1, 1] = "ID";
                ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[1, 2] = "实验包";
                ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[1, 3] = "目标程序";
                ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[1, 4] = "缺陷版本";

                ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[1, 1] = "ID";
                ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[1, 2] = "实验包";
                ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[1, 3] = "目标程序";
                ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[1, 4] = "缺陷版本";

                ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[1, 1] = "ID";
                ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[1, 2] = "实验包";
                ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[1, 3] = "目标程序";
                ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[1, 4] = "缺陷版本";
            }

            ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[1, 5 + methodIndex] = methodList[methodIndex];
            ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[1, 5 + methodIndex] = methodList[methodIndex];
            ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[1, 5 + methodIndex] = methodList[methodIndex];
            ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[1, 5 + methodIndex] = methodList[methodIndex];

            for (int versionIndex = 0; versionIndex < mDataSet.Tables[0].Rows.Count; versionIndex++)
            {
                if (0 == methodIndex)
                {
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 1] = Convert.ToInt32(mDataSet.Tables[0].Rows[versionIndex]["ID"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 2] = mDataSet.Tables[0].Rows[versionIndex]["实验包"].ToString();
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 3] = mDataSet.Tables[0].Rows[versionIndex]["目标程序"].ToString();
                    ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 4] = mDataSet.Tables[0].Rows[versionIndex]["缺陷版本"].ToString();

                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 1] = Convert.ToInt32(mDataSet.Tables[0].Rows[versionIndex]["ID"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 2] = mDataSet.Tables[0].Rows[versionIndex]["实验包"].ToString();
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 3] = mDataSet.Tables[0].Rows[versionIndex]["目标程序"].ToString();
                    ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 4] = mDataSet.Tables[0].Rows[versionIndex]["缺陷版本"].ToString();

                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 1] = Convert.ToInt32(mDataSet.Tables[0].Rows[versionIndex]["ID"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 2] = mDataSet.Tables[0].Rows[versionIndex]["实验包"].ToString();
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 3] = mDataSet.Tables[0].Rows[versionIndex]["目标程序"].ToString();
                    ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 4] = mDataSet.Tables[0].Rows[versionIndex]["缺陷版本"].ToString();

                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 1] = Convert.ToInt32(mDataSet.Tables[0].Rows[versionIndex]["ID"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 2] = mDataSet.Tables[0].Rows[versionIndex]["实验包"].ToString();
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 3] = mDataSet.Tables[0].Rows[versionIndex]["目标程序"].ToString();
                    ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 4] = mDataSet.Tables[0].Rows[versionIndex]["缺陷版本"].ToString();
                }
                ((Worksheet)m_ExcelObject.Sheets["最优排位下的expense"]).Cells[2 + versionIndex, 5 + methodIndex] = Convert.ToDouble(mDataSet.Tables[0].Rows[versionIndex]["最优排位下的expense"].ToString());
                ((Worksheet)m_ExcelObject.Sheets["平均排位下的expense"]).Cells[2 + versionIndex, 5 + methodIndex] = Convert.ToDouble(mDataSet.Tables[0].Rows[versionIndex]["平均排位下的expense"].ToString());
                ((Worksheet)m_ExcelObject.Sheets["最次排位下的expense"]).Cells[2 + versionIndex, 5 + methodIndex] = Convert.ToDouble(mDataSet.Tables[0].Rows[versionIndex]["最次排位下的expense"].ToString());
                ((Worksheet)m_ExcelObject.Sheets["绝对排位下的expense"]).Cells[2 + versionIndex, 5 + methodIndex] = Convert.ToDouble(mDataSet.Tables[0].Rows[versionIndex]["绝对排位下的expense"].ToString());
            }
        }

        /// <summary>
        /// 关闭Excel对象 回收内存
        /// </summary>
        public static void CloseExcel(string fileName)
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
    

        //-----------------------------------------------------------------------------------

        

        //-----------------------------------------------------------------------------------
    }
}
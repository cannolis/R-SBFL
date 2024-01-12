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
    public class FLExcelInfo
    {
        /// <summary>
        /// Excel对象
        /// </summary>
        private static Microsoft.Office.Interop.Excel.Application m_ExcelObject = null;
        /// <summary>
        /// 工作簿
        /// </summary>
        private static Workbook workbook1 = null;

        private static void InitialExcel()
        {
            // 实例化对象
            m_ExcelObject = new Microsoft.Office.Interop.Excel.Application();
            workbook1 = m_ExcelObject.Workbooks.Add(true);

            m_ExcelObject.Sheets.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);
            m_ExcelObject.Sheets.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);

            ((Worksheet)m_ExcelObject.Sheets[1]).Name = "缺陷桩点对照表";
            ((Worksheet)m_ExcelObject.Sheets[2]).Name = "多缺陷设置表";

            m_ExcelObject.Visible = false;
        }


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

        public static void ExportInfo(string resultPath)
        {
            InitialExcel();

            #region 单缺陷设置
            ((Worksheet)m_ExcelObject.Sheets[1]).Cells[1, 1] = "缺陷编号";
            ((Worksheet)m_ExcelObject.Sheets[1]).Cells[1, 2] = "实验包";
            ((Worksheet)m_ExcelObject.Sheets[1]).Cells[1, 3] = "目标程序";
            ((Worksheet)m_ExcelObject.Sheets[1]).Cells[1, 4] = "缺陷名称";
            ((Worksheet)m_ExcelObject.Sheets[1]).Cells[1, 5] = "缺陷语句行";

            // 读取缺陷桩点
            string strSQLA = "SELECT 缺陷描述表.缺陷编号, 实验包,目标程序,缺陷名称,缺陷语句行"
                           + " FROM 缺陷描述表, 缺陷桩点对照表"
                           + " WHERE 缺陷描述表.缺陷编号=缺陷桩点对照表.缺陷编号"
                           + " ORDER BY 缺陷描述表.缺陷编号";

            DataSet mDataSetA = FLDBServer.ReadDataToDataSet(strSQLA);

            // 未读取数据 || 数据为空
            if ((null == mDataSetA) || 0 == mDataSetA.Tables[0].Rows.Count)
            {
                return;
            }

            Console.Write("共有" + mDataSetA.Tables[0].Rows.Count.ToString() + "个设置");

            int pid = -1;
            int j = 0;
            int k = -1;
            for (int i = 0; i < mDataSetA.Tables[0].Rows.Count; i++)
            {
                int id = Convert.ToInt32(mDataSetA.Tables[0].Rows[i]["缺陷编号"]);
                if (id != pid)
                {
                    k++;
                    ((Worksheet)m_ExcelObject.Sheets[1]).Cells[2 + k, 1] = Convert.ToDouble(mDataSetA.Tables[0].Rows[i]["缺陷编号"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets[1]).Cells[2 + k, 2] = mDataSetA.Tables[0].Rows[i]["实验包"];
                    ((Worksheet)m_ExcelObject.Sheets[1]).Cells[2 + k, 3] = mDataSetA.Tables[0].Rows[i]["目标程序"];
                    ((Worksheet)m_ExcelObject.Sheets[1]).Cells[2 + k, 4] = mDataSetA.Tables[0].Rows[i]["缺陷名称"];
                    j = 0;
                }
                else
                {
                    j++;
                }
                ((Worksheet)m_ExcelObject.Sheets[1]).Cells[2 + k, j + 5] = Convert.ToDouble(mDataSetA.Tables[0].Rows[i]["缺陷语句行"].ToString());
                pid = id;
            }
            #endregion

            #region 多缺陷设置
            ((Worksheet)m_ExcelObject.Sheets[2]).Cells[1, 1] = "ID";
            ((Worksheet)m_ExcelObject.Sheets[2]).Cells[1, 2] = "实验包";
            ((Worksheet)m_ExcelObject.Sheets[2]).Cells[1, 3] = "目标程序";
            ((Worksheet)m_ExcelObject.Sheets[2]).Cells[1, 4] = "缺陷版本";
            ((Worksheet)m_ExcelObject.Sheets[2]).Cells[1, 5] = "缺陷数量";
            ((Worksheet)m_ExcelObject.Sheets[2]).Cells[1, 6] = "缺陷编号";
            // 读取缺陷桩点
            strSQLA = "SELECT 实验对象数据表.ID, 实验包,目标程序,缺陷版本,缺陷数量,缺陷编号"
                           + " FROM 实验对象数据表, 缺陷设置表"
                           + " WHERE 实验对象数据表.ID=缺陷设置表.ID"
                           + " AND 缺陷数量>1"
                           + " ORDER BY 实验对象数据表.ID";

            mDataSetA = FLDBServer.ReadDataToDataSet(strSQLA);

            Console.Write("2.共有" + mDataSetA.Tables[0].Rows.Count.ToString() + "个设置");

            // 未读取数据 || 数据为空
            if ((null == mDataSetA) || 0 == mDataSetA.Tables[0].Rows.Count)
            {
                return;
            }

            pid = -1;
            j = 0;
            k = -1;
            for (int i = 0; i < mDataSetA.Tables[0].Rows.Count; i++)
            {
                int id = Convert.ToInt32(mDataSetA.Tables[0].Rows[i]["ID"]);
                if (id != pid)
                {
                    k++;
                    ((Worksheet)m_ExcelObject.Sheets[2]).Cells[2 + k, 1] = Convert.ToDouble(mDataSetA.Tables[0].Rows[i]["ID"].ToString());
                    ((Worksheet)m_ExcelObject.Sheets[2]).Cells[2 + k, 2] = mDataSetA.Tables[0].Rows[i]["实验包"];
                    ((Worksheet)m_ExcelObject.Sheets[2]).Cells[2 + k, 3] = mDataSetA.Tables[0].Rows[i]["目标程序"];
                    ((Worksheet)m_ExcelObject.Sheets[2]).Cells[2 + k, 4] = mDataSetA.Tables[0].Rows[i]["缺陷版本"];
                    ((Worksheet)m_ExcelObject.Sheets[2]).Cells[2 + k, 5] = mDataSetA.Tables[0].Rows[i]["缺陷数量"];
                    j = 0;
                }
                else
                {
                    j++;
                }
                ((Worksheet)m_ExcelObject.Sheets[2]).Cells[2 + k, j + 6] = Convert.ToDouble(mDataSetA.Tables[0].Rows[i]["缺陷编号"].ToString());
                pid = id;
            }


            #endregion


            CloseExcel(resultPath + "/info.xlsx");
        }




    }
}

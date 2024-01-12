/**************************************文档说明************************************
 * Copyright(C), 2010, BUAA, 软件与控制研究室
 * 文件名称:    ExcelFileOperation.cs
 * 作者:        Liuwei
 * 版本:        1.0        
 * 创建日期:    2011.11.14, 22:09
 * 完成日期:    2011
 * 文件描述:    通过Com组件进行Excel文件操作
 *              
 * 调用关系:    using Microsoft.Office.Interop.Excel;
 *              using Microsoft.Office.Core;
 * 继承关系:    
 * 其它:        
 *              
 * 属性列表:    略
 * 
 * 修改历史:
 * 1.   修改日期:   
 *      修改人:     
 *      修改功能:   
***********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
//
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Core;

namespace ExcelDll
{
    /// <summary>
    /// Excel文件操作类
    /// </summary>
    public class ExcelFileOperation
    {
        /// <summary>
        /// 空对象
        /// </summary>
        private object mMissing = Missing.Value;

        /// <summary>
        /// Excel对象
        /// </summary>
        private /*Microsoft.Office.Interop.Excel.*/Application m_ExcelApp = null;
        /// <summary>
        /// Excel对象
        /// </summary>
        public Application ExcelApp
        {
            get { return m_ExcelApp; }
        }

        /// <summary>
        /// Excel工作簿
        /// </summary>
        private /*Microsoft.Office.Interop.Excel.*/Workbook m_ExcelBook = null;
        /// <summary>
        /// Excel工作簿
        /// </summary>
        public Workbook ExcelBook
        {
            get { return m_ExcelBook; }
        }

        /// <summary>
        /// Excel表
        /// </summary>
        private /*Microsoft.Office.Interop.Excel.*/Worksheet m_ExcelSheet = null;
        /// <summary>
        /// Excel表
        /// </summary>
        public Worksheet ExcelSheet
        {
            get { return m_ExcelSheet; }
        }

        /// <summary>
        /// Excel表格对象
        /// </summary>
        private /*Microsoft.Office.Interop.Excel.*/Range m_ExcelRange = null;
        /// <summary>
        /// Excel表格对象
        /// </summary>
        public Range ExcelRange
        {
            get { return m_ExcelRange; }
        }

        /// <summary>
        /// Excel表格内容
        /// </summary>
        private object m_ExcelRangeValue = null;
        /// <summary>
        /// Excel表格内容
        /// </summary>
        public object ExcelRangeValue
        {
            get { return m_ExcelRangeValue; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Visible">是否可见</param>
        public ExcelFileOperation(bool Visible)
        {
            m_ExcelApp = new Application();
            //是否以可见的方式打开
            m_ExcelApp.Visible = Visible;
        }

        /// <summary>
        /// 根据文件名称打开工作薄
        /// </summary>
        /// <param name="filename">要打开的Excel文件名称</param>
        /// <returns></returns>
        public bool OpenExcel(string filename)
        {
            //初始化
            m_ExcelBook = null;
            //
            //如果文件不存在
            if (!File.Exists(filename))
            {
                return false;
            }
            if (null == m_ExcelApp)
            {
                return false;
            }
            //得到WorkBook对象, 可以用两种方式
            m_ExcelBook = m_ExcelApp.Workbooks.Open(filename, mMissing, mMissing, mMissing, mMissing, mMissing, mMissing, mMissing, mMissing, mMissing, mMissing, mMissing, mMissing, mMissing, mMissing);
            if (null == m_ExcelBook)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 新建工作薄
        /// </summary>
        /// <returns></returns>
        public bool NewExcel()
        {
            //初始化
            m_ExcelBook = null;
            //
            if (null == m_ExcelApp)
            {
                return false;
            }
            //新建工作薄
            m_ExcelBook = m_ExcelApp.Workbooks.Add(mMissing);

            return true;
        }

        /// <summary>
        /// 打开默认的Sheet
        /// </summary>
        /// <returns></returns>
        public bool OpenSheet()
        {
            //初始化
            m_ExcelSheet = null;
            //
            if (null == m_ExcelBook)
            {
                return false;
            }
            if (1 > m_ExcelBook.Sheets.Count)
            {
                return false;
            }
            m_ExcelSheet = (Worksheet)m_ExcelBook.Sheets[1];
            //激活当前工作溥
            //m_ExcelSheet.Activate();

            //方式2
            //m_ExcelSheet = (Worksheet)m_ExcelApp.ActiveSheet;
            
            return true;
        }

        /// <summary>
        /// 打开指定的Sheet
        /// </summary>
        /// <param name="sheetIndex">Sheet索引</param>
        /// <returns></returns>
        public bool OpenSheet(int sheetIndex)
        {
            //初始化
            m_ExcelSheet = null;
            //
            if (null == m_ExcelBook)
            {
                return false;
            }
            if (sheetIndex > m_ExcelBook.Sheets.Count)
            {
                return false;
            }
            m_ExcelSheet = (Worksheet)m_ExcelBook.Sheets[sheetIndex];
            //m_ExcelSheet = (Worksheet)m_ExcelBook.Sheets.get_Item(sheetIndex);
            //激活当前工作溥
            //m_ExcelSheet.Activate();

            return true;
        }

        /// <summary>
        /// 打开指定的Sheet
        /// </summary>
        /// <param name="sheetIndex">Sheet索引</param>
        /// <returns></returns>
        public bool OpenSheet(string sheetIndex)
        {
            //初始化
            m_ExcelSheet = null;
            //
            if (null == m_ExcelBook)
            {
                return false;
            }
            //
            bool isExist = false;//是否存在
            for (int i=1; i<=m_ExcelBook.Sheets.Count; ++i)
            {
                if (sheetIndex == ((Worksheet)m_ExcelBook.Sheets[i]).Name)
                {
                    isExist = true;
                    break;
                }
            }
            if (!isExist)
            {
                return false;
            }
            //
            m_ExcelSheet = (Worksheet)m_ExcelBook.Sheets[sheetIndex];
            //m_ExcelSheet = (Worksheet)m_ExcelBook.Sheets.get_Item(sheetIndex);
            //激活当前工作溥
            //m_ExcelSheet.Activate();

            return true;
        }

        /// <summary>
        /// 根据名称读取单元格。如"C3", "A15"等。
        /// </summary>
        /// <param name="cellSign">单元格名称</param>
        /// <returns></returns>
        public bool ReadCell(string cellSign)
        {
            //初始化
            m_ExcelRange = null;
            //
            if (null == m_ExcelSheet)
            {
                return false;
            }
            m_ExcelRange = m_ExcelSheet.get_Range(cellSign, mMissing);
            m_ExcelRangeValue = m_ExcelRange.Value2;
            //
            return true;
        }

        /// <summary>
        /// 根据行和列的索引读取单元格。
        /// </summary>
        /// <param name="rowIndex">行索引</param>
        /// <param name="colummIndex">列索引</param>
        /// <returns>读取成功</returns>
        public bool ReadCell(int rowIndex, int colummIndex)
        {
            //初始化
            m_ExcelRange = null;
            //
            if (null == m_ExcelSheet)
            {
                return false;
            }
            m_ExcelRange = (Range)m_ExcelSheet.Cells[rowIndex, colummIndex];
            m_ExcelRangeValue = m_ExcelRange.Value2;
            //
            return true;
        }

        /// <summary>
        /// 读取多个表格数据
        /// </summary>
        /// <param name="startRowIndex">开始行索引</param>
        /// <param name="startColumnIndex">开始列索引</param>
        /// <param name="endRowIndex">结束行索引</param>
        /// <param name="endColumnIndex">结束列索引</param>
        /// <returns></returns>
        public object[,] ReadCells(int startRowIndex, int startColumnIndex, int endRowIndex, int endColumnIndex)
        {
            if (null == m_ExcelSheet)
            {
                return null;
            }
            //
            //结束表格要大于开始表格
            if ((endRowIndex < startRowIndex) || (endColumnIndex < endRowIndex))
            {
                return null;
            }

            object[,] mValues = new object[endRowIndex - startRowIndex + 1, endColumnIndex - startColumnIndex + 1];
            for (int i = startRowIndex; i <= endRowIndex; ++i)
            {
                for (int j = startColumnIndex; j <= endColumnIndex; ++j)
                {
                    mValues[i - startRowIndex, j - startColumnIndex] = ((Range)m_ExcelSheet.Cells[i, j]).Value2;
                }
            }
            //
            return mValues;
        }

        /// <summary>
        /// 读取多个表格数据
        /// </summary>
        /// <param name="startCell">开始表格名称</param>
        /// <param name="endCell">结束表格名称</param>
        /// <returns></returns>
        public object[,] ReadCells(string startCell, string endCell)
        {
            if (null == m_ExcelSheet)
            {
                return null;
            }

            //
            int[] start = CellNameToIndex(startCell);
            int[] end = CellNameToIndex(endCell);

            //结束表格要大于开始表格
            if ((end[1] < start[1]) || (end[0] < start[0]))
            {
                return null;
            }

            object[,] mValues = new object[end[0] - start[0] + 1, end[1] - start[1] + 1];
            for (int i = start[0]; i <= end[0]; ++i)
            {
                for (int j = start[1]; j <= end[1]; ++j)
                {
                    mValues[i - start[0], j - start[1]] = ((Range)m_ExcelSheet.Cells[i, j]).Value2;
                    //m_ExcelSheet.get_Range(cellSign, mMissing).Value2;
                }
            }
            //
            return mValues;
        }

        /// <summary>
        /// 将数据写到表格
        /// </summary>
        /// <param name="rowIndex">行索引</param>
        /// <param name="colummIndex">列索引</param>
        /// <param name="mValue">写入的值</param>
        /// <returns></returns>
        public bool WriteCell(int rowIndex, int colummIndex, object mValue)
        {
            if (null == m_ExcelSheet)
            {
                return false;
            }
            m_ExcelSheet.Cells[rowIndex, colummIndex] = mValue;
            return true;
        }

        /// <summary>
        /// 将数据写到表格
        /// </summary>
        /// <param name="cellSign">表格标志</param>
        /// <param name="mValue">写入的值，如"C3", "AB15"等。</param>
        /// <returns></returns>
        public bool WriteCell(string cellSign, object mValue)
        {
            if (null == m_ExcelSheet)
            {
                return false;
            }
            m_ExcelSheet.get_Range(cellSign,mMissing).Value2 = mValue;
            return true;
        }

        /// <summary>
        /// 一次写入数据到多个表格
        /// </summary>
        /// <param name="startRowIndex">开始行索引</param>
        /// <param name="startColumnIndex">开始列索引</param>
        /// <param name="endRowIndex">结束行索引</param>
        /// <param name="endColumnIndex">结束列索引</param>
        /// <param name="mValues">待写入的值数组</param>
        /// <returns></returns>
        public bool WriteCells(int startRowIndex, int startColumnIndex, int endRowIndex, int endColumnIndex, object[,] mValues)
        {
            if (null == m_ExcelSheet)
            {
                return false;
            }

            //结束表格要大于开始表格
            if ((endRowIndex < startRowIndex) || (endColumnIndex < startColumnIndex))
            {
                return false;
            }

            //长度不符
            if ((endRowIndex - startRowIndex + 1) != mValues.GetLength(0))
            {
                return false;
            }
            if ((endColumnIndex - startColumnIndex + 1) != mValues.GetLength(1))
            {
                return false;
            }

            //开始写入
            for (int i = startRowIndex; i <= endRowIndex; ++i)
            {
                for (int j = startColumnIndex; j <= endColumnIndex; ++j)
                {
                    m_ExcelSheet.Cells[i, j] = mValues[i - startRowIndex, j - startColumnIndex];
                }
            }
            return true;
        }

        /// <summary>
        /// 一次写入数据到多个表格
        /// </summary>
        /// <param name="startCell">开始表格名称</param>
        /// <param name="endCell">结束表格名称</param>
        /// <param name="mValues">待写入的值数组</param>
        /// <returns></returns>
        public bool WriteCells(string startCell, string endCell, object[,] mValues)
        {
            if (null == m_ExcelSheet)
            {
                return false;
            }

            int[] start = CellNameToIndex(startCell);
            int[] end = CellNameToIndex(endCell);

            //结束表格要大于开始表格
            if ((end[1] < start[1]) || (end[0] < start[0]))
            {
                return false;
            }

            //长度不符
            if ((end[0] - start[0] + 1) != mValues.GetLength(0))
            {
                return false;
            }
            if ((end[1] - start[1] + 1) != mValues.GetLength(1))
            {
                return false;
            }

            //开始写入
            for (int i = start[0]; i <= end[0]; ++i)
            {
                for (int j = start[1]; j <= end[1]; ++j)
                {
                    m_ExcelSheet.Cells[i, j] = mValues[i - start[0], j - start[1]];
                }
            }
            return true;
        }

        /// <summary>
        /// 保存Excel
        /// </summary>
        public bool SaveExcel()
        {
            if (null == m_ExcelBook)
            {
                return false;
            }
            m_ExcelBook.Save();
            //设置DisplayAlerts
            m_ExcelApp.DisplayAlerts = false;
            m_ExcelApp.Visible = true;

            return true;
        }

        /// <summary>
        /// 保存Excel
        /// </summary>
        /// <param name="filename">文件名称</param>
        /// <returns></returns>
        public bool SaveExcel(string filename)
        {
            //保存方式一：保存WorkBook 
            //if (null == m_ExcelBook)
            //{
            //    return false;
            //}
            //m_ExcelBook.SaveAs(filename, mMissing, mMissing, mMissing, mMissing, mMissing, XlSaveAsAccessMode.xlNoChange, mMissing, mMissing, mMissing, mMissing, mMissing);
            //m_ExcelBook.SaveCopyAs(filename);

            //保存方式二：保存WorkSheet 
            if (null == m_ExcelSheet)
            {
                return false;
            }
            m_ExcelSheet.SaveAs(filename, mMissing, mMissing, mMissing, mMissing, mMissing, mMissing, mMissing, mMissing, mMissing);

            return true;
        }

        /// <summary>
        /// 关闭Excel
        /// </summary>
        public void CloseExcel()
        {
            //释放对象
            m_ExcelSheet = null;
            m_ExcelBook = null;
            m_ExcelApp.Quit();//这一句是非常重要的，否则Excel对象不能从内存中退出 
            m_ExcelApp = null;
            //释放内存
            GcCollect();
        }

        /// <summary>
        /// 保存并退出Excel
        /// </summary>
        /// <returns></returns>
        public bool SaveAndCloseExcel()
        {
            bool bSuccess = SaveExcel();
            CloseExcel();
            //
            return bSuccess;
        }

        /// <summary>
        /// 保存并退出Excel
        /// </summary>
        /// <returns></returns>
        public bool SaveAndCloseExcel(string filename)
        {
            bool bSuccess = SaveExcel(filename);
            CloseExcel();
            //
            return bSuccess;
        }

        /// <summary>
        /// 垃圾回收
        /// </summary>
        public void GcCollect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
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
            char[] alphabet = new char[] {'A', 'B', 'C', 'D','E', 'F', 'G', 'H', 'I', 'J' ,'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
            cellIndex[0] = Convert.ToInt32(cellName.ToUpper().TrimStart(alphabet));

            //列索引
            char[] number = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            string mLetter = cellName.TrimEnd(number).ToUpper();
            if ( 1 == mLetter.Length )
            {
                cellIndex[1] = Convert.ToInt32((short)(Encoding.ASCII.GetBytes(mLetter.Trim())[0])) - 64;
            }
            else if ( 2 == mLetter.Length )
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
            string[] alphabet = new string[] { "", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            string result = "";
            int temp = index / 26;
            int temp2 = index % 26;
            if (temp > 0)
            {
                result = result + alphabet[temp];
            }
            result = result + alphabet[temp2];
            return result;
        }
    }
}

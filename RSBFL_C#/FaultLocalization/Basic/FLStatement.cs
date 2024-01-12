/**********************************************************************
 * 
 *  FLStatementInfo.cs
 *  
 *  功能： 记录了某条语句的信息:
 *         1) 语句桩点号和行号
 * 
 * **********************************************GaoYichao.2013.08.12**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FaultLocalization
{
    public class FLStatement
    {
        protected int m_ID = -1;
        /// <summary>
        /// 获取编号(桩点号)
        /// </summary>
        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        protected int m_LineNumber = -1;
        /// <summary>
        /// 获取或设置语句行号
        /// </summary>
        public int LineNumber
        {
            get { return m_LineNumber; }
            set { m_LineNumber = value; }
        }

        /// <summary>
        /// 根据参考文件计算语句桩点编号
        /// </summary>
        /// <param name="map">参考文件</param>
        public void LineNumber2ID(string map)
        {
            StreamReader refeReader = new StreamReader(map);
            string refeLine = refeReader.ReadLine();
            string[] strLineNumbers = refeLine.Split(' ');
            // 依次对比各个语句行号
            m_ID = -1;
            for (int i = 0; i < strLineNumbers.Length; i++)
            {
                int itempNumber;
                if (int.TryParse(strLineNumbers[i], out itempNumber) && m_LineNumber == itempNumber)
                {
                    m_ID = i;
                }
            }
        }
        /// <summary>
        /// 根据参考文件计算语句行号
        /// </summary>
        /// <param name="map">参考文件</param>
        public void ID2LineNumber(string map)
        {
            StreamReader refeReader = new StreamReader(map);
            string refeLine = refeReader.ReadLine();
            string[] strLineNumbers = refeLine.Split(' ');

            int itempNumber;
            if (-1 != m_ID && int.TryParse(strLineNumbers[m_ID], out itempNumber))
            {
                m_LineNumber = itempNumber;
            }
        }

        /// <summary>
        /// 深度拷贝
        /// </summary>
        /// <returns>副本</returns>
        public virtual object Clone()
        {
            FLStatement result = new FLStatement();

            // 语句索引
            result.ID = this.ID;
            result.LineNumber = this.LineNumber;

            return result;
        }
        /// <summary>
        /// 深度拷贝到
        /// </summary>
        /// <param name="dst">副本</param>
        public virtual void CopyTo(object newStatement)
        {
            if (typeof(FLStatement) != newStatement.GetType())
            {
                throw new Exception("期望newStatement是" + typeof(FLStatement).ToString() + "型数据");
            }
            else
            {
                FLStatement dst = (FLStatement)newStatement;
                // 语句索引
                dst.ID = this.ID;
                dst.LineNumber = this.LineNumber;
            }
        }
    }
}

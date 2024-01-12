/**********************************************************************
 * 
 *  FLStatementInfo.cs
 *  
 *  功能： 记录了某条语句的信息:
 *         1) 语句桩点号和行号
 *         2) 语句覆盖的4个参数(a_ep a_np a_ef a_nf)
 *         3) 语句的可疑度和排位
 * 
 * **********************************************GaoYichao.2013.08.12**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FaultLocalization
{
    /// <summary>
    /// 可执行语句的执行信息
    /// </summary>
    public class FLStatementInfo : FLStatement
    {
        #region 语句覆盖信息

        private double m_Aep = 0;
        /// <summary>
        /// 获取或设置覆盖语句a的成例数
        /// </summary>
        public double a_ep
        {
            get { return m_Aep; }
            set { m_Aep = value; }
        }

        private double m_Anp = 0;
        /// <summary>
        /// 获取或设置没有覆盖语句a的成例数
        /// </summary>
        public double a_np
        {
            get { return m_Anp; }
            set { m_Anp = value; }
        }

        private double m_Aef = 0;
        /// <summary>
        /// 获取或设置覆盖语句a的失例数
        /// </summary>
        public double a_ef
        {
            get { return m_Aef; }
            set { m_Aef = value; }
        }

        private double m_Anf = 0;
        /// <summary>
        /// 获取或设置没有覆盖语句a的示例数
        /// </summary>
        public double a_nf
        {
            get { return m_Anf; }
            set { m_Anf = value; }
        }
        #endregion


        #region 语句可疑度和排序

        private double m_fSuspiciousness1 = 0;
        /// <summary>
        /// 获取或设置可疑度1
        /// </summary>
        public double suspiciousness1
        {
            get { return m_fSuspiciousness1; }
            set { m_fSuspiciousness1 = value; }
        }

        private double m_fSuspiciousness2 = 0;
        /// <summary>
        /// 获取或设置可疑度2
        /// </summary>
        public double suspiciousness2
        {
            get { return m_fSuspiciousness2; }
            set { m_fSuspiciousness2 = value; }
        }

        private int m_Sort = -1;
        /// <summary>
        /// 获取或设置语句的排序
        /// </summary>
        public int sort
        {
            get { return m_Sort; }
            set { m_Sort = value; }
        }

        private double m_ExpectedSort = -1;
        /// <summary>
        /// 获取或设置语句的期望排位(具有相同可疑度的语句排位的均值)
        /// </summary>
        public double ExpectedSort
        {
            get { return m_ExpectedSort; }
            set { m_ExpectedSort = value; }
        }

        #endregion

        /// <summary>
        /// 深度拷贝
        /// </summary>
        /// <returns>副本</returns>
        public override object Clone()
        {
            FLStatementInfo result = new FLStatementInfo();

            // 语句索引
            result.ID = this.ID;
            result.LineNumber = this.LineNumber;
            // 语句覆盖信息
            result.a_ep = this.a_ep;
            result.a_np = this.a_np;
            result.a_ef = this.a_ef;
            result.a_nf = this.a_nf;
            // 语句可疑度和排序
            result.suspiciousness1 = this.suspiciousness1;
            result.suspiciousness2 = this.suspiciousness2;

            return result;
        }
        /// <summary>
        /// 深度拷贝到
        /// </summary>
        /// <param name="dst">副本</param>
        public override void CopyTo(object newStatement)
        {
            if (typeof(FLStatementInfo) != newStatement.GetType())
            {
                throw new Exception("期望newStatement是" + typeof(FLStatementInfo).ToString() + "型数据");
            }
            else
            {
                FLStatementInfo dst = (FLStatementInfo)newStatement;
                // 语句索引
                dst.ID = this.ID;
                dst.LineNumber = this.LineNumber;
                // 语句覆盖信息
                dst.a_ep = this.a_ep;
                dst.a_np = this.a_np;
                dst.a_ef = this.a_ef;
                dst.a_nf = this.a_nf;
                // 语句可疑度和排序
                dst.suspiciousness1 = this.suspiciousness1;
                dst.suspiciousness2 = this.suspiciousness2;
            }
        }
    }
}

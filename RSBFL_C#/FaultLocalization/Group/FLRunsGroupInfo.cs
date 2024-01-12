/************************************************************************
 * 
 * class: TestGroup
 * 
 * 功能：记录了一个测试用例组的信息包括：
 *       1) 选用成例和失利编号
 *       2) 计算可疑度时所用算法名称
 *       3) 该测试用例下各个语句的信息
 * 
 * ************************************************GaoYichao.2013.08.12**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
//
using FaultLocalization;

namespace FaultLocalization
{
    /// <summary>
    /// 一个用例组的信息
    /// </summary>
    public class FLRunsGroupInfo
    {
        #region 基本属性
        private int[] m_SucCaseIDs = null;

        public int[] SucCaseIDs
        {
            get { return m_SucCaseIDs; }
        }

        private int[] m_FalCaseIDs = null;

        public int[] FalCaseIDs
        {
            get { return m_FalCaseIDs; }
        }
        /// <summary>
        /// 测试用例组下各语句信息（a_ef、a_np...、suspecious等）
        /// </summary>
        public FLStatementInfo[] Statements = null;

        /// <summary>
        /// 语句数
        /// </summary>
        public int NumStatements
        {
            get { return Statements.Length; }
        }
        /// <summary>
        /// 成例个数
        /// </summary>
        public int NumSucCase
        {
            get { return m_SucCaseIDs.Length; }
        }
        /// <summary>
        /// 失例个数
        /// </summary>
        public int NumFalCase
        {
            get { return m_FalCaseIDs.Length; }
        }
        /// <summary>
        /// 类别比例
        /// </summary>
        public double Ratio
        {
            get { return Convert.ToDouble(NumSucCase) / Convert.ToDouble(NumFalCase); }
        }

        /// <summary>
        /// 覆盖矩阵
        /// </summary>
        public FLBoolCovMatrix m_CovMatrix = null;
        #endregion

        /// <summary>
        /// 用于标识该用例组是否经过拆分
        /// </summary>
        public bool IsDivided = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="covMatrix">覆盖矩阵</param>
        public FLRunsGroupInfo(FLBoolCovMatrix covMatrix)
        {
            m_CovMatrix = covMatrix;
        }

        //李成龙添加
        public double[] aveSucSusp = null;
        public double[] aveFalSusp = null;
        public double[] sucWeight = null;
        public double[] falWeight = null;


        /// <summary>
        /// 李成龙添加
        /// </summary>
        /// <param name="sucCaseIDs">选用成例编号</param>
        /// <param name="falCaseIDs">选用失利编号</param>
        public void LoadStaCoverageUnderWeightCases()
        {
            if ((sucWeight != null) && (falWeight != null))
                Statements = m_CovMatrix.GetStaCoverageUnderWeightCases(m_SucCaseIDs, m_FalCaseIDs, sucWeight, falWeight);//李成龙改
            else
                Statements = m_CovMatrix.GetStaCoverageUnderCases(m_SucCaseIDs, m_FalCaseIDs);
        }

        /// <summary>
        /// 装载指定测试用例覆盖信息
        /// </summary>
        /// <param name="sucCaseIDs">选用成例编号</param>
        /// <param name="falCaseIDs">选用失利编号</param>
        public void LoadStaCoverageUnderCases(int[] sucCaseIDs, int[] falCaseIDs)
        {
            m_SucCaseIDs = new int[sucCaseIDs.Length];
            for (int i = 0; i < sucCaseIDs.Length; i++)
                m_SucCaseIDs[i] = sucCaseIDs[i];
            m_FalCaseIDs = new int[falCaseIDs.Length];
            for (int i = 0; i < falCaseIDs.Length; i++)
                m_FalCaseIDs[i] = falCaseIDs[i];
            Statements = m_CovMatrix.GetStaCoverageUnderCases(m_SucCaseIDs, m_FalCaseIDs);
        }
        /// <summary>
        /// 装载所有测试用例覆盖信息
        /// </summary>
        public void LoadStaCoverageUnderAllCases()
        {
            // 加载所有成例
            m_SucCaseIDs = new int[m_CovMatrix.NumSucRuns];
            for (int i = 0; i < m_CovMatrix.NumSucRuns; i++)
                m_SucCaseIDs[i] = i;
            // 加载所有失例
            m_FalCaseIDs = new int[m_CovMatrix.NumFalRuns];
            for (int i = 0; i < m_CovMatrix.NumFalRuns; i++)
                m_FalCaseIDs[i] = i;
            Statements = m_CovMatrix.GetStaCoverageUnderCases(m_SucCaseIDs, m_FalCaseIDs);
        }

    }

}
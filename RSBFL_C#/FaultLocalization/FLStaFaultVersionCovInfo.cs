/**********************************************************************
 * 
 *  FLStaFaultVersionInfo.cs
 *  
 *  继承：
 *  派生：FLStaMulFaultVersionInfo
 *  
 *  功能： 记录了所有的缺陷语句信息包括:
 *         1) 实验包名称
 *         2) 实验程序名称
 *         3) 缺陷版本名称
 *         4) 测试用例数量(总数=成例数+失例数)
 *         5) 可执行语句数量
 *         6) 覆盖矩阵
 *         7) 缺陷语句信息
 *         8) 定位负载
 *         
 *  函数：1) ParseCoverageInfo  解析语句覆盖矩阵
 *        1) GetStaInfoUnderCases   获取指定测试用例下的语句信息
 *        2) GetStaInfoUnderAllCases    获取所有测试用例下的语句信息
 * 
 * **********************************************GaoYichao.2013.08.12**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
//
using FaultLocalization;

namespace FaultLocalization
{
    #region [Obsolete!!!]
    /// <summary>
    /// 语句级的缺陷版本对象
    /// </summary>
    public class FLStaFaultVersionCovInfo
    {
        public bool keep = true;

        private Regex mulFaultPattern = new Regex(@"crash(\d+)");

        // 李成龙添加
        protected int m_NumFaults = 0;
        /// <summary>
        /// 获取或设置缺陷数量
        /// </summary>
        public int NumFaults
        {
            get { return m_NumFaults; }
            set { m_NumFaults = value; }
        }

        #region 测试用例数量 && 语句数量
        protected int m_NumRuns = 0;
        /// <summary>
        /// 获取或设置测试用例总数
        /// </summary>
        public int NumRuns
        {
            get { return m_NumRuns; }
            set { m_NumRuns = value; }
        }

        protected int m_NumSucRuns = 0;
        /// <summary>
        /// 获取或设置成功用例总数
        /// </summary>
        public int NumSucRuns
        {
            get { return m_NumSucRuns; }
            set { m_NumSucRuns = value; }
        }

        protected int m_NumFalRuns = 0;
        /// <summary>
        /// 获取或设置失败用例总数
        /// </summary>
        public int NumFalRuns
        {
            get { return m_NumFalRuns; }
            set { m_NumFalRuns = value; }
        }

        protected int m_NumStatements = 0;
        /// <summary>
        /// 获取或设置语句数量
        /// </summary>
        public int NumStatements
        {
            get { return m_NumStatements; }
            set { m_NumStatements = value; }
        }
        #endregion

        #region 覆盖矩阵
        /// <summary>
        /// 成例语句覆盖矩阵(bool) Metrix[statementIndex, caseIndex]
        /// </summary>
        public bool[,] m_bSucCoverageMetrix = null;
        /// <summary>
        /// 失例语句覆盖矩阵(bool) Metrix[statementIndex, caseIndex]
        /// </summary>
        public bool[,] m_bFalCoverageMetrix = null;
        #endregion

        public FLStaFaultVersionCovInfo()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sucFileName">成例数据</param>
        /// <param name="falFileName">失例数据</param>
        public FLStaFaultVersionCovInfo(string sucFileName, string falFileName)
        {
            // 装载数据
            string[] mSucData = File.ReadAllLines(sucFileName);
            string[] mFalData = File.ReadAllLines(falFileName);
            //  获取用例数
            m_NumSucRuns = mSucData.Length;
            m_NumFalRuns = mFalData.Length;

            m_NumStatements = (mSucData[0].Split(' ').Length - 1);
            // 初始化覆盖矩阵
            m_bSucCoverageMetrix = new bool[m_NumStatements, m_NumSucRuns];
            m_bFalCoverageMetrix = new bool[m_NumStatements, m_NumFalRuns];

            try
            {
                BuildbMetrix(mSucData, ref m_bSucCoverageMetrix);
                BuildbMetrix(mFalData, ref m_bFalCoverageMetrix);
                keep = true;
            }
            catch
            {
                keep = false;
            }
        }

        /// <summary>
        /// 构建覆盖矩阵
        /// </summary>
        /// <param name="data_str">文件中读取的源数据</param>
        /// <param name="metrix">ref:覆盖矩阵</param>
        protected void BuildbMetrix(string[] strSource, ref bool[,] metrix)
        {
            //测试用例数量
            int iNumCase = strSource.Length;
            //判断存在相应的测试用例
            if (iNumCase != 0)
            {
                //对于每个用例
                for (int caseIndex = 0; caseIndex < iNumCase; caseIndex++)
                {
                    //----拆分为每个语句
                    string[] statement = strSource[caseIndex].Split(' ');

                    //对于每条语句
                    for (int statementIndex = 0; statementIndex < metrix.GetLength(0); statementIndex++)
                    {
                        //覆盖为true
                        if (statement[statementIndex] != "0")
                        {
                            metrix[statementIndex, caseIndex] = true;
                        }
                        //否则为false
                        else
                        {
                            metrix[statementIndex, caseIndex] = false;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取用例池中的测试用例总数
        /// </summary>
        /// <param name="tankName">用例池</param>
        public int GetNumRunsInTank(string tankName)
        {
            string[] mAllData = File.ReadAllLines(tankName);
            m_NumRuns = mAllData.Length;

            int numCrush = 0;

            for (int i = 0; i < m_NumRuns; i++)
            {
                Match m = mulFaultPattern.Match(mAllData[i]);
                if (m.Success)
                {
                    numCrush++;
                }
            }

            m_NumRuns = m_NumRuns - numCrush;
            return m_NumRuns;
        }

        /// <summary>
        /// 获取指定测试用例下的语句信息
        /// </summary>
        /// <param name="sucCaseIDs">选用成例编号</param>
        /// <param name="falCaseIDs">选用失利编号</param>
        /// <returns>记录了4个覆盖参数的语句信息列表</returns>
        public FLStatementInfo[] GetStaCoverageUnderCases(int[] sucCaseIDs, int[] falCaseIDs)
        {
            //新建语句信息
            FLStatementInfo[] mStatementsInfo = new FLStatementInfo[m_NumStatements];
            //----采样用例个数
            int mNumSucCaseChosen = sucCaseIDs.Length;
            int mNumFalCaseChosen = falCaseIDs.Length;
            //装载语句信息
            for (int statementIndex = 0; statementIndex < m_NumStatements; statementIndex++)
            {
                mStatementsInfo[statementIndex] = new FLStatementInfo();

                mStatementsInfo[statementIndex].ID = statementIndex;
                mStatementsInfo[statementIndex].suspiciousness1 = 0;
                mStatementsInfo[statementIndex].sort = 0;

                //装载失例
                for (int i = 0; i < mNumFalCaseChosen; i++)
                {
                    int falIndex = falCaseIDs[i];
                    if (m_bFalCoverageMetrix[statementIndex, falIndex])
                    {
                        mStatementsInfo[statementIndex].a_ef++;
                    }
                    else
                    {
                        mStatementsInfo[statementIndex].a_nf++;
                    }
                }

                //装载成例
                for (int i = 0; i < mNumSucCaseChosen; i++)
                {
                    int sucIndex = sucCaseIDs[i];
                    if (m_bSucCoverageMetrix[statementIndex, sucIndex])
                    {
                        mStatementsInfo[statementIndex].a_ep++;
                    }
                    else
                    {
                        mStatementsInfo[statementIndex].a_np++;
                    }
                }

            }
            return mStatementsInfo;
        }

    }
    #endregion
}

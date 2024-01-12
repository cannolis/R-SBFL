using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FaultLocalization
{
    /// <summary>
    /// 覆盖矩阵
    /// </summary>
    public class FLBoolCovMatrix
    {

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
        /// 成例语句覆盖矩阵(bool) Metrix[caseIndex, statementIndex]
        /// </summary>
        private List<bool[]> m_bSucCoverageMetrix = null;
        /// <summary>
        /// 获取成例语句覆盖矩阵
        /// </summary>
        public List<bool[]> SucCoverageMetrix
        {
            get { return m_bSucCoverageMetrix; }
        }
        /// <summary>
        /// 失例语句覆盖矩阵(bool) Metrix[caseIndex, statementIndex]
        /// </summary>
        private List<bool[]> m_bFalCoverageMetrix = null;
        /// <summary>
        /// 获取失例语句覆盖矩阵
        /// </summary>
        public List<bool[]> FalCoverageMetrix
        {
            get { return m_bFalCoverageMetrix; }
        }
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sucFileName">成例数据</param>
        /// <param name="falFileName">失例数据</param>
        public FLBoolCovMatrix(string sucFileName, string falFileName)
        {
            // 装载数据
            string[] mSucData = File.ReadAllLines(sucFileName);
            string[] mFalData = File.ReadAllLines(falFileName);
            // 获取用例数
            m_NumSucRuns = mSucData.Length;
            m_NumFalRuns = mFalData.Length;
            m_NumRuns = m_NumSucRuns + m_NumFalRuns;
            m_NumStatements = (mSucData[0].Split(' ').Length - 1);
            // 初始化覆盖矩阵
            m_bSucCoverageMetrix = new List<bool[]>();
            m_bFalCoverageMetrix = new List<bool[]>();

            BuildMetrix(mSucData, ref m_bSucCoverageMetrix);
            BuildMetrix(mFalData, ref m_bFalCoverageMetrix);
        }

        /// <summary>
        /// 构造函数2
        /// </summary>
        /// <param name="sucFileName">成例数据</param>
        /// <param name="falFileName">失例数据</param>
        public FLBoolCovMatrix(List<bool[]>[] matrix)
        {
            // 获取用例数
            m_NumSucRuns = matrix[0].Count;
            m_NumFalRuns = matrix[1].Count;
            m_NumRuns = m_NumSucRuns + m_NumFalRuns;
            m_NumStatements = (matrix[0][0].Length);
            // 初始化覆盖矩阵
            m_bSucCoverageMetrix = matrix[0];
            m_bFalCoverageMetrix = matrix[1];

        }

        /// <summary>
        /// 构建覆盖矩阵
        /// </summary>
        /// <param name="strDatas">从文件中读取的源数据,每行对应一条测试用例</param>
        /// <param name="metrix">ref:覆盖矩阵Metrix[caseIndex, statementIndex]</param>
        /// <returns>是否成功构建矩阵</returns>
        protected bool BuildMetrix(string[] strDatas, ref List<bool[]> metrix)
        {
            int iNumCase = strDatas.Length;
            // 判断存在相应的测试用例
            if (0 == iNumCase)
                return false;

            metrix.Clear();
            // 对于每个测试用例
            for (int caseIndex = 0; caseIndex < iNumCase; caseIndex++)
            {
                // 拆分为每个语句
                string[] statement = strDatas[caseIndex].Split(' ');
                int iNumSta = statement.Length - 1;
                bool[] coverage = new bool[iNumSta];
                // 对于每条语句
                for (int statementIndex = 0; statementIndex < iNumSta; statementIndex++)
                {
                    if ("0" == statement[statementIndex])
                        coverage[statementIndex] = false;
                    else
                        coverage[statementIndex] = true;
                }
                //
                metrix.Add(coverage);
            }
            return true;
        }

        /// <summary>
        /// 李成龙增加
        /// </summary>
        /// <param name="sucCaseIDs">选用成例编号</param>
        /// <param name="falCaseIDs">选用失利编号</param>
        /// <returns>记录了4个覆盖参数的语句信息列表</returns>
        public FLStatementInfo[] GetStaCoverageUnderWeightCases(int[] sucCaseIDs, int[] falCaseIDs, double[] sucWeight, double[] falWeight)
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
                mStatementsInfo[statementIndex].suspiciousness2 = 0;
                mStatementsInfo[statementIndex].sort = 0;


                //装载失例
                for (int i = 0; i < mNumFalCaseChosen; i++)
                {
                    int falIndex = falCaseIDs[i];
                    if (m_bFalCoverageMetrix[falIndex][statementIndex])
                    {
                        mStatementsInfo[statementIndex].a_ef += falWeight[i];
                    }
                    else
                    {
                        mStatementsInfo[statementIndex].a_nf += falWeight[i];
                    }

                }
                //装载成例
                for (int i = 0; i < mNumSucCaseChosen; i++)
                {
                    int sucIndex = sucCaseIDs[i];
                    if (m_bSucCoverageMetrix[sucIndex][statementIndex])
                    {
                        mStatementsInfo[statementIndex].a_ep += sucWeight[i];
                    }
                    else
                    {
                        mStatementsInfo[statementIndex].a_np += sucWeight[i];
                    }
                }
            }

            return mStatementsInfo;
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
                mStatementsInfo[statementIndex].suspiciousness2 = 0; //李成龙添加
                mStatementsInfo[statementIndex].sort = 0;

                
                //装载失例
                for (int i = 0; i < mNumFalCaseChosen; i++)
                {
                    int falIndex = falCaseIDs[i];
                    if (m_bFalCoverageMetrix[falIndex][statementIndex])
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
                    if (m_bSucCoverageMetrix[sucIndex][statementIndex])
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

        /// <summary>
        /// 从成例覆盖矩阵中抽取出部分用例。
        /// 1. 从原来的矩阵中删去对应用例
        /// 2. 返回对用用例的覆盖信息
        /// </summary>
        /// <param name="sucCaseIDs">抽取的成例编号</param>
        /// <returns>抽取成例覆盖信息矩阵</returns>
        public List<bool[]> ExtractSucCases(List<int> sucCaseIDs)
        {
            List<bool[]> result = new List<bool[]>();
            // 对抽取用例从大到小排序
            sucCaseIDs.Sort();
            sucCaseIDs.Reverse();
            // 抽取用例
            for (int i = 0; i < sucCaseIDs.Count; i++)
            {
                result.Add(m_bSucCoverageMetrix[sucCaseIDs[i]]);
                m_bSucCoverageMetrix.RemoveAt(sucCaseIDs[i]);
            }
            // 获取用例数
            m_NumSucRuns -= sucCaseIDs.Count;
            return result;
        }
        /// <summary>
        /// 从失例覆盖矩阵中抽取出部分用例。
        /// 1. 从原来的矩阵中删去对应用例
        /// 2. 返回对用用例的覆盖信息
        /// </summary>
        /// <param name="falCaseIDs">抽取的失例编号</param>
        /// <returns>抽取失例覆盖信息矩阵</returns>
        public List<bool[]> ExtractFalCases(List<int> falCaseIDs)
        {
            List<bool[]> result = new List<bool[]>();
            // 对抽取用例从大到小排序
            falCaseIDs.Sort();
            falCaseIDs.Reverse();
            // 抽取用例
            for (int i = 0; i < falCaseIDs.Count; i++)
            {
                result.Add(m_bFalCoverageMetrix[falCaseIDs[i]]);
                m_bFalCoverageMetrix.RemoveAt(falCaseIDs[i]);
            }
            m_NumFalRuns -= falCaseIDs.Count;
            return result;
        }

        /// <summary>
        /// 添加成例覆盖矩阵
        /// </summary>
        /// <param name="cov">成例覆盖矩阵</param>
        public void AppendSucCases(List<bool[]> cov)
        {
            if (0 == cov.Count)
                return;
            int numSta = cov[0].Length;
            if (numSta != m_NumStatements)
                throw new Exception("新增覆盖矩阵,语句数不匹配");

            m_bSucCoverageMetrix.AddRange(cov);
            m_NumSucRuns += cov.Count;
        }
        /// <summary>
        /// 添加失例覆盖矩阵
        /// </summary>
        /// <param name="cov">失例覆盖矩阵</param>
        public void AppendFalCases(List<bool[]> cov)
        {
            if (0 == cov.Count)
                return;
            int numSta = cov[0].Length;
            if (numSta != m_NumStatements)
                throw new Exception("新增覆盖矩阵,语句数不匹配");

            m_bFalCoverageMetrix.AddRange(cov);
            m_NumFalRuns += cov.Count;
        }

    }
}

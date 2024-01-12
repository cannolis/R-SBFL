/************************************************************************
 * 
 * class: FLTestCaseFilter
 * 
 * 功能：用于根据某种规则对测试用例进行筛选
 * 
 * 规则：1) RandomIntsWithoutBack 随机不放回的抽取测试用例
 *       2) RandomIntsWithBack 随机有放回的抽取测试用例
 * 
 * ************************************************GaoYichao.2013.08.12**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaultLocalization
{
    /// <summary>
    /// 测试用例筛选器
    /// </summary>
    public class FLRunsFilter
    {
        /// <summary>
        /// 随机数对象  一个Operation只有一个对象
        /// </summary>
        private static Random m_Random = new Random();

        #region 随机有无放回
        /// <summary>
        /// 从iCandidateList中随机不放回的抽取iCount个测试用例,改变候选集
        /// </summary>
        /// <param name="iCount">测试用例数量</param>
        /// <param name="iCandidateList">测试用例候选集</param>
        /// <returns>选用的测试用例编号</returns>
        public static int[] RandomIntsNB(int iCount, List<int> iCandidateList)
        {
            // 确保有足够的候选对象
            if (null != iCandidateList && iCandidateList.Count != 0 && iCount <= iCandidateList.Count)
            {
                int[] result = new int[iCount];
                // 依次随机产生
                for (int i = 0; i < iCount; i++)
                {
                    int chosenID = m_Random.Next(0, iCandidateList.Count - 1);
                    result[i] = iCandidateList[chosenID];
                    iCandidateList.RemoveAt(chosenID);
                }
                return result;
            }
            else
            {
                // 否则其他任何情况都返回null;
                return null;
            }    
        }
        /// <summary>
        /// 从iClist1和iClist2两个列表中随机不放回的抽取iCount个测试用例,改变候选集
        /// </summary>
        /// <param name="iCount">选择用例数量</param>
        /// <param name="iClist1">候选集1</param>
        /// <param name="iClist2">候选集2</param>
        /// <returns>result[0]:iClist1中选择对象,result[1]:iClist2中选择对象</returns>
        public static List<int>[] RandomIntsNB(int iCount, List<int> iClist1, List<int> iClist2)
        {
            List<int>[] result = new List<int>[2];
            result[0] = new List<int>();
            result[1] = new List<int>();
            // 确保有足够的候选对象
            if (null == iClist1 || iClist1.Count == 0 || null == iClist2 || iClist2.Count == 0)
                return null;
            int count = iClist1.Count + iClist2.Count;
            if (iCount > count)
                return null;

            for (int i = 0; i < iCount; i++)
            {
                int chosenID = m_Random.Next(0, count - 1);
                if (chosenID < iClist1.Count)
                {
                    result[0].Add(iClist1[chosenID]);
                    iClist1.RemoveAt(chosenID);
                }
                else
                {
                    result[1].Add(iClist2[chosenID - iClist1.Count]);
                    iClist2.RemoveAt(chosenID - iClist1.Count);
                }
                count--;
            }    
            return result;
        }
        /// <summary>
        /// 从iCandidateList中随机有放回的抽取iCount个测试用例，可以重复
        /// </summary>
        /// <param name="iCount">测试用例数量</param>
        /// <param name="iCandidateList">测试用例候选集</param>
        /// <returns>选用的测试用例编号</returns>
        public static int[] RandomInts(int iCount, List<int> iCandidateList)
        {
            // 确保有足够的候选对象
            if (null != iCandidateList && iCandidateList.Count != 0 && iCount <= iCandidateList.Count)
            {
                int[] result = new int[iCount];
                // 依次随机产生
                for (int i = 0; i < iCount; i++)
                {
                    int chosenID = m_Random.Next(0, iCandidateList.Count - 1);
                    result[i] = iCandidateList[chosenID];
                }
                return result;
            }
            else
            {
                // 否则其他任何情况都返回null;
                return null;
            }    
        }
        /// <summary>
        /// 从iClist1和iClist2两个列表中随机有放回的抽取iCount个测试用例,可以重复
        /// </summary>
        /// <param name="iCount">选择用例数量</param>
        /// <param name="iClist1">候选集1</param>
        /// <param name="iClist2">候选集2</param>
        /// <returns>result[0]:iClist1中选择对象,result[1]:iClist2中选择对象</returns>
        public static List<int>[] RandomInts(int iCount, List<int> iClist1, List<int> iClist2)
        {
            List<int>[] result = new List<int>[2];
            // 确保有足够的候选对象
            if (null == iClist1 || iClist1.Count == 0 || null == iClist2 || iClist2.Count == 0)
                return null;
            int count = iClist1.Count + iClist2.Count;
            if (iCount < count)
                return null;

            for (int i = 0; i < iCount; i++)
            {
                int chosenID = m_Random.Next(0, count - 1);
                if (chosenID < iClist1.Count)
                    result[0].Add(iClist1[chosenID]);
                else
                    result[1].Add(iClist2[chosenID - iClist1.Count]);
                count--;
            }
            return result;
        }

        /// <summary>
        /// 从iCandidateList中随机有放回的抽取iCount个测试用例，可以重复
        /// 如果剩余测试用例不够，则一并使用
        /// </summary>
        /// <param name="iCount">测试用例数量</param>
        /// <param name="icandidateList">测试用例候选集</param>
        /// <returns>选用的测试用例编号</returns>
        public static int[] RandomIntsKeepRest(int iCount, List<int> iCandidateList)
        {
            if (null != iCandidateList && iCandidateList.Count != 0)
            {
                iCount = Math.Min(iCount, iCandidateList.Count);
                int[] result = new int[iCount];
                // 依次随机产生
                for (int i = 0; i < iCount; i++)
                {
                    int chosenID = m_Random.Next(0, iCandidateList.Count);
                    result[i] = iCandidateList[chosenID];
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 从iCandidateList中随机有放回的抽取iCount个测试用例,没有重复
        /// </summary>
        /// <param name="iCount">测试用例数量</param>
        /// <param name="iCandidateList">测试用例候选集</param>
        /// <returns>选用的测试用例编号</returns>
        public static int[] RandomIntsNR(int iCount, List<int> iCandidateList)
        {
            int[] itempList = new int[iCandidateList.Count];
            iCandidateList.CopyTo(itempList);
            List<int> tempList = itempList.ToList();

            return RandomIntsNB(iCount, tempList);
        }
        /// <summary>
        /// 从iClist1和iClist2两个列表中随机有放回的抽取iCount个测试用例,没有重复
        /// </summary>
        /// <param name="iCount">选择用例数量</param>
        /// <param name="iClist1">候选集1</param>
        /// <param name="iClist2">候选集2</param>
        /// <returns>result[0]:iClist1中选择对象,result[1]:iClist2中选择对象</returns>
        public static List<int>[] RandomIntsNR(int iCount, List<int> iClist1, List<int> iClist2)
        {
            int[] itempList1 = new int[iClist1.Count];
            iClist1.CopyTo(itempList1);
            List<int> tempList1 = itempList1.ToList();

            int[] itempList2 = new int[iClist2.Count];
            iClist2.CopyTo(itempList2);
            List<int> tempList2 = itempList2.ToList();

            return RandomIntsNB(iCount, tempList1, tempList2);
        }
        #endregion

        #region ART
        /// <summary>
        /// 从已有的成例池中，按照一定的规则选取成功用例
        /// （1）先生成t个种子用例
        /// （2）再随机生成k个用例
        /// （3）以最大最小距离为准则 选取一个用例添加入种子用例
        /// （4）重复(2)(3)直到生成足够的用例(iNumCase个)为止
        /// </summary>
        /// <param name="iCount">所需用例数</param>
        /// <param name="iNumSeedCases">初始种子用例数</param>
        /// <param name="iNumCandidates">每次随机生成的用例数</param>
        /// <param name="coverageMetrix">覆盖矩阵</param>
        /// <returns>选用成例编号</returns>
        public static int[] ARTIntsNB(int iCount, int iNumSeedCases, int iNumCandidates, List<int> iCandidateList, List<bool[]> coverageMetrix)
        {
            // 确保有足够的候选对象
            if (null != iCandidateList && iCandidateList.Count != 0 && iCount <= iCandidateList.Count)
            {
                //  (1)生成t个种子用例
                List<int> iSeedCases = RandomIntsNB(iNumSeedCases, iCandidateList).ToList();

                #region  (4)重复步骤2,3直到生成足够用例为止
                while (iSeedCases.Count < iCount)
                {
                    //  (2)生成k个候选用例
                    int[] iCandidates = RandomIntsKeepRest(iNumCandidates, iCandidateList);
                    if (null == iCandidateList)
                        break;
                    //  计算k个候选用例与t个种子用例之间的最小距离
                    int[] iMinDiffs = new int[iNumCandidates];
                    for (int i = 0; i < iNumCandidates; i++)
                    {
                        iMinDiffs[i] = coverageMetrix[0].Length + 1;
                    }
                    //  遍历种子用例
                    for (int seedIndex = 0; seedIndex < iSeedCases.Count; seedIndex++)
                    {
                        //  遍历候选用例
                        for (int candidateIndex = 0; candidateIndex < iCandidates.Length; candidateIndex++)
                        {
                            int temp = DiffCoverage(iSeedCases[seedIndex], iCandidates[candidateIndex], coverageMetrix);
                            if (temp < iMinDiffs[candidateIndex])
                            {
                                iMinDiffs[candidateIndex] = temp;
                            }
                        }
                    }
                    //  (3)以最大最小距离为准则选用测试用例
                    int nextSeed = 0;
                    for (int candidateIndex = 0; candidateIndex < iCandidates.Length; candidateIndex++)
                    {
                        if (iMinDiffs[candidateIndex] > iMinDiffs[nextSeed])
                        {
                            nextSeed = candidateIndex;
                        }
                    }
                    //  添入种子用例组
                    iSeedCases.Add(iCandidates[nextSeed]);
                    iCandidateList.Remove(iCandidates[nextSeed]);
                }
                #endregion

                return iSeedCases.ToArray();
            }
            else
            {
                // 否则其他任何情况都返回null;
                return null;
            }   
        }

        /// <summary>
        /// 计算两个成例之间的覆盖不同代码数
        /// </summary>
        /// <param name="icase1">成例1</param>
        /// <param name="icase2">成例2</param>
        /// <returns>覆盖不同代码数</returns>
        public static int DiffCoverage(int icase1, int icase2, List<bool[]> coverageMetrix)
        {   
            // 计数器 记录覆盖不同代码数
            int result = 0;
            // 遍历所有语句 计数
            int numSta = coverageMetrix[0].Length;
            for (int statementIndex = 0; statementIndex < numSta; statementIndex++)
            {
                // 求异或
                if ((coverageMetrix[icase1][statementIndex]) && (!coverageMetrix[icase2][statementIndex]))
                {
                    result++;
                }
                else if ((!coverageMetrix[icase1][statementIndex]) && (coverageMetrix[icase2][statementIndex]))
                {
                    result++;
                }
            }
            return result;
        }
        #endregion

        #region K-mean
        /// <summary>
        /// 从每个等价类中【有放回地】抽取1个测试用例构成用例组
        /// </summary>
        /// <param name="clas">等价类划分</param>
        /// <param name="count">每组用例数量</param>
        /// <returns>选用成例编号</returns>
        public static int[] KmeanInts(List<int>[] clas, int count)
        {
            if (clas.Length != count)
                throw new Exception("Kmean:等价类划分有误！");

            int[] result = new int[count];
            for (int i = 0; i < count; i++)
            {
                int j = m_Random.Next(clas[i].Count);
                result[i] = clas[i][j];
            }

            return result;
        }

        /// <summary>
        /// 将已有的成例池，按照一定规则划分为k个等价类
        /// </summary>
        /// <param name="k">等价类数量</param>
        /// <param name="iCandidateList">候选用例组</param>
        /// <param name="coverageMetrix">覆盖矩阵</param>
        /// <param name="flag">是否限制各类数量</param>
        /// <returns>等价类划分</returns>
        public static List<int>[] KmeanClassify(int k, List<int> iCandidateList, List<bool[]> coverageMetrix)
        {
            List<int>[] result = null;
            int n = iCandidateList.Count;
            if (n < k)
                return null;
            // 初始化聚类中心
            double delta = double.MaxValue;
            double[][] centers = KmeanInitCenters(k, coverageMetrix[0].Length);

            while (k < delta)
            {
                // Max:划分等价类
                result = KmeanClassify(iCandidateList, centers, coverageMetrix);
                // Mean:更新聚类中心
                delta = KmeanUpdateCenters(centers, result, coverageMetrix);
            }

            return result;
        }
        /// <summary>
        /// 初始化K-mean的聚类中心
        /// </summary>
        /// <param name="k">中心数量</param>
        /// <param name="n">向量维度</param>
        /// <returns>聚类中心[k][n]</returns>
        private static double[][] KmeanInitCenters(int k, int n)
        {
            double[][] result = new double[k][];

            for (int i = 0; i < k; i++)
            {
                result[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    result[i][j] = m_Random.NextDouble();
                }
            }

            return result;
        }
        /// <summary>
        /// 等价类划分
        /// </summary>
        /// <param name="iCandidateList">候选集</param>
        /// <param name="centers">中心</param>
        /// <param name="coverage">覆盖矩阵</param>
        /// <param name="flag">是否限制各类数量</param>
        /// <returns>划分[k][m]</returns>
        private static List<int>[] KmeanClassify(List<int> iCandidateList, double[][] centers, List<bool[]> coverage)
        {
            int n = iCandidateList.Count;
            int k = centers.Length;
            // 构建输出内存
            List<int>[] result = new List<int>[k];
            for (int i = 0; i < k; i++)
            {
                result[i] = new List<int>();
            }
            // 每个测试用例的最优分类编号和距离
            int[] bestClassIndex = new int[n];
            double[] bestClassDist = new double[n];
            for (int i = 0; i < n; i++)
            {
                bestClassIndex[i] = 0;
                bestClassDist[i] = double.PositiveInfinity;
            }
            // 距离每个聚类中心最近的测试用例编号和距离
            int[] bestCaseIndex = new int[k];
            double[] bestCaseDist = new double[n];
            for (int i = 0; i < k; i++)
            {
                bestClassIndex[i] = 0;
                bestClassDist[i] = double.PositiveInfinity;
            }
            // 分类
            for (int i = 0; i < iCandidateList.Count; i++)
            {
                int iCase = iCandidateList[i];
                for (int iClass = 0; iClass < centers.Length; iClass++)
                {
                    double dist = KmeanCalCaseCenterDist(centers[iClass], iCase, coverage);
                    if (dist < bestClassDist[i])
                    {
                        bestClassIndex[i] = iClass;
                        bestClassDist[i] = dist;
                    }
                    if (dist < bestCaseDist[i])
                    {
                        bestCaseIndex[iClass] = i;
                        bestCaseDist[iClass] = dist;
                    }
                }
            }
            // 保证每类至少有1个用例
            for (int i = 0; i < k; i++)
            {
                int iCase = iCandidateList[bestCaseIndex[i]];
                result[i].Add(iCase);
            }
            // 为每个测试用例分配一个类别
            for (int i = 0; i < n; i++)
            {
                int iClass = bestClassIndex[i];
                int iCase = iCandidateList[i];
                if (result[iClass][0] == iCase)
                    continue;
                result[iClass].Add(iCase);
            }

            return result;
        }
        /// <summary>
        /// 样本到中心距离
        /// </summary>
        /// <param name="center">中心向量</param>
        /// <param name="iCase">样本编号</param>
        /// <param name="coverage">覆盖矩阵</param>
        /// <returns>距离</returns>
        private static double KmeanCalCaseCenterDist(double[] center, int iCase, List<bool[]> coverage)
        {
            double result = 0;
            for (int i = 0; i < center.Length; i++)
            {
                result += Math.Pow((Convert.ToInt32(coverage[iCase][i]) - center[i]),2);
            }
            return Math.Sqrt(result);
        }
        /// <summary>
        /// 更新聚类中心
        /// </summary>
        /// <param name="centers">聚类中心</param>
        /// <param name="candidates">候选集</param>
        /// <param name="coverage">覆盖矩阵</param>
        /// <returns>聚类中心偏移量</returns>
        private static double KmeanUpdateCenters(double[][] centers, List<int>[] candidates, List<bool[]> coverage)
        {
            int K = centers.Length;

            double delta = 0;
            // 遍历每个等价类
            for (int i = 0; i < K; i++)
            {
                // 新聚类中心
                int M = candidates[i].Count;
                int L = centers[i].Length;
                double[] tmp = new double[L];
                for (int j = 0; j < L; j++)
                {
                    tmp[j] = 0;
                }
                // 遍历语句
                for (int j = 0; j < L; j++)
                {
                    // 遍历用例
                    for (int k = 0; k < M; k++)
                    {
                        int icase = candidates[i][k];
                        tmp[j] += Convert.ToInt32(coverage[icase][j]);
                    }
                    tmp[j] = tmp[j] / M;
                }
                // 计算偏移量
                double tmp2 = 0;
                for (int j = 0; j < L; j++)
                {
                    tmp2 += Math.Pow((tmp[j] - centers[i][j]), 2);
                }
                delta += Math.Sqrt(tmp2);
                // 更新中心
                centers[i] = tmp;
            }

            return delta;
        }
        #endregion

    }
}

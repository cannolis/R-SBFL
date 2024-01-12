/************************************************************************
 * 
 * class: FLTestGroupDivider.Random
 * 
 * 功能：用于随机放回和不放回的对测试用例分组
 * 
 * ************************************************GaoYichao.2013.08.12**/
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
//

namespace FaultLocalization
{
    /// <summary>
    /// 测试用例分组器
    /// </summary>
    public partial class FLRunsGroupDivider
    {
        #region 候选测试用例信息

        /// <summary>
        /// 待拆分覆盖矩阵配置信息 李成龙添加
        /// </summary>
        private int m_ID;
        private FLConfigure m_cfg;
        private int m_itimes;
        private int m_dtimes;

        private List<int> m_SucCandidates = null;
        /// <summary>
        /// 获取成例候选集
        /// </summary>
        public List<int> SucCandidates
        {
            get { return m_SucCandidates; }
        }
        /// <summary>
        /// 获取成例候选用例数量
        /// </summary>
        public int NumSucCandi
        {
            get { return m_SucCandidates.Count; }
        }

        private List<int> m_FalCandidates = null;
        /// <summary>
        /// 获取失例候选集
        /// </summary>
        public List<int> FalCandidates
        {
            get { return m_FalCandidates; }
        }
        /// <summary>
        /// 获取失例候选用例数量
        /// </summary>
        public int NumFalCandi
        {
            get { return m_FalCandidates.Count; }
        }
        #endregion

        /// <summary>
        /// 覆盖矩阵信息
        /// </summary>
        private FLBoolCovMatrix m_CovMatrix = null;
        /// <summary>
        /// 参考用例组,候选用例应当是针对该用例组的
        /// </summary>
        private FLRunsGroupInfo m_RefGroup = null;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">缺陷版本编号</param>
        /// <param name="description">用例选取描述</param>
        /// <param name="itimes">实验次数</param>
        /// <param name="covMatrix">覆盖矩阵</param>
        public FLRunsGroupDivider(int id, FLConfigure cfg, int itimes, int dtimes, FLBoolCovMatrix covMatrix)
        {
            if (null == covMatrix.FalCoverageMetrix || null == covMatrix.SucCoverageMetrix)
                throw new Exception("没有计算覆盖矩阵");
            m_ID = id;
            m_cfg = cfg;
            m_itimes = itimes;
            m_dtimes = dtimes;
            m_CovMatrix = covMatrix;
            m_RefGroup = new FLRunsGroupInfo(covMatrix);
            m_RefGroup.LoadStaCoverageUnderAllCases();

            // 初始化候选集
            ResetCandidates();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="covMatrix">覆盖矩阵</param>
        public FLRunsGroupDivider(FLBoolCovMatrix covMatrix)
        {
            if (null == covMatrix.FalCoverageMetrix || null == covMatrix.SucCoverageMetrix)
                throw new Exception("没有计算覆盖矩阵");

            m_CovMatrix = covMatrix;
            m_RefGroup = new FLRunsGroupInfo(covMatrix);
            m_RefGroup.LoadStaCoverageUnderAllCases();

            // 初始化候选集
            ResetCandidates();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="covMatrix">覆盖矩阵</param>
        /// <param name="group">参考用例组</param>
        public FLRunsGroupDivider(FLBoolCovMatrix covMatrix, FLRunsGroupInfo group)
        {
            if (null == covMatrix.FalCoverageMetrix || null == covMatrix.SucCoverageMetrix)
                throw new Exception("没有计算覆盖矩阵");

            m_CovMatrix = covMatrix;
            m_RefGroup = CreateAGroup(group.SucCaseIDs, group.FalCaseIDs, false);

            // 初始化候选集
            ResetCandidates();
        }

        /// <summary>
        /// 重置候选集
        /// </summary>
        public void ResetCandidates()
        {
            int[] suc = m_RefGroup.SucCaseIDs;
            int[] fal = m_RefGroup.FalCaseIDs;
            // 初始化候选集
            m_SucCandidates = new List<int>();
            for (int i = 0; i < suc.Length; i++)
                m_SucCandidates.Add(suc[i]);
            m_FalCandidates = new List<int>();
            for (int i = 0; i < fal.Length; i++)
                m_FalCandidates.Add(fal[i]);
        }

        /// <summary>
        /// 创建一个用例组
        /// </summary>
        /// <param name="sucList">选用成例列表</param>
        /// <param name="falList">选用失例列表</param>
        /// <param name="isDivided">是否经过拆分</param>
        public FLRunsGroupInfo CreateAGroup(int[] sucList, int[] falList, bool isDivided)
        {
            // 构建测试用例组
            FLRunsGroupInfo result = new FLRunsGroupInfo(m_CovMatrix);
            result.LoadStaCoverageUnderCases(sucList, falList);
            result.IsDivided = isDivided;

            return result;
        }

        /// <summary>
        /// 不拆分
        /// </summary>
        /// <returns>用例组</returns>
        public FLRunsGroupInfo NoDivide()
        {
            //// 计算选用的测试用例
            //int[] sucList = new int[m_SucCandidates.Count];
            //int[] falList = new int[m_FalCandidates.Count];
            //m_SucCandidates.CopyTo(sucList);
            //m_FalCandidates.CopyTo(falList);
            //// 构建测试用例组
            //return CreateAGroup(sucList, falList, false);

            return m_RefGroup;
        }

        /// <summary>
        /// 装载测试用例组
        /// </summary>
        /// <param name="suc">成例组,与失例组一一对应</param>
        /// <param name="fal">失例组</param>
        /// <returns></returns>
        public List<FLRunsGroupInfo> LoadGroups(List<int[]> suc, List<int[]> fal)
        {
            if (suc.Count != fal.Count)
                throw new Exception("用例组不匹配");

            List<FLRunsGroupInfo> divGroups = new List<FLRunsGroupInfo>();
            for (int i = 0; i < suc.Count; i++)
                divGroups.Add(CreateAGroup(suc[i], fal[i], true));

            return divGroups;
        }

        #region 按[成例:失例]的类别比例分组
        /// <summary>
        /// 按[成例:失例]的类别比例分组
        /// </summary>
        /// <param name="divider">拆分器</param>
        /// <param name="stra">拆分策略</param>
        /// <param name="fRatio">分组用例比例</param>
        /// <returns>分组列表</returns>
        public List<FLRunsGroupInfo> DivideClassRatioGroups(string stra, double fRatio)
        {
            DivideClassRatioGroupsDelegate divide = GeteClassRatioDivideStrategy(stra);
            return divide(fRatio);
        }

        #region 测试用例按比例分组 之 准备工作
        /// <summary>
        /// 拆分方案委托
        /// </summary>
        /// <param name="divider">拆分器</param>
        /// <param name="fRatio">分组用例比例</param>
        /// <returns>分组列表</returns>
        public delegate List<FLRunsGroupInfo> DivideClassRatioGroupsDelegate(double fRatio);
        /// <summary>
        /// 拆分方案查询列表
        /// </summary>
        private Dictionary<string, DivideClassRatioGroupsDelegate> m_ClassRatioDividerDictionary = null;
        /// <summary>
        /// 初始化查询列表
        /// </summary>
        private void InitClassRatioDivideDictionary()
        {
            m_ClassRatioDividerDictionary = new Dictionary<string, DivideClassRatioGroupsDelegate>();

            m_ClassRatioDividerDictionary.Add("随机拆分", DivideClassRatioGroupsNB);
            m_ClassRatioDividerDictionary.Add("ART拆分成例", DivideClassRatioGroupsART);
            m_ClassRatioDividerDictionary.Add("KMean拆分成例", DivideClassRatioGroupsKMean);
            m_ClassRatioDividerDictionary.Add("EnsembleClassifier", DivideClassRatioGroupsEC);  // 李成龙添加
        }
        /// <summary>
        /// 获取拆分方案
        /// </summary>
        /// <param name="sStrategyName">拆分策略名称</param>
        /// <returns>拆分方案入口函数</returns>
        public DivideClassRatioGroupsDelegate GeteClassRatioDivideStrategy(string sStrategyName)
        {
            if (null == m_ClassRatioDividerDictionary)
            {
                InitClassRatioDivideDictionary();
            }
            return new DivideClassRatioGroupsDelegate(m_ClassRatioDividerDictionary[sStrategyName]);
        }
        #endregion

        #region 测试用例按比例分组 之 策略
        /// <summary>
        /// 拆分测试用例组 Suc NoBack Fal NoPardon
        /// </summary>
        /// <param name="divider">拆分器</param>
        /// <param name="fRatio">分组用例比例</param>
        /// <returns>分组列表</returns>
        private List<FLRunsGroupInfo> DivideClassRatioGroupsNBNR(double fRatio)  //李成龙添加
        {
            List<FLRunsGroupInfo> divGroups = new List<FLRunsGroupInfo>();
            int iNumSuc = Convert.ToInt32(Math.Max(0, Math.Ceiling(2.0/3.0 * fRatio * NumFalCandi))); 

            // 确认存在满足条件的测试用例组
            if (m_CovMatrix.NumSucRuns < iNumSuc || m_CovMatrix.NumFalRuns < iNumSuc)
            {
                divGroups.Add(CreateAGroup(SucCandidates.ToArray(), FalCandidates.ToArray(), false));
                return divGroups;
            }
            // 构建测试用例组
            while (SucCandidates.Count >= iNumSuc)
                divGroups.Add(DivideSucFalGroupNBNR(iNumSuc));

            return divGroups;
        }

        /// <summary>
        /// 拆分测试用例组 Suc NoBack
        /// </summary>
        /// <param name="divider">拆分器</param>
        /// <param name="fRatio">分组用例比例</param>
        /// <returns>分组列表</returns>
        private List<FLRunsGroupInfo> DivideClassRatioGroupsNB(double fRatio)
        {
            List<FLRunsGroupInfo> divGroups = new List<FLRunsGroupInfo>();
            int iNumSuc = Convert.ToInt32(Math.Max(0, Math.Floor(fRatio * NumFalCandi)));

            // 确认存在满足条件的测试用例组
            if (m_CovMatrix.NumSucRuns < iNumSuc)
            {
                divGroups.Add(CreateAGroup(SucCandidates.ToArray(), FalCandidates.ToArray(), false));
                return divGroups;
            }
            // 构建测试用例组
            while (SucCandidates.Count >= iNumSuc)
                divGroups.Add(DivideSucGroupNB(iNumSuc));

            return divGroups;
        }

        /// <summary>
        /// 拆分成例组 ART
        /// </summary>
        /// <param name="fRatio">分组用例比例</param>
        /// <returns>分组列表</returns>
        private List<FLRunsGroupInfo> DivideClassRatioGroupsART(double fRatio)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 拆分成例组 K-mean;划分为k个等价类，从每个等价类中抽取1个用例构成一个用例组
        /// </summary>
        /// <param name="fRatio">分组用例比例</param>
        /// <returns>分组列表</returns>
        private List<FLRunsGroupInfo> DivideClassRatioGroupsKMean(double fRatio)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 拆分测试用例组 SPE方法
        /// </summary>
        /// <param name="divider">拆分器</param>
        /// <param name="fRatio">分组用例比例</param>
        /// <returns>分组列表</returns>
        private List<FLRunsGroupInfo> DivideClassRatioGroupsEC(double fRatio)  // 李成龙添加
        {
            List<FLRunsGroupInfo> divGroups = new List<FLRunsGroupInfo>();

            string data = m_ID.ToString() + ',' + m_cfg.ClassChangeSelectStrategy.ToString() + ',' + m_itimes.ToString() + ','
                        + m_cfg.ClassRatioDivideStrategy.ToString() + ',' + fRatio.ToString() + ',' + m_dtimes.ToString();
            string recive_data = m_cfg.client.DataTransfer(data);
            if (recive_data == "FINISHED")
            {
                // 构建测试用例组
                List<int[]>[] dividedLists = FLDBServer.ReadTestCaseChangeClassDivInfo(m_ID, m_cfg, m_itimes, m_dtimes);
                divGroups = LoadGroups(dividedLists[0], dividedLists[1]);
            }
            if (recive_data == null){ throw new Exception("服务端无响应"); }
            if (recive_data.Length == 0) { throw new Exception("返回值为空"); }
            
            return divGroups;
        }
        #endregion

        #endregion

        #region 抽样方法
        #region 只拆分成例,指定成例数量或者类别比例
        /// <summary>
        /// 不放回抽取成例，与所有失例一起构成一个用例组
        /// </summary>
        /// <param name="iNumSuc">成例数量</param>
        /// <returns></returns>
        public FLRunsGroupInfo DivideSucGroupNB(int iNumSuc)
        {
            int iNumFal = m_FalCandidates.Count;
            // 确认存在满足条件的测试用例组
            if (m_SucCandidates.Count < iNumSuc)
            {
                return NoDivide();
            }

            // 计算选用的测试用例
            int[] sucList = FLRunsFilter.RandomIntsNB(iNumSuc, m_SucCandidates);
            int[] falList = new int[iNumFal];
            m_FalCandidates.CopyTo(falList);
            // 构建测试用例组
            return CreateAGroup(sucList, falList, true);
        }
        /// <summary>
        /// 不放回抽取成例，与所有失例一起构成一个用例组
        /// </summary>
        /// <param name="fRatio">类别比例，正例:失例</param>
        /// <returns></returns>
        public FLRunsGroupInfo DivideSucGroupNB(double fRatio)
        {
            int iNumSuc = Convert.ToInt32(Math.Max(0, Math.Floor(fRatio * m_CovMatrix.NumFalRuns)));
            if (m_CovMatrix.NumSucRuns < iNumSuc)
                return null;
            return DivideSucGroupNR(iNumSuc);
        }

        /// <summary>
        /// 有放回抽取成例，与所有失例一起构成一个用例组
        /// 成例有可能重复
        /// </summary>
        /// <param name="iNumSuc">成例数量</param>
        /// <returns></returns>
        public FLRunsGroupInfo DivideSucGroup(int iNumSuc)
        {
            // 计算选用的测试用例
            int[] sucList = FLRunsFilter.RandomInts(iNumSuc, m_SucCandidates);
            int[] falList = new int[m_FalCandidates.Count];
            m_FalCandidates.CopyTo(falList);
            // 构建测试用例组
            return CreateAGroup(sucList, falList, true);
        }
        /// <summary>
        /// 有放回抽取成例，与所有失例一起构成一个用例组
        /// 成例有可能重复
        /// </summary>
        /// <param name="fRatio">类别比例，正例:失例</param>
        /// <returns></returns>
        public FLRunsGroupInfo DivideSucGroup(double fRatio)
        {
            int iNumSuc = Convert.ToInt32(Math.Max(0, Math.Floor(fRatio * m_CovMatrix.NumFalRuns)));
            if (m_CovMatrix.NumSucRuns < iNumSuc)
                return null;
            return DivideSucGroup(iNumSuc);
        }

        /// <summary>
        /// 有放回不重复抽取成例,与所有失例一起构成一个用例组
        /// </summary>
        /// <param name="iNumSuc">成例数量</param>
        /// <returns></returns>
        public FLRunsGroupInfo DivideSucGroupNR(int iNumSuc)
        {
            // 确认存在满足条件的测试用例组
            if (m_SucCandidates.Count < iNumSuc)
            {
                return NoDivide();
            }

            // 计算选用的测试用例
            int[] sucList = FLRunsFilter.RandomIntsNR(iNumSuc, m_SucCandidates);
            int[] falList = new int[m_FalCandidates.Count];
            m_FalCandidates.CopyTo(falList);
            // 构建测试用例组
            return CreateAGroup(sucList, falList, true);
        }
        /// <summary>
        /// 有放回不重复抽取成例,与所有失例一起构成一个用例组
        /// </summary>
        /// <param name="fRatio">类别比例，正例:失例</param>
        /// <returns></returns>
        public FLRunsGroupInfo DivideSucGroupNR(double fRatio)
        {
            int iNumSuc = Convert.ToInt32(Math.Max(0, Math.Floor(fRatio * m_CovMatrix.NumFalRuns)));
            if (m_CovMatrix.NumSucRuns < iNumSuc)
                return null;
            return DivideSucGroupNR(iNumSuc);
        }
        #endregion

        #region 拆分成例和失例，根据两类用例数量
        public FLRunsGroupInfo DivideSucFalGroupNB(int iNumSuc, int iNumFal)
        {
            // 确认存在满足条件的测试用例组
            if (m_SucCandidates.Count < iNumSuc || m_FalCandidates.Count < iNumFal)
            {
                return NoDivide();
            }

            // 计算选用的测试用例
            int[] sucList = FLRunsFilter.RandomIntsNB(iNumSuc, m_SucCandidates);
            int[] falList = FLRunsFilter.RandomIntsNB(iNumFal, m_FalCandidates);
            // 构建测试用例组
            return CreateAGroup(sucList, falList, true);
        }

        public FLRunsGroupInfo DivideSucFalGroup(int iNumSuc, int iNumFal)
        {
            // 确认存在满足条件的测试用例组
            if (m_SucCandidates.Count < iNumSuc || m_FalCandidates.Count < iNumFal)
            {
                return NoDivide();
            }

            // 计算选用的测试用例
            int[] sucList = FLRunsFilter.RandomInts(iNumSuc, m_SucCandidates);
            int[] falList = FLRunsFilter.RandomInts(iNumFal, m_FalCandidates);
            // 构建测试用例组
            return CreateAGroup(sucList, falList, true);
        }
        
        public FLRunsGroupInfo DivideSucFalGroupNR(int iNumSuc, int iNumFal)
        {
            // 确认存在满足条件的测试用例组
            if (m_SucCandidates.Count < iNumSuc || m_FalCandidates.Count < iNumFal)
            {
                return NoDivide();
            }

            // 计算选用的测试用例
            int[] sucList = FLRunsFilter.RandomIntsNR(iNumSuc, m_SucCandidates);
            int[] falList = FLRunsFilter.RandomIntsNR(iNumFal, m_FalCandidates);
            // 构建测试用例组
            return CreateAGroup(sucList, falList, true);
        }

        /// <summary>
        /// 不放回抽取成例，与固定比例的失例一起构成一个用例组
        /// </summary>
        /// <param name="iNumSuc">成例数量</param>
        /// <returns></returns>
        public FLRunsGroupInfo DivideSucFalGroupNB(int iNumSuc)
        {
            int iNumFal = m_FalCandidates.Count;
            // 确认存在满足条件的测试用例组
            if (m_SucCandidates.Count < iNumSuc)
            {
                return NoDivide();
            }

            // 计算选用的测试用例
            int[] sucList = FLRunsFilter.RandomIntsNB(iNumSuc, m_SucCandidates);
            int[] falList = FLRunsFilter.RandomIntsNB(iNumSuc, m_FalCandidates);
            // 构建测试用例组
            return CreateAGroup(sucList, falList, true);
        }

        /// <summary>
        /// 不放回抽取成例，与有放回的固定数量的失例一起构成一个用例组
        /// </summary>
        /// <param name="iNumSuc">成例数量</param>
        /// <returns></returns>
        public FLRunsGroupInfo DivideSucFalGroupNBNR(int iNumSuc)  //李成龙添加
        {
            // 确认存在满足条件的测试用例组
            if (m_SucCandidates.Count < iNumSuc || m_FalCandidates.Count < iNumSuc)
            {
                return NoDivide();
            }

            // 计算选用的测试用例
            int[] sucList = FLRunsFilter.RandomIntsNB(iNumSuc, m_SucCandidates);
            int[] falList = FLRunsFilter.RandomIntsNR(iNumSuc, m_FalCandidates);
            // 构建测试用例组
            return CreateAGroup(sucList, falList, true);
        }

        #endregion

        #region 随机抽取部分测试用例
        /// <summary>
        /// 把成例和失例倒在一起选取
        /// </summary>
        /// <param name="fRate">选取比例</param>
        /// <returns></returns>
        public FLRunsGroupInfo PartialAllNR(double fRate)
        {
            int all = m_SucCandidates.Count + m_FalCandidates.Count;
            int iCount = Convert.ToInt32(Math.Max(0, Math.Floor(fRate * all)));

            int times = 0;

            List<int>[] lists = FLRunsFilter.RandomIntsNR(iCount, m_SucCandidates, m_FalCandidates);
            while (0 == lists[0].Count || 0 == lists[1].Count)
            {
                times++;
                if (times > 10000)
                    return null;
                lists = FLRunsFilter.RandomIntsNR(iCount, m_SucCandidates, m_FalCandidates);
            }

            return CreateAGroup(lists[0].ToArray(), lists[1].ToArray(), true);
        }
        /// <summary>
        /// 选择部分成例，部分失例
        /// </summary>
        /// <param name="fSucRate">成例比例</param>
        /// <param name="fFalRate">失例比例</param>
        /// <returns></returns>
        public FLRunsGroupInfo PartialSucPartialFalGroupNR(double fSucRate, double fFalRate)
        {
            int iNumSuc = Convert.ToInt32(Math.Max(0, Math.Floor(fSucRate * m_SucCandidates.Count)));
            int iNumFal = Convert.ToInt32(Math.Max(0, Math.Floor(fSucRate * m_SucCandidates.Count)));

            return DivideSucFalGroupNR(iNumSuc, iNumFal);
        }
        /// <summary>
        /// 选择部分成例，全部失例
        /// </summary>
        /// <param name="fSucRate">成例比例</param>
        /// <returns></returns>
        public FLRunsGroupInfo PartialSucFullFalGroupNR(double fSucRate)
        {
            return PartialSucPartialFalGroupNR(fSucRate, 1.0);
        }
        /// <summary>
        /// 选择全部成例，部分失例
        /// </summary>
        /// <param name="fFalRate">失例比例</param>
        /// <returns></returns>
        public FLRunsGroupInfo FullSucPartialFalGroupNR(double fFalRate)
        {
            return PartialSucPartialFalGroupNR(1.0, fFalRate);
        }
        #endregion

        #region ART

        public FLRunsGroupInfo DivideSucGroupART(int iNumSuc)
        {
            int iNumFal = m_FalCandidates.Count;
            // 确认存在满足条件的测试用例组
            if (m_SucCandidates.Count < iNumSuc)
            {
                return NoDivide();
            }

            // 计算选用的测试用例
            int[] sucList = FLRunsFilter.ARTIntsNB(iNumSuc, 3, 3, m_SucCandidates, m_CovMatrix.SucCoverageMetrix);
            int[] falList = new int[iNumFal];
            m_FalCandidates.CopyTo(falList);
            // 构建测试用例组
            return CreateAGroup(sucList, falList, true);
        }

        #endregion

        #region Kmean
        /// <summary>
        /// 划分为iNumSuc个等价类，【有放回地】从每个等价类中抽取一个测试用例
        /// </summary>
        /// <param name="iNumSuc">每组成例数量</param>
        /// <returns>用例组</returns>
        public List<FLRunsGroupInfo> DivideSucGroupsKmean(int iNumSuc)
        {
            int iNumFal = m_FalCandidates.Count;
            int m = Convert.ToInt32(Math.Floor(1.0 * m_SucCandidates.Count / iNumSuc));
            // 等价类划分
            List<int>[] sucClass = FLRunsFilter.KmeanClassify(iNumSuc, m_SucCandidates, m_CovMatrix.SucCoverageMetrix);

            List<FLRunsGroupInfo> result = new List<FLRunsGroupInfo>();
            for (int i = 0; i < m; i++)
            {
                // 从每个等价类中抽取一个测试用例，构成一个用例组
                int[] sucList = FLRunsFilter.KmeanInts(sucClass, iNumSuc);
                int[] falList = new int[iNumFal];
                m_FalCandidates.CopyTo(falList);
                result.Add(CreateAGroup(sucList, falList, true));
            }

            return result;
        }        
        #endregion
        #endregion





    }
}
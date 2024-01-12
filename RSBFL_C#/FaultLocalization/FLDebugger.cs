using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FaultLocalization
{
    public class FLDebugger
    {
        private FLBoolCovMatrix m_CovMatrix = null;
        /// <summary>
        /// 获取缺陷版本信息
        /// </summary>
        public FLBoolCovMatrix CoverageInfo
        {
            get { return m_CovMatrix; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="covmatrix">覆盖矩阵</param>
        public FLDebugger(FLBoolCovMatrix covmatrix)
        {
            m_CovMatrix = covmatrix;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public FLDebugger(string sucFileName, string falFileName)
        {
            // 创建缺陷版本 读取数据
            m_CovMatrix = new FLBoolCovMatrix(sucFileName, falFileName);
        }

        /// <summary>
        /// 用一组用例组测试
        /// </summary>
        /// <param name="methodName">可疑度计算公式</param>
        /// <param name="group">用例组</param>
        /// <returns></returns>
        public FLStatementInfo[] LocateFaultsInGroup(string methodName, FLRunsGroupInfo group)
        {
            if (null == group)
                return null;
            CalSuspicious(group, methodName);
            //
            return Sort(ref group.Statements);
        }

        #region 镜像方法


        /// <summary>
        /// 用一组用例组jingxiang测试
        /// </summary>
        /// <param name="methodName">可疑度计算公式</param>
        /// <param name="group">用例组</param>
        /// <returns></returns>
        public FLStatementInfo[] LocateFaultsSymmetryInGroup(string methodName, FLRunsGroupInfo group)
        {
            if (null == group)
                return null;
            CalSymmetrySuspicious(group, methodName);
            //
            return Sort(ref group.Statements);
        }


        /// <summary>
        /// 应用排位集成定位缺陷
        /// </summary>
        /// <param name="groups">用例分组</param>
        /// <param name="methodName">算法名称</param>
        /// <param name="cfg">配置</param>
        /// <returns>语句可疑度列表</returns>
        public FLStatementInfo[] LocateFaultSymmetrySort(List<FLRunsGroupInfo> groups, string methodName, FLConfigure cfg)
        {
            // 计算各组可疑度
            CalSymmetrySusOfGroups(groups, methodName);
            // sort each group
            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
                Sort(ref groups[groupIndex].Statements);
            //FLStatementInfo[] result = IntegrateSort(groups, cfg.WeightFormulaId, cfg.IntegrateKernel);
            FLStatementInfo[] result = IntegrateSus(groups, cfg.WeightFormulaId);
            // 排序
            return Sort(ref result);
        }


        /// <summary>
        /// 计算各个用例组的可疑度
        /// </summary>
        /// <param name="mGroups">测试用例组列表</param>
        /// <param name="sMethod">可疑度计算方法</param>
        public void CalSymmetrySusOfGroups(List<FLRunsGroupInfo> mGroups, string sMethod)
        {
            // 确定存在用例组
            if (null == mGroups || 0 == mGroups.Count)
                throw new Exception("没有可用的测试用例组");
            // 遍历个用例组求解
            for (int i = 0; i < mGroups.Count; i++)
                CalSymmetrySuspicious(mGroups[i], sMethod);
        }


        ///// <summary>
        ///// 计算各个用例组的可疑度
        ///// </summary>
        ///// <param name="mGroups">测试用例组列表</param>
        ///// <param name="sMethod">可疑度计算方法</param>
        //public void CalSymmetrySusOfGroups(List<FLRunsGroupInfo> mGroups, string sMethod)
        //{
        //    // 确定存在用例组
        //    if (null == mGroups || 0 == mGroups.Count)
        //        throw new Exception("没有可用的测试用例组");

        //    double[] middleSus = new double[mGroups.Count];
        //    // 遍历个用例组求解
        //    for (int i = 0; i < mGroups.Count; i++)
        //    {
        //        CalSymmetrySuspicious(mGroups[i], sMethod);
        //        List<double> p_list = new List<double>();
        //        // 遍历语句
        //        for (int statementIndex = 0; statementIndex < mGroups[i].NumStatements; statementIndex++)
        //        {
        //            if (!Double.IsNaN(mGroups[i].Statements[statementIndex].suspiciousness1))
        //            {
        //                p_list.Add(mGroups[i].Statements[statementIndex].suspiciousness1);
        //            }
        //        }
        //        double[] new_p_list = p_list.ToArray();
        //        middleSus[i] = GetMiddleNum(new_p_list);
        //    }

        //    for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
        //    {
        //        // 遍历statementIndex
        //        for (int statementIndex = 0; statementIndex < mGroups[groupIndex].NumStatements; statementIndex++)
        //        {
        //            if (Double.IsNaN(mGroups[groupIndex].Statements[statementIndex].suspiciousness1))
        //            {
        //                mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = middleSus[groupIndex];
        //            }
        //        }
        //    }

        //}

        /// <summary>
        /// 计算指定用例组的可疑度
        /// </summary>
        /// <param name="mGroup">测试用例组</param>
        /// <param name="sMethod">可疑度计算方法</param>
        private void CalSymmetrySuspicious(FLRunsGroupInfo mGroup, string sMethod)
        {
            // 获取算法入口
            FLMetrics.MethodDelegate pMethod = FLMetrics.GetFormula(sMethod);

            // 计算语句可疑度
            double P = Convert.ToDouble(mGroup.NumSucCase);
            double F = Convert.ToDouble(mGroup.NumFalCase);
            List<double> p_list = new List<double>();
            // 遍历每条语句计算可疑度
            for (int statementIndex = 0; statementIndex < mGroup.NumStatements; statementIndex++)
            {
                double a_ep = Convert.ToDouble(mGroup.Statements[statementIndex].a_ep);
                double a_ef = Convert.ToDouble(mGroup.Statements[statementIndex].a_ef);
                double a_np = P - a_ep;
                double a_nf = F - a_ef;

                if ((a_ep + a_ef) == 0)
                {
                    mGroup.Statements[statementIndex].suspiciousness1 = Double.NaN;
                }
                else
                {
                    //a_ep = a_ep * F / P;
                    //a_np = a_np * F / P;
                    //a_ef = a_ef * P / F;
                    //a_nf = a_nf * P / F;
                    mGroup.Statements[statementIndex].suspiciousness1 = pMethod(a_ef, a_ep, a_nf, a_np);
                    //mGroup.Statements[statementIndex].suspiciousness1 = pMethod((a_ef + a_np) / 2, a_ep, a_nf, (a_ef + a_np) / 2);  //李成龙改 镜像方法
                    //mGroup.Statements[statementIndex].suspiciousness1 = pMethod((a_ef+a_np)/2, (a_ep+a_nf)/2, (a_nf+a_ep)/2, (a_ef + a_np) / 2);  //李成龙改 镜像方法
                    //mGroup.Statements[statementIndex].suspiciousness1 = pMethod(a_ef, a_ep, a_nf, a_np) + pMethod(a_np, a_nf, a_ep, a_ef);  //李成龙改 镜像方法 */
                    ////mGroup.Statements[statementIndex].suspiciousness1 = pMethod(a_ef, a_ep, a_nf, a_np) - pMethod(a_nf, a_np, a_ef, a_ep);  //李成龙改 镜像方法 */
                    //mGroup.Statements[statementIndex].suspiciousness1 = pMethod(a_ef-a_ep, a_ep+a_nf, a_nf+a_ep, a_np-a_nf);  //李成龙改 镜像方法 */
                    /*mGroup.Statements[statementIndex].suspiciousness1 = pMethod(a_ef, a_ep, a_nf, a_np) - pMethod(a_ep, a_ef, a_np, a_nf);  //李成龙改 镜像方法 */
                    //mGroup.Statements[statementIndex].suspiciousness1 = pMethod(a_ef, a_ep, a_nf, a_np) - pMethod(a_ep, a_ef, a_np, a_nf);  //李成龙改 镜像方法 */
                    ////mGroup.Statements[statementIndex].suspiciousness1 = pMethod(a_ef + a_np, a_ep, a_nf-a_np, a_np);  //李成龙改 镜像方法
                    ////mGroup.Statements[statementIndex].suspiciousness1 = pMethod(a_ef-a_ep, a_ep-a_ef, a_nf+a_ep, a_np+a_ef);  //李成龙改 镜像方法
                    ////mGroup.Statements[statementIndex].suspiciousness1 = pMethod(a_ef - a_ep, 2*a_ep, a_nf - a_np, 2*a_np);  //李成龙改 镜像方法
                    /*mGroup.Statements[statementIndex].suspiciousness1 = pMethod((a_ef + a_np)/2, a_ep + (a_np - a_ef)/2, a_nf + (a_ef - a_np)/2, (a_ef + a_np)/2);  //李成龙改 镜像方法 */
                    ////mGroup.Statements[statementIndex].suspiciousness1 = pMethod((a_ef + a_np) / 2, a_ep + (a_ef - a_np) / 2, a_nf + (a_np - a_ef) / 2, (a_ef + a_np) / 2);  //李成龙改 镜像方法
                    p_list.Add(mGroup.Statements[statementIndex].suspiciousness1);
                }
            }
            double[] new_p_list = p_list.ToArray();
            double ave = new_p_list.Average();
            double min = new_p_list.Min();
            double mid = GetMiddleNum(new_p_list);

            // 遍历每条语句,将
            for (int statementIndex = 0; statementIndex < mGroup.NumStatements; statementIndex++)
            {
                if (new_p_list.Length == 0)
                {
                    if (Double.IsNaN(mGroup.Statements[statementIndex].suspiciousness1))
                    {
                        mGroup.Statements[statementIndex].suspiciousness1 = 1.0 / mGroup.NumStatements;
                    }
                }
                else
                {
                    if (Double.IsNaN(mGroup.Statements[statementIndex].suspiciousness1))
                    {
                        //mGroup.Statements[statementIndex].suspiciousness1 = min;
                        mGroup.Statements[statementIndex].suspiciousness1 = mid; //效果不如min
                        //mGroup.Statements[statementIndex].suspiciousness1 = ave;
                    }
                }
            }



        }


        #endregion


        #region
        /// <summary>
        /// 李成龙添加，根据平均可疑度重新分配用例权重
        /// </summary>
        /// <param name="groups">用例分组</param>
        /// <param name="methodName">算法名称</param>
        /// <param name="cfg">配置</param>
        /// <returns>语句可疑度列表</returns>
        public FLStatementInfo[] LocateFaultsIterateSort(List<FLRunsGroupInfo> groups, string methodName, FLConfigure cfg)
        {
            // 计算各组可疑度
            CalSusOfGroups(groups, methodName);
            // sort each group
            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
                Sort(ref groups[groupIndex].Statements);

            //FLStatementInfo[] result = IntegrateSort(groups, cfg.WeightFormulaId, cfg.IntegrateKernel);
            FLStatementInfo[] result = IntegrateSus(groups, cfg.WeightFormulaId);
            Sort(ref result);

            double[] resultSort = new double[result.Length];
            for (int i = 0; i < result.Length; i++)
            {
                resultSort[i] = result[i].ExpectedSort;
            }

            int iterateNum = 0;
            while (iterateNum <= 100)
            {
                iterateNum += 1;
                //计算各用例组用例的可疑度
                for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
                {
                    double[] aveSucSusp = new double[groups[groupIndex].NumSucCase];
                    //计算某用例组里成功用例平均可疑度
                    for (int SucCaseIndex = 0; SucCaseIndex < groups[groupIndex].NumSucCase; SucCaseIndex++)
                    {
                        double sumSucSusp = 0;
                        int sucCoveredNum = 0;
                        for (int statementIndex = 0; statementIndex < groups[groupIndex].NumStatements; statementIndex++)
                        {
                            if (groups[groupIndex].m_CovMatrix.SucCoverageMetrix[SucCaseIndex][statementIndex])
                            {
                                sucCoveredNum = sucCoveredNum + 1;
                                sumSucSusp = sumSucSusp + groups[groupIndex].Statements[statementIndex].suspiciousness1;
                            }
                        }
                        aveSucSusp[SucCaseIndex] = sumSucSusp / sucCoveredNum;
                    }
                    groups[groupIndex].aveSucSusp = aveSucSusp;

                    //计算某用例组里失败用例平均可疑度
                    double[] aveFalSusp = new double[groups[groupIndex].NumFalCase];
                    for (int FalCaseIndex = 0; FalCaseIndex < groups[groupIndex].NumFalCase; FalCaseIndex++)
                    {
                        double sumFalSusp = 0;
                        int falCoveredNum = 0;
                        for (int statementIndex = 0; statementIndex < groups[groupIndex].NumStatements; statementIndex++)
                        {
                            if (groups[groupIndex].m_CovMatrix.FalCoverageMetrix[FalCaseIndex][statementIndex])
                            {
                                falCoveredNum = falCoveredNum + 1;
                                sumFalSusp = sumFalSusp + groups[groupIndex].Statements[statementIndex].suspiciousness1;
                            }
                        }
                        aveFalSusp[FalCaseIndex] = sumFalSusp / falCoveredNum;
                    }
                    groups[groupIndex].aveFalSusp = aveFalSusp;

                    // 找出统计值
                    double maxSucCaseSusp = groups[groupIndex].aveSucSusp.Max();
                    double q3SucCaseSusp = GetQ3Num(groups[groupIndex].aveSucSusp);
                    double meanSucCaseSusp = groups[groupIndex].aveSucSusp.Average();
                    double middleSucCaseSusp = GetMiddleNum(groups[groupIndex].aveSucSusp);
                    double q1SucCaseSusp = GetQ1Num(groups[groupIndex].aveSucSusp);
                    double minSucCaseSusp = groups[groupIndex].aveSucSusp.Min();
                    double upperboundSucCaseSusp = q3SucCaseSusp + 1.5 * (q3SucCaseSusp - q1SucCaseSusp);
                    double lowerboundSucCaseSusp = q1SucCaseSusp - 1.5 * (q3SucCaseSusp - q1SucCaseSusp);

                    double maxFalCaseSusp = groups[groupIndex].aveFalSusp.Max();
                    double q3FalCaseSusp = GetQ3Num(groups[groupIndex].aveFalSusp);
                    double meanFalCaseSusp = groups[groupIndex].aveFalSusp.Average();
                    double middleFalCaseSusp = GetMiddleNum(groups[groupIndex].aveFalSusp);
                    double q1FalCaseSusp = GetQ1Num(groups[groupIndex].aveFalSusp);
                    double minFalCaseSusp = groups[groupIndex].aveFalSusp.Min();
                    double upperboundFalCaseSusp = q3FalCaseSusp + 1.5 * (q3FalCaseSusp - q1FalCaseSusp);
                    double lowerboundFalCaseSusp = q1FalCaseSusp - 1.5 * (q3FalCaseSusp - q1FalCaseSusp);

                    double difference = Math.Max(-1, Math.Min(1, (middleFalCaseSusp - middleSucCaseSusp) / (upperboundSucCaseSusp - middleSucCaseSusp)));
                    //difference = 1;
                    //计算成功用例下一轮权重
                    double[] sucWeight = new double[groups[groupIndex].NumSucCase];
                    for (int SucCaseIndex = 0; SucCaseIndex < groups[groupIndex].NumSucCase; SucCaseIndex++)
                    {
                        double thisSucSusp = groups[groupIndex].aveSucSusp[SucCaseIndex];
                        if (thisSucSusp >= lowerboundSucCaseSusp && thisSucSusp <= upperboundSucCaseSusp)
                            sucWeight[SucCaseIndex] = 1;
                        else
                        {
                            double theWeight = 1 - difference * (0.5 * H(difference) + (thisSucSusp - upperboundFalCaseSusp / 2 - lowerboundFalCaseSusp / 2) / (upperboundFalCaseSusp - lowerboundFalCaseSusp));
                            if (theWeight > 0 && theWeight < 1)
                                sucWeight[SucCaseIndex] = theWeight;
                            else if (theWeight <= 0)
                                sucWeight[SucCaseIndex] = 0;
                            else
                                sucWeight[SucCaseIndex] = 1;
                        }

                    }
                    groups[groupIndex].sucWeight = sucWeight;

                    //计算失败用例下一轮权重
                    double[] falWeight = new double[groups[groupIndex].NumFalCase];
                    for (int FalCaseIndex = 0; FalCaseIndex < groups[groupIndex].NumFalCase; FalCaseIndex++)
                    {
                        double thisFalSusp = groups[groupIndex].aveFalSusp[FalCaseIndex];
                        if (thisFalSusp >= lowerboundFalCaseSusp && thisFalSusp <= upperboundFalCaseSusp)
                            falWeight[FalCaseIndex] = 1;
                        else
                        {
                            double theWeight = 1 - difference * (0.5 + H(difference) * (upperboundSucCaseSusp / 2 + lowerboundSucCaseSusp / 2 - thisFalSusp) / (upperboundSucCaseSusp - lowerboundSucCaseSusp));
                            if (theWeight > 0 && theWeight < 1)
                                falWeight[FalCaseIndex] = theWeight;
                            else if (theWeight >= 1)
                                falWeight[FalCaseIndex] = 1;
                            else
                                falWeight[FalCaseIndex] = 0;
                        }
                    }
                    groups[groupIndex].falWeight = falWeight;

                    //重新装载aef aep anf anp
                    groups[groupIndex].LoadStaCoverageUnderWeightCases();
                }
                // 根据权重重新计算各组可疑度
                CalSusOfWeightGroups(groups, methodName);

                // sort each group
                for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
                    Sort(ref groups[groupIndex].Statements);

                //FLStatementInfo[] result = IntegrateSort(groups, cfg.WeightFormulaId, cfg.IntegrateKernel);
                result = IntegrateSus(groups, cfg.WeightFormulaId);
                Sort(ref result);

                double[] thisResultSort = new double[result.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    thisResultSort[i] = result[i].ExpectedSort;
                }

                int N = Convert.ToInt32(result.Length * 0.25);
                var topNInFirstResult = resultSort.Select((val, i) => new { val, i })
                                .OrderBy(x => x.val)
                                .ThenBy(x => x.i)
                                .Take(N)
                                .ToArray();
                var topNInSecondResult = thisResultSort.Select((val, i) => new { val, i })
                                .OrderBy(x => x.val)
                                .ThenBy(x => x.i)
                                .Take(N)
                                .ToArray();

                thisResultSort.CopyTo(resultSort, 0);

                if (Enumerable.SequenceEqual(topNInFirstResult, topNInSecondResult))
                    break;

            }

            // 返回排序结果
            return Sort(ref result);
        }

        public double H(double x)
        {
            if (x > 0)
                return 1;
            else if (x == 0)
                return 0;
            else
                return -1;
        }

        public double GetMiddleNum(double[] numlist)
        {
            double middle = 0;
            double[] NewNumlist = numlist.OrderBy(a => a).ToArray(); //将数组从小到大排列
            int count = NewNumlist.Count();
            //求中位数
            if (count == 1)
            {
                middle = NewNumlist[0];
            }
            else if (count / 2 == 0)
            {
                //偶数
                var mindex = count / 2;
                middle = (NewNumlist[mindex - 1] + NewNumlist[mindex]) / 2;
            }
            else
            {
                //奇数
                var mindex = (count + 1) / 2;
                middle = NewNumlist[mindex - 1];
            }

            return middle;

        }

        public double GetQ1Num(double[] numlist)
        {
            double[] NewNumlist = numlist.OrderBy(a => a).ToArray(); //将数组从小到大排列
            int count = NewNumlist.Count();
            int q1_index = (count + 1) / 4;
            double q1 = NewNumlist[q1_index - 1];

            return q1;
        }
        public double GetQ3Num(double[] numlist)
        {
            double[] NewNumlist = numlist.OrderBy(a => a).ToArray(); //将数组从小到大排列
            int count = NewNumlist.Count();
            int q3_index = (3 * (count + 1)) / 4;
            double q3 = NewNumlist[q3_index - 1];

            return q3;
        }

        public double GetMiddleAverage(double[] numlist)
        {
            double[] NewNumlist = numlist.OrderBy(a => a).ToArray(); //将数组从小到大排列
            double count = (double)NewNumlist.Count();

            double avg = numlist.Average();
            double variance = numlist.Sum(x => Math.Pow(x - avg, 2)) / numlist.Count();
            double sigma = Math.Sqrt(variance);
            int q1_num = (int)Math.Floor(count / 4);
            var Middlelist = NewNumlist.Skip(q1_num).Take((int)Math.Ceiling(count / 2));
            double average = Middlelist.Average();
            return average;
        }


        /// <summary>
        /// 计算各个用例组的可疑度
        /// </summary>
        /// <param name="mGroups">测试用例组列表</param>
        /// <param name="sMethod">可疑度计算方法</param>
        public void CalSusOfWeightGroups(List<FLRunsGroupInfo> mGroups, string sMethod)
        {
            // 确定存在用例组
            if (null == mGroups || 0 == mGroups.Count)
                throw new Exception("没有可用的测试用例组");
            // 遍历个用例组求解
            for (int i = 0; i < mGroups.Count; i++)
                CalWeightSuspicious(mGroups[i], sMethod);
        }

        /// <summary>
        /// 计算指定用例组的可疑度
        /// </summary>
        /// <param name="mGroup">测试用例组</param>
        /// <param name="sMethod">可疑度计算方法</param>
        private void CalWeightSuspicious(FLRunsGroupInfo mGroup, string sMethod)
        {
            // 获取算法入口
            FLMetrics.MethodDelegate pMethod = FLMetrics.GetFormula(sMethod);

            //// 计算语句可疑度
            //double P = Convert.ToDouble(mGroup.NumSucCase);
            //double F = Convert.ToDouble(mGroup.NumFalCase);
            // 遍历每条语句计算可疑度
            for (int statementIndex = 0; statementIndex < mGroup.NumStatements; statementIndex++)
            {
                double a_ep = Convert.ToDouble(mGroup.Statements[statementIndex].a_ep);
                double a_ef = Convert.ToDouble(mGroup.Statements[statementIndex].a_ef);
                double a_np = Convert.ToDouble(mGroup.Statements[statementIndex].a_np);
                double a_nf = Convert.ToDouble(mGroup.Statements[statementIndex].a_nf);            //    李成龙改

                mGroup.Statements[statementIndex].suspiciousness1 = pMethod(a_ef, a_ep, a_nf, a_np);
            }
        }


        #endregion




        /// <summary>
        /// 应用排位集成定位缺陷
        /// </summary>
        /// <param name="groups">用例分组</param>
        /// <param name="methodName">算法名称</param>
        /// <param name="cfg">配置</param>
        /// <returns>语句可疑度列表</returns>
        public FLStatementInfo[] LocateFaultsEnsembleSort(List<FLRunsGroupInfo> groups, string methodName, FLConfigure cfg)
        {
            // 计算各组可疑度
            CalSusOfGroups(groups, methodName);
            // sort each group
            //for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            //    Sort(ref groups[groupIndex].Statements);

            if (groups.Count == 1) { Sort(ref groups[0].Statements); }
            else
            {
                Parallel.For(0, groups.Count, groupIndex =>
                {
                    Sort(ref groups[groupIndex].Statements);
                });
            }
                   
            //FLStatementInfo[] result = IntegrateSort(groups, cfg.WeightFormulaId, cfg.IntegrateKernel);
            FLStatementInfo[] result = IntegrateSus(groups, cfg.WeightFormulaId);
            // 排序
            return Sort(ref result);
        }


        /// <summary>
        /// 计算各个用例组的可疑度
        /// </summary>
        /// <param name="mGroups">测试用例组列表</param>
        /// <param name="sMethod">可疑度计算方法</param>
        public void CalSusOfGroups(List<FLRunsGroupInfo> mGroups, string sMethod)
        {
            // 确定存在用例组
            if (null == mGroups || 0 == mGroups.Count)
                throw new Exception("没有可用的测试用例组");
            // 遍历个用例组求解
            for (int i = 0; i < mGroups.Count; i++)
                CalSuspicious(mGroups[i], sMethod);
        }

        /// <summary>
        /// 计算指定用例组的可疑度
        /// </summary>
        /// <param name="mGroup">测试用例组</param>
        /// <param name="sMethod">可疑度计算方法</param>
        private void CalSuspicious(FLRunsGroupInfo mGroup, string sMethod)
        {
            // 获取算法入口
            FLMetrics.MethodDelegate pMethod = FLMetrics.GetFormula(sMethod);

            // 计算语句可疑度
            double P = Convert.ToDouble(mGroup.NumSucCase);
            double F = Convert.ToDouble(mGroup.NumFalCase);
            // 遍历每条语句计算可疑度
            for (int statementIndex = 0; statementIndex < mGroup.NumStatements; statementIndex++)
            {
                double a_ep = Convert.ToDouble(mGroup.Statements[statementIndex].a_ep);
                double a_ef = Convert.ToDouble(mGroup.Statements[statementIndex].a_ef);
                double a_np = P - a_ep;
                double a_nf = F - a_ef;

                //double a_ep = mGroup.Statements[statementIndex].a_epC;
                //double a_ef = mGroup.Statements[statementIndex].a_efC;
                //double a_np = mGroup.Statements[statementIndex].a_npC;
                //double a_nf = mGroup.Statements[statementIndex].a_nfC;            #    李成龙改
                //double P = mGroup.Statements[statementIndex].PC;
                //double F = mGroup.Statements[statementIndex].FC;
                //double P = a_ep + a_np;
                //double F = a_ef + a_nf;

                mGroup.Statements[statementIndex].suspiciousness1 = pMethod(a_ef, a_ep, a_nf, a_np);
            }
        }

        private string bools2string(bool[] bools)
        {
            string the_case = "";
            foreach (bool cov in bools)
            {
                if (cov)
                    the_case = the_case + '1';
                else
                    the_case = the_case + '0';
            }
            return the_case;
        }


        #region 可疑度集成
        /// <summary>
        /// 可疑度集成
        /// </summary>
        /// <param name="mGroups">已经计算了可疑度的测试用例组</param>
        /// <param name="iMethodIndex">可疑度计算方法</param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus(List<FLRunsGroupInfo> mGroups, int iMethodIndex)
        {
            IntegrateDelegate pIntegrate = GetIntegrate(iMethodIndex);
            return pIntegrate(mGroups);
        }

        #region 可疑度集成准备工作

        // 集成公式委托
        public delegate FLStatementInfo[] IntegrateDelegate(List<FLRunsGroupInfo> mGroups);

        // 集成公式委托查询列表
        private List<IntegrateDelegate> IntegrateList = null;

        // 初始化集成公式委托查询列表
        private void InitialDelegateList()
        {
            IntegrateList = new List<IntegrateDelegate>();

            IntegrateList.Add(IntegrateSus0);   // a_ef / (a_ep * (a_ef + a_ep))  
            IntegrateList.Add(IntegrateSus1);   // a_ef / a_ep
            IntegrateList.Add(IntegrateSus2);   // 等概率权值
            IntegrateList.Add(IntegrateSus3);   // a_ef
            IntegrateList.Add(IntegrateSus4);   // a_ef / (a_ef + a_ep)
            IntegrateList.Add(IntegrateSus5);   // a_np / (A_np)

            IntegrateList.Add(IntegrateSus6);   // a_np
            IntegrateList.Add(IntegrateSus7);   // 成例覆盖F
            IntegrateList.Add(IntegrateSus8);   // 成例不覆盖F
            IntegrateList.Add(IntegrateSus9);   // 成例覆盖F-P
            IntegrateList.Add(IntegrateSus10);  // 成例不覆盖F-P
            IntegrateList.Add(IntegrateSus11);  // 最大可疑度 李成龙改
            IntegrateList.Add(IntegrateSus12);  // 最大可疑度 李成龙改
            IntegrateList.Add(IntegrateSus13);  // 最大可疑度 李成龙改
            IntegrateList.Add(IntegrateSus14);  // 中间二分之一平均可疑度 李成龙改
            IntegrateList.Add(IntegrateSus15);  // a_ef+a_ep权值集成 李成龙改
            IntegrateList.Add(IntegrateSus16);  // a_ef+a_ep权值集成 李成龙改
            IntegrateList.Add(IntegrateSus17);
            IntegrateList.Add(IntegrateSus18);
            IntegrateList.Add(IntegrateSus19);
            IntegrateList.Add(IntegrateSus20);
            IntegrateList.Add(IntegrateSus21);
            IntegrateList.Add(IntegrateSus22);
            IntegrateList.Add(IntegrateSus23);
            IntegrateList.Add(IntegrateSus24);
            IntegrateList.Add(IntegrateSus25);
            IntegrateList.Add(IntegrateSus26);
        }

        // 获取一个集成公式
        private IntegrateDelegate GetIntegrate(int iMethodIndex)
        {
            if (null == IntegrateList)
            {
                InitialDelegateList();
            }

            return new IntegrateDelegate(IntegrateList[iMethodIndex]);
        }
        #endregion





        #region 可疑度集成权值公式



        public static bool ContainsInfinity(double[] inputArray)
        {
            foreach (double element in inputArray)
            {
                if (double.IsInfinity(element))
                {
                    return true;
                }
            }

            return false;
        }

        public static double[] GetInfinityArray(double[] inputArray)
        {
            double[] outputArray = new double[inputArray.Length];

            for (int i = 0; i < inputArray.Length; i++)
            {
                if (double.IsInfinity(inputArray[i]))
                {
                    outputArray[i] = 1;
                }
                else
                {
                    outputArray[i] = 0;
                }
            }

            return outputArray;
        }


        /// <summary>
        /// 根据用例集的重复度作为权重 - 可疑度集成 李成龙添加
        /// </summary>
        public FLStatementInfo[] IntegrateSus26(List<FLRunsGroupInfo> mGroups)
        {

            double CosineSimilarity(List<bool[]> vectorA, List<bool[]> vectorB)
            {

                double[] averageVector1 = CalculateAverageVector(vectorA);
                double[] averageVector2 = CalculateAverageVector(vectorB);

                if (averageVector1.Length != averageVector2.Length)
                    throw new ArgumentException("Vectors must be of the same length.");

                double dotProduct = 0.0;
                double normA = 0.0;
                double normB = 0.0;

                for (int i = 0; i < averageVector1.Length; i++)
                {
                    dotProduct += averageVector1[i] * averageVector2[i];
                    normA += averageVector1[i] * averageVector1[i];
                    normB += averageVector2[i] * averageVector2[i];
                }

                return (dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB)) + 1) / 2;
            }


            double[] CalculateAverageVector(List<bool[]> vectors)
            {
                if (vectors == null || vectors.Count == 0)
                    throw new ArgumentException("Vector list is null or empty.");

                int dimensions = vectors[0].Length;
                double[] averageVector = new double[dimensions];

                foreach (bool[] vector in vectors)
                {
                    for (int i = 0; i < dimensions; i++)
                    {
                        averageVector[i] += vector[i] ? 1 : 0;
                    }
                }

                for (int i = 0; i < dimensions; i++)
                {
                    averageVector[i] /= vectors.Count;
                }

                return averageVector;
            }

            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;

            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];
            double[] sumSus = new double[mGroups.Count];
            double[] maxSus = new double[mGroups.Count];
            double[] minSus = new double[mGroups.Count];
            // 依次计算每组各个语句中可疑度的最大值、最小值以及总和
            Parallel.For(0, mGroups.Count, groupIndex =>
            {
                maxSus[groupIndex] = Double.MinValue;
                minSus[groupIndex] = Double.MaxValue;
                // 遍历语句
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 > maxSus[groupIndex])
                    { maxSus[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1; }

                    if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 < minSus[groupIndex])
                    { minSus[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1; }
                }
            });


            // 标准化
            Parallel.For(0, mGroups.Count, groupIndex =>
            {
                // 遍历mGroups
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    if ((maxSus[groupIndex] - minSus[groupIndex]) != 0)
                    {
                        mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 - minSus[groupIndex]) / (maxSus[groupIndex] - minSus[groupIndex]);
                    }

                    sumSus[groupIndex] += mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            });


            // 归一化
            double[] weight = new double[mGroups.Count];

            Parallel.For(0, mGroups.Count, groupIndex =>
            {
                double h = 0;
                // 遍历mGroups
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    if (sumSus[groupIndex] == 0)
                    {
                        mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = 1.0 / iNumStatements;
                    }
                    else
                    {
                        mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = mGroups[groupIndex].Statements[statementIndex].suspiciousness1 / sumSus[groupIndex];
                    }

                    if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 != 0)
                    {
                        h = h - mGroups[groupIndex].Statements[statementIndex].suspiciousness1 * Math.Log(mGroups[groupIndex].Statements[statementIndex].suspiciousness1, 2);
                    }
                }
                var selectedSuc = mGroups[groupIndex].SucCaseIDs.Where(index => index >= 0 && index < mGroups[groupIndex].m_CovMatrix.SucCoverageMetrix.Count)
                                   .Select(index => mGroups[groupIndex].m_CovMatrix.SucCoverageMetrix[index])
                                   .ToList();
                var selectedFal = mGroups[groupIndex].FalCaseIDs.Where(index => index >= 0 && index < mGroups[groupIndex].m_CovMatrix.FalCoverageMetrix.Count)
                   .Select(index => mGroups[groupIndex].m_CovMatrix.FalCoverageMetrix[index])
                   .ToList();

                weight[groupIndex] = 1.0 - CosineSimilarity(selectedSuc, selectedFal);
            });

            // 依次计算各个语句的加权和
            Parallel.For(0, iNumStatements, statementIndex =>
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    if (sumSus[groupIndex] == 0)
                    {
                        result[statementIndex].suspiciousness1 += weight[groupIndex] * 1.0 / iNumStatements;
                    }
                    else
                    {
                        result[statementIndex].suspiciousness1 += weight[groupIndex] * mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                    }
                }

            });

            return result;
        }

        /// <summary>
        /// 根据用例集的重复度作为权重 - 可疑度集成 李成龙添加
        /// </summary>
        public FLStatementInfo[] IntegrateSus25(List<FLRunsGroupInfo> mGroups)
        {

            //double CalculateSimilarity(List<bool[]> set1, List<bool[]> set2)
            //{
            //    HashSet<bool[]> commonPoints = new HashSet<bool[]>(set1);
            //    commonPoints.IntersectWith(set2);

            //    int totalPoints = set1.Count + set2.Count;
            //    int commonPointsCount = commonPoints.Count;

            //    // 计算重复度
            //    return totalPoints == 0 ? 0 : (2.0 * commonPointsCount) / totalPoints;
            //}

            double CosineSimilarity(List<bool[]> vectorA, List<bool[]> vectorB)
            {

                double[] averageVector1 = CalculateAverageVector(vectorA);
                double[] averageVector2 = CalculateAverageVector(vectorB);

                if (averageVector1.Length != averageVector2.Length)
                    throw new ArgumentException("Vectors must be of the same length.");

                double dotProduct = 0.0;
                double normA = 0.0;
                double normB = 0.0;

                for (int i = 0; i < averageVector1.Length; i++)
                {
                    dotProduct += averageVector1[i] * averageVector2[i];
                    normA += averageVector1[i] * averageVector1[i];
                    normB += averageVector2[i] * averageVector2[i];
                }

                return (dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB)) + 1) / 2;
            }


            double[] CalculateAverageVector(List<bool[]> vectors)
            {
                if (vectors == null || vectors.Count == 0)
                    throw new ArgumentException("Vector list is null or empty.");

                int dimensions = vectors[0].Length;
                double[] averageVector = new double[dimensions];

                foreach (bool[] vector in vectors)
                {
                    for (int i = 0; i < dimensions; i++)
                    {
                        averageVector[i] += vector[i] ? 1 : 0;
                    }
                }

                for (int i = 0; i < dimensions; i++)
                {
                    averageVector[i] /= vectors.Count;
                }

                return averageVector;
            }


            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 归一化
            double[] weight = new double[mGroups.Count];
            // 遍历mGroups
            Parallel.For(0, mGroups.Count, groupIndex =>
            {
                var selectedSuc = mGroups[groupIndex].SucCaseIDs.Where(index => index >= 0 && index < mGroups[groupIndex].m_CovMatrix.SucCoverageMetrix.Count)
                                   .Select(index => mGroups[groupIndex].m_CovMatrix.SucCoverageMetrix[index])
                                   .ToList();
                var selectedFal = mGroups[groupIndex].FalCaseIDs.Where(index => index >= 0 && index < mGroups[groupIndex].m_CovMatrix.FalCoverageMetrix.Count)
                   .Select(index => mGroups[groupIndex].m_CovMatrix.FalCoverageMetrix[index])
                   .ToList();

                weight[groupIndex] = 1.0 - CosineSimilarity(selectedSuc, selectedFal);
            });

            // 依次计算各个语句的加权和
            Parallel.For(0, iNumStatements, statementIndex =>
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    result[statementIndex].suspiciousness1 += weight[groupIndex] * mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }

            });

            return result;
        }

        /// <summary>
        /// 根据用例集的重复度作为权重 - 可疑度集成 李成龙添加
        /// </summary>
        public FLStatementInfo[] IntegrateSus24(List<FLRunsGroupInfo> mGroups)
        {

            //double CalculateSimilarity(List<bool[]> set1, List<bool[]> set2)
            //{
            //    HashSet<bool[]> commonPoints = new HashSet<bool[]>(set1);
            //    commonPoints.IntersectWith(set2);

            //    int totalPoints = set1.Count + set2.Count;
            //    int commonPointsCount = commonPoints.Count;

            //    // 计算重复度
            //    return totalPoints == 0 ? 0 : (2.0 * commonPointsCount) / totalPoints;
            //}

            double CalculateSimilarity(List<bool[]> set1, List<bool[]> set2)
            {
                var set1Hashes = set1.Select(arr => ComputeHash(arr)).ToList();
                var set2Hashes = set2.Select(arr => ComputeHash(arr)).ToList();

                int commonHashes = set1Hashes.Intersect(set2Hashes).Count();
                int totalHashes = set1Hashes.Count + set2Hashes.Count;

                // return totalHashes == 0 ? 0 : (2.0 * commonHashes) / totalHashes;
                return totalHashes == 0 ? 0 : (2.0 * commonHashes) / Math.Min(set1Hashes.Count, set2Hashes.Count);
            }

            string ComputeHash(bool[] array)
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    // 将布尔数组转换为字符串
                    var arrayString = string.Join("", array.Select(b => b ? "1" : "0"));
                    var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(arrayString));
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }

            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 归一化
            double[] weight = new double[mGroups.Count];
            // 遍历mGroups
            Parallel.For(0, mGroups.Count, groupIndex =>
            {
                var selectedSuc = mGroups[groupIndex].SucCaseIDs.Where(index => index >= 0 && index < mGroups[groupIndex].m_CovMatrix.SucCoverageMetrix.Count)
                                   .Select(index => mGroups[groupIndex].m_CovMatrix.SucCoverageMetrix[index])
                                   .ToList();
                var selectedFal = mGroups[groupIndex].FalCaseIDs.Where(index => index >= 0 && index < mGroups[groupIndex].m_CovMatrix.FalCoverageMetrix.Count)
                   .Select(index => mGroups[groupIndex].m_CovMatrix.FalCoverageMetrix[index])
                   .ToList();

                weight[groupIndex] = 1.0 - CalculateSimilarity(selectedSuc, selectedFal);
            });

            // 依次计算各个语句的加权和
            Parallel.For(0, iNumStatements, statementIndex =>
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    result[statementIndex].suspiciousness1 += weight[groupIndex] * mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }

            });

            return result;
        }

        /// <summary>
        /// 根据用例集的熵作为权重 - 可疑度集成 李成龙添加
        /// </summary>
        public FLStatementInfo[] IntegrateSus21(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;

            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];
            double[] sumSus = new double[mGroups.Count];
            double[] maxSus = new double[mGroups.Count];
            double[] minSus = new double[mGroups.Count];
            // 依次计算每组各个语句中可疑度的最大值、最小值以及总和
            Parallel.For(0, mGroups.Count, groupIndex =>
            {
                maxSus[groupIndex] = Double.MinValue;
                minSus[groupIndex] = Double.MaxValue;
                // 遍历语句
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 > maxSus[groupIndex])
                    { maxSus[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1; }

                    if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 < minSus[groupIndex])
                    { minSus[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1; }
                }
            });


            // 标准化
            Parallel.For(0, mGroups.Count, groupIndex =>
            {
                // 遍历mGroups
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                        if ((maxSus[groupIndex] - minSus[groupIndex]) != 0)
                        {
                            mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 - minSus[groupIndex]) / (maxSus[groupIndex] - minSus[groupIndex]);
                        }

                        sumSus[groupIndex] += mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            });


            // 归一化
            double[] weight = new double[mGroups.Count];

            Parallel.For(0, mGroups.Count, groupIndex =>
            {
                double h = 0;
                // 遍历mGroups
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    if (sumSus[groupIndex] == 0)
                    {
                        mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = 1.0 / iNumStatements;
                    }
                    else
                    {
                        mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = mGroups[groupIndex].Statements[statementIndex].suspiciousness1 / sumSus[groupIndex];
                    }

                    if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 != 0)
                    {
                        h = h - mGroups[groupIndex].Statements[statementIndex].suspiciousness1 * Math.Log(mGroups[groupIndex].Statements[statementIndex].suspiciousness1, 2);
                    }
                }
                weight[groupIndex] = 1 / h;
                // weight[groupIndex] = 1 / (h + Double.Epsilon);
            });

            if (ContainsInfinity(weight)) { weight = GetInfinityArray(weight);}

            // 依次计算各个语句的加权和
            Parallel.For(0, iNumStatements, statementIndex =>
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    if (sumSus[groupIndex] == 0)
                    {
                        result[statementIndex].suspiciousness1 += weight[groupIndex] * 1.0 / iNumStatements;
                    }
                    else
                    {
                        result[statementIndex].suspiciousness1 += weight[groupIndex] * mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                    }
                }

            });

            return result;
        }


        //public FLStatementInfo[] IntegrateSus21(List<FLRunsGroupInfo> mGroups)
        //{
        //    // 获取语句数
        //    int iNumStatements = mGroups[0].NumStatements;

        //    // 建立输出内存
        //    FLStatementInfo[] result = new FLStatementInfo[iNumStatements];
        //    double[] sumSus = new double[mGroups.Count];
        //    double[] maxSus = new double[mGroups.Count];
        //    double[] minSus = new double[mGroups.Count];
        //    // 依次计算每组各个语句中可疑度的最大值、最小值以及总和
        //    for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
        //    {
        //        maxSus[groupIndex] = Double.MinValue;
        //        minSus[groupIndex] = Double.MaxValue;
        //        // 遍历语句
        //        for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
        //        {
        //            if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 > maxSus[groupIndex])
        //            { maxSus[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1; }

        //            if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 < minSus[groupIndex])
        //            { minSus[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1; }
        //        }
        //    }

        //    // 标准化
        //    for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
        //    {
        //        // 遍历mGroups
        //        for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
        //        {
        //            if ((maxSus[groupIndex] - minSus[groupIndex]) != 0)
        //            {
        //                mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 - minSus[groupIndex]) / (maxSus[groupIndex] - minSus[groupIndex]);
        //            }

        //            sumSus[groupIndex] += mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
        //        }
        //    }

        //    // 归一化
        //    double[] weight = new double[mGroups.Count];
        //    for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
        //    {

        //        double h = 0;
        //        for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
        //        {
        //            if (sumSus[groupIndex] == 0)
        //            {
        //                mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = 1.0 / iNumStatements;
        //            }
        //            else
        //            {
        //                mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = mGroups[groupIndex].Statements[statementIndex].suspiciousness1 / sumSus[groupIndex];
        //            }

        //            if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 != 0)
        //            {
        //                h = h - mGroups[groupIndex].Statements[statementIndex].suspiciousness1 * Math.Log(mGroups[groupIndex].Statements[statementIndex].suspiciousness1, 2);
        //            }
        //        }
        //        weight[groupIndex] = 1.0 / h;

        //    }

        //    // 依次计算各个语句的加权和
        //    for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
        //    {
        //        result[statementIndex] = new FLStatementInfo();
        //        result[statementIndex].ID = statementIndex;
        //        result[statementIndex].suspiciousness1 = 0;
        //        result[statementIndex].suspiciousness2 = 0;
        //        // 遍历mGroups
        //        for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
        //        {
        //            if (sumSus[groupIndex] == 0)
        //            {
        //                result[statementIndex].suspiciousness1 += weight[groupIndex] * 1.0 / iNumStatements;
        //            }
        //            else
        //            {
        //                result[statementIndex].suspiciousness1 += weight[groupIndex] * mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
        //            }
        //        }
        //    }

        //    return result;
        //}


        /// <summary>
        /// 等概率权值 - 标准化后可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus22(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];
            double[] sumSus = new double[mGroups.Count];
            double[] maxSus = new double[mGroups.Count];
            double[] minSus = new double[mGroups.Count];

            // 依次计算每组各个语句中可疑度的最大值、最小值、中位值以及归一化后的总和
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                List<double> p_list = new List<double>();
                // 遍历语句
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    p_list.Add(mGroups[groupIndex].Statements[statementIndex].suspiciousness1);
                }
                double[] new_p_list = p_list.ToArray();
                maxSus[groupIndex] = new_p_list.Max();
                minSus[groupIndex] = new_p_list.Min();
            }

            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                // 遍历statementIndex
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    if ((maxSus[groupIndex] - minSus[groupIndex]) != 0)
                    {
                        mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 - minSus[groupIndex]) / (maxSus[groupIndex] - minSus[groupIndex]);
                    }
                    sumSus[groupIndex] += mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            }

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    if (sumSus[groupIndex] == 0)
                    {
                        result[statementIndex].suspiciousness1 += 1.0 / iNumStatements;
                    }
                    else
                    {
                        result[statementIndex].suspiciousness1 += mGroups[groupIndex].Statements[statementIndex].suspiciousness1 / sumSus[groupIndex];
                    }
                }
            }

            return result;
        }










        /// <summary>
        /// 根据用例集的熵作为权重 - 可疑度集成  李成龙添加
        /// </summary>
        public FLStatementInfo[] IntegrateSus23(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;

            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];
            double[] sumSus = new double[mGroups.Count];
            double[] maxSus = new double[mGroups.Count];
            double[] minSus = new double[mGroups.Count];
            // 依次计算每组各个语句中可疑度的最大值、最小值以及总和
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                maxSus[groupIndex] = Double.MinValue;
                minSus[groupIndex] = Double.MaxValue;
                // 遍历语句
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 > maxSus[groupIndex])
                    { maxSus[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1; }

                    if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 < minSus[groupIndex])
                    { minSus[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1; }
                }
            }

            // 标准化
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                // 遍历mGroups
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    if ((maxSus[groupIndex] - minSus[groupIndex]) != 0)
                    {
                        mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 - minSus[groupIndex]) / (maxSus[groupIndex] - minSus[groupIndex]);
                    }

                    sumSus[groupIndex] += mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            }

            // 归一化
            double[] weight = new double[mGroups.Count];
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {

                double h = 0;
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    if (sumSus[groupIndex] == 0)
                    {
                        mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = 1.0 / iNumStatements;
                    }
                    else
                    {
                        mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = mGroups[groupIndex].Statements[statementIndex].suspiciousness1 / sumSus[groupIndex];
                    }

                    if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 != 0)
                    {
                        h = h - mGroups[groupIndex].Statements[statementIndex].suspiciousness1 * Math.Log(mGroups[groupIndex].Statements[statementIndex].suspiciousness1, 2);
                    }
                }
                weight[groupIndex] = h;

            }

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    if (sumSus[groupIndex] == 0)
                    {
                        result[statementIndex].suspiciousness1 += weight[groupIndex] * 1.0 / iNumStatements;
                    }
                    else
                    {
                        result[statementIndex].suspiciousness1 += weight[groupIndex] * mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                    }
                }
            }

            return result;
        }



        /// <summary>
        /// 根据用例集的密度作为权重 - 可疑度集成  李成龙添加
        /// </summary>
        public FLStatementInfo[] IntegrateSus20(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;

            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 遍历mGroups
            double[] weight = new double[mGroups.Count];
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                double A_e = 0;
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {

                    A_e += mGroups[groupIndex].Statements[statementIndex].a_ef + mGroups[groupIndex].Statements[statementIndex].a_ep;

                }

                weight[groupIndex] = 1 - Math.Abs(0.5 - A_e / (iNumStatements * (mGroups[groupIndex].NumFalCase + mGroups[groupIndex].NumSucCase)));
            }

            double[] sumSus = new double[mGroups.Count];
            double[] maxSus = new double[mGroups.Count];
            double[] minSus = new double[mGroups.Count];
            // 依次计算每组各个语句中可疑度的最大值、最小值以及总和
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                maxSus[groupIndex] = Double.MinValue;
                minSus[groupIndex] = Double.MaxValue;
                // 遍历语句
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 > maxSus[groupIndex])
                    { maxSus[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1; }

                    if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 < minSus[groupIndex])
                    { minSus[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1; }
                }
            }

            // 依次计算各个语句的加权和
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                // 遍历mGroups
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    if ((maxSus[groupIndex] - minSus[groupIndex]) != 0)
                    {
                        mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 - minSus[groupIndex]) / (maxSus[groupIndex] - minSus[groupIndex]);
                    }

                    sumSus[groupIndex] += mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            }

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    if (sumSus[groupIndex] == 0)
                    {
                        result[statementIndex].suspiciousness1 += weight[groupIndex] * 1.0 / iNumStatements;
                    }
                    else
                    {
                        result[statementIndex].suspiciousness1 += weight[groupIndex] * mGroups[groupIndex].Statements[statementIndex].suspiciousness1 / sumSus[groupIndex];
                    }
                }
            }

            return result;
        }



        /// <summary>
        /// 等概率权值 - 标准化后可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus19(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];
            double[] sumSus = new double[mGroups.Count];
            double[] maxSus = new double[mGroups.Count];
            double[] minSus = new double[mGroups.Count];
            // 依次计算每组各个语句中可疑度的最大值、最小值以及总和
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                maxSus[groupIndex] = Double.MinValue;
                minSus[groupIndex] = Double.MaxValue;
                // 遍历语句
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 > maxSus[groupIndex])
                    { maxSus[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1; }

                    if (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 < minSus[groupIndex])
                    { minSus[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1; }
                }
            }
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                // 遍历mGroups
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    if ((maxSus[groupIndex] - minSus[groupIndex]) != 0)
                    {
                        mGroups[groupIndex].Statements[statementIndex].suspiciousness1 = (mGroups[groupIndex].Statements[statementIndex].suspiciousness1 - minSus[groupIndex]) / (maxSus[groupIndex] - minSus[groupIndex]);
                    }

                    sumSus[groupIndex] += mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            }

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    if (sumSus[groupIndex] == 0)
                    {
                        result[statementIndex].suspiciousness1 += 1.0 / iNumStatements;
                    }
                    else
                    {
                        result[statementIndex].suspiciousness1 += mGroups[groupIndex].Statements[statementIndex].suspiciousness1 / sumSus[groupIndex];
                    }
                }
            }

            return result;
        }



        /// <summary>
        /// 根据用例的不同作为权重 - 排位集成  李成龙添加
        /// </summary>
        public FLStatementInfo[] IntegrateSus18(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;

            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 遍历mGroups
            double[] weight = new double[mGroups.Count];
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                Dictionary<string, int> dic = new Dictionary<string, int>();
                for (int sucIndex = 0; sucIndex < mGroups[groupIndex].NumSucCase; sucIndex++)
                {
                    string the_case = bools2string(mGroups[groupIndex].m_CovMatrix.SucCoverageMetrix[mGroups[groupIndex].SucCaseIDs[sucIndex]]);
                    if (dic.Keys.Contains(the_case))
                    {
                        dic[the_case]++;
                    }
                    else
                    {
                        dic.Add(the_case, 1);

                    }
                }

                //Dictionary<string, int> dic2 = new Dictionary<string, int>();
                for (int falIndex = 0; falIndex < mGroups[groupIndex].NumFalCase; falIndex++)
                {
                    string the_case = bools2string(mGroups[groupIndex].m_CovMatrix.FalCoverageMetrix[mGroups[groupIndex].FalCaseIDs[falIndex]]);
                    if (dic.Keys.Contains(the_case))
                    {
                        dic[the_case]++;
                    }
                    else
                    {
                        dic.Add(the_case, 1);
                    }

                }
                //weight[groupIndex] = Convert.ToDouble(dic.Count) + Convert.ToDouble(dic2.Count);
                weight[groupIndex] = Convert.ToDouble(dic.Count);
            }

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {

                    result[statementIndex].suspiciousness1 += weight[groupIndex]
                        * mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            }

            return result;
        }





        /// <summary>
        /// 根据可区分度作为权重 - 排位集成  李成龙添加
        /// </summary>
        public FLStatementInfo[] IntegrateSus17(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 遍历mGroups
            double[] weight = new double[mGroups.Count];
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                Dictionary<string, int> dic = new Dictionary<string, int>();
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    string spectra = string.Format("{0}#{1}#{2}#{3}", mGroups[groupIndex].Statements[statementIndex].a_ef, mGroups[groupIndex].Statements[statementIndex].a_ep, mGroups[groupIndex].Statements[statementIndex].a_nf, mGroups[groupIndex].Statements[statementIndex].a_np);
                    if (dic.Keys.Contains(spectra))
                    {
                        dic[spectra]++;
                    }
                    else
                    {
                        dic.Add(spectra, 1);

                    }
                }
                weight[groupIndex] = Convert.ToDouble(dic.Count);
            }

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    result[statementIndex].suspiciousness1 += weight[groupIndex]
                        * mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            }

            return result;
        }


        /// <summary>
        /// 等概率权值 - 可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus2(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    result[statementIndex].suspiciousness1 += mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            }

            return result;
        }

        /// <summary>
        /// a_ef / a_ep - 可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus1(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    FLStatementInfo temp = mGroups[groupIndex].Statements[statementIndex];
                    // 计算权值
                    double weight;
                    if (temp.a_ep == 0)
                    {
                        weight = temp.a_ef;
                    }
                    else
                    {
                        weight = Convert.ToDouble(temp.a_ef) / Convert.ToDouble(temp.a_ep);
                    }
                    // 加权求和
                    result[statementIndex].suspiciousness1 += weight * temp.suspiciousness1;
                }
            }

            return result;
        }

        /// <summary>
        /// a_ef / (a_ep * (a_ef + a_ep)) - 可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus0(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    FLStatementInfo temp = mGroups[groupIndex].Statements[statementIndex];
                    // 计算权值
                    double weight;
                    if (temp.a_ep == 0)
                    {
                        weight = temp.a_ef;
                    }
                    else
                    {
                        weight = Convert.ToDouble(temp.a_ef) / (Convert.ToDouble(temp.a_ep) * Convert.ToDouble(temp.a_ef + temp.a_ep));
                    }
                    // 加权求和
                    result[statementIndex].suspiciousness1 += weight * temp.suspiciousness1;
                }
            }

            return result;
        }

        /// <summary>
        /// a_ef - 可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus3(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    FLStatementInfo temp = mGroups[groupIndex].Statements[statementIndex];
                    // 计算权值
                    double weight = Convert.ToDouble(temp.a_ef);
                    // 加权求和
                    result[statementIndex].suspiciousness1 += weight * temp.suspiciousness1;
                }
            }

            return result;
        }

        /// <summary>
        /// a_ef / (a_ef + a_ep) - 可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus4(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    FLStatementInfo temp = mGroups[groupIndex].Statements[statementIndex];
                    // 计算权值
                    double weight;
                    if ((temp.a_ef + temp.a_ep) == 0)
                    {
                        weight = temp.a_ef;
                    }
                    else
                    {
                        weight = Convert.ToDouble(temp.a_ef) / Convert.ToDouble(temp.a_ef + temp.a_ep);
                    }
                    // 加权求和
                    result[statementIndex].suspiciousness1 += weight * temp.suspiciousness1;
                }
            }

            return result;
        }

        /// <summary>
        /// a_np / (A_np) - 可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus5(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;

                // 计算参与集成的用例组中总体的A_np
                double A_np = 0;
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    A_np += mGroups[groupIndex].Statements[statementIndex].a_np;
                }

                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    FLStatementInfo temp = mGroups[groupIndex].Statements[statementIndex];
                    // 计算权值
                    double weight;
                    // 如果一条语句被所有成例覆盖则不具有可疑度
                    if (0 == A_np)
                    {
                        weight = 0.0;
                    }
                    else
                    {
                        // 以考察用例组中没有覆盖考察语句的成例数占总体成例的比例
                        weight = Convert.ToDouble(temp.a_np) / Convert.ToDouble(A_np);
                    }

                    // 加权求和
                    result[statementIndex].suspiciousness1 += weight * temp.suspiciousness1;
                }
            }

            return result;
        }

        /// <summary>
        /// a_np - 可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus6(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    FLStatementInfo temp = mGroups[groupIndex].Statements[statementIndex];
                    // 计算权值
                    double weight = Convert.ToDouble(temp.a_np);
                    // 加权求和
                    result[statementIndex].suspiciousness1 += weight * temp.suspiciousness1;
                }
            }

            return result;
        }

        /// <summary>
        /// 成例覆盖F - 可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus7(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];
            for (int statementIndex = 0; statementIndex < result.Length; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
            }

            // 针对每个用例组
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                // 计算该用例组的权值
                double weight = 0;
                // 遍历每一条语句
                for (int statementIndex = 0; statementIndex < mGroups[groupIndex].NumStatements; statementIndex++)
                {
                    // 查看是否被所有的失例所覆盖
                    if (mGroups[groupIndex].Statements[statementIndex].a_ef == mGroups[groupIndex].NumFalCase)
                    {
                        // 如果是 增加权值
                        weight += mGroups[groupIndex].Statements[statementIndex].a_ep;
                    }
                }

                // 依次计算每个语句的加权和
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    result[statementIndex].suspiciousness1 += weight * mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            }

            return result;
        }

        /// <summary>
        /// 成例不覆盖F - 可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus8(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];
            for (int statementIndex = 0; statementIndex < result.Length; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
            }

            // 针对每个用例组
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                // 计算该用例组的权值
                double weight = 0;
                // 遍历每一条语句
                for (int statementIndex = 0; statementIndex < mGroups[groupIndex].NumStatements; statementIndex++)
                {
                    // 查看是否被所有的失例所覆盖
                    if (mGroups[groupIndex].Statements[statementIndex].a_ef == mGroups[groupIndex].NumFalCase)
                    {
                        // 如果是 增加权值
                        weight += mGroups[groupIndex].Statements[statementIndex].a_np;
                    }
                }

                // 依次计算每个语句的加权和
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    result[statementIndex].suspiciousness1 += weight * mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            }

            return result;
        }

        /// <summary>
        /// 成例覆盖F-P - 可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus9(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];
            for (int statementIndex = 0; statementIndex < result.Length; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
            }

            // 针对每个用例组
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                // 计算该用例组的权值
                double weight = 0;
                // 遍历每一条语句
                for (int statementIndex = 0; statementIndex < mGroups[groupIndex].NumStatements; statementIndex++)
                {
                    // 查看是否被所有的失例所覆盖
                    if ((mGroups[groupIndex].Statements[statementIndex].a_ef == mGroups[groupIndex].NumFalCase)
                     && (mGroups[groupIndex].Statements[statementIndex].a_ep != mGroups[groupIndex].NumSucCase))
                    {
                        // 如果是 增加权值
                        weight += mGroups[groupIndex].Statements[statementIndex].a_ep;
                    }
                }

                // 依次计算每个语句的加权和
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    result[statementIndex].suspiciousness1 += weight * mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            }

            return result;
        }

        /// <summary>
        /// 成例不覆盖F-P - 可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus10(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];
            for (int statementIndex = 0; statementIndex < result.Length; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
            }

            // 针对每个用例组
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                // 计算该用例组的权值
                double weight = 0;
                // 遍历每一条语句
                for (int statementIndex = 0; statementIndex < mGroups[groupIndex].NumStatements; statementIndex++)
                {
                    // 查看是否被所有的失例所覆盖
                    if ((mGroups[groupIndex].Statements[statementIndex].a_ef == mGroups[groupIndex].NumFalCase)
                     && (mGroups[groupIndex].Statements[statementIndex].a_ep != mGroups[groupIndex].NumSucCase))
                    {
                        // 如果是 增加权值
                        weight += mGroups[groupIndex].Statements[statementIndex].a_np;
                    }
                }

                // 依次计算每个语句的加权和
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    result[statementIndex].suspiciousness1 += weight * mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            }

            return result;
        }


        /// <summary>
        /// 最大可疑度 - 可疑度集成 李成龙改
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus11(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = -1;
                double p = -1;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    p = mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                    if (result[statementIndex].suspiciousness1 < p)
                        result[statementIndex].suspiciousness1 = p;
                }
            }

            return result;
        }



        /// <summary>
        /// 中位可疑度 - 可疑度集成 李成龙改
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus12(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                double[] p_list = new double[mGroups.Count];
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {

                    p_list[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
                double avg = p_list.Average();
                double variance = p_list.Sum(x => Math.Pow(x - avg, 2)) / p_list.Count();
                result[statementIndex].suspiciousness1 = avg;
                result[statementIndex].suspiciousness2 = 1 / (1 + Math.Sqrt(variance));

            }

            return result;
        }


        /// <summary>
        /// 失例数量作为权重 - 可疑度集成 李成龙改
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus13(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    double ratio = Convert.ToDouble(mGroups[groupIndex].NumFalCase) / Convert.ToDouble(mGroups[groupIndex].m_CovMatrix.NumFalRuns);
                    result[statementIndex].suspiciousness1 += ratio * mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
            }

            return result;
        }



        /// <summary>
        /// 去掉前1/4和后1/4的平均可疑度 - 可疑度集成 李成龙改
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus14(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                double[] p_list = new double[mGroups.Count];
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {

                    p_list[groupIndex] = mGroups[groupIndex].Statements[statementIndex].suspiciousness1;
                }
                result[statementIndex].suspiciousness1 = GetMiddleAverage(p_list);

            }

            return result;
        }


        /// <summary>
        /// (a_ef + a_ep) - 可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus15(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    FLStatementInfo temp = mGroups[groupIndex].Statements[statementIndex];
                    // 计算权值
                    double weight = Convert.ToDouble(temp.a_ef + temp.a_ep);
                    // 加权求和
                    result[statementIndex].suspiciousness1 += weight * temp.suspiciousness1;
                }
            }

            return result;
        }


        /// <summary>
        /// (a_ef + a_ep) - 可疑度集成
        /// </summary>
        /// <param name="mGroups"></param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSus16(List<FLRunsGroupInfo> mGroups)
        {
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    FLStatementInfo temp = mGroups[groupIndex].Statements[statementIndex];
                    // 加权求和
                    if (temp.suspiciousness1 != 0)
                    {
                        result[statementIndex].suspiciousness1 += temp.suspiciousness1 / Math.Pow(Math.Abs(temp.suspiciousness1), 4 / 5);
                    }

                }
            }

            return result;
        }



        #endregion
        #endregion

        #region 排位集成
        /// <summary>
        /// 排位集成
        /// 参与实验的各个语句的绝对排位
        /// </summary>
        /// <param name="mGroups">已经计算了可疑度并排序的测试用例组</param>
        /// <param name="iMethodIndex">可疑度计算方法</param>
        /// <param name="strKernelName">TieBreak</param>
        /// <returns></returns>
        public FLStatementInfo[] IntegrateSort(List<FLRunsGroupInfo> mGroups, int iMethodIndex, string strKernelName)
        {
            SortIntegrateDelegate pIntegrate = GetSortIntegrate(iMethodIndex);
            return pIntegrate(mGroups, strKernelName);
        }

        #region 排位集成准备工作

        // 集成公式委托
        public delegate FLStatementInfo[] SortIntegrateDelegate(List<FLRunsGroupInfo> mGroups, string strKernelName);

        // 集成公式委托查询列表
        private List<SortIntegrateDelegate> SortIntegrateList = null;

        // 初始化集成公式委托查询列表
        private void InitialSortDelegateList()
        {
            SortIntegrateList = new List<SortIntegrateDelegate>();

            SortIntegrateList.Add(IntegrateSort0);   // 加权均值（test）
            SortIntegrateList.Add(IntegrateSort1);   // 最大Rank
            SortIntegrateList.Add(IntegrateSort2);   // 简单均值
            SortIntegrateList.Add(IntegrateSort3);   // 加权均值（test），应该和Sort0是同一种方法
            SortIntegrateList.Add(IntegrateSort4);   // Ochai权重集成
            SortIntegrateList.Add(IntegrateSort5);   // 加权（数量）均值
            SortIntegrateList.Add(IntegrateSort6);   // 最大Rank 李成龙改
            SortIntegrateList.Add(IntegrateSort7);   // 最大Rank 李成龙改
            SortIntegrateList.Add(IntegrateSort8);   // 最大Rank 李成龙改
            SortIntegrateList.Add(IntegrateSort9);
            SortIntegrateList.Add(IntegrateSort10);

        }

        // 获取一个集成公式
        private SortIntegrateDelegate GetSortIntegrate(int iMethodIndex)
        {
            if (null == SortIntegrateList)
            {
                InitialSortDelegateList();
            }
            return new SortIntegrateDelegate(SortIntegrateList[iMethodIndex]);
        }

        #endregion

        #region 排位集成的Kernel

        // kernel委托
        public delegate double SortIntKernelDelegate(int iNumStatements, FLStatementInfo theStatement);

        // kernel委托查询列表
        private Dictionary<string, SortIntKernelDelegate> SortIntKernelDictionary = null;

        // 初始化Kernel查询列表
        private void InitialKernelDictionary()
        {
            SortIntKernelDictionary = new Dictionary<string, SortIntKernelDelegate>();

            // Num - Sort 语句总数减去排位
            SortIntKernelDictionary.Add("NumSUBSort", NumSUBSort);

            // Num - ExpectedSort 语句总数减去期望排位
            SortIntKernelDictionary.Add("NumSExpSort", NumSUBExpectedSort);
        }

        // 获取kernel入口
        private SortIntKernelDelegate GetSortKernel(string strkernelName)
        {
            if (null == SortIntKernelDictionary)
            {
                InitialKernelDictionary();
            }

            return new SortIntKernelDelegate(SortIntKernelDictionary[strkernelName]);
        }

        // "NumSUBSort"
        private double NumSUBSort(int iNumStatements, FLStatementInfo theStatement)
        {
            return (iNumStatements - theStatement.sort);
        }
        // "NumSUBExpectedSort"
        private double NumSUBExpectedSort(int iNumStatements, FLStatementInfo theStatement)
        {
            return (iNumStatements - theStatement.ExpectedSort);
        }

        #endregion

        #region 排位集成权值公式


        /// <summary>
        /// 根据用例的不同作为权重 - 排位集成  李成龙添加
        /// </summary>
        public FLStatementInfo[] IntegrateSort10(List<FLRunsGroupInfo> mGroups, string strKernelName)
        {
            SortIntKernelDelegate theKernel = GetSortKernel(strKernelName);
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;

            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 遍历mGroups
            double[] weight = new double[mGroups.Count];
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                Dictionary<string, int> dic = new Dictionary<string, int>();
                for (int sucIndex = 0; sucIndex < mGroups[groupIndex].NumSucCase; sucIndex++)
                {
                    string the_case = bools2string(mGroups[groupIndex].m_CovMatrix.SucCoverageMetrix[mGroups[groupIndex].SucCaseIDs[sucIndex]]);
                    if (dic.Keys.Contains(the_case))
                    {
                        dic[the_case]++;
                    }
                    else
                    {
                        dic.Add(the_case, 1);

                    }
                }

                Dictionary<string, int> dic2 = new Dictionary<string, int>();
                for (int falIndex = 0; falIndex < mGroups[groupIndex].NumFalCase; falIndex++)
                {
                    string the_case = bools2string(mGroups[groupIndex].m_CovMatrix.FalCoverageMetrix[mGroups[groupIndex].FalCaseIDs[falIndex]]);
                    if (dic2.Keys.Contains(the_case))
                    {
                        dic2[the_case]++;
                    }
                    else
                    {
                        dic2.Add(the_case, 1);
                    }

                }
                weight[groupIndex] = Convert.ToDouble(dic.Count) + Convert.ToDouble(dic2.Count);
            }

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {

                    result[statementIndex].suspiciousness1 += weight[groupIndex]
                        * theKernel(iNumStatements, mGroups[groupIndex].Statements[statementIndex]);
                }
            }

            return result;
        }





        /// <summary>
        /// 根据可区分度作为权重 - 排位集成  李成龙添加
        /// </summary>
        public FLStatementInfo[] IntegrateSort9(List<FLRunsGroupInfo> mGroups, string strKernelName)
        {
            SortIntKernelDelegate theKernel = GetSortKernel(strKernelName);
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 遍历mGroups
            double[] weight = new double[mGroups.Count];
            for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
            {
                Dictionary<string, int> dic = new Dictionary<string, int>();
                for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
                {
                    string spectra = string.Format("{0}#{1}#{2}#{3}", mGroups[groupIndex].Statements[statementIndex].a_ef, mGroups[groupIndex].Statements[statementIndex].a_ep, mGroups[groupIndex].Statements[statementIndex].a_nf, mGroups[groupIndex].Statements[statementIndex].a_np);
                    if (dic.Keys.Contains(spectra))
                    {
                        dic[spectra]++;
                    }
                    else
                    {
                        dic.Add(spectra, 1);

                    }
                }
                weight[groupIndex] = Convert.ToDouble(dic.Count);
            }

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {

                    result[statementIndex].suspiciousness1 += weight[groupIndex]
                        * theKernel(iNumStatements, mGroups[groupIndex].Statements[statementIndex]);
                }
            }

            return result;
        }










        /// <summary>
        /// 等概率权值 - (Num - sort)
        /// </summary>
        public FLStatementInfo[] IntegrateSort0(List<FLRunsGroupInfo> mGroups, string strKernelName)
        {
            SortIntKernelDelegate theKernel = GetSortKernel(strKernelName);
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            double w, f, p;
            f = 0; p = 0; w = 0;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            p = 0;
            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;

                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    f = mGroups[groupIndex].Statements[statementIndex].a_ef;  // 郑征改
                    p = mGroups[groupIndex].Statements[statementIndex].a_ep;  // 郑征改
                    if (p == 0)
                        w = 2 * f;
                    else
                        w = f / p;
                    result[statementIndex].suspiciousness1 += w * (iNumStatements - mGroups[groupIndex].Statements[statementIndex].ExpectedSort);
                }
            }

            return result;
        }
        /// <summary>
        /// 最大Rank - 排位集成
        /// </summary>
        public FLStatementInfo[] IntegrateSort1(List<FLRunsGroupInfo> mGroups, string strKernelName)
        {
            SortIntKernelDelegate theKernel = GetSortKernel(strKernelName);
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                double p = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    p = theKernel(iNumStatements, mGroups[groupIndex].Statements[statementIndex]);
                    if (result[statementIndex].suspiciousness1 < p)
                        result[statementIndex].suspiciousness1 = p;

                }
            }

            return result;
        }
        /// <summary>
        /// 等权值相加 - 排位集成
        /// </summary>
        public FLStatementInfo[] IntegrateSort2(List<FLRunsGroupInfo> mGroups, string strKernelName)
        {
            SortIntKernelDelegate theKernel = GetSortKernel(strKernelName);
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    result[statementIndex].suspiciousness1 += theKernel(iNumStatements, mGroups[groupIndex].Statements[statementIndex]);
                }
            }

            return result;
        }
        /// <summary>
        /// a_ef / a_ep - 排位集成
        /// </summary>
        public FLStatementInfo[] IntegrateSort3(List<FLRunsGroupInfo> mGroups, string strKernelName)
        {
            SortIntKernelDelegate theKernel = GetSortKernel(strKernelName);
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            double w, f, p;
            f = 0; p = 0; w = 0;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            p = 0;
            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    f = mGroups[groupIndex].Statements[statementIndex].a_ef;// 郑征改
                    p = mGroups[groupIndex].Statements[statementIndex].a_ep;//郑征改
                    if (p == 0)
                        w = 2 * f;
                    else
                        w = f / p;
                    result[statementIndex].suspiciousness1 += w * theKernel(iNumStatements, mGroups[groupIndex].Statements[statementIndex]);
                }
            }

            return result;
        }

        /// <summary>
        /// 使用Ochiai:a_ef / sqrt((a_ef + a_nf)*(a_ef + a_ep)) - 排位集成   李成龙添加
        /// </summary>
        public FLStatementInfo[] IntegrateSort4(List<FLRunsGroupInfo> mGroups, string strKernelName)
        {
            SortIntKernelDelegate theKernel = GetSortKernel(strKernelName);
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            double w, ef, ep, nf, np;
            ef = 0; ep = 0; nf = 0; np = 0; w = 0;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];
            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    ef = mGroups[groupIndex].Statements[statementIndex].a_ef;
                    ep = mGroups[groupIndex].Statements[statementIndex].a_ep;
                    nf = mGroups[groupIndex].Statements[statementIndex].a_nf;
                    np = mGroups[groupIndex].Statements[statementIndex].a_np;
                    if ((ef + nf) * (ef + ep) == 0)
                        w = 2 * ef;
                    else
                        w = ef / Math.Sqrt((ef + nf) * (ef + ep));
                    result[statementIndex].suspiciousness1 += w * theKernel(iNumStatements, mGroups[groupIndex].Statements[statementIndex]);
                }
            }

            return result;
        }


        /// <summary>
        /// 根据用例数量作为权重 - 排位集成  李成龙添加
        /// </summary>
        public FLStatementInfo[] IntegrateSort5(List<FLRunsGroupInfo> mGroups, string strKernelName)
        {
            SortIntKernelDelegate theKernel = GetSortKernel(strKernelName);
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    double ratio = Convert.ToDouble(mGroups[groupIndex].NumFalCase) / Convert.ToDouble(mGroups[groupIndex].m_CovMatrix.NumFalRuns);
                    result[statementIndex].suspiciousness1 += (ratio + 1)
                        * theKernel(iNumStatements, mGroups[groupIndex].Statements[statementIndex]);
                }
            }

            return result;
        }

        /// <summary>
        /// 最大Rank - 排位集成 李成龙改
        /// </summary>
        public FLStatementInfo[] IntegrateSort6(List<FLRunsGroupInfo> mGroups, string strKernelName)
        {
            SortIntKernelDelegate theKernel = GetSortKernel(strKernelName);
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                double p = 0;
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    p = theKernel(iNumStatements, mGroups[groupIndex].Statements[statementIndex]);
                    if (result[statementIndex].suspiciousness1 < p)
                    //if (Convert.ToDouble(mGroups[groupIndex].NumFalCase) / Convert.ToDouble(mGroups[groupIndex].m_CovMatrix.NumFalRuns) > 0.1)
                    {
                        result[statementIndex].suspiciousness1 = p;

                    }

                    else if (result[statementIndex].suspiciousness1 == p)
                    {
                        result[statementIndex].suspiciousness2 += 1;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// 最大Rank - 排位集成 李成龙改
        /// </summary>
        public FLStatementInfo[] IntegrateSort7(List<FLRunsGroupInfo> mGroups, string strKernelName)
        {
            SortIntKernelDelegate theKernel = GetSortKernel(strKernelName);
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                double p = 0;
                List<double> p_list = new List<double>();
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    p = theKernel(iNumStatements, mGroups[groupIndex].Statements[statementIndex]);
                    p_list.Add(p);
                }
                p_list.Sort();    // 升序排序
                p_list.Reverse(); // 反转顺序
                Dictionary<double, int> p_final = new Dictionary<double, int>();
                p_final.Add(p_list[0], 1);
                for (int groupIndex = 1; groupIndex < mGroups.Count; groupIndex++)
                {
                    Dictionary<double, int>.KeyCollection keyCol = p_final.Keys;
                    bool found = false;
                    foreach (double key in keyCol)
                    {
                        if (p_list[groupIndex] >= 0.99 * key)
                        {
                            double newKey = (key + p_list[groupIndex]) / 2;
                            int newValue = p_final[key] + 1;
                            p_final.Remove(key);
                            p_final.Add(newKey, newValue);
                            found = true;
                            break;
                        }

                    }
                    if (!found)
                    {
                        p_final.Add(p_list[groupIndex], 1);
                    }
                }
                int maxNum = 0;
                foreach (KeyValuePair<double, int> kvp in p_final)
                {
                    if (kvp.Value > maxNum)
                    {
                        result[statementIndex].suspiciousness1 = kvp.Key;
                        maxNum = kvp.Value;
                    }

                }
                //var res = from n in p_list
                //          group n by n into g
                //          orderby g.Count() descending
                //          select g;
                //var gr = res.First();
                //if (gr.Count() == 1)
                //    result[statementIndex].suspiciousness1 = p_list.Average();
                //else
                //    result[statementIndex].suspiciousness1 = gr.Average();
            }

            return result;
        }


        /// <summary>
        /// 最大Rank - 排位集成 李成龙改
        /// </summary>
        public FLStatementInfo[] IntegrateSort8(List<FLRunsGroupInfo> mGroups, string strKernelName)
        {
            SortIntKernelDelegate theKernel = GetSortKernel(strKernelName);
            // 获取语句数
            int iNumStatements = mGroups[0].NumStatements;
            // 建立输出内存
            FLStatementInfo[] result = new FLStatementInfo[iNumStatements];

            // 依次计算各个语句的加权和
            for (int statementIndex = 0; statementIndex < iNumStatements; statementIndex++)
            {
                result[statementIndex] = new FLStatementInfo();
                result[statementIndex].ID = statementIndex;
                result[statementIndex].suspiciousness1 = 0;
                result[statementIndex].suspiciousness2 = 0;
                double p = 0;
                List<double> p_list = new List<double>();
                // 遍历mGroups
                for (int groupIndex = 0; groupIndex < mGroups.Count; groupIndex++)
                {
                    p = theKernel(iNumStatements, mGroups[groupIndex].Statements[statementIndex]);
                    p_list.Add(p);
                }
                p_list.Sort();    // 升序排序
                p_list.Reverse(); // 反转顺序
                result[statementIndex].suspiciousness1 = p_list[0];
                for (int groupIndex = 1; groupIndex < mGroups.Count; groupIndex++)
                {
                    if (p_list[groupIndex] >= 0.95 * result[statementIndex].suspiciousness1)
                        result[statementIndex].suspiciousness2 = result[statementIndex].suspiciousness2 + 1;
                }
            }
            return result;
        }


        #endregion
        #endregion

        private static int HeapTieBreak(FLStatementInfo a, FLStatementInfo b)
        {
            if (a.suspiciousness1 < b.suspiciousness1)
            {
                return -1;
            }
            else if (a.suspiciousness1 == b.suspiciousness1)
            {
                if (a.suspiciousness2 < b.suspiciousness2)
                {
                    return -1;
                }
                else if (a.suspiciousness2 == b.suspiciousness2)
                {
                    if (a.ID > b.ID)
                        return -1;
                    else if (a.ID == b.ID)
                        return 0;
                    else
                        return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        #region 堆排序

        //private static int heapSize;
        //----------------------------------------------------------------
        //  HeapSort
        //  功能：实现堆排序
        //  参数：
        //          ref:     Statement_info[]    heap    待排序的堆
        //          IN:      int                 low     排序的低位
        //                   int                 high    排序的高位
        public static void HeapSort(ref FLStatementInfo[] heap, int low, int high)
        {
            int heapSize = high - low + 1;
            BuildMaxHeap(ref heap, heapSize);
            for (int i = high; i >= low + 1; i--)
            {
                //1.每次在构建好最大堆后，将第一个元素和最后一个元素交换； 
                //2.第一次以索引1到length-1出的元素组成新的堆，第二次1到length-2，直到剩下最后两个元素组成堆 
                //3.每次新组成的堆除了根节点其他节点都能保持最大堆的特性，因此只要DoBuildMaxHeap(heap, 1)就可以得到新的最大堆 
                Swap(ref heap, low, i);
                heapSize--;
                MaxHeapfy(ref heap, low, heapSize);
            }
        }
        //  BuidMaxHeap
        //  功能：构建最大堆
        //  参数：
        //          ref:     Statement_info[]    heap    待排序的堆
        private static void BuildMaxHeap(ref FLStatementInfo[] heap, int heapSize)
        {
            for (int i = (heap.Length - 1) / 2; i >= 0; i--)
            {
                MaxHeapfy(ref heap, i, heapSize);
            }
        }

        //  MaxHeapfy
        //  功能：递归调用MaxHeapfy使堆最大化
        //  参数：ref:       double[]    heap
        //        IN:        int         index
        public static void MaxHeapfy(ref FLStatementInfo[] heap, int index, int heapSize)
        {
            int smallerItemIndex = index;
            int leftChildIndex = (index * 2) + 1;
            int rightChildIndex = (index * 2) + 2;

            if (leftChildIndex < heapSize && HeapTieBreak(heap[leftChildIndex], heap[index]) < 0)
            {
                //largerItemIndex = rightChildIndex;
                smallerItemIndex = leftChildIndex;
            }
            if (rightChildIndex < heapSize && HeapTieBreak(heap[rightChildIndex], heap[smallerItemIndex]) < 0)
            {
                //largerItemIndex = leftChildIndex;
                smallerItemIndex = rightChildIndex;
            }
            if (index != smallerItemIndex)
            {
                Swap(ref heap, index, smallerItemIndex);
                MaxHeapfy(ref heap, smallerItemIndex, heapSize);
            }
        }
        //  Swap
        //  功能：交换两个指针所指元素
        //  参数： ref:     Statement_info[]    heap
        //         IN:      int         index1
        //                  int         index2
        public static void Swap(ref FLStatementInfo[] heap, int index1, int index2)
        {
            FLStatementInfo temp = heap[index1];
            heap[index1] = heap[index2];
            heap[index2] = temp;
        }
        #endregion

        // 返回编序的语句列表，原语句列表更新排位属性
        public FLStatementInfo[] Sort(ref FLStatementInfo[] statementsInfo)
        {
            // 语句数量
            int iNumStatements = statementsInfo.Length;
            // 排序的语句列表
            FLStatementInfo[] sortedStatement = new FLStatementInfo[iNumStatements];
            for (int i = 0; i < iNumStatements; i++)
            {
                sortedStatement[i] = statementsInfo[i];
            }

            // 对dstStatement堆排序
            HeapSort(ref sortedStatement, 0, iNumStatements - 1);

            // 更新语句信息的排位编号
            for (int index = 0; index < iNumStatements; index++)
            {
                sortedStatement[index].sort = index + 1;
            }

            AssignSort(sortedStatement);
            return sortedStatement;
        }


        public void Sort2(FLStatementInfo[] statementsInfo)
        {
            // 语句数量
            int iNumStatements = statementsInfo.Length;
            //// 排序的语句列表
            //FLStatementInfo[] sortedStatement = new FLStatementInfo[iNumStatements];
            //for (int i = 0; i < iNumStatements; i++)
            //{
            //    sortedStatement[i] = statementsInfo[i];
            //}

            // 对dstStatement堆排序
            HeapSort(ref statementsInfo, 0, iNumStatements - 1);

            // 更新语句信息的排位编号
            for (int index = 0; index < iNumStatements; index++)
            {
                statementsInfo[index].sort = index + 1;
            }

            AssignSort(statementsInfo);
        }


        // 计算各语句的期望排序
        public void AssignSort(FLStatementInfo[] sortedStatements)
        {
            int minSort = 1;
            int maxSort = 2;
            double tempSus = sortedStatements[0].suspiciousness1;
            double expectedSort = -1;

            // 依次逐个扫描
            for (int i = 0; i < sortedStatements.Length; i++)
            {
                if (tempSus == sortedStatements[i].suspiciousness1)
                {
                    maxSort = sortedStatements[i].sort;
                }
                else if (tempSus > sortedStatements[i].suspiciousness1)
                {
                    tempSus = sortedStatements[i].suspiciousness1;

                    // 计算前段的期望排位
                    expectedSort = Convert.ToDouble(minSort + maxSort) * 0.5;
                    for (int j = minSort - 1; j < maxSort; j++)
                    {
                        sortedStatements[j].ExpectedSort = expectedSort;
                    }

                    // 更新标记
                    minSort = sortedStatements[i].sort;
                    maxSort = sortedStatements[i].sort;
                }
                else
                {
                    throw new Exception("确认输入的排位表是按照可疑度从大到小排序的");
                }
            }

            // 计算前段的期望排位
            expectedSort = Convert.ToDouble(minSort + maxSort) * 0.5;
            for (int j = minSort - 1; j < maxSort; j++)
            {
                sortedStatements[j].ExpectedSort = expectedSort;
            }

        }
    }
}




// 应用集成的策略定位缺陷
//public FLStatementInfo[] EnsembleLocateFaultsSus(string methodName, int iIntegrateMethodIndex)
//{
//    if (null != m_DivGroups)
//    {
//        // 计算各组可疑度
//        CalSusOfGroups(m_DivGroups, methodName);
//        // 
//        FLStatementInfo[] result = IntegrateSus(m_DivGroups, iIntegrateMethodIndex);
//        // 排序
//        return Sort(ref result);
//    }
//    else
//    {
//        return null;
//    }
//}

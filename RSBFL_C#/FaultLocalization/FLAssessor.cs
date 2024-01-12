using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaultLocalization
{
    public class FLAssessor
    {
        private FLStaFaultVersionSetInfo m_VersionSetting = null;
        /// <summary>
        /// 获取当前实验对象
        /// </summary>
        public FLStaFaultVersionSetInfo VersionSetting
        {
            get { return m_VersionSetting; }
        }

        /// <summary>
        /// 装载错误信息
        /// </summary>
        /// <param name="cfg">实验配置</param>
        /// <returns></returns>
        public FLStaFaultVersionSetInfo LoadFaultInfo(FLConfigure cfg)
        {
            string sLineMap = cfg.DataRootInfo.FullName + "\\"
                            + cfg.SuiteName + "\\"
                            + cfg.ProgramName + "\\"
                            + cfg.VersionName + "\\2.txt";

            FLStaFaultVersionSetInfo result = FLDBServer.ReadFaultVersionSettings(cfg.SuiteName, cfg.ProgramName, cfg.VersionName);
            if (null != result)
            {
                result.MapLineNumber2ID(sLineMap);
                // 李成龙添加
                for (int faultIndex = 0; faultIndex < result.Faults.Count; faultIndex++)
                {
                    for (int faultyStatementsIndex = 0; faultyStatementsIndex < result.Faults[faultIndex].FaultyStatements.Count; faultyStatementsIndex++)
                    {
                        if (result.Faults[faultIndex].FaultyStatements[faultyStatementsIndex].ID != -1)
                        {
                            cfg.isInstrumented = true;
                            m_VersionSetting = result;
                            return result;
                        }
                    }
                }
            }

            cfg.isInstrumented = false;
            m_VersionSetting = result;
            return result;
        }

        /// <summary>
        /// 评估排位表
        /// </summary>
        /// <param name="rankedList">排位表</param>
        /// <param name="cfg">实验配置</param>
        /// <returns></returns>
        public FLStaLocationEffort AssessRankedList(FLStatementInfo[] rankedList)
        {
            FLStaLocationEffort theEffort = new FLStaLocationEffort();
            if (null != rankedList)
                // 计算expense
                theEffort = LocateFirstFault(rankedList, m_VersionSetting.GetAllFaultyStatementsID());

            return theEffort;
        }

        /// <summary>
        /// 定位第一个缺陷,计算Expense
        /// </summary>
        /// <param name="mSortedStatements">排序后的语句列表</param>
        /// <param name="faultyStatementID">缺陷语句索引</param>
        /// <returns>定位效果</returns>
        public FLStaLocationEffort LocateFirstFault(FLStatementInfo[] mSortedStatements, List<int> faultyStatementID)
        {
            FLStaLocationEffort locationEffort = new FLStaLocationEffort();

            // 遍历每条语句
            for (int statementIndex = 0; statementIndex < mSortedStatements.Length; statementIndex++)
            {
                // 判断是否是缺陷 找到第一个缺陷即可
                if (faultyStatementID.Contains(mSortedStatements[statementIndex].ID))
                {
                    locationEffort.AbsSort = statementIndex + 1;
                    locationEffort.BestSort = UpperBound(locationEffort.AbsSort, mSortedStatements) + 1;
                    locationEffort.WorstSort = LowerBound(locationEffort.AbsSort, mSortedStatements) + 1;
                    locationEffort.AveSort = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(locationEffort.BestSort + locationEffort.WorstSort) / 2));

                    locationEffort.AbsExpense = Convert.ToDouble(locationEffort.AbsSort) / Convert.ToDouble(mSortedStatements.Length);
                    locationEffort.LeastExpense = Convert.ToDouble(locationEffort.BestSort) / Convert.ToDouble(mSortedStatements.Length);
                    locationEffort.MostExpense = Convert.ToDouble(locationEffort.WorstSort) / Convert.ToDouble(mSortedStatements.Length);
                    locationEffort.AveExpense = Convert.ToDouble(locationEffort.AveSort) / Convert.ToDouble(mSortedStatements.Length);

                    break;
                }
            }

            return locationEffort;
        }
        
        // 根据iAbsLocation计算最优排位
        private int UpperBound(int iAbsLocation, FLStatementInfo[] mSortedStatements)
        {
            iAbsLocation = iAbsLocation - 1;
            int result = iAbsLocation;
            //  缺陷语句的可疑度
            double theSuspicious = mSortedStatements[iAbsLocation].suspiciousness1;

            double tempSuspicious = mSortedStatements[result].suspiciousness1;
            while (tempSuspicious == theSuspicious)
            {
                result--;
                if (result < 0)
                {
                    break;
                }
                tempSuspicious = mSortedStatements[result].suspiciousness1;
            }
            result += 1;
            return result;
        }
        
        // 根据iAbsLocation计算最次排位
        private int LowerBound(int iAbsLocation, FLStatementInfo[] mSortedStatements)
        {
            iAbsLocation = iAbsLocation - 1;
            int result = iAbsLocation;
            //  缺陷语句的可疑度
            double theSuspicious = mSortedStatements[iAbsLocation].suspiciousness1;

            double tempSuspicious = mSortedStatements[result].suspiciousness1;
            while (tempSuspicious == theSuspicious)
            {
                result++;
                if (result >= mSortedStatements.Length)
                {
                    break;
                }
                tempSuspicious = mSortedStatements[result].suspiciousness1;
            }
            result -= 1;
            return result;
        }

        // 保存测试结果
        public void SaveResult(FLStaFaultVersionSetInfo setting, FLStaLocationEffort theEffort)
        {
            // 获取对象ID
            int theID = FLDBServer.GetIDofVersion(setting.SuiteName, setting.ProgramName, setting.VersionName);
            //
            FLDBServer.InsertLocationEffortofVersion(theID, theEffort);
        }

        // 保存测试结果
        public void SaveResult(FLStaFaultVersionSetInfo setting, FLStaLocationEffortStatic theEffort)
        {
            // 获取对象ID
            int theID = FLDBServer.GetIDofVersion(setting.SuiteName, setting.ProgramName, setting.VersionName);
            //
            FLDBServer.InsertStatisticLocationEffortofVersion(theID, theEffort);
        }


    }
}

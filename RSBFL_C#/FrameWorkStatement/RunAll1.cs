/************************************************************************
 * 
 * class: RunAll
 * 
 * 功能：复现原始算法，没有任何特殊的操作
 * 
 * ************************************************GaoYichao.2013.08.14**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using System.Text.RegularExpressions;
//
using DBDll;
using ConfigDll;
//
using FaultLocalization;

namespace FrameWorkStatement
{

    public class RunAll
    {
        #region 设置
        /// <summary>
        /// 数据源
        /// </summary>
        private DirectoryInfo m_DataRootInfo = null;

        /// <summary>
        /// 实验算法
        /// </summary>
        private string[] methodlist = null;
        public string[] MethodList
        {
            get { return methodlist; }
            set { methodlist = value; }
        }
        /// <summary>
        /// 实验算法数
        /// </summary>
        public int NumMethods
        {
            get { return methodlist.Length; }
        }

        private FLAssessor m_Assessor = null;
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mDataRootInfo">数据源</param>
        public RunAll(DirectoryInfo mDataRootInfo)
        {
            m_DataRootInfo = mDataRootInfo;

            string dataSrcPath = mDataRootInfo.FullName;
            m_Assessor = new FLAssessor();
        }
        /// <summary>
        /// 创建一个Debugger
        /// </summary>
        /// <param name="cfg">配置信息</param>
        /// <returns>Debugger</returns>
        public static FLDebugger CreateDebugger(FLConfigure cfg)
        {
            string sucFileName = cfg.DataRootInfo.FullName + "\\"
                               + cfg.SuiteName + "\\"
                               + cfg.ProgramName + "\\"
                               + cfg.VersionName + "\\1_success_traces";

            string falFileName =  cfg.DataRootInfo.FullName + "\\"
                               + cfg.SuiteName + "\\"
                               + cfg.ProgramName + "\\"
                               + cfg.VersionName + "\\1_crash_traces";
            return new FLDebugger(sucFileName, falFileName);
        }
        /// <summary>
        /// 创建一个覆盖矩阵
        /// </summary>
        /// <param name="cfg">配置信息</param>
        /// <returns>覆盖矩阵</returns>
        public static FLBoolCovMatrix CreateBoolCovMatrix(FLConfigure cfg)
        {
            string sucFileName = cfg.DataRootInfo.FullName + "\\"
                   + cfg.SuiteName + "\\"
                   + cfg.ProgramName + "\\"
                   + cfg.VersionName + "\\1_success_traces";

            string falFileName = cfg.DataRootInfo.FullName + "\\"
                               + cfg.SuiteName + "\\"
                               + cfg.ProgramName + "\\"
                               + cfg.VersionName + "\\1_crash_traces";
            return new FLBoolCovMatrix(sucFileName, falFileName);
        }
        /// <summary>
        /// 选择用于变更符号的测试用例
        /// </summary>
        /// <param name="fRate">选择用例比例</param>
        /// <param name="covMatrix">覆盖矩阵</param>
        /// <returns>选择用例lists[0]中选择出来的是成例,lists[1]中选择出来的是失例</returns>
        public static List<int>[] SelectTestCasesToChangeClass(int ID, FLConfigure cfg, FLBoolCovMatrix covMatrix, int itimes)
        {
            List<int>[] lists = FLDBServer.ReadTestCaseChangeClassInfo(ID, cfg.ClassChangeSelectStrategy, itimes);
            if (null != lists)
                return lists;

            double fRate = cfg.ClassChangeRatio;
            List<int> sucCandidates = new List<int>();
            List<int> falCandidates = new List<int>();

            for (int i = 0; i < covMatrix.NumSucRuns; i++)
                sucCandidates.Add(i);
            for (int i = 0; i < covMatrix.NumFalRuns; i++)
                falCandidates.Add(i);

            int iCount = Convert.ToInt32(Math.Max(0, Math.Floor(fRate * covMatrix.NumRuns)));
            if (cfg.ChangeSucRatio < 0)
            {
                lists = FLRunsFilter.RandomIntsNB(iCount, sucCandidates, falCandidates);
            }
            else
            {
                int iSucCount = Convert.ToInt32(Math.Max(0, Math.Floor(cfg.ChangeSucRatio * iCount)));
                int iFalCount = iCount - iSucCount;
                lists = new List<int>[2];
                lists[0] = new List<int>(FLRunsFilter.RandomIntsNB(iSucCount, sucCandidates));
                lists[1] = new List<int>(FLRunsFilter.RandomIntsNB(iFalCount, falCandidates));
            }

            int times = 0;
            // lists[0]中选择出来的是成例
            // lists[1]中选择出来的是失例
            while ((0 == (sucCandidates.Count + lists[1].Count)) || (0 == (falCandidates.Count + lists[0].Count)))
            {
                times++;
                if (times > 10000)
                    return null;

                if (cfg.ChangeSucRatio < 0)
                {
                    lists = FLRunsFilter.RandomIntsNB(iCount, sucCandidates, falCandidates);
                }
                else
                {
                    int iSucCount = Convert.ToInt32(Math.Max(0, Math.Floor(cfg.ChangeSucRatio * iCount)));
                    int iFalCount = iCount - iSucCount;
                    lists = new List<int>[2];
                    lists[0] = new List<int>(FLRunsFilter.RandomIntsNB(iSucCount, sucCandidates));
                    lists[1] = new List<int>(FLRunsFilter.RandomIntsNB(iFalCount, falCandidates));
                }
            }

            FLDBServer.InsertTestCaseChangeClassInfo(ID, cfg.ClassChangeSelectStrategy, itimes, lists);
            return lists;
        }

        private bool CheckExperimentConfig(FLConfigure cfg)
        {
            FLStaFaultVersionCovInfo info = FLDBServer.ReadFaultVersionData(cfg.SuiteName, cfg.ProgramName, cfg.VersionName);
            if (null == info || info.NumRuns < cfg.MinRuns)
                return false;

            if (cfg.MinClassRatio > 0 && cfg.MaxClassRatio > 0)
            {
                double ratio = Convert.ToDouble(info.NumSucRuns) / Convert.ToDouble(info.NumFalRuns);
                if (ratio < cfg.MinClassRatio || ratio > cfg.MaxClassRatio)
                    return false;
            }

            return true;
        }

        #region 变更部分用例不集成实验
        /// <summary>
        /// 变更部分用例不集成实验
        /// </summary>
        /// <param name="strSuiteName">实验包</param>
        /// <param name="cfg">实验配置</param>
        public void ChangeClassExperimentof(string strSuiteName, FLConfigure cfg)
        {
            DirectoryInfo[] suiteRoots = cfg.DataRootInfo.GetDirectories();
            int suiteIndex = 0;
            for (suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
                if (strSuiteName == suiteRoots[suiteIndex].Name)
                    break;

            // 遍历所有实验程序
            DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();
            for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
            {
                // 遍历所有缺陷版本
                DirectoryInfo[] versionRoots = programRoots[programIndex].GetDirectories();
                for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
                {
                    // FaultVersionInfo;
                    cfg.SuiteName = strSuiteName;
                    cfg.ProgramName = programRoots[programIndex].Name;
                    cfg.VersionName = versionRoots[versionIndex].Name;
                    //
                    ChangeClassExperimentof(cfg);
                } // end of versionIndex
            } // end of programIndex
        }

        public void ChangeClassExperimentof(string strSuiteName, string strProgramName, FLConfigure cfg)
        {
            DirectoryInfo[] suiteRoots = cfg.DataRootInfo.GetDirectories();
            int suiteIndex = 0;
            for (suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
                if (strSuiteName == suiteRoots[suiteIndex].Name)
                    break;

            // 遍历所有实验程序
            DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();
            for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
            {
                DirectoryInfo[] versionRoots = programRoots[programIndex].GetDirectories();
                if (strProgramName != programRoots[programIndex].Name)
                    continue;
                // 遍历所有缺陷版本
                for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
                {
                    // FaultVersionInfo;
                    cfg.SuiteName = strSuiteName;
                    cfg.ProgramName = programRoots[programIndex].Name;
                    cfg.VersionName = versionRoots[versionIndex].Name;
                    //
                    ChangeClassExperimentof(cfg);
                } // end of versionIndex
            } // end of programIndex

        }
        /// <summary>
        /// 变更部分用例不集成实验
        /// </summary>
        /// <param name="cfg">实验配置</param>
        public void ChangeClassExperimentof(FLConfigure cfg)
        {
            if (!CheckExperimentConfig(cfg))
                return;

            string expDisc = cfg.ClassChangeSelectStrategy + "_不集成";
            Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + ".Start");
            Console.WriteLine(expDisc);
            // 构建覆盖矩阵
            int ID = FLDBServer.GetIDofVersion(cfg.SuiteName, cfg.ProgramName, cfg.VersionName);
            if (null == m_Assessor.LoadFaultInfo(cfg))
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的缺陷信息");
                return;
            }
            FLBoolCovMatrix covMatrix = CreateBoolCovMatrix(cfg);
            if (null == covMatrix)
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的覆盖矩阵");
                return;
            }

            #region 准备统计信息
            FLStaLocationEffortStatic[] theStaticEfforts = new FLStaLocationEffortStatic[NumMethods];
            double[][] staList = new double[NumMethods][];
            for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
            {
                theStaticEfforts[methodIndex] = new FLStaLocationEffortStatic();
                theStaticEfforts[methodIndex].AlgorithmName = methodlist[methodIndex];
                theStaticEfforts[methodIndex].ExperimentDiscription = expDisc;
                staList[methodIndex] = new double[8];
                for (int tmpIndex = 0; tmpIndex < 8; tmpIndex++)
                    staList[methodIndex][tmpIndex] = 0;
            }
            #endregion

            #region 重复实验iTimes
            for (int i = 0; i < cfg.RepeatTimes; i++)
            {
                // 变更用例类别
                List<int>[] lists = SelectTestCasesToChangeClass(ID, cfg, covMatrix, i);
                List<bool[]> sucCases = covMatrix.ExtractSucCases(lists[0]);
                List<bool[]> falCases = covMatrix.ExtractFalCases(lists[1]);
                covMatrix.AppendSucCases(falCases);
                covMatrix.AppendFalCases(sucCases);
                // 初始化debugger
                FLDebugger debugger = new FLDebugger(covMatrix);
                FLRunsGroupDivider divider = new FLRunsGroupDivider(covMatrix);
                FLRunsGroupInfo group = divider.NoDivide();
                // 遍历每一种算法
                for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
                {
                    // 评估
                    FLStaLocationEffort theEffort = FLDBServer.ReadLocationEffortofVersion(ID, methodlist[methodIndex], expDisc + "." + i.ToString());
                    if (null == theEffort)
                    {
                        FLStatementInfo[] rankedList = debugger.LocateFaultsInGroup(methodlist[methodIndex], group);
                        theEffort = m_Assessor.AssessRankedList(rankedList);
                        theEffort.ExperimentDiscription = expDisc + "." + i.ToString();
                        theEffort.AlgorithmName = methodlist[methodIndex];
                        // 保存
                        FLDBServer.DeleLocationEffortofVersion(ID, methodlist[methodIndex], theEffort.ExperimentDiscription);
                        m_Assessor.SaveResult(m_Assessor.VersionSetting, theEffort);
                    }
                    // 更新统计信息
                    UpdateStatic(staList[methodIndex], theEffort);
                }
            }
            #endregion

            #region 保存统计信息
            for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
            {
                StaticAveVar(staList[methodIndex], cfg.RepeatTimes, theStaticEfforts[methodIndex]);
                // 保存
                m_Assessor.SaveResult(m_Assessor.VersionSetting, theStaticEfforts[methodIndex]);
            }
            #endregion

            Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + ".Finished");
        }
        #endregion

        #region 变更部分用例排位集成实验
        /// <summary>
        /// 变更部分用例排位集成实验
        /// </summary>
        /// <param name="strSuiteName">实验包</param>
        /// <param name="cfg">实验配置</param>
        public void ChangeClassEnsembleSortExperimentof(string strSuiteName, FLConfigure cfg)
        {
            DirectoryInfo[] suiteRoots = cfg.DataRootInfo.GetDirectories();
            int suiteIndex = 0;
            for (suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
                if (strSuiteName == suiteRoots[suiteIndex].Name)
                    break;

            // 遍历所有实验程序
            DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();
            for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
            {
                // 遍历所有缺陷版本
                DirectoryInfo[] versionRoots = programRoots[programIndex].GetDirectories();
                for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
                {
                    // FaultVersionInfo;
                    cfg.SuiteName = strSuiteName;
                    cfg.ProgramName = programRoots[programIndex].Name;
                    cfg.VersionName = versionRoots[versionIndex].Name;
                    //
                    ChangeClassEnsembleSortExperimentof(cfg);

                } // end of versionIndex
            } // end of programIndex
        }

        public void ChangeClassEnsembleSortExperimentof(string strSuiteName, string strProgramName, FLConfigure cfg)
        {
            DirectoryInfo[] suiteRoots = cfg.DataRootInfo.GetDirectories();
            int suiteIndex = 0;
            for (suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
                if (strSuiteName == suiteRoots[suiteIndex].Name)
                    break;

            // 遍历所有实验程序
            DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();
            for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
            {
                DirectoryInfo[] versionRoots = programRoots[programIndex].GetDirectories();
                if (strProgramName != programRoots[programIndex].Name)
                    continue;
                // 遍历所有缺陷版本
                for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
                {
                    // FaultVersionInfo;
                    cfg.SuiteName = strSuiteName;
                    cfg.ProgramName = programRoots[programIndex].Name;
                    cfg.VersionName = versionRoots[versionIndex].Name;
                    //
                    ChangeClassEnsembleSortExperimentof(cfg);
                } // end of versionIndex
            } // end of programIndex

        }
        /// <summary>
        /// 变更部分用例排位集成实验
        /// </summary>
        /// <param name="cfg">实验配置</param>
        public void ChangeClassEnsembleSortExperimentof(FLConfigure cfg)
        {
            if (!CheckExperimentConfig(cfg))
                return;

            string expDisc = cfg.ClassChangeSelectStrategy + "_" + cfg.ClassRatioDivideStrategy + "_排位_权值" + cfg.WeightFormulaId.ToString() + "_" + cfg.IntegrateKernel;

            Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + ".Start");
            Console.WriteLine(expDisc);
            // 构建覆盖矩阵
            int ID = FLDBServer.GetIDofVersion(cfg.SuiteName, cfg.ProgramName, cfg.VersionName);
            if (null == m_Assessor.LoadFaultInfo(cfg))
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的缺陷信息");
                return;
            }
            FLBoolCovMatrix covMatrix = CreateBoolCovMatrix(cfg);
            if (null == covMatrix)
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的覆盖矩阵");
                return;
            }

            #region 准备统计信息
            FLStaLocationEffortStatic[] theStaticEfforts = new FLStaLocationEffortStatic[NumMethods];
            double[][] staList = new double[NumMethods][];
            for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
            {
                theStaticEfforts[methodIndex] = new FLStaLocationEffortStatic();
                theStaticEfforts[methodIndex].AlgorithmName = methodlist[methodIndex];
                theStaticEfforts[methodIndex].ExperimentDiscription = expDisc;
                staList[methodIndex] = new double[8];
                for (int tmpIndex = 0; tmpIndex < 8; tmpIndex++)
                    staList[methodIndex][tmpIndex] = 0;
            }
            #endregion

            #region 重复实验iTimes
            for (int i = 0; i < cfg.RepeatTimes; i++)
            {
                // 变更用例类别
                List<int>[] lists = SelectTestCasesToChangeClass(ID, cfg, covMatrix, i);
                List<bool[]> sucCases = covMatrix.ExtractSucCases(lists[0]);
                List<bool[]> falCases = covMatrix.ExtractFalCases(lists[1]);
                covMatrix.AppendSucCases(falCases);
                covMatrix.AppendFalCases(sucCases);
                // 初始化debugger,用例分组
                FLDebugger debugger = new FLDebugger(covMatrix);
                FLRunsGroupDivider divider = new FLRunsGroupDivider(covMatrix);
                List<int[]>[] dividedLists = FLDBServer.ReadTestCaseChangeClassDivInfo(ID, cfg, i, 1);
                List<FLRunsGroupInfo> groups = null;
                if (null == dividedLists)
                {
                    groups = divider.DivideClassRatioGroups(cfg.ClassRatioDivideStrategy, cfg.ClassRatio);
                    FLDBServer.InsertTestCaseChangeClassDivInfo(ID, cfg, i, 1, groups);
                }
                else
                {
                    groups = divider.LoadGroups(dividedLists[0], dividedLists[1]);
                }
                // 遍历每一种算法
                for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
                {
                    // 评估
                    FLStaLocationEffort theEffort = FLDBServer.ReadLocationEffortofVersion(ID, methodlist[methodIndex], expDisc + "." + i.ToString());
                    if (null == theEffort)
                    {
                        FLStatementInfo[] rankedList = debugger.LocateFaultsEnsembleSort(groups, methodlist[methodIndex], cfg);
                        theEffort = m_Assessor.AssessRankedList(rankedList);
                        theEffort.AlgorithmName = methodlist[methodIndex];
                        theEffort.ExperimentDiscription = expDisc + "." + i.ToString();
                        // 保存
                        FLDBServer.DeleLocationEffortofVersion(ID, methodlist[methodIndex], theEffort.ExperimentDiscription);
                        m_Assessor.SaveResult(m_Assessor.VersionSetting, theEffort);
                    }
                    // 更新统计信息
                    UpdateStatic(staList[methodIndex], theEffort);
                }
            }
            #endregion

            #region 保存统计信息
            for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
            {
                StaticAveVar(staList[methodIndex], cfg.RepeatTimes, theStaticEfforts[methodIndex]);
                // 保存
                m_Assessor.SaveResult(m_Assessor.VersionSetting, theStaticEfforts[methodIndex]);
            }
            #endregion

            Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + ".Finished");
        }
        #endregion

        #region 无变更用例集成实验
        /// <summary>
        /// 无变更用例集成实验
        /// </summary>
        /// <param name="strSuiteName">实验包</param>
        /// <param name="cfg">实验配置</param>
        public void NoChangeClassEnsembleSortExperimentof(string strSuiteName, FLConfigure cfg)
        {
            DirectoryInfo[] suiteRoots = cfg.DataRootInfo.GetDirectories();
            int suiteIndex = 0;
            for (suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
                if (strSuiteName == suiteRoots[suiteIndex].Name)
                    break;

            // 遍历所有实验程序
            DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();
            for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
            {
                // 遍历所有缺陷版本
                DirectoryInfo[] versionRoots = programRoots[programIndex].GetDirectories();
                for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
                {
                    // FaultVersionInfo;
                    cfg.SuiteName = strSuiteName;
                    cfg.ProgramName = programRoots[programIndex].Name;
                    cfg.VersionName = versionRoots[versionIndex].Name;
                    //
                    NoChangeClassEnsembleSortExperimentof(cfg);
                } // end of versionIndex
            } // end of programIndex
        }

        public void NoChangeClassEnsembleSortExperimentof(string strSuiteName, string strProgramName, FLConfigure cfg)
        {
            DirectoryInfo[] suiteRoots = cfg.DataRootInfo.GetDirectories();
            int suiteIndex = 0;
            for (suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
                if (strSuiteName == suiteRoots[suiteIndex].Name)
                    break;

            // 遍历所有实验程序
            DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();
            for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
            {
                DirectoryInfo[] versionRoots = programRoots[programIndex].GetDirectories();
                if (strProgramName != programRoots[programIndex].Name)
                    continue;
                // 遍历所有缺陷版本
                for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
                {
                    // FaultVersionInfo;
                    cfg.SuiteName = strSuiteName;
                    cfg.ProgramName = programRoots[programIndex].Name;
                    cfg.VersionName = versionRoots[versionIndex].Name;
                    //
                    NoChangeClassEnsembleSortExperimentof(cfg);
                } // end of versionIndex
            } // end of programIndex
        }

        /// <summary>
        /// 无变更用例集成实验
        /// </summary>
        /// <param name="cfg">实验配置</param>
        public void NoChangeClassEnsembleSortExperimentof(FLConfigure cfg)
        {
            if (!CheckExperimentConfig(cfg))
                return;

            string expDisc = "无变更用例" + cfg.ClassRatioDivideStrategy + "_排位_权值" + cfg.WeightFormulaId.ToString() + "_" + cfg.IntegrateKernel;

            Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + ".Start");
            Console.WriteLine(expDisc);
            // 构建覆盖矩阵
            int ID = FLDBServer.GetIDofVersion(cfg.SuiteName, cfg.ProgramName, cfg.VersionName);
            if (null == m_Assessor.LoadFaultInfo(cfg))
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的缺陷信息");
                return;
            }
            FLBoolCovMatrix covMatrix = CreateBoolCovMatrix(cfg);
            if (null == covMatrix)
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的覆盖矩阵");
                return;
            }

            #region 准备统计信息
            FLStaLocationEffortStatic[] theStaticEfforts = new FLStaLocationEffortStatic[NumMethods];
            double[][] staList = new double[NumMethods][];
            for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
            {
                theStaticEfforts[methodIndex] = new FLStaLocationEffortStatic();
                theStaticEfforts[methodIndex].AlgorithmName = methodlist[methodIndex];
                theStaticEfforts[methodIndex].ExperimentDiscription = expDisc;
                staList[methodIndex] = new double[8];
                for (int tmpIndex = 0; tmpIndex < 8; tmpIndex++)
                    staList[methodIndex][tmpIndex] = 0;
            }
            #endregion

            #region 重复实验iTimes
            for (int i = 0; i < cfg.RepeatTimes; i++)
            {
                // 变更用例类别
               /* List<int>[] lists = SelectTestCasesToChangeClass(ID, cfg, covMatrix, i);
                List<bool[]> sucCases = covMatrix.ExtractSucCases(lists[0]);
                List<bool[]> falCases = covMatrix.ExtractFalCases(lists[1]);
                covMatrix.AppendSucCases(falCases);
                covMatrix.AppendFalCases(sucCases);*/
                // 初始化debugger,用例分组
                FLDebugger debugger = new FLDebugger(covMatrix);
                FLRunsGroupDivider divider = new FLRunsGroupDivider(covMatrix);
                List<int[]>[] dividedLists = FLDBServer.ReadTestCaseChangeClassDivInfo(ID, cfg, i, 1);
                List<FLRunsGroupInfo> groups = null;
                if (null == dividedLists)
                {
                    groups = divider.DivideClassRatioGroups(cfg.ClassRatioDivideStrategy, cfg.ClassRatio);
                    FLDBServer.InsertTestCaseChangeClassDivInfo(ID, cfg, i, 1, groups);
                }
                else
                {
                    groups = divider.LoadGroups(dividedLists[0], dividedLists[1]);
                }
                // 遍历每一种算法
                for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
                {
                    // 评估
                    FLStaLocationEffort theEffort = FLDBServer.ReadLocationEffortofVersion(ID, methodlist[methodIndex], expDisc + "." + i.ToString());
                    if (null == theEffort)
                    {
                        FLStatementInfo[] rankedList = debugger.LocateFaultsEnsembleSort(groups, methodlist[methodIndex], cfg);
                        theEffort = m_Assessor.AssessRankedList(rankedList);
                        theEffort.AlgorithmName = methodlist[methodIndex];
                        theEffort.ExperimentDiscription = expDisc + "." + i.ToString();
                        // 保存
                        FLDBServer.DeleLocationEffortofVersion(ID, methodlist[methodIndex], theEffort.ExperimentDiscription);
                        m_Assessor.SaveResult(m_Assessor.VersionSetting, theEffort);
                    }
                    // 更新统计信息
                    UpdateStatic(staList[methodIndex], theEffort);
                }
            }
        }            
        #endregion

        #region 不集成实验
        /// <summary>
        /// 不集成实验
        /// </summary>
        /// <param name="strSuiteName">实验包</param>
        /// <param name="cfg">实验配置</param>
        public void NoSpecialOperationExperimentOf(string strSuiteName, FLConfigure cfg)
        {
            DirectoryInfo[] suiteRoots = cfg.DataRootInfo.GetDirectories();
            int suiteIndex = 0;
            for (suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
                if (strSuiteName == suiteRoots[suiteIndex].Name)
                    break;

            // 遍历所有实验程序
            DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();
            for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
            {
                // 遍历所有缺陷版本
                DirectoryInfo[] versionRoots = programRoots[programIndex].GetDirectories();
                for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
                {
                    // FaultVersionInfo;
                    cfg.SuiteName = strSuiteName;
                    cfg.ProgramName = programRoots[programIndex].Name;
                    cfg.VersionName = versionRoots[versionIndex].Name;
                    //
                    NoSpecialOperationExperimentOf(cfg);
                } // end of versionIndex
            } // end of programIndex
        }

        public void NoSpecialOperationExperimentOf(string strSuiteName, string strProgramName, FLConfigure cfg)
        {
            DirectoryInfo[] suiteRoots = cfg.DataRootInfo.GetDirectories();
            int suiteIndex = 0;
            for (suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
                if (strSuiteName == suiteRoots[suiteIndex].Name)
                    break;

            // 遍历所有实验程序
            DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();
            for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
            {
                DirectoryInfo[] versionRoots = programRoots[programIndex].GetDirectories();
                if (strProgramName != programRoots[programIndex].Name)
                    continue;
                // 遍历所有缺陷版本
                for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
                {
                    // FaultVersionInfo;
                    cfg.SuiteName = strSuiteName;
                    cfg.ProgramName = programRoots[programIndex].Name;
                    cfg.VersionName = versionRoots[versionIndex].Name;
                    //
                    NoSpecialOperationExperimentOf(cfg);
                } // end of versionIndex
            } // end of programIndex
        }

        /// <summary>
        /// 不集成实验
        /// </summary>
        /// <param name="cfg">实验配置</param>
        public void NoSpecialOperationExperimentOf(FLConfigure cfg)
        {
            Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + ".Start");
            Console.WriteLine("不集成");
            // 构建覆盖矩阵
            int ID = FLDBServer.GetIDofVersion(cfg.SuiteName, cfg.ProgramName, cfg.VersionName);
            if (null == m_Assessor.LoadFaultInfo(cfg))
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的缺陷信息");
                return;
            }
            FLBoolCovMatrix covMatrix = CreateBoolCovMatrix(cfg);
            if (null == covMatrix)
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的覆盖矩阵");
                return;
            }
            // 初始化debugger,用例分组
            FLDebugger debugger = new FLDebugger(covMatrix);
            FLRunsGroupDivider divider = new FLRunsGroupDivider(covMatrix);
            FLRunsGroupInfo group = divider.NoDivide();
            // 遍历每一种算法
            for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
            {
                // 评估
                FLStaLocationEffort theEffort = FLDBServer.ReadLocationEffortofVersion(ID, methodlist[methodIndex], "不集成");
                if (null == theEffort)
                {
                    FLStatementInfo[] rankedList = debugger.LocateFaultsInGroup(methodlist[methodIndex], group);
                    theEffort = m_Assessor.AssessRankedList(rankedList);
                    theEffort.ExperimentDiscription = "不集成";
                    theEffort.AlgorithmName = methodlist[methodIndex];
                    // 保存
                    FLDBServer.DeleLocationEffortofVersion(ID, methodlist[methodIndex], theEffort.ExperimentDiscription);
                    m_Assessor.SaveResult(m_Assessor.VersionSetting, theEffort);
                }
            }
            Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + ".Finished");
        }

        #endregion

        private FLStaLocationEffortStatic InitStaticEffort(int id, string method, string description)
        {
            FLStaLocationEffortStatic staEffort = FLDBServer.ReadStatisticLocationEffortofVersion(id, method, description);

            if (null != staEffort)
                return staEffort;

            staEffort = new FLStaLocationEffortStatic();
            staEffort.AlgorithmName = method;
            staEffort.ExperimentDiscription = description;

            return staEffort;
        }

        private void UpdateStatic(double[] sta, FLStaLocationEffort theEffort)
        {
            sta[0] += theEffort.AveExpense;
            sta[1] += theEffort.AbsExpense;
            sta[2] += theEffort.LeastExpense;
            sta[3] += theEffort.MostExpense;
            sta[4] += (theEffort.AveExpense * theEffort.AveExpense);
            sta[5] += (theEffort.AbsExpense * theEffort.AbsExpense);
            sta[6] += (theEffort.LeastExpense * theEffort.LeastExpense);
            sta[7] += (theEffort.MostExpense * theEffort.MostExpense);
        }

        private void StaticAveVar(double[] sta, int iTimes, FLStaLocationEffortStatic effort)
        {
            effort.AveExpense = sta[0] / Convert.ToDouble(iTimes);
            effort.AbsExpense = sta[1] / Convert.ToDouble(iTimes);
            effort.LeastExpense = sta[2] / Convert.ToDouble(iTimes);
            effort.MostExpense = sta[3] / Convert.ToDouble(iTimes);

            effort.AveExpenseVariance = sta[4] / Convert.ToDouble(iTimes) - Math.Pow(effort.AveExpense, 2);
            effort.AbsExpenseVariance = sta[5] / Convert.ToDouble(iTimes) - Math.Pow(effort.AbsExpense, 2);
            effort.LeastExpenseVariance = sta[6] / Convert.ToDouble(iTimes) - Math.Pow(effort.LeastExpense, 2);
            effort.MostExpenseVariance = sta[7] / Convert.ToDouble(iTimes) - Math.Pow(effort.MostExpense, 2);
        }

        public void RemoveResultOf(string suiteName, string programName, string description)
        {
            // 获取versionNames
            List<string> versionNames = FLDBServer.GetVersionNameofProgram(suiteName, programName);
            //
            for(int versionIndex = 0; versionIndex < versionNames.Count; versionIndex++)
            {
                int ID = FLDBServer.GetIDofVersion(suiteName, programName, versionNames[versionIndex]);
                FLDBServer.DeleLocationEffortofVersion(ID, description);
                FLDBServer.DeleStatisticLocationEffortofVersion(ID, description);
            }
        }

        public void RemoveResultOf(string suiteName, string description)
        {
            // 获取programNames
            List<string> programNames = FLDBServer.GetProgramNameofSuite(suiteName);
            // 依次删除
            for (int programIndex = 0; programIndex < programNames.Count; programIndex++)
            {
                RemoveResultOf(suiteName, programNames[programIndex], description);
            }
        }

        public void RemoveResultOfFault(string suiteName, string programName, string faultName, string description)
        {
            int faultIndex = FLDBServer.GetIDofFault(suiteName, programName, faultName);
            FLDBServer.DeleLocationEffortofFault(faultIndex, description);
        }

        public void RemoveResultOf(string suiteName, string programName, string description, int iFaultNum)
        {
            int[] versionIDs = FLDBServer.GetIDsofVersionByNumFault(suiteName, programName, iFaultNum);
            for (int i = 0; i < versionIDs.Length; i++)
            {
                FLDBServer.DeleLocationEffortofVersion(versionIDs[i], description);
            }
        }
    }
}

/*
        #region 不集成实验
        /// <summary>
        /// 不集成
        /// </summary>
        public void NoSpecialOperationExperiment()
        {
            // 遍历所有实验包
            DirectoryInfo[] suiteRoots = m_DataRootInfo.GetDirectories();
            for (int suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
            {
                // 遍历所有实验程序
                DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();

                string hehe = suiteRoots[suiteIndex].Name;

                for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
                {
                    // 遍历所有缺陷版本
                    DirectoryInfo[] versionRoots = programRoots[programIndex].GetDirectories();
                    for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
                    {
                        // FaultVersionInfo;
                        string strSuiteName = suiteRoots[suiteIndex].Name;
                        string strProgramName = programRoots[programIndex].Name;
                        string strVersionName = versionRoots[versionIndex].Name;
                        
                        NoSpecialOperationExperimentOf(strSuiteName, strProgramName, strVersionName);

                        
                    }// end of versionIndex

                }// end of programIndex
            }// end of suiteIndex

        }

        public void NoSpecialOperationExperimentOf(string strSuiteName)
        {
            DirectoryInfo[] suiteRoots = m_DataRootInfo.GetDirectories();
            int suiteIndex = 0;
            for (suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
            {
                if (strSuiteName == suiteRoots[suiteIndex].Name)
                {
                    break;
                }
            }
            // 遍历所有实验程序
            DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();
            for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
            {
                // 遍历所有缺陷版本
                DirectoryInfo[] versionRoots = programRoots[programIndex].GetDirectories();
                for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
                {
                    // FaultVersionInfo;
                    string strProgramName = programRoots[programIndex].Name;
                    string strVersionName = versionRoots[versionIndex].Name;
                    //
                    NoSpecialOperationExperimentOf(strSuiteName, strProgramName, strVersionName);
                    
                }// end of versionIndex

            }// end of programIndex
        }

        public void NoSpecialOperationExperimentOf(string strSuiteName, string strProgramName, string strVersionName)
        {
            Console.WriteLine(strSuiteName + "." + strProgramName + "." + strVersionName + ".Start");

            // 初始化Debugger
            int ID = FLDBServer.GetIDofVersion(strSuiteName, strProgramName, strVersionName);
            if (m_Assessor.InitialDebugger(strSuiteName, strProgramName, strVersionName))
            {
                // 遍历每一种算法
                for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
                {

                    FLStaLocationEffort fromDB = FLDBServer.ReadLocationEffortofVersion(ID, methodlist[methodIndex], "不集成");
                    if (null == fromDB)
                    {
                        // 评估
                        FLStaLocationEffort theEffort = m_Assessor.AssessMethodUnderVersion(strSuiteName, strProgramName, strVersionName, methodlist[methodIndex]);
                        theEffort.ExperimentDiscription = "不集成";
                        // 保存
                        m_Assessor.SaveResult(m_Assessor.VersionSetting, theEffort);
                    }
                }
                Console.WriteLine(strSuiteName + "." + strProgramName + "." + strVersionName + ".Finished");
            }
            else
            {
                Console.WriteLine(strSuiteName + "." + strProgramName + "." + strVersionName + ".Not Exist");
            }
        }
    
        public void NoSpecialOperationExperimentOfFault(string suiteName, string programName, string faultName)
        {
            // 计算相关缺陷版本ID
            int[] IDs = FLDBServer.ReadFaultVersionIDWithFault(suiteName, programName, faultName);
            // 遍历每个缺陷版本 计算
            for (int idIndex = 0; idIndex < IDs.Length; idIndex++)
            {
                FLStaFaultVersionName theFault = FLDBServer.GetVersionNameByID(IDs[idIndex]);

                NoSpecialOperationExperimentOf(theFault.suiteName, theFault.programName, theFault.versionName);
            }
        }
      
        public void NoSpecialOperationExperimentOf(int iNumFaults)
        {
            Regex mulFaultPattern = new Regex(@"Mul_(\d+)_(\d+)");
            // 遍历所有实验包
            DirectoryInfo[] suiteRoots = m_DataRootInfo.GetDirectories();
            for (int suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
            {
                // 遍历所有实验程序
                DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();
                for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
                {
                    // 遍历所有缺陷版本
                    DirectoryInfo[] versionRoots = programRoots[programIndex].GetDirectories();
                    for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
                    {
                        // FaultVersionInfo;
                        string strSuiteName = suiteRoots[suiteIndex].Name;
                        string strProgramName = programRoots[programIndex].Name;
                        string strVersionName = versionRoots[versionIndex].Name;

                        int mNumFault = 1;
                        Match m = mulFaultPattern.Match(strVersionName);
                        int.TryParse(m.Groups[1].Value, out mNumFault);
                        if (iNumFaults == mNumFault)
                        {
                            NoSpecialOperationExperimentOf(strSuiteName, strProgramName, strVersionName);
                        }
                    }// end of versionIndex

                }// end of programIndex
            }// end of suiteIndex
        }
      
        public void NoSpecialOperationExperimentOf(string suiteName, string programName, int iNumFault)
        {
            // 计算相关缺陷版本ID
            int[] IDs = FLDBServer.GetIDsofVersionByNumFault(suiteName, programName, iNumFault);
            // 遍历每个缺陷版本 计算
            for (int idIndex = 0; idIndex < IDs.Length; idIndex++)
            {
                FLStaFaultVersionName theFault = FLDBServer.GetVersionNameByID(IDs[idIndex]);

                NoSpecialOperationExperimentOf(theFault.suiteName, theFault.programName, theFault.versionName);
            }
        }
        #endregion
*/
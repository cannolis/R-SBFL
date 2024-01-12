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

            string falFileName = cfg.DataRootInfo.FullName + "\\"
                               + cfg.SuiteName + "\\"
                               + cfg.ProgramName + "\\"
                               + cfg.VersionName + "\\1_crash_traces";
            return new FLDebugger(sucFileName, falFileName);
        }

        /// <summary>
        /// 查询一个覆盖矩阵
        /// </summary>
        /// <param name="cfg">配置信息</param>
        /// <returns>覆盖矩阵</returns>
        public static FLBoolCovMatrix CheckBoolCovMatrix(FLConfigure cfg)
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

        // 李成龙添加
        public static int randomOne(double weight)
        {
            Random rnd = new Random();
            int choice = rnd.Next(100);
            if (choice <= Convert.ToInt32(Math.Floor(100 * weight)))
                return 1;
            else
                return 0;
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
                return lists;  // TODO: 若直接返回，会导致相同变更比例的用例不进行更新（即使改变了用例挑选策略）

            double fRate = cfg.ClassChangeRatio;
            List<int> iSucCandidates = new List<int>();
            List<int> iFalCandidates = new List<int>();

            for (int i = 0; i < covMatrix.NumSucRuns; i++)
                iSucCandidates.Add(i);
            for (int i = 0; i < covMatrix.NumFalRuns; i++)
                iFalCandidates.Add(i);

            int iCountInterger = Convert.ToInt32(Math.Max(0, Math.Truncate(fRate * covMatrix.NumRuns)));
            double iCountFractional = fRate * covMatrix.NumRuns - iCountInterger;


            // 保证各类别变更数量尽量不超过其一定比例，但至少存在一个变更用例
            double iMaxChangeCount = Math.Max(1.0, Math.Min(Math.Floor(covMatrix.NumFalRuns * cfg.MaxClassChangeRatioInOriginal),
                Math.Floor(covMatrix.NumSucRuns * cfg.MaxClassChangeRatioInOriginal)));
            double rate = (double)covMatrix.NumSucRuns / (covMatrix.NumFalRuns + covMatrix.NumSucRuns);


            List<int> sucCandidates = new FastDeepCloner(iSucCandidates).Clone<List<int>>();
            List<int> falCandidates = new FastDeepCloner(iFalCandidates).Clone<List<int>>();
            if (cfg.ChangeSucRatio < 0) //不区分成例失例  李成龙改（分开标错）
            {
                // 李成龙改
                int iCount = iCountInterger + randomOne(iCountFractional);

                //List<int>[] tempLists = FLRunsFilter.RandomIntsNR(iCount, sucCandidates, falCandidates); //预先暂定成例和失利的变更数量
                //int iSucCount = tempLists[0].Count;//暂定选取的成例数  
                //int iFalCount = iCount - iSucCount;//暂定选取的失例数

                int iSucCountInterger = Convert.ToInt32(Math.Max(0, Math.Truncate(rate * iCount)));
                double iSucCountFractional = rate * iCount - iSucCountInterger;
                int iSucCount = Math.Max(0, Math.Min(iCount, iSucCountInterger + randomOne(iSucCountFractional)));//选取的成例数
                int iFalCount = Math.Max(0, Math.Min(iCount, iCount - iSucCount));//选取的失例数
                int sucCount = (int)Math.Min(iSucCount, iMaxChangeCount); // 确保变更数量小于该类别最大变更数量
                int falCount = (int)Math.Min(iFalCount, iMaxChangeCount);
                lists = new List<int>[2];
                lists[0] = new List<int>(FLRunsFilter.RandomIntsNB(sucCount, sucCandidates));
                lists[1] = new List<int>(FLRunsFilter.RandomIntsNB(falCount, falCandidates));
            }
            else  //区分成例失例
            {
                // 李成龙改
                int iCount = iCountInterger + randomOne(iCountFractional);

                //int iSucCount = Math.Min(iCount,Convert.ToInt32(Math.Max(0, Math.Ceiling(cfg.ChangeSucRatio * iCount))));//选取的成例数  李成龙改
                //int iFalCount = iCount - iSucCount;//选取的失例数

                int iSucCountInterger = Convert.ToInt32(Math.Max(0, Math.Truncate(cfg.ChangeSucRatio * iCount)));
                double iSucCountFractional = cfg.ChangeSucRatio * iCount - iSucCountInterger;
                int iSucCount = Math.Max(0, Math.Min(iCount, iSucCountInterger + randomOne(iSucCountFractional)));//选取的成例数
                int iFalCount = Math.Max(0, Math.Min(iCount, iCount - iSucCount));//选取的失例数
                int sucCount = (int)Math.Min(iSucCount, iMaxChangeCount);  // 确保变更数量小于该类别原始数量
                int falCount = (int)Math.Min(iFalCount, iMaxChangeCount);
                lists = new List<int>[2];
                lists[0] = new List<int>(FLRunsFilter.RandomIntsNB(sucCount, sucCandidates));
                lists[1] = new List<int>(FLRunsFilter.RandomIntsNB(falCount, falCandidates));
            }

            int times = 0;
            // lists[0]中选择出来的是成例
            // lists[1]中选择出来的是失例
            while ((0 == (sucCandidates.Count + lists[1].Count)) || (0 == (falCandidates.Count + lists[0].Count)))  // 如果变更之后导致某类别数量为0，则重新变更
            {

                times++;
                if (times > 10000)
                    return null;
                sucCandidates = new FastDeepCloner(iSucCandidates).Clone<List<int>>(); // 李成龙改 从原始集合中抽取
                falCandidates = new FastDeepCloner(iFalCandidates).Clone<List<int>>();

                if (cfg.ChangeSucRatio < 0)//不区分成例失例  李成龙改（分开标错）
                {

                    // 李成龙改
                    int iCount = iCountInterger + randomOne(iCountFractional);
                    //List<int>[] tempLists = FLRunsFilter.RandomIntsNR(iCount, sucCandidates, falCandidates); //预先暂定成例和失利的变更数量
                    //int iSucCount = tempLists[0].Count;//暂定选取的成例数  
                    //int iFalCount = iCount - iSucCount;//暂定选取的失例数

                    int iSucCountInterger = Convert.ToInt32(Math.Max(0, Math.Truncate(rate * iCount)));
                    double iSucCountFractional = rate * iCount - iSucCountInterger;
                    int iSucCount = Math.Max(0, Math.Min(iCount, iSucCountInterger + randomOne(iSucCountFractional)));//选取的成例数
                    int iFalCount = Math.Max(0, Math.Min(iCount, iCount - iSucCount));//选取的失例数

                    int sucCount = (int)Math.Min(iSucCount, iMaxChangeCount); //  确保变更数量小于该类别原始数量
                    int falCount = (int)Math.Min(iFalCount, iMaxChangeCount);
                    lists = new List<int>[2];
                    lists[0] = new List<int>(FLRunsFilter.RandomIntsNB(sucCount, sucCandidates));
                    lists[1] = new List<int>(FLRunsFilter.RandomIntsNB(falCount, falCandidates));
                }
                else  //区分成例失例
                {
                    // 李成龙改
                    int iCount = iCountInterger + randomOne(iCountFractional);
                    //int iSucCount = Math.Min(iCount, Convert.ToInt32(Math.Max(0, Math.Ceiling(cfg.ChangeSucRatio * iCount))));//选取的成例数  李成龙改
                    //int iFalCount = iCount - iSucCount;//选取的失例数

                    int iSucCountInterger = Convert.ToInt32(Math.Max(0, Math.Truncate(cfg.ChangeSucRatio * iCount)));
                    double iSucCountFractional = cfg.ChangeSucRatio * iCount - iSucCountInterger;
                    int iSucCount = Math.Max(0, Math.Min(iCount, iSucCountInterger + randomOne(iSucCountFractional)));//选取的成例数
                    int iFalCount = Math.Max(0, Math.Min(iCount, iCount - iSucCount));//选取的失例数

                    int sucCount = (int)Math.Min(iSucCount, iMaxChangeCount);  // 确保变更数量小于该类别原始数量
                    int falCount = (int)Math.Min(iFalCount, iMaxChangeCount);
                    lists = new List<int>[2];
                    lists[0] = new List<int>(FLRunsFilter.RandomIntsNB(sucCount, sucCandidates));
                    lists[1] = new List<int>(FLRunsFilter.RandomIntsNB(falCount, falCandidates));
                }
            }

            FLDBServer.InsertTestCaseChangeClassInfo(ID, cfg.ClassChangeSelectStrategy, itimes, lists);
            return lists;
        }




        // 李成龙增加，以少数类的变更比例作为整体的变更比例
        public static List<int>[] SelectTestCasesToChangeClassNew(int ID, FLConfigure cfg, FLBoolCovMatrix covMatrix, int itimes)
        {
            List<int>[] lists = FLDBServer.ReadTestCaseChangeClassInfo(ID, cfg.ClassChangeSelectStrategy, itimes);
            if (null != lists)
                return lists;  // TODO: 若直接返回，会导致相同变更比例的用例不进行更新（即使改变了用例挑选策略）

            double fRate = cfg.ClassChangeRatio;
            List<int> iSucCandidates = new List<int>();
            List<int> iFalCandidates = new List<int>();

            for (int i = 0; i < covMatrix.NumSucRuns; i++)
                iSucCandidates.Add(i);
            for (int i = 0; i < covMatrix.NumFalRuns; i++)
                iFalCandidates.Add(i);

            int numMinorityRuns = Math.Min(covMatrix.NumFalRuns, covMatrix.NumSucRuns);
            int iCountInterger = Convert.ToInt32(Math.Max(0, Math.Truncate(fRate * numMinorityRuns)));
            double iCountFractional = fRate * numMinorityRuns - iCountInterger;

            double rate = (double)covMatrix.NumSucRuns / (covMatrix.NumFalRuns + covMatrix.NumSucRuns);


            List<int> sucCandidates = new FastDeepCloner(iSucCandidates).Clone<List<int>>();
            List<int> falCandidates = new FastDeepCloner(iFalCandidates).Clone<List<int>>();
            if (cfg.ChangeSucRatio < 0) //不区分成例失例  李成龙改（分开标错）
            {
                // 李成龙改
                int iCount = iCountInterger + randomOne(iCountFractional);
                int sucCountInterger = Convert.ToInt32(Math.Max(0, Math.Truncate(rate * iCount)));
                double sucCountFractional = rate * iCount - sucCountInterger;
                int iSucCount = Math.Max(0, Math.Min(iCount, sucCountInterger + randomOne(sucCountFractional)));//选取的成例数
                int iFalCount = Math.Max(0, Math.Min(iCount, iCount - iSucCount));//选取的失例数
                lists = new List<int>[2];
                lists[0] = new List<int>(FLRunsFilter.RandomIntsNB(iSucCount, sucCandidates));
                lists[1] = new List<int>(FLRunsFilter.RandomIntsNB(iFalCount, falCandidates));
            }
            else  //区分成例失例
            {
                // 李成龙改
                int iCount = iCountInterger + randomOne(iCountFractional);
                int sucCountInterger = Convert.ToInt32(Math.Max(0, Math.Truncate(cfg.ChangeSucRatio * iCount)));
                double sucCountFractional = cfg.ChangeSucRatio * iCount - sucCountInterger;
                int iSucCount = Math.Max(0, Math.Min(iCount, sucCountInterger + randomOne(sucCountFractional)));//选取的成例数
                int iFalCount = Math.Max(0, Math.Min(iCount, iCount - iSucCount));//选取的失例数
                lists = new List<int>[2];
                lists[0] = new List<int>(FLRunsFilter.RandomIntsNB(iSucCount, sucCandidates));
                lists[1] = new List<int>(FLRunsFilter.RandomIntsNB(iFalCount, falCandidates));
            }

            int times = 0;
            // lists[0]中选择出来的是成例
            // lists[1]中选择出来的是失例
            while ((0 == (sucCandidates.Count + lists[1].Count)) || (0 == (falCandidates.Count + lists[0].Count)))  // 如果变更之后导致某类别数量为0，则重新变更
            {

                times++;
                if (times > 10000)
                    return null;
                sucCandidates = new FastDeepCloner(iSucCandidates).Clone<List<int>>(); // 李成龙改 从原始集合中抽取
                falCandidates = new FastDeepCloner(iFalCandidates).Clone<List<int>>();

                if (cfg.ChangeSucRatio < 0)//不区分成例失例  李成龙改（分开标错）
                {
                    // 李成龙改
                    int iCount = iCountInterger + randomOne(iCountFractional);
                    int sucCountInterger = Convert.ToInt32(Math.Max(0, Math.Truncate(rate * iCount)));
                    double sucCountFractional = rate * iCount - sucCountInterger;
                    int iSucCount = Math.Max(0, Math.Min(iCount, sucCountInterger + randomOne(sucCountFractional)));//选取的成例数
                    int iFalCount = Math.Max(0, Math.Min(iCount, iCount - iSucCount));//选取的失例数
                    lists = new List<int>[2];
                    lists[0] = new List<int>(FLRunsFilter.RandomIntsNB(iSucCount, sucCandidates));
                    lists[1] = new List<int>(FLRunsFilter.RandomIntsNB(iFalCount, falCandidates));
                }
                else  //区分成例失例
                {
                    // 李成龙改
                    int iCount = iCountInterger + randomOne(iCountFractional);
                    int sucCountInterger = Convert.ToInt32(Math.Max(0, Math.Truncate(cfg.ChangeSucRatio * iCount)));
                    double sucCountFractional = cfg.ChangeSucRatio * iCount - sucCountInterger;
                    int iSucCount = Math.Max(0, Math.Min(iCount, sucCountInterger + randomOne(sucCountFractional)));//选取的成例数
                    int iFalCount = Math.Max(0, Math.Min(iCount, iCount - iSucCount));//选取的失例数
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
            {
                Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的用例数目不足");
                return false;

            }

            //// 李成龙添加
            //if (info.NumFaults > 1)
            //{
            //    Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "跳过");
            //    return false;
            //}



            double ratio = Convert.ToDouble(info.NumSucRuns) / Convert.ToDouble(info.NumFalRuns);
            if (cfg.MinClassRatio > 0)
            {
                if (ratio < cfg.MinClassRatio)
                {
                    Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的成失例比例不符");
                    return false;
                }
            }
            if (cfg.MaxClassRatio > 0)
            {
                if (ratio > cfg.MaxClassRatio)
                {
                    Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的成失例比例不符");
                    return false;
                }
            }


            if ((cfg.MaxClassChangeRatioInOriginal * info.NumFalRuns < 1) || (cfg.MaxClassChangeRatioInOriginal * info.NumSucRuns < 1)) // 李成龙改 最大变更数量不得低于1个，以免该用例集丢失扰动
            {
                Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的变更数量不足");
                return false;
            }

            return true;
        }



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

            if (!CheckExperimentConfig(cfg))  // 李成龙改
                return;

            // 构建覆盖矩阵
            int ID = FLDBServer.GetIDofVersion(cfg.SuiteName, cfg.ProgramName, cfg.VersionName);
            if (null == m_Assessor.LoadFaultInfo(cfg))
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的缺陷信息");
                return;
            }
            if (cfg.isInstrumented == false)     // 李成龙添加
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的插桩信息");
                return;
            }


            //// 初始化debugger,用例分组
            //FLDebugger debugger = new FLDebugger(covMatrix);
            //FLRunsGroupDivider divider = new FLRunsGroupDivider(covMatrix);
            //FLRunsGroupInfo group = divider.NoDivide();

            FLBoolCovMatrix covMatrix = null;
            FLDebugger debugger = null;
            FLRunsGroupInfo group = null;

            // 遍历每一种算法
            for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
            {
                // 评估
                FLStaLocationEffort theEffort = FLDBServer.ReadLocationEffortofVersion(ID, methodlist[methodIndex], "不集成");
                if (null == theEffort)
                {
                    if (null == covMatrix)
                    {
                        covMatrix = CreateBoolCovMatrix(cfg);
                        if (null == covMatrix)
                        {
                            Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的覆盖矩阵");
                            return;
                        }
                    }
                    if (null == debugger) { debugger = new FLDebugger(covMatrix); }
                    if (null == group) { FLRunsGroupDivider divider = new FLRunsGroupDivider(covMatrix); group = divider.NoDivide(); }


                    FLStatementInfo[] rankedList = debugger.LocateFaultsInGroup(methodlist[methodIndex], group);
                    //FLStatementInfo[] rankedList = debugger.LocateFaultsSymmetryInGroup(methodlist[methodIndex], group);
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

        #region 不变更用例集成实验
        /// <summary>
        /// 不变更用例集成实验
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
        /// 不变更用例排位集成实验
        /// </summary>
        /// <param name="cfg">实验配置</param>
        public void NoChangeClassEnsembleSortExperimentof(FLConfigure cfg)
        {

            //string expDisc = "不变更用例" + "_" + cfg.ClassRatioDivideStrategy + "_排位_权值" + cfg.WeightFormulaId.ToString() + "_" + cfg.IntegrateKernel;
            string expDisc = "不变更用例" + "_集成";


            Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + ".Start");
            Console.WriteLine(expDisc);

            if (!CheckExperimentConfig(cfg))   // 李成龙改
                return;

            // 构建覆盖矩阵
            int ID = FLDBServer.GetIDofVersion(cfg.SuiteName, cfg.ProgramName, cfg.VersionName);
            if (null == m_Assessor.LoadFaultInfo(cfg))
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的缺陷信息");
                return;
            }
            if (cfg.isInstrumented == false)     // 李成龙添加
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的插桩信息");
                return;
            }

            FLBoolCovMatrix covMatrix = null;


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

            #region 重复实验1次，让集成模块重复itimes
            for (int i = 0; i < cfg.RepeatTimes; i++)
            {

                FLDebugger debugger = null;
                List<FLRunsGroupInfo> groups = null;


                //// 初始化debugger,用例分组
                //FLDebugger debugger = new FLDebugger(covMatrix);
                //FLRunsGroupDivider divider = new FLRunsGroupDivider(ID, cfg, i, 1, covMatrix);  // 李成龙改

                // 遍历每一种算法
                for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
                {
                    // 评估
                    
                    FLStaLocationEffort theEffort = FLDBServer.ReadLocationEffortofVersion(ID, methodlist[methodIndex], expDisc + "." + i.ToString());
                    if (null == theEffort)   // 李成龙改
                    {
                        if (null == covMatrix)
                        {
                            covMatrix = FLDBServer.ReadCovMatrixInfo(ID, "不变更用例", 0);
                            //李成龙新增 将coVMatrix存储到数据库中
                            if (null == covMatrix)
                            {
                                covMatrix = CreateBoolCovMatrix(cfg);
                                if (null == covMatrix)
                                {
                                    Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的覆盖矩阵");
                                    return;
                                }

                                FLDBServer.InsertCovMatrixInfo(ID, "不变更用例", 0, covMatrix);
                            }
                        }
                        if (null == debugger) { debugger = new FLDebugger(covMatrix); }
                        if (null == groups)
                        {
                            FLRunsGroupDivider divider = new FLRunsGroupDivider(ID, cfg, i, cfg.RepeatTimes, covMatrix);
                            #region 李成龙改，上层函数
                            List<int[]>[] dividedLists = FLDBServer.ReadTestCaseChangeClassDivInfo(ID, cfg, i, cfg.RepeatTimes);
                            if (null == dividedLists)
                            {
                                groups = divider.DivideClassRatioGroups(cfg.ClassRatioDivideStrategy, cfg.ClassRatio);
                                // FLDBServer.InsertTestCaseChangeClassDivInfo(ID, cfg, i, 1, groups); // 李成龙改
                            }
                            else
                            {
                                groups = divider.LoadGroups(dividedLists[0], dividedLists[1]);
                            }
                            #endregion
                        }

                        FLStatementInfo[] rankedList = debugger.LocateFaultsEnsembleSort(groups, methodlist[methodIndex], cfg);
                        //FLStatementInfo[] rankedList = debugger.LocateFaultSymmetrySort(groups, methodlist[methodIndex], cfg); // 李成龙改 定位公式镜像
                        //FLStatementInfo[] rankedList = debugger.LocateFaultsIterateSort(groups, methodlist[methodIndex], cfg); // 李成龙改 定位程序谱鲁棒性
                        theEffort = m_Assessor.AssessRankedList(rankedList);
                        theEffort.AlgorithmName = methodlist[methodIndex];
                        theEffort.ExperimentDiscription = expDisc + "." + i.ToString();
                        // 保存
                        FLDBServer.DeleLocationEffortofVersion(ID, methodlist[methodIndex], theEffort.ExperimentDiscription);
                        m_Assessor.SaveResult(m_Assessor.VersionSetting, theEffort);
                    }
                    // 更新统计信息
                    UpdateStatic(ref staList[methodIndex], theEffort);

                }
            }
            #endregion


            #region 保存统计信息
            for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
            {
                StaticAveVar(staList[methodIndex], cfg.RepeatTimes, ref theStaticEfforts[methodIndex]);
                // 保存
                m_Assessor.SaveResult(m_Assessor.VersionSetting, theStaticEfforts[methodIndex]);
            }
            #endregion

            Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + ".Finished");
        }
        #endregion

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

            string expDisc = cfg.ClassChangeSelectStrategy + "_不集成";
            Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + ".Start");
            Console.WriteLine(expDisc);

            if (!CheckExperimentConfig(cfg))   // 李成龙改
                return;

            // 构建覆盖矩阵
            int ID = FLDBServer.GetIDofVersion(cfg.SuiteName, cfg.ProgramName, cfg.VersionName);
            if (null == m_Assessor.LoadFaultInfo(cfg))
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的缺陷信息");
                return;
            }
            if (cfg.isInstrumented == false)    // 李成龙添加
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的插桩信息");
                return;
            }

            //FLBoolCovMatrix tempCovMatrix = CreateBoolCovMatrix(cfg);
            //if (null == tempCovMatrix)
            //{
            //    Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的覆盖矩阵");
            //    return;
            //}

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


            FLBoolCovMatrix covMatrix = null;
            #region 重复实验iTimes
            for (int i = 0; i < cfg.RepeatTimes; i++)
            {
                FLDebugger debugger = null;
                FLRunsGroupInfo group = null;
                FLBoolCovMatrix changedCovMatrix = null;


                //// 初始化debugger
                //FLDebugger debugger = new FLDebugger(covMatrix);
                //FLRunsGroupDivider divider = new FLRunsGroupDivider(covMatrix);
                //FLRunsGroupInfo group = divider.NoDivide();
                // 遍历每一种算法
                for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
                {
                    // 评估
                    FLStaLocationEffort theEffort = FLDBServer.ReadLocationEffortofVersion(ID, methodlist[methodIndex], expDisc + "." + i.ToString());
                    if (null == theEffort)
                    {
                        if (null == changedCovMatrix)
                        {
                            if (null == covMatrix)
                            {
                                covMatrix = FLDBServer.ReadCovMatrixInfo(ID, "不变更用例", 0);
                                //李成龙新增 将coVMatrix存储到数据库中
                                if (null == covMatrix)
                                {
                                    covMatrix = CreateBoolCovMatrix(cfg);
                                    if (null == covMatrix)
                                    {
                                        Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的覆盖矩阵");
                                        return;
                                    }
                                    FLDBServer.InsertCovMatrixInfo(ID, "不变更用例", 0, covMatrix);
                                }
                            }
                            changedCovMatrix = CreateBoolCovMatrix(cfg);
                            // 变更用例类别
                            List<int>[] lists = SelectTestCasesToChangeClass(ID, cfg, changedCovMatrix, i);  // 李成龙临时改
                            List<bool[]> sucCases = changedCovMatrix.ExtractSucCases(lists[0]);
                            List<bool[]> falCases = changedCovMatrix.ExtractFalCases(lists[1]);
                            changedCovMatrix.AppendSucCases(falCases);
                            changedCovMatrix.AppendFalCases(sucCases);
                        }

                        if (null == debugger) { debugger = new FLDebugger(changedCovMatrix); }
                        if (null == group)
                        {
                            FLRunsGroupDivider divider = new FLRunsGroupDivider(changedCovMatrix);
                            group = divider.NoDivide();
                        }

                        FLStatementInfo[] rankedList = debugger.LocateFaultsInGroup(methodlist[methodIndex], group);
                        //FLStatementInfo[] rankedList = debugger.LocateFaultsSymmetryInGroup(methodlist[methodIndex], group);
                        theEffort = m_Assessor.AssessRankedList(rankedList);
                        theEffort.ExperimentDiscription = expDisc + "." + i.ToString();
                        theEffort.AlgorithmName = methodlist[methodIndex];
                        // 保存
                        FLDBServer.DeleLocationEffortofVersion(ID, methodlist[methodIndex], theEffort.ExperimentDiscription);
                        m_Assessor.SaveResult(m_Assessor.VersionSetting, theEffort);
                    }
                    // 更新统计信息
                    UpdateStatic(ref staList[methodIndex], theEffort);
                }
            }
            #endregion

            #region 保存统计信息
            for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
            {
                StaticAveVar(staList[methodIndex], cfg.RepeatTimes, ref theStaticEfforts[methodIndex]);
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

            //string expDisc = cfg.ClassChangeSelectStrategy + "_" + cfg.ClassRatioDivideStrategy + "_排位_权值" + cfg.WeightFormulaId.ToString() + "_" + cfg.IntegrateKernel;
            string expDisc = cfg.ClassChangeSelectStrategy + "_集成";


            Console.WriteLine(cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + ".Start");
            Console.WriteLine(expDisc);

            if (!CheckExperimentConfig(cfg))   // 李成龙改 
                return;

            // 构建覆盖矩阵
            int ID = FLDBServer.GetIDofVersion(cfg.SuiteName, cfg.ProgramName, cfg.VersionName);
            if (null == m_Assessor.LoadFaultInfo(cfg))
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的缺陷信息");
                return;
            }

            if (cfg.isInstrumented == false)     // 李成龙添加
            {
                Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的插桩信息");
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

            FLBoolCovMatrix covMatrix = null;
            #region 重复实验iTimes

            for (int i = 0; i < cfg.RepeatTimes; i++)
            {
                FLDebugger debugger = null;
                List<FLRunsGroupInfo> groups = null;
                FLBoolCovMatrix changedCovMatrix = null;

                //// 初始化debugger,用例分组
                //FLDebugger debugger = new FLDebugger(covMatrix);
                //FLRunsGroupDivider divider = new FLRunsGroupDivider(ID, cfg, i, 1, covMatrix);  // 李成龙改


                // 遍历每一种算法
                for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
                {
                    // 评估
                    
                    FLStaLocationEffort theEffort = FLDBServer.ReadLocationEffortofVersion(ID, methodlist[methodIndex], expDisc + "." + i.ToString());
                    if (null == theEffort)  // 李成龙改
                    {
                        if (null == changedCovMatrix)
                        {
                            if (null == covMatrix)
                            {
                                covMatrix = FLDBServer.ReadCovMatrixInfo(ID, "不变更用例", 0);
                                //李成龙新增 将coVMatrix存储到数据库中
                                if (null == covMatrix)
                                {
                                    covMatrix = CreateBoolCovMatrix(cfg);
                                    if (null == covMatrix)
                                    {
                                        Console.WriteLine("找不到" + cfg.SuiteName + "." + cfg.ProgramName + "." + cfg.VersionName + "的覆盖矩阵");
                                        return;
                                    }
                                    FLDBServer.InsertCovMatrixInfo(ID, "不变更用例", 0, covMatrix);
                                }
                            }
                            changedCovMatrix = CreateBoolCovMatrix(cfg);
                            // 变更用例类别
                            List<int>[] lists = SelectTestCasesToChangeClass(ID, cfg, changedCovMatrix, i);  // 李成龙临时改
                            List<bool[]> sucCases = changedCovMatrix.ExtractSucCases(lists[0]);
                            List<bool[]> falCases = changedCovMatrix.ExtractFalCases(lists[1]);
                            changedCovMatrix.AppendSucCases(falCases);
                            changedCovMatrix.AppendFalCases(sucCases);
                        }
                        if (null == debugger) { debugger = new FLDebugger(changedCovMatrix); }
                        if (null == groups)
                        {
                            FLRunsGroupDivider divider = new FLRunsGroupDivider(ID, cfg, i, cfg.RepeatTimes, changedCovMatrix);
                            #region 李成龙改，上层函数
                            List<int[]>[] dividedLists = FLDBServer.ReadTestCaseChangeClassDivInfo(ID, cfg, i, cfg.RepeatTimes);
                            if (null == dividedLists)
                            {
                                groups = divider.DivideClassRatioGroups(cfg.ClassRatioDivideStrategy, cfg.ClassRatio);
                                // FLDBServer.InsertTestCaseChangeClassDivInfo(ID, cfg, i, 1, groups); // 李成龙改
                            }
                            else
                            {
                                groups = divider.LoadGroups(dividedLists[0], dividedLists[1]);
                            }
                            #endregion
                        }

                        FLStatementInfo[] rankedList = debugger.LocateFaultsEnsembleSort(groups, methodlist[methodIndex], cfg);
                        //FLStatementInfo[] rankedList = debugger.LocateFaultSymmetrySort(groups, methodlist[methodIndex], cfg); // 李成龙改 定位公式镜像
                        //FLStatementInfo[] rankedList = debugger.LocateFaultsIterateSort(groups, methodlist[methodIndex], cfg); // 李成龙改 定位程序谱鲁棒性
                        theEffort = m_Assessor.AssessRankedList(rankedList);
                        theEffort.AlgorithmName = methodlist[methodIndex];
                        theEffort.ExperimentDiscription = expDisc + "." + i.ToString();
                        // 保存
                        FLDBServer.DeleLocationEffortofVersion(ID, methodlist[methodIndex], theEffort.ExperimentDiscription);
                        m_Assessor.SaveResult(m_Assessor.VersionSetting, theEffort);
                    }
                    // 更新统计信息
                    UpdateStatic(ref staList[methodIndex], theEffort);
                }
            }
            #endregion

            #region 保存统计信息
            for (int methodIndex = 0; methodIndex < NumMethods; methodIndex++)
            {
                StaticAveVar(staList[methodIndex], cfg.RepeatTimes, ref theStaticEfforts[methodIndex]);
                // 保存
                m_Assessor.SaveResult(m_Assessor.VersionSetting, theStaticEfforts[methodIndex]);
            }
            #endregion

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

        private void UpdateStatic(ref double[] sta, FLStaLocationEffort theEffort)
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

        private void StaticAveVar(double[] sta, int iTimes, ref FLStaLocationEffortStatic effort)
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
            for (int versionIndex = 0; versionIndex < versionNames.Count; versionIndex++)
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Remoting.Messaging;
//
using FaultLocalization;
//
using DBDll;

namespace FaultLocalization
{
    /// <summary>
    /// 解析缺陷版本信息
    /// </summary>
    public class FLFaultVersionInfoParser
    {
        /// <summary>
        /// 数据源
        /// </summary>
        private DirectoryInfo m_DataRootInfo = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mDataRootInfo">数据源</param>
        public FLFaultVersionInfoParser(DirectoryInfo mDataRootInfo)
        {
            m_DataRootInfo = mDataRootInfo;
        }

        #region 线程同步实现
        //执行任务的委托声明 - 解决长任务导致程序假死
        delegate void TaskFunctionDelegate();

        /// <summary>
        /// 委托(线程)完成之后回调的函数 
        /// EndInvoke的返回值类型必须与TaskFunction()的返回值类型一样.
        /// 当线程执行完毕后执行的TaskFinished()中,使用EndInvoke来取得这个函数的返回值.
        /// </summary>
        /// <param name="result"></param>
        private void TaskFinished(IAsyncResult result)
        {
            //获取Delegate句柄 - 也可以通过定义全局变量的方式来实现
            TaskFunctionDelegate mTaskFunctionDelegate = result.AsyncState as TaskFunctionDelegate;

            mTaskFunctionDelegate.EndInvoke(result);

            MessageBox.Show("haha");
        }
        #endregion

        /// <summary>
        /// 解析实验数据
        /// </summary>
        public void ParseFaultVersionsInfo()
        {
            //同步读取数据
            TaskFunctionDelegate mTaskFunctionDelegate = ParseAllFaultVersionsInfo;
            mTaskFunctionDelegate.BeginInvoke(new AsyncCallback(TaskFinished), mTaskFunctionDelegate);
        }

        public void ParseMulFaultVersionsInfo()
        {
            //同步读取数据
            TaskFunctionDelegate mTaskFunctionDelegate = ParseAllMulFaultVersionsInfo;
            mTaskFunctionDelegate.BeginInvoke(new AsyncCallback(TaskFinished), mTaskFunctionDelegate);
        }

        /// <summary>
        /// 解析所有缺陷版本
        /// </summary>
        private void ParseAllFaultVersionsInfo()
        {
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
                        // FaultVersionInfo
                        FLStaFaultVersionInfo mFaultVersion = new FLStaFaultVersionInfo();
                        mFaultVersion.SuiteName = suiteRoots[suiteIndex].Name;
                        mFaultVersion.ProgramName = programRoots[programIndex].Name;
                        mFaultVersion.VersionName = versionRoots[versionIndex].Name;
                        //
                        string fileSucTraces = versionRoots[versionIndex].FullName + "\\1_success_traces";
                        string fileFalTraces = versionRoots[versionIndex].FullName + "\\1_crash_traces";
                        string fileAllTraces = versionRoots[versionIndex].FullName + "\\1.txt";
                        if (File.Exists(fileSucTraces) && File.Exists(fileFalTraces))
                        {
                            // 装载成例与失例数据
                            string[] mSucData = File.ReadAllLines(fileSucTraces);
                            string[] mFalData = File.ReadAllLines(fileFalTraces);
                            //  获取两类用例数
                            mFaultVersion.NumSucRuns = mSucData.Length;
                            mFaultVersion.NumFalRuns = mFalData.Length;
                            //  获取用例总数
                            string[] mAllData = File.ReadAllLines(fileAllTraces);
                            mFaultVersion.NumRuns = mAllData.Length;
                            //
                            if (mFaultVersion.NumRuns != (mFaultVersion.NumSucRuns + mFaultVersion.NumFalRuns))
                            {
                                //throw new Exception("Error NumRuns---" + mFaultVersion.SuiteName + "-" + mFaultVersion.ProgramName + "-" + mFaultVersion.VersionName);
                            }
                            //  获取语句数
                            string[] temp = mSucData[0].Split(' ');
                            mFaultVersion.NumStatements = temp.Length - 1;
                            //  写入数据库
                            FLDBServer.InsertSingleFaultVersion(mFaultVersion);
                        }// end of File Exists
                        else
                        {
                            throw new Exception("Error roots---" + fileSucTraces);
                        }

                    }//end of versionIndex

                }// end of programIndex

            }// end of suiteIndex

        }
        /// <summary>
        /// 解析缺陷版本
        /// </summary>
        private void ParseAllMulFaultVersionsInfo()
        {
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
                        // FaultVersionInfo
                        FLStaMulFaultVersionInfo mFaultVersion = new FLStaMulFaultVersionInfo();
                        mFaultVersion.SuiteName = suiteRoots[suiteIndex].Name;
                        mFaultVersion.ProgramName = programRoots[programIndex].Name;
                        mFaultVersion.VersionName = versionRoots[versionIndex].Name;
                        //
                        string fileSucTraces = versionRoots[versionIndex].FullName + "\\1_success_traces";
                        string fileFalTraces = versionRoots[versionIndex].FullName + "\\1_crash_traces";
                        string fileAllTraces = versionRoots[versionIndex].FullName + "\\1.txt";
                        if (File.Exists(fileSucTraces) && File.Exists(fileFalTraces))
                        {
                            // 装载成例与失例数据
                            string[] mSucData = File.ReadAllLines(fileSucTraces);
                            string[] mFalData = File.ReadAllLines(fileFalTraces);
                            //  获取两类用例数
                            mFaultVersion.NumSucRuns = mSucData.Length;
                            mFaultVersion.NumFalRuns = mFalData.Length;
                            //  获取用例总数
                            string[] mAllData = File.ReadAllLines(fileAllTraces);
                            mFaultVersion.NumRuns = mAllData.Length;
                            //
                            if (mFaultVersion.NumRuns != (mFaultVersion.NumSucRuns + mFaultVersion.NumFalRuns))
                            {
                                //throw new Exception("Error NumRuns---" + mFaultVersion.SuiteName + "-" + mFaultVersion.ProgramName + "-" + mFaultVersion.VersionName);
                            }
                            //  获取语句数
                            string[] temp = mSucData[0].Split(' ');
                            mFaultVersion.NumStatements = temp.Length - 1;
                            //  写入数据库
                            FLDBServer.InsertMultiFaultVersion(mFaultVersion);
                        }// end of File Exists
                        else
                        {
                            throw new Exception("Error roots---" + fileSucTraces);
                        }

                    }//end of versionIndex

                }// end of programIndex

            }// end of suiteIndex
        }

        /// <summary>
        /// 更新缺陷编号
        /// </summary>
        public void UpdataFaultyID()
        {
            //同步读取数据
            TaskFunctionDelegate mTaskFunctionDelegate = UpdateAllFaultyID;
            mTaskFunctionDelegate.BeginInvoke(new AsyncCallback(TaskFinished), mTaskFunctionDelegate);
        }
        /// <summary>
        /// 更新缺陷列表
        /// </summary>
        public void UpdateFaultList()
        {
            //同步读取数据
            TaskFunctionDelegate mTaskFunctionDelegate = UpdateMulFaultyList;
            mTaskFunctionDelegate.BeginInvoke(new AsyncCallback(TaskFinished), mTaskFunctionDelegate);
        }

        /// <summary>
        /// 更新所有缺陷编号
        /// </summary>
        private void UpdateAllFaultyID()
        {
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
                        // FaultVersionInfo
                        FLStaFaultVersionInfo mFaultVersion = new FLStaFaultVersionInfo();
                        mFaultVersion.SuiteName = suiteRoots[suiteIndex].Name;
                        mFaultVersion.ProgramName = programRoots[programIndex].Name;
                        mFaultVersion.VersionName = versionRoots[versionIndex].Name;
                        //
                        string fileStatementLines = versionRoots[versionIndex].FullName + "\\2.txt";
                        if (File.Exists(fileStatementLines))
                        {
                            // 读取缺陷行号
                            List<FLStatementInfo> faultyLines = FLDBServer.ReadFaultyStatementofVersion(mFaultVersion.SuiteName, mFaultVersion.ProgramName, mFaultVersion.VersionName);
                            if (null != faultyLines)
                            {
                                for (int i = 0; i < faultyLines.Count; i++)
                                {
                                    faultyLines[i].ID = GetFaultyStatementID(faultyLines[i].LineNumber, fileStatementLines);
                                }
                                FLDBServer.UpdateFaultyStatementofVersion(mFaultVersion);
                            }
                        }// end of File Exists
                        else
                        {
                            throw new Exception("Error roots---" + fileStatementLines);
                        }

                    }//end of versionIndex

                }// end of programIndex

            }// end of suiteIndex
        }

        /// <summary>
        /// 更新多缺陷的缺陷列表
        /// </summary>
        private void UpdateMulFaultyList()
        {
            // 遍历所有实验包
            DirectoryInfo[] suiteRoots = m_DataRootInfo.GetDirectories();
            for (int suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
            {
                // 遍历所有实验程序
                DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();
                for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
                {
                    // Load map
                    StreamReader faultMap = new StreamReader(programRoots[programIndex].FullName + "\\faultMapFile");
                    string allMulFault = faultMap.ReadToEnd();

                    string[] allMulFaultList = allMulFault.Split('\n');

                    for (int mulIndex = 0; mulIndex < allMulFaultList.Length; mulIndex++)
                    {
                        string[] aMulFault = allMulFaultList[mulIndex].Split(' ');
                        if (3 < aMulFault.Length)
                        {
                            string mulVersionName = aMulFault[0];

                            List<string> faultList = new List<string>();
                            faultList.Add(aMulFault[1]);
                            faultList.Add(aMulFault[3]);

                            FLDBServer.InsertFaultListofMultiFaultVersion(suiteRoots[suiteIndex].Name, programRoots[programIndex].Name, mulVersionName, faultList);
                        }
                    }

                }// end of programIndex

            }// end of suiteIndex
        }

        /// <summary>
        /// 根据参考文件计算语句编号
        /// </summary>
        /// <param name="iLineNumber">语句行号</param>
        /// <param name="refe">参考文件</param>
        /// <returns>语句编号</returns>
        public int GetFaultyStatementID(int iLineNumber, string refe)
        {
            StreamReader refeReader = new StreamReader(refe);
            string refeLine = refeReader.ReadLine();
            string[] strLineNumbers = refeLine.Split(' ');
            // 依次对比各个语句行号
            for (int i = 0; i < strLineNumbers.Length; i++)
            {
                int itempNumber;
                if (int.TryParse(strLineNumbers[i], out itempNumber) && iLineNumber == itempNumber)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}

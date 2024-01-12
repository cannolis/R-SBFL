using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace FaultLocalization
{
    public class FLInfoParser
    {
        /// <summary>
        /// 数据源
        /// </summary>
        private DirectoryInfo m_DataRootInfo = null;

        private Regex mulFaultPattern = new Regex(@"Mul_(\d+)_(\d+)");
        private Regex faultPattern = new Regex(@"/\* #*define (.*\d+) +\*/");
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mDataRootInfo">数据源</param>
        public FLInfoParser(DirectoryInfo mDataRootInfo)
        {
            m_DataRootInfo = mDataRootInfo;
        }

        /// <summary>
        /// 解析更多的缺陷版本
        /// </summary>
        public void MoreFaultVersionsInfo()
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
                        string suiteName = suiteRoots[suiteIndex].Name;
                        string programName = programRoots[programIndex].Name;
                        string versionName = versionRoots[versionIndex].Name;
                        // 确认数据库中没有相应的缺陷版本内容
                        int ID = FLDBServer.GetIDofVersion(suiteName, programName, versionName);
                        if (-1 == ID)
                        {
                            //
                            string fileSucTraces = versionRoots[versionIndex].FullName + "\\1_success_traces";
                            string fileFalTraces = versionRoots[versionIndex].FullName + "\\1_crash_traces";
                            string fileAllTraces = versionRoots[versionIndex].FullName + "\\1.txt";
                            if (File.Exists(fileSucTraces) && File.Exists(fileFalTraces))
                            {
                                int iNumFault = 1;
                                Match m = mulFaultPattern.Match(versionName);
                                if (m.Success && int.TryParse(m.Groups[1].Value, out iNumFault))
                                {
                                    Console.WriteLine(versionName);
                                }
                                
                                FLStaFaultVersionCovInfo theVersion = new FLStaFaultVersionCovInfo(fileSucTraces, fileFalTraces);
                                if (theVersion.keep)
                                {
                                    theVersion.GetNumRunsInTank(fileAllTraces);
                                    FLDBServer.InsertFaultVersionData(suiteName, programName, versionName, iNumFault, theVersion);
                                }

                            }// end of File Exists
                            else
                            {
                                throw new Exception("Error roots---" + fileSucTraces);
                            }
                        }
                    }//end of versionIndex

                }// end of programIndex

            }// end of suiteIndex
        }
        /// <summary>
        /// 解析缺陷描述
        /// </summary>
        public void ParseFaultDescription()
        {
            // 遍历所有实验包
            DirectoryInfo[] suiteRoots = m_DataRootInfo.GetDirectories();
            for (int suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
            {
                // 遍历所有实验程序
                DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();
                for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
                {
                    string suiteName = suiteRoots[suiteIndex].Name;
                    string programName = programRoots[programIndex].Name;
                    // FaultVersionInfo
                    string headFileName = programRoots[programIndex].FullName + "\\FaultSeeds.h";
                    // 从头文件中 写描述
                    string[] singleFaults = File.ReadAllLines(headFileName);
                    for (int faultIndex = 0; faultIndex < singleFaults.Length; faultIndex++)
                    {
                        // 匹配头文件判断是否是缺陷
                        if (faultPattern.IsMatch(singleFaults[faultIndex]))
                        {
                            // 提取缺陷名称
                            Match m = faultPattern.Match(singleFaults[faultIndex]);
                            string faultName = m.Groups[1].Value;
                            // 确定没有记录过
                            int faultID = FLDBServer.GetIDofFault(suiteName, programName, faultName);
                            if (-1 == faultID)
                            {
                                Console.WriteLine("hehe");

                                FLStaFault theFault = new FLStaFault();
                                theFault.FaultName = faultName;
                                theFault.FaultyStatements = new List<FLStatement>();
                                FLDBServer.InsertFaultof(suiteName, programName, theFault);
                            }
                        }
                    }

                }
            }
        }
        /// <summary>
        /// 解析缺陷设置
        /// </summary>
        public void ParseFaultSetting()
        {
            // 遍历所有实验包
            DirectoryInfo[] suiteRoots = m_DataRootInfo.GetDirectories();
            for (int suiteIndex = 0; suiteIndex < suiteRoots.Length; suiteIndex++)
            {
                string suiteName = suiteRoots[suiteIndex].Name;
                // 遍历所有实验程序
                DirectoryInfo[] programRoots = suiteRoots[suiteIndex].GetDirectories();
                for (int programIndex = 0; programIndex < programRoots.Length; programIndex++)
                {
                    string programName = programRoots[programIndex].Name;

                    #region 从Map中 读设置
                    string faultMap = programRoots[programIndex].FullName + "\\faultMapFile";
                    string[] faultSettings = File.ReadAllLines(faultMap);

                    Dictionary<string, List<string>> faultMapDic = new Dictionary<string, List<string>>();
                    for (int i = 0; i < faultSettings.Length; i++)
                    {
                        string[] setting = Regex.Split(faultSettings[i], " +");
                        if (setting.Length > 2)
                        {
                            // 确认是多缺陷版本
                            if (mulFaultPattern.IsMatch(setting[0]))
                            {
                                Match m = mulFaultPattern.Match(setting[0]);
                                // 缺陷数量
                                int iNumFault = 1;
                                int.TryParse(m.Groups[1].Value, out iNumFault);
                                if (iNumFault == (setting.Length - 2))
                                {
                                    string versionName = setting[0];
                                    List<string> faultList = new List<string>();
                                    for (int faultIndex = 0; faultIndex < iNumFault; faultIndex++)
                                    {
                                        faultList.Add(setting[faultIndex + 1]);
                                    }
                                    if (!faultMapDic.Keys.Contains(versionName))
                                    {
                                        faultMapDic.Add(versionName, faultList);
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    // 遍历所有缺陷版本
                    DirectoryInfo[] versionRoots = programRoots[programIndex].GetDirectories();
                    for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
                    {
                        string versionName = versionRoots[versionIndex].Name;

                        if (mulFaultPattern.IsMatch(versionName))
                        {
                            List<string> faultList = faultMapDic[versionName];
                            FLDBServer.InsertFaultVersionSetting(suiteName, programName, versionName, faultList);
                        }
                        else
                        {
                            List<string> faultList = new List<string>();
                            faultList.Add(versionName);
                            FLDBServer.InsertFaultVersionSetting(suiteName, programName, versionName, faultList);
                        }

                    }//end of versionIndex
                }
            }
        }




        public void ParseFaultVersionsInfo(string suiteName, string programName)
        {
            // 遍历所有缺陷版本
            DirectoryInfo programRoot = new DirectoryInfo(m_DataRootInfo.FullName + "\\" + suiteName + "\\" + programName);
            DirectoryInfo[] versionRoots = programRoot.GetDirectories();
            for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
            {
                // FaultVersionInfo
                string versionName = versionRoots[versionIndex].Name;
                //
                string fileSucTraces = versionRoots[versionIndex].FullName + "\\1_success_traces";
                string fileFalTraces = versionRoots[versionIndex].FullName + "\\1_crash_traces";
                string fileAllTraces = versionRoots[versionIndex].FullName + "\\1.txt";
                if (File.Exists(fileSucTraces) && File.Exists(fileFalTraces))
                {
                    bool isMultiFaulty = mulFaultPattern.IsMatch(versionName);
                    int iNumFault = 1;
                    if (isMultiFaulty)
                    {
                        Match m = mulFaultPattern.Match(versionName);
                        int.TryParse(m.Groups[1].Value, out iNumFault);
                        Console.WriteLine("hehe");
                    }


                    FLStaFaultVersionCovInfo theVersion = new FLStaFaultVersionCovInfo(fileSucTraces, fileFalTraces);
                    theVersion.GetNumRunsInTank(fileAllTraces);
                    FLDBServer.InsertFaultVersionData(suiteName, programName, versionName, iNumFault, theVersion);


                }// end of File Exists
                else
                {
                    throw new Exception("Error roots---" + fileSucTraces);
                }

            }//end of versionIndex
        }

        public void ParseFaultDescription(string suiteName, string programName)
        {
            DirectoryInfo programRoot = new DirectoryInfo(m_DataRootInfo.FullName + "\\" + suiteName + "\\" + programName);

            #region 从头文件中 写描述
            // FaultVersionInfo
            string headFileName = programRoot.FullName + "\\FaultSeeds.h";
            // 从头文件中 写描述
            string[] singleFaults = File.ReadAllLines(headFileName);
            for (int i = 0; i < singleFaults.Length; i++)
            {
                if (faultPattern.IsMatch(singleFaults[i]))
                {
                    Match m = faultPattern.Match(singleFaults[i]);
                    string faultName = m.Groups[1].Value;
                    Console.WriteLine(faultName);

                    FLStaFault theFault = new FLStaFault();
                    theFault.FaultName = faultName;
                    theFault.FaultyStatements = new List<FLStatement>();
                    FLDBServer.InsertFaultof(suiteName, programName, theFault);
                }
            }
            #endregion
        }

        public void ParseFaultMap(string suiteName, string programName)
        {
            DirectoryInfo programRoot = new DirectoryInfo(m_DataRootInfo.FullName + "\\" + suiteName + "\\" + programName);

            #region  从Map中 读设置
            // 从Map中 读设置
            string faultMap = programRoot.FullName + "\\faultMapFile";
            string[] faultSettings = File.ReadAllLines(faultMap);
            Dictionary<string, List<string>> faultMapDic = new Dictionary<string, List<string>>();
            for (int i = 0; i < faultSettings.Length; i++)
            {
                string[] setting = Regex.Split(faultSettings[i], " +");
                if (setting.Length > 2)
                {

                    bool isMultiFaulty = mulFaultPattern.IsMatch(setting[0]);
                    int iNumFault = 1;
                    if (isMultiFaulty)
                    {
                        Match m = mulFaultPattern.Match(setting[0]);
                        int.TryParse(m.Groups[1].Value, out iNumFault);
                        if (iNumFault == (setting.Length - 2))
                        {
                            string versionName = setting[0];
                            List<string> faultList = new List<string>();
                            for (int faultIndex = 0; faultIndex < iNumFault; faultIndex++)
                            {
                                faultList.Add(setting[faultIndex + 1]);
                            }

                            faultMapDic.Add(versionName, faultList);
                        }
                    }
                }
            }
            #endregion


            // 遍历所有缺陷版本
            DirectoryInfo[] versionRoots = programRoot.GetDirectories();
            for (int versionIndex = 0; versionIndex < versionRoots.Length; versionIndex++)
            {
                string versionName = versionRoots[versionIndex].Name;

                bool isMultiFaulty = mulFaultPattern.IsMatch(versionName);
                if (isMultiFaulty)
                {
                    List<string> faultList = faultMapDic[versionName];
                    FLDBServer.InsertFaultVersionSetting(suiteName, programName, versionName, faultList);
                }
                else
                {
                    List<string> faultList = new List<string>();
                    faultList.Add(versionName);
                    FLDBServer.InsertFaultVersionSetting(suiteName, programName, versionName, faultList);
                }

            }//end of versionIndex
        }

    }
}
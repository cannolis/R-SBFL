using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
//
using FaultLocalization;
using FaultLocalization.TestCases;  
using DBDll;
using ConfigDll;
using System.Net;
using PlanningAlgorithmInterface.Socket4DataTrans;

namespace FrameWorkStatement
{
    class Program
    {
        static void Main(string[] args)
        {
            // 设置数据库字符串并备份数据库
            string connString = SQLServerConnectionItem.ConstructUsableConnectionStringFromUDL(Application.StartupPath + "\\DataBase\\Connection.udl");
            AppConfigOperation.UpdateConfigurationItemValue("ConnectionString_SQLServer", connString);
            FLDBServer.SetDBServer(AppConfigOperation.GetConfigurationValue("ConnectionString_SQLServer"));
            FLDataBaseTestCase theDataBaseCase = new FLDataBaseTestCase();
            // 实验对象、数据源、存放实验结果的路径
            string[] suitelist = new string[] { "Sed", "defects4j", "Siemens",  "Gzip", "Grep" ,"Flex", "Space" };
            //string[] suitelist = new string[] { "defects4j", "Flex", "Gzip", "Sed" , "Grep"/*,"Siemens", "Space" */};
            DirectoryInfo srcInfo = new DirectoryInfo(@"G:\Data");      
            string dataDirectoryName = @"G:\Results";


            //FLExcelInfo.ExportInfo(dataDirectoryName);

            // 实验算法 
            string[] methodlist = new string[] { "Op1",     "Op2",      "Jaccard" ,        /* "Anderberg",        "Sqrensen_Dice",
                                                 /*"Dice",    "Goodman", */ "Tarantula",       /* "qe",               "CBI_Inc", */
                                                 "Wong2",   /*"Hamann",   "Simple_Matching",  "Sokal",            "Rogers_and_Tanimoto",
                                                 "Hamming", "Euclid", */  "Wong1",        /*    "Russel_And_Rao",*/   "Binary",
                                                 "Scott",/*   "Rogot1",*/   "Kulczynski2",      "Ochiai",           "M2",
                                                 "Ample2",  "Wong3",    "Arithmetic_Mean",  "Cohen",            "Fleiss",
                                                 "Zoltar",  "Ochiai2",  "Harmonic_Mean",    "CrossTab", "Dstar2", "Heuristic_b", "Heuristic_c"
                                                 };
            //string[] methodlist = new string[] { "Heuristic_a" };
            /*配置*/
            FLConfigure configure = new FLConfigure();
            configure.Experiment = "随机变更用例标签";//实验名称
            configure.DataRootInfo = srcInfo;// 数据源

            configure.RepeatTimes = 20;// 重复实验次数
            configure.MinClassRatio = -1;// 最小类别比例(成例数量:失例数量)  小于等于零时不检查

            configure.ChangeSucRatio = -1;// 成例占所有变更用例的比例,若为负数则不区分成例失例

            configure.ClassChangeRatio = 0.1;// Author改 变更标签的用例比例
            //configure.MinRuns = 1;/// 最少的测试用例数量
            configure.MinRuns = Convert.ToInt32(Math.Ceiling(1.0 /configure.ClassChangeRatio));// Author改 最少的测试用例数量  Author改 避免因变更比例太小而出现无变更
            //configure.MaxClassChangeRatioInOriginal = configure.ClassChangeRatio;  // Author改 变更为某类的用例数量最大占原用例数的比例
            configure.MaxClassChangeRatioInOriginal = 0.2;
            configure.MaxClassRatio = Double.MaxValue;// 最大类别比例(成例数量:失例数量)
            //configure.MaxClassRatio = configure.MaxClassChangeRatioInOriginal / configure.ClassChangeRatio;//Author改 最大类别比例(成例数量:失例数量)  
            configure.ClassChangeSelectStrategy = "变更用例" + configure.ClassChangeRatio.ToString("0.0000"); // 变更类别用例的选取策略描述
            configure.WeightFormulaId = 21;// Author改 集成时权值公式索引
            configure.ClassRatioDivideStrategy = "EnsembleClassifier";//Author改 按类别比例拆分用例策略
            configure.IntegrateKernel = "NumSExpSort";// 集成加权核
            //configure.serverIP = IPAddress.Loopback;
            //configure.serverIP = IPAddress.Parse("xx.xxx.xx.xx");    //服务器ip地址
            configure.serverIP = IPAddress.Parse("127.0.0.1");    //本地
            configure.port = 12223;

            Console.Write("Press Any Button and Enter to start.\r\nIt may take more than one days.\r\nPlease be patient.>>>\r\n");
            Console.ReadLine();
            Console.Write("Let's go. Good luck.\r\n");

            RunAll haha = new RunAll(srcInfo);
            haha.MethodList = methodlist;

            #region //Author添加 打开客户端，准备开始与服务端通信
            SocketClient client = new SocketClient(configure.serverIP, configure.port);
            if (client.Connect() != true)
            {
                throw new Exception("无法连接到服务端");
            }
            configure.client = client;
            # endregion

            string tempStrategy = configure.ClassChangeSelectStrategy;  //Author添加 临时存储一下

            for (int suiteIndex = 0; suiteIndex < suitelist.Length; suiteIndex++)
            {
                int iNumFaults = -1;
                configure.SuiteName = suitelist[suiteIndex];

                configure.ClassChangeSelectStrategy = "不变更用例";   //Author添加 必须要改变cfg，因为后续向数据库写入时需要用到
                string desc0 = "不变更用例" + "_集成";  //Author添加 必须要改变cfg，因为后续向数据库写入时需要用到


                #region /*不集成*/
                Console.Write("开始" + configure.SuiteName + "不集成" + "\r\n");
                haha.NoSpecialOperationExperimentOf(configure.SuiteName, configure);
                Console.Write("完成" + configure.SuiteName + "不集成" + "\r\n");
                #endregion

                #region/*不变更测试用例的集成实验*/
                // Author改
                Console.Write("开始:" + configure.SuiteName + ":" + desc0 + "\r\n");
                Console.Write("Repeating Times:" + configure.RepeatTimes + "\r\n");

                haha.NoChangeClassEnsembleSortExperimentof(configure.SuiteName, configure);

                Console.Write("完成:" + configure.SuiteName + ":" + desc0 + "\r\n");

                #region 输出excel 对比集成方法对原始定位方法的影响
                /*输出1缺陷Excel*/
                iNumFaults = 1;
                Console.WriteLine("The number of faults:" + iNumFaults);
                FLExcelAB.ABAveAsExcel(configure, configure.SuiteName, iNumFaults, haha.MethodList.ToList(), desc0, dataDirectoryName);
                Console.WriteLine("完成" + configure.SuiteName + "单缺陷结果输出");

                /*输出2缺陷Excel*/
                iNumFaults = 2;
                Console.WriteLine("The number of faults:" + iNumFaults);
                FLExcelAB.ABAveAsExcel(configure, configure.SuiteName, iNumFaults, haha.MethodList.ToList(), desc0, dataDirectoryName);
                Console.WriteLine("完成" + configure.SuiteName + "2缺陷结果输出");

                /*输出3缺陷Excel*/
                iNumFaults = 3;
                Console.WriteLine("The number of faults:" + iNumFaults);
                FLExcelAB.ABAveAsExcel(configure, configure.SuiteName, iNumFaults, haha.MethodList.ToList(), desc0, dataDirectoryName);
                Console.WriteLine("完成" + configure.SuiteName + "3缺陷结果输出");

                #endregion

                #endregion

                configure.ClassChangeSelectStrategy = tempStrategy; //Author添加 将变更比例更改回来
                string desc1 = configure.ClassChangeSelectStrategy + "_不集成";
                string desc2 = configure.ClassChangeSelectStrategy + "_集成";

                #region/*变更测试用例的不集成实验*/
                //string desc1 = configure.ClassChangeSelectStrategy + "_不集成";
                Console.WriteLine("开始:" + configure.SuiteName + ":" + desc1);
                Console.Write("Repeating Times:" + configure.RepeatTimes + "\r\n");
                haha.ChangeClassExperimentof(configure.SuiteName, configure);
                Console.WriteLine("完成:" + configure.SuiteName + ":" + desc1);

                //#region 输出excel 对比分析扰动影响（鲁棒性）
                ///*输出1缺陷Excel*/
                //iNumFaults = 1;
                //Console.WriteLine("The number of faults:" + iNumFaults);
                //FLExcelAB.ABAveAsExcel(configure, configure.SuiteName, iNumFaults, haha.MethodList.ToList(), desc1, dataDirectoryName);
                //Console.WriteLine("完成" + configure.SuiteName + "单缺陷结果输出");

                ///*输出2缺陷Excel*/
                //iNumFaults = 2;
                //Console.WriteLine("The number of faults:" + iNumFaults);
                //FLExcelAB.ABAveAsExcel(configure, configure.SuiteName, iNumFaults, haha.MethodList.ToList(), desc1, dataDirectoryName);
                //Console.WriteLine("完成" + configure.SuiteName + "2缺陷结果输出");

                ///*输出3缺陷Excel*/
                //iNumFaults = 3;//Second Author改
                //Console.WriteLine("The number of " + "faults" + ":" + iNumFaults);
                //FLExcelAB.ABAveAsExcel(configure, configure.SuiteName, iNumFaults, haha.MethodList.ToList(), desc1, dataDirectoryName);
                //Console.WriteLine("完成" + configure.SuiteName + "3缺陷结果输出");
                //#endregion

                #endregion

                #region/*变更测试用例的集成实验 */
                // Author改
                Console.Write("开始:" + configure.SuiteName + ":" + desc2 + "\r\n");
                Console.Write("Repeating Times:" + configure.RepeatTimes + "\r\n");

                haha.ChangeClassEnsembleSortExperimentof(configure.SuiteName, configure);

                Console.Write("完成:" + configure.SuiteName + ":" + desc2 + "\r\n");

                #region 输出excel 对比分析集成方法提高（扰动下的）定位准确度
                /*输出1缺陷Excel*/
                iNumFaults = 1;
                Console.WriteLine("The number of faults:" + iNumFaults);
                FLExcelAB.ABAveAsExcel(configure, configure.SuiteName, iNumFaults, haha.MethodList.ToList(), desc1, desc2, dataDirectoryName);
                Console.WriteLine("完成" + configure.SuiteName + "单缺陷结果输出");

                /*输出2缺陷Excel*/
                iNumFaults = 2;
                Console.WriteLine("The number of faults:" + iNumFaults);
                FLExcelAB.ABAveAsExcel(configure, configure.SuiteName, iNumFaults, haha.MethodList.ToList(), desc1, desc2, dataDirectoryName);
                Console.WriteLine("完成" + configure.SuiteName + "2缺陷结果输出");

                /*输出3缺陷Excel*/
                iNumFaults = 3;
                Console.WriteLine("The number of faults:" + iNumFaults);
                FLExcelAB.ABAveAsExcel(configure, configure.SuiteName, iNumFaults, haha.MethodList.ToList(), desc1, desc2, dataDirectoryName);
                Console.WriteLine("完成" + configure.SuiteName + "3缺陷结果输出");
                #endregion

                #region 输出excel 集成方法对鲁棒性的提高
                /*输出1缺陷Excel*/
                iNumFaults = 1;
                Console.WriteLine("开始输出" + configure.SuiteName + "的鲁棒性统计表格");
                Console.WriteLine("The number of faults:" + iNumFaults);
                FLExcelAB.WriteABCDintoExcel(configure, configure.SuiteName, iNumFaults, haha.MethodList.ToList(), desc0, desc1, desc2, dataDirectoryName);
                Console.WriteLine("完成" + configure.SuiteName + "单缺陷鲁棒性结果输出");

                /*输出2缺陷Excel*/
                iNumFaults = 2;
                Console.WriteLine("The number of faults:" + iNumFaults);
                FLExcelAB.WriteABCDintoExcel(configure, configure.SuiteName, iNumFaults, haha.MethodList.ToList(), desc0, desc1, desc2, dataDirectoryName);
                Console.WriteLine("完成" + configure.SuiteName + "2缺陷鲁棒性结果输出");

                /*输出3缺陷Excel*/
                iNumFaults = 3;
                Console.WriteLine("The number of faults:" + iNumFaults);
                FLExcelAB.WriteABCDintoExcel(configure, configure.SuiteName, iNumFaults, haha.MethodList.ToList(), desc0, desc1, desc2, dataDirectoryName);
                Console.WriteLine("完成" + configure.SuiteName + "3缺陷鲁棒性结果输出");
                #endregion

                #endregion

            }


            //Author添加 关闭客户端
            client.Close();

        }

    }
}

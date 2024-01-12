using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaultLocalization.TestCases
{
    public class FLDataBaseTestCase
    {
        public FLDataBaseTestCase()
        {
            if (!FLDBServer.OpenConnection())
            {
                throw new Exception("数据库没有打开");
            }
        }

        public void WriteAVersion()
        {
            string sucMatricesFile = @"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验中间数据\Statement_8_27\Flex\v1\F_AA_2\1_success_traces";
            string falMatricesFile = @"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验中间数据\Statement_8_27\Flex\v1\F_AA_2\1_crash_traces";

            FLStaFaultVersionCovInfo theVersion = new FLStaFaultVersionCovInfo(sucMatricesFile, falMatricesFile);
            theVersion.GetNumRunsInTank(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验中间数据\Statement_8_27\Flex\v1\F_AA_2\1.txt");

            FLDBServer.InsertFaultVersionData("Flex", "v1", "F_AA_2", 1,theVersion);
            FLDBServer.DeleFaultVersionData("Flex", "v1", "F_AA_2");

            //FLDBServer.UpdateFaultVersion("Flex", "v1", "F_AA_2", false,theVersion);
        }

        public void WriteAFault()
        {
            FLStaFault theFault = new FLStaFault();
            theFault.FaultName = "F_AA_2";
            theFault.FaultyStatements = new List<FLStatement>();
            
            // 增加
            FLStatement faultyStatement = new FLStatement();
            faultyStatement.ID = 10;
            faultyStatement.LineNumber = 44;
            theFault.FaultyStatements.Add(faultyStatement);
            FLDBServer.InsertFaultof("Flex", "v1", theFault);
            // 修改
            int faultID = FLDBServer.GetIDofFault("Flex","v1",theFault.FaultName);
            FLStatement faultyStatement1 = new FLStatement();
            faultyStatement1.ID = 11;
            faultyStatement1.LineNumber = 49;
            theFault.FaultyStatements.Add(faultyStatement1);
            theFault.FaultName = "hahahah";
            FLDBServer.UpdateFault(faultID, "Flex", "v1", theFault);
            // 读
            FLStaFault newFault = FLDBServer.ReadStaFault(faultID);
            // 删除
            FLDBServer.DeleFault("Flex", "v1", newFault);
        }

        public void WriteAFaultVersion()
        {
            // 写一个缺陷版本
            string sucMatricesFile = @"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验中间数据\Statement_8_27\Flex\v1\F_AA_2\1_success_traces";
            string falMatricesFile = @"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验中间数据\Statement_8_27\Flex\v1\F_AA_2\1_crash_traces";

            FLStaFaultVersionCovInfo theVersion = new FLStaFaultVersionCovInfo(sucMatricesFile, falMatricesFile);
            theVersion.GetNumRunsInTank(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验中间数据\Statement_8_27\Flex\v1\F_AA_2\1.txt");

            FLDBServer.InsertFaultVersionData("Flex", "v1", "F_AA_2", 1, theVersion);

            // 写一个缺陷
            FLStaFault theFault = new FLStaFault();
            theFault.FaultName = "F_AA_2";
            theFault.FaultyStatements = new List<FLStatement>();

            // 增加
            FLStatement faultyStatement = new FLStatement();
            faultyStatement.ID = 990;
            faultyStatement.LineNumber = 2681;
            theFault.FaultyStatements.Add(faultyStatement);
            FLDBServer.InsertFaultof("Flex", "v1", theFault);

            // 写一个缺陷版本的缺陷设置
            List<string> faultList = new List<string>();
            faultList.Add("F_AA_2");

            FLDBServer.InsertFaultVersionSetting("Flex", "v1", "F_AA_2", faultList);
            // 读一个缺陷版本的缺陷设置
            FLStaFaultVersionSetInfo hehe = FLDBServer.ReadFaultVersionSettings("Flex", "v1", "F_AA_2");

           // FLDBServer.DeleFaultVersionData("Flex", "v1", "F_AA_2");
           // FLDBServer.DeleFault("Flex", "v1", theFault);
        }


        public void OutputExcel()
        {
            string[] methodlist = new string[] { "Op1", "Jaccard", "Tarantula", "Wong2", "Wong1", "Scott", "Ochiai", "Kulczynski2",
                                                     "M2", "Ample2", "Wong3", "Arithmetic_Mean", "Cohen", "Fleiss","Zoltar", "CrossTab" };

            FLDBServer.InitialExcel();
            FLDBServer.OutputExcel("Flex", 2, methodlist.ToList(), "不集成");
            FLDBServer.CloseExcel(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验结果\多缺陷实验结果\temp\Flex.2.不集成.xlsx");

            FLDBServer.InitialExcel();
            FLDBServer.OutputExcel("Flex", 2, methodlist.ToList(), "可疑度集成_权值公式0");
            FLDBServer.CloseExcel(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验结果\多缺陷实验结果\temp\Flex.2.可疑度集成.权值公式0.xlsx");

            FLDBServer.InitialExcel();
            FLDBServer.OutputExcel("Flex", 2, methodlist.ToList(), "排位集成_权值公式0");
            FLDBServer.CloseExcel(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验结果\多缺陷实验结果\temp\Flex.2.排位集成.权值公式0.xlsx");
            //----------------------------------------------------------------------------------------
            FLDBServer.InitialExcel();
            FLDBServer.OutputExcel("Grep", 2, methodlist.ToList(), "不集成");
            FLDBServer.CloseExcel(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验结果\多缺陷实验结果\temp\Grep.2.不集成.xlsx");

            FLDBServer.InitialExcel();
            FLDBServer.OutputExcel("Grep", 2, methodlist.ToList(), "可疑度集成_权值公式0");
            FLDBServer.CloseExcel(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验结果\多缺陷实验结果\temp\Grep.2.可疑度集成.权值公式0.xlsx");

            FLDBServer.InitialExcel();
            FLDBServer.OutputExcel("Grep", 2, methodlist.ToList(), "排位集成_权值公式0");
            FLDBServer.CloseExcel(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验结果\多缺陷实验结果\temp\Grep.2.排位集成.权值公式0.xlsx");
            //----------------------------------------------------------------------------------------
            FLDBServer.InitialExcel();
            FLDBServer.OutputExcel("Siemens", 2, methodlist.ToList(), "不集成");
            FLDBServer.CloseExcel(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验结果\多缺陷实验结果\temp\Siemens.2.不集成.xlsx");

            FLDBServer.InitialExcel();
            FLDBServer.OutputExcel("Siemens", 2, methodlist.ToList(), "可疑度集成_权值公式0");
            FLDBServer.CloseExcel(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验结果\多缺陷实验结果\temp\Siemens.2.可疑度集成.权值公式0.xlsx");

            FLDBServer.InitialExcel();
            FLDBServer.OutputExcel("Siemens", 2, methodlist.ToList(), "排位集成_权值公式0");
            FLDBServer.CloseExcel(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验结果\多缺陷实验结果\temp\Siemens.2.排位集成.权值公式0.xlsx");
            //----------------------------------------------------------------------------------------
            FLDBServer.InitialExcel();
            FLDBServer.OutputExcel("Space", 2, methodlist.ToList(), "不集成");
            FLDBServer.CloseExcel(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验结果\多缺陷实验结果\temp\Space.2.不集成.xlsx");

            FLDBServer.InitialExcel();
            FLDBServer.OutputExcel("Space", 2, methodlist.ToList(), "可疑度集成_权值公式0");
            FLDBServer.CloseExcel(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验结果\多缺陷实验结果\temp\Space.2.可疑度集成.权值公式0.xlsx");

            FLDBServer.InitialExcel();
            FLDBServer.OutputExcel("Space", 2, methodlist.ToList(), "排位集成_权值公式0");
            FLDBServer.CloseExcel(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验结果\多缺陷实验结果\temp\Space.2.排位集成.权值公式0.xlsx");
        }

        public void TestSuiteNames()
        {
            List<string> suitesInDB = FLDBServer.GetSuitesNameInDB();
            List<string> programsofSuitesInDB = FLDBServer.GetProgramNameofSuite(suitesInDB[0]);
            List<string> versionsofProgramInDB = FLDBServer.GetVersionNameofProgram(suitesInDB[0],programsofSuitesInDB[0]);
        }

    }
}

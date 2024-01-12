using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaultLocalization.TestCases
{
    public class FLStaFaultVersionInfoTestCase
    {
        public FLStaFaultVersionInfoTestCase()
        {

        }

        public void TestOneProgram()
        {
            //string sucMatricesFile = @"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验中间数据\Statement_8_27\Flex\v1\F_AA_2\1_success_traces";
            //string falMatricesFile = @"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验中间数据\Statement_8_27\Flex\v1\F_AA_2\1_crash_traces";
            //string tankFile = @"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验中间数据\Statement_8_27\Flex\v1\F_AA_2\1.txt";

            //FLStaFaultVersionCovInfo theInfo = new FLStaFaultVersionCovInfo(sucMatricesFile, falMatricesFile);

            //int numStatement = theInfo.NumStatements;
            //int numSucRuns = theInfo.NumSucRuns;
            //int numFalRuns = theInfo.NumFalRuns;


            //if (theInfo.GetNumRunsInTank(tankFile) != (numSucRuns + numFalRuns))
            //{
            //    Console.WriteLine("测试用例总数 和 成例数 失例数 不符");
            //}
            //else if(theInfo.m_bSucCoverageMetrix.GetLength(0) != numStatement || theInfo.m_bFalCoverageMetrix.GetLength(0) != numStatement)
            //{
            //    throw new Exception("语句数和矩阵不符");
            //}
            //else if (theInfo.m_bSucCoverageMetrix.GetLength(1) != numSucRuns || theInfo.m_bFalCoverageMetrix.GetLength(1) != numFalRuns)
            //{
            //    throw new Exception("用例数和矩阵不符");
            //}

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaultLocalization.TestCases
{
    public class FLDebuggerTestCase
    {
        public FLDebuggerTestCase()
        {

        }

        public void TestOneAlgorithmForOneProgram()
        {
            //string sucMatricesFile = @"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验中间数据\Statement_8_27\Flex\v1\F_AA_2\1_success_traces";
            //string falMatricesFile = @"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验中间数据\Statement_8_27\Flex\v1\F_AA_2\1_crash_traces";

            //FLDebugger theDebugger = new FLDebugger(sucMatricesFile, falMatricesFile);


            //string method = "Tarantula";

            //FLStatementInfo[] rankList = theDebugger.NormalLocateFaults(method);

            //if (rankList.Length != theDebugger.FaultVersionInfo.NumStatements)
            //{
            //    throw new Exception("排位表 计算错误");
            //}

            //rankList = theDebugger.EnsembleLocateFaultsSus(method,0);
            //if (rankList.Length != theDebugger.FaultVersionInfo.NumStatements)
            //{
            //    throw new Exception("排位表 计算错误");
            //}

            //List<FLRunsGroupInfo> theGroups = theDebugger.DivideSucGroupsNB(1);
            //theDebugger.CalSusOfGroups(theGroups, method);
            //FLStatementInfo[] theList = theDebugger.IntegrateSus(theGroups, 0);
            //rankList = theDebugger.Sort(ref theList);

            //if (rankList.Length != theDebugger.FaultVersionInfo.NumStatements)
            //{
            //    throw new Exception("排位表 计算错误");
            //}
        }

        public void TestHeapSort()
        {
            // 模拟的输入列表
            FLStatementInfo[] src = new FLStatementInfo[10];
            // 模拟的输出列表
            FLStatementInfo[] dst = new FLStatementInfo[10];

            // 初始化输入列表 输出列表
            for (int i = 0; i < src.Length; i++)
            {
                src[i] = new FLStatementInfo();
                src[i].ID = i;
                src[i].sort = -1;
                src[i].suspiciousness1 = i;

                dst[i] = src[i];
            }

            FLDebugger.HeapSort(ref dst, 0, src.Length - 1);
            Console.Write("\nSRC:");
            for (int i = 0; i < src.Length; i++)
            {
                Console.Write(src[i].ID);
                Console.Write(" ");
            }
            Console.Write("\nDST:");
            for (int i = 0; i < dst.Length; i++)
            {
                dst[i].sort = i + 1;
                Console.Write(dst[i].ID);
                Console.Write(" ");
            }
            Console.Write("\nThe Sort of SRC is:\n");
            for (int i = 0; i < src.Length; i++)
            {
                Console.Write(src[i].ID.ToString() + ":" + src[i].sort.ToString());
                Console.Write(" ");
            }
            
        }
    }
}

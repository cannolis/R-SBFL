using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FaultLocalization.TestCases
{
    public class FLInfoParserTestCase
    {
        public FLInfoParserTestCase()
        {

        }

        public void ParseAllVersion()
        {
            FLInfoParser theParser = new FLInfoParser(new DirectoryInfo(@"D:\软件缺陷定位\软件缺陷定位实验平台1.0\实验中间数据\Statement_8_27"));
            
            //theParser.ParseAllFaultVersionsInfo();
            //theParser.ParseFaultDescription();

            //theParser.ParseFaultVersionsInfo("Siemens", "print_tokens");
            //theParser.ParseFaultVersionsInfo("Siemens", "schedule2");
            //theParser.ParseFaultVersionsInfo("Siemens", "tot_info");


            //theParser.ParseFaultDescription("Siemens", "schedule2");
            //theParser.ParseFaultDescription("Siemens", "tot_info");

            //theParser.ParseFaultMap("Siemens", "print_tokens");
            //theParser.ParseFaultMap("Siemens", "schedule2");
            //theParser.ParseFaultMap("Siemens", "tot_info");


            theParser.MoreFaultVersionsInfo();
            theParser.ParseFaultDescription();
            theParser.ParseFaultSetting();
        }
    }
}

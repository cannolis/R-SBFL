using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningAlgorithmInterface.Socket4DataTrans
{
    public class SocketConst
    {
        public const string StrEOF = "<EOF>";

        public const int ByteBufferLen = 4096;

        public const string StrConnTest = "<ConnectionTestMessage>";

        public const int DataTransferMillionSecondsTimeout = 100; 
    }
}

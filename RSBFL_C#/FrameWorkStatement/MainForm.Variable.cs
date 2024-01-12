using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
//
using DBDll;
//
using FaultLocalization;

namespace FrameWorkStatement
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// 数据源
        /// </summary>
        private DirectoryInfo m_DataRootInfo = null;

        /// <summary>
        /// 实验算法
        /// </summary>
        private string[] methodlist = new string[] { "Op1", "Jaccard", "Tarantula", "Wong2", "Wong1", "Scott", "Ochiai", "Kulczynski2",
                                                     "M2", "Ample2", "Wong3", "Arithmetic_Mean", "Cohen", "Fleiss","Zoltar", "CrossTab" };
        /// <summary>
        /// 实验算法数
        /// </summary>
        public int NumMethods
        {
            get { return methodlist.Length; }
        }



    }
}

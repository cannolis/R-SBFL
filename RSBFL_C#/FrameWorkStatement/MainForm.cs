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
using FaultLocalization;

namespace FrameWorkStatement
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }


        #region 单缺陷桩点
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            SuiteComboBox.Items.Clear();
            ProgramComboBox.Items.Clear();
            FaultComboBox.Items.Clear();
            SuiteComboBox.Text = "";
            ProgramComboBox.Text = "";
            FaultComboBox.Text = "";
            LineNumberTextBox.Text = "";
            // 实验包
            List<string> suiteNames = FLDBServer.GetSuitesNameInDB();
            SuiteComboBox.Items.AddRange(suiteNames.ToArray());
        }

        private void SuiteComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProgramComboBox.Items.Clear();
            FaultComboBox.Items.Clear();
            ProgramComboBox.Text = "";
            FaultComboBox.Text = "";
            LineNumberTextBox.Text = "";
            List<string> programNames = FLDBServer.GetProgramNameofSuite(SuiteComboBox.Text);
            ProgramComboBox.Items.AddRange(programNames.ToArray());
        }

        private void ProgramComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                FaultComboBox.Items.Clear();
                FaultComboBox.Text = "";
                LineNumberTextBox.Text = "";
                List<string> versionNames = FLDBServer.GetFaultNamesofProgram(SuiteComboBox.Text, ProgramComboBox.Text);
                FaultComboBox.Items.AddRange(versionNames.ToArray());
            }
            catch (System.Exception ex)
            {

            }
        }

        private void FaultComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LineNumberTextBox.Text = "";
            int id = FLDBServer.GetIDofFault(SuiteComboBox.Text, ProgramComboBox.Text, FaultComboBox.Text);
                       
            List<FLStatement> statements = FLDBServer.GetFaultyStatements(id);
            if (null != statements)
            {
                for (int i = 0; i < statements.Count; i++)
                {
                    LineNumberTextBox.Text += statements[i].LineNumber.ToString() + "\r\n";
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            FLStaFault theFault = new FLStaFault();
            theFault.FaultName = FaultComboBox.Text;
            theFault.FaultyStatements = new List<FLStatement>();

            // 解析文本
            string[] LineNumbers = LineNumberTextBox.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            //
            for (int i = 0; i < LineNumbers.Length; i++)
            {
                int lineNumber;
                if (int.TryParse(LineNumbers[i], out lineNumber))
                {
                    FLStatementInfo temp = new FLStatementInfo();
                    // 调用LineNumber to ID的函数 Remark [7/10/2013 gyc]
                    ////FLFaultVersionInfoParser passer = new FLFaultVersionInfoParser(m_DBServer, m_DataRootInfo);
                    ////temp.ID = passer.GetFaultyStatementID(lineNumber, DataRootTextBox.Text + "\\" + faultVersion.FullName + "\\2.txt");
                    
                    temp.LineNumber = lineNumber;
                    temp.ID = -1;

                    theFault.FaultyStatements.Add(temp);
                }
            }
            // 写入数据库
            FLDBServer.InsertFaultof(SuiteComboBox.Text, ProgramComboBox.Text, theFault);
        }
        #endregion

        // 指定数据源
        private void DataRootTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string strDataRoot = DataRootTextBox.Text;
                m_DataRootInfo = new DirectoryInfo(strDataRoot);
            }
            catch (System.Exception ex)
            {

            }
        }











    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Remoting.Messaging;
//
using DBDll;
using ConfigDll;
//
using FaultLocalization;


namespace FrameWorkStatement
{
    public partial class MainForm : Form
    {
        public void InitProperties()
        {
            //设置数据库字符串并备份数据库
            string connString = SQLServerConnectionItem.ConstructUsableConnectionStringFromUDL(Application.StartupPath + "\\DataBase\\Connection.udl");
            AppConfigOperation.UpdateConfigurationItemValue("ConnectionString_SQLServer", connString);

            //测试数据库是否可以连通
            FLDBServer.SetDBServer(AppConfigOperation.GetConfigurationValue("ConnectionString_SQLServer"));
            if (!FLDBServer.OpenConnection())
            {
                return;
            }
        }




    }
}
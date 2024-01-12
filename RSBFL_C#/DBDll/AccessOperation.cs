/*****************************************************************
 * Copyright(C), 2010, BUAA, 软件与控制研究室
 * 文件名称:    ODBCOperation.cs
 * 作者:        Liuwei
 * 版本:        1.0        
 * 创建日期:    2010.07.22
 * 完成日期:    2010
 * 文件描述:    数据库操作类。利用OleDb操作Access数据库。
 *              
 * 引用:        引用系统ODBC空间: System.Data.OleDb
 * 
 * 修改历史:
 * 1.   修改日期:
 *      修改人:
 *      修改功能:
 * 2.   
*****************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace DBDll
{
    /// <summary>
    /// Access数据库操作类
    /// </summary>
    public class AccessOperation
    {        
        /// <summary>
        /// 数据库文件完整地址
        /// </summary>
        private string sDatabaseFullname = "Data.mdb";

        /// <summary>
        /// 连接字符串
        /// </summary>
        //private string sConnectionString = @"Provider=Microsoft.Jet.OleDb.4.0;Data Source=Data.mdb";//Access 2000-2003版本的连接字符串
        private string sConnectionString = @"Provider=Microsoft.ACE.Oledb.12.0;Data Source=Data.accdb";//Access 2007/2010版本的连接字符串

        /// <summary>
        /// 存储数据库连接(保护类,只有由它派生的类才能访问)
        /// </summary>
        protected OleDbConnection mOleDbConnection;

        /// <summary>
        /// 错误字符串
        /// </summary>
        private string m_sExceptionString = "";
        /// <summary>
        /// 获取错误字符串
        /// </summary>
        public string ExceptionString
        {
            get { return m_sExceptionString; }
        }

        /// <summary>
        /// 虚函数:输出错误信息, 在派生类中被重写
        /// </summary>
        /// <returns>错误输出字符串</returns>
        public virtual string ExceptionOutput(Exception ex)
        {
            m_sExceptionString = ex.Message;
            return m_sExceptionString;
        }

        /// <summary>
        /// 构造函数：数据库的默认连接
        /// </summary>
        public AccessOperation()
        {
            AccessSetDatabase(sDatabaseFullname);
        }

        /// <summary>
        /// 构造函数：带有数据库文件名参数的数据库连接
        /// </summary>
        public AccessOperation(string sDBFullname)
        {
            AccessSetDatabase(sDBFullname);
        }

        /// <summary>
        /// 获取数据库文件完整地址
        /// </summary>
        /// <returns></returns>
        public string AccessGetDBFullname()
        {
            return sDatabaseFullname;
        }

        /// <summary>
        /// 设置数据库文件完整地址
        /// </summary>
        /// <param name="sDBFullname">数据库文件完整地址</param>
        public void AccessSetDBFullname(string sDBFullname)
        {
            sDatabaseFullname = sDBFullname;
        }

        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <returns></returns>
        public string AccessGetConnectionString()
        {
            return sConnectionString;
        }

        /// <summary>
        /// 设置数据库连接字符串
        /// </summary>
        /// <param name="sConnString">数据库连接字符串</param>
        public void AccessSetConnectionString(string sConnString)
        {
            sConnectionString = sConnString;
        }

        /// <summary>
        /// 设置数据库并创建链接对象
        /// </summary>
        /// <param name="sDBFullname">数据库文件完整路径+名称</param>
        public void AccessSetDatabase(string sDBFullname)
        {
            sDatabaseFullname = sDBFullname;
            //sConnectionString = @"Provider=Microsoft.Jet.OleDb.4.0;Data Source=" + sDatabaseFullname;//Access 2000-2003版本的连接字符串
            sConnectionString = @"Provider=Microsoft.ACE.Oledb.12.0;Data Source=" + sDatabaseFullname;//Access 2007/2010版本的连接字符串

            //连接不为空时先释放连接资源
            if (mOleDbConnection != null)
            {
                mOleDbConnection.Dispose();
            }
            //创建一个连接对象
            mOleDbConnection = new OleDbConnection(sConnectionString);
        }

        /// <summary>
        /// 打开Access数据库连接
        /// </summary>
        /// <returns>是否成功打开数据库连接</returns>
        public bool AccessOpenConnection()
        {
            bool isConnSuccess = false;//是否连接成功
            if (mOleDbConnection.State == ConnectionState.Open)//如果连接打开
            {
                mOleDbConnection.Close();
            }
            if (mOleDbConnection.State == ConnectionState.Closed)//如果连接关闭
            {
                try
                {
                    mOleDbConnection.Open();
                    isConnSuccess = true;
                }
                catch (Exception e)
                {
                    ExceptionOutput(e);
                }
            }
            return isConnSuccess;
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        /// <returns>是否成功关闭数据库连接</returns>
        public bool AccessCloseConnection()
        {
            bool isCloseConnSuccess = false;//是否连接成功
            if (mOleDbConnection.State == ConnectionState.Open)//如果连接打开
            {
                try
                {
                    mOleDbConnection.Close();
                    mOleDbConnection.Dispose();
                    isCloseConnSuccess = true;
                }
                catch (Exception e)
                {
                    ExceptionOutput(e);
                }
            }
            else if (mOleDbConnection.State == ConnectionState.Closed)//如果连接关闭
            {
                isCloseConnSuccess = true;
            }
            return isCloseConnSuccess;
        }

        /// <summary>
        /// 执行SQL命令字符串,不返回结果
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串,可以为UPDATE、INSERT 或 DELETE 等语句</param>
        /// <returns>SQL命令是否成功执行</returns>
        public bool AccessExecuteSQLString(string sSQLString)
        {
            bool isExecuteSuccess = false;//是否连接成功

            //如果连接关闭则首先打开连接
            if (mOleDbConnection.State == ConnectionState.Closed)
            {
                if (!AccessOpenConnection())
                {
                    return false;
                }
            }
            //定义事务
            OleDbTransaction mOleDbTransaction = mOleDbConnection.BeginTransaction();
            //定义执行命令
            OleDbCommand mOleDbCommand = new OleDbCommand(sSQLString, mOleDbConnection, mOleDbTransaction);

            try
            {
                // 使用 ExecuteNonQuery 执行编录操作（例如查询数据库的结构或创建诸如表等的数据库对象）
                // 或通过执行 UPDATE、INSERT 或 DELETE 语句更改数据库中的数据。
                // 虽然 ExecuteNonQuery 不返回任何行，但是映射到参数的任何输出参数或返回值都会用数据进行填充。
                // 对于 UPDATE、INSERT 和 DELETE 语句，返回值为该命令所影响的行数。对于其他所有类型的语句，返回值为 -1。
                mOleDbCommand.ExecuteNonQuery();//执行命令
                mOleDbTransaction.Commit();//提交事务
                isExecuteSuccess = true;
            }
            catch (Exception e)
            {
                try
                {
                    mOleDbTransaction.Rollback();//尝试回滚事务
                }
                catch
                {
                    // 什么也不做: transaction 非活动
                }
                ExceptionOutput(e);
            }
            return isExecuteSuccess;
        }

        /// <summary>
        /// 执行批量SQL命令字符串,不返回结果
        /// </summary>
        /// <param name="sSQLStrings">SQL命令字符串数组,可以为UPDATE、INSERT 或 DELETE 等语句组</param>
        /// <param name="sSQLStringsCount">SQL命令字符串组执行语句数量</param>
        /// <returns>SQL命令是否成功执行</returns>
        public bool AccessExecuteSQLString(string[] sSQLStrings, int sSQLStringsCount)
        {
            bool isExecuteSuccess = true;//是否执行成功

            //如果连接关闭则首先打开连接
            if (mOleDbConnection.State == ConnectionState.Closed)
            {
                if (!AccessOpenConnection())
                {
                    return false;
                }
            }

            //定义事务
            OleDbTransaction mOleDbTransaction = mOleDbConnection.BeginTransaction();
            //定义执行命令
            OleDbCommand mOleDbCommand = new OleDbCommand();
            mOleDbCommand.Connection = mOleDbConnection;
            mOleDbCommand.CommandType = CommandType.Text;
            mOleDbCommand.Transaction = mOleDbTransaction;
            try
            {
                for (int i = 0; i < sSQLStringsCount; ++i)
                {
                    mOleDbCommand.CommandText = sSQLStrings[i];
                    // 使用 ExecuteNonQuery 执行编录操作（例如查询数据库的结构或创建诸如表等的数据库对象）
                    // 或通过执行 UPDATE、INSERT 或 DELETE 语句更改数据库中的数据。
                    // 虽然 ExecuteNonQuery 不返回任何行，但是映射到参数的任何输出参数或返回值都会用数据进行填充。
                    // 对于 UPDATE、INSERT 和 DELETE 语句，返回值为该命令所影响的行数。对于其他所有类型的语句，返回值为 -1。
                    mOleDbCommand.ExecuteNonQuery();//执行命令
                }
                mOleDbTransaction.Commit();//提交事务
            }
            catch (Exception e)
            {
                mOleDbTransaction.Rollback();//尝试回滚事务
                isExecuteSuccess = false;
                ExceptionOutput(e);
            }

            return isExecuteSuccess;
        }

        /// <summary>
        /// 执行INSERT的SQL命令字符串,不返回新插入数据中自动增长字段的值(命名为: AutoID)
        /// </summary>
        /// <param name="sSQLString"></param>
        /// <returns></returns>
        public int AccessExecuteInsertSQLStringForAutoID(string sSQLString)
        {
            int nID = -1;//是否连接成功

            //如果连接关闭则首先打开连接
            if (mOleDbConnection.State == ConnectionState.Closed)
            {
                if (!AccessOpenConnection())
                {
                    return nID;
                }
            }
            //定义事务
            OleDbTransaction mOleDbTransaction = mOleDbConnection.BeginTransaction(/*IsolationLevel.ReadCommitted*/);
            //定义执行命令
            OleDbCommand mOleDbCommand = new OleDbCommand(sSQLString, mOleDbConnection, mOleDbTransaction);

            try
            {
                // 使用 ExecuteNonQuery 执行编录操作（例如查询数据库的结构或创建诸如表等的数据库对象）
                // 或通过执行 UPDATE、INSERT 或 DELETE 语句更改数据库中的数据。
                // 虽然 ExecuteNonQuery 不返回任何行，但是映射到参数的任何输出参数或返回值都会用数据进行填充。
                // 对于 UPDATE、INSERT 和 DELETE 语句，返回值为该命令所影响的行数。对于其他所有类型的语句，返回值为 -1。
                int iRow = mOleDbCommand.ExecuteNonQuery();//执行命令
                if (iRow > 0)
                {
                    mOleDbCommand.CommandText = "SELECT @@IDENTITY AS AutoID";
                    nID = int.Parse(mOleDbCommand.ExecuteScalar().ToString());
                    //nID = Convert.ToInt32(mOleDbCommand.ExecuteScalar());
                }

                mOleDbTransaction.Commit();//提交事务
            }
            catch (Exception e)
            {
                try
                {
                    mOleDbTransaction.Rollback();//尝试回滚事务
                }
                catch
                {
                    // 什么也不做: transaction 非活动
                }
                ExceptionOutput(e);
            }
            return nID;
        }

        /// <summary>
        /// 执行SQL语句返回结果到DataReader中
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>包含执行结果的OleDbDataReader数据集合</returns>
        public OleDbDataReader AccessReadDataToDataReader(string sSQLString)
        {
            //如果连接关闭则首先打开连接
            if (mOleDbConnection.State == ConnectionState.Closed)
            {
                if (!AccessOpenConnection())
                {
                    return null;
                }
            }
            //定义执行命令
            OleDbCommand mOleDbCommand = new OleDbCommand(sSQLString, mOleDbConnection);
            OleDbDataReader mOleDbDataReader;
            try
            {
                //执行读取操作
                mOleDbDataReader = mOleDbCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                mOleDbDataReader = null;
                ExceptionOutput(e);
            }

            return mOleDbDataReader;

            //mOleDbDataReader.Close();//使用时要调用关闭函数
        }

        /// <summary>
        /// 执行SQL语句返回结果到DataSet中
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>包含执行结果的DataSet</returns>
        public DataSet AccessReadDataToDataSet(string sSQLString)
        {
            //如果连接关闭则首先打开连接
            if (mOleDbConnection.State == ConnectionState.Closed)
            {
                if (!AccessOpenConnection())
                {
                    return null;
                }
            }
            //定义数据集
            DataSet mDataSet = new DataSet();
            //定义执行命令
            OleDbCommand mOleDbCommand = new OleDbCommand(sSQLString, mOleDbConnection);
            OleDbDataAdapter mOleDbDataAdapter = new OleDbDataAdapter(mOleDbCommand);

            //定义执行命令 - 下句也可以
            //OleDbDataAdapter mOleDbDataAdapter = new OleDbDataAdapter(sSQLString, mOleDbConnection);

            try
            {
                //填充数据集
                mOleDbDataAdapter.Fill(mDataSet, "objDataSet");
            }
            catch (Exception e)
            {
                mDataSet = null;
                ExceptionOutput(e);
            }

            return mDataSet;
        }

        /// <summary>
        /// 执行SQL语句返回结果到DataTable中
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>包含执行结果的DataTable</returns>
        public DataTable AccessReadDataToDataTable(string sSQLString)
        {
            //如果连接关闭则首先打开连接
            if (mOleDbConnection.State == ConnectionState.Closed)
            {
                if (!AccessOpenConnection())
                {
                    return null;
                }
            }
            //定义数据表
            DataTable mDataTable = new DataTable();
            //定义执行命令
            OleDbCommand mOleDbCommand = new OleDbCommand(sSQLString, mOleDbConnection);
            OleDbDataAdapter mOleDbDataAdapter = new OleDbDataAdapter(mOleDbCommand);

            try
            {
                //填充数据集
                mOleDbDataAdapter.Fill(mDataTable);
            }
            catch (Exception e)
            {
                mDataTable = null;
                ExceptionOutput(e);
            }

            return mDataTable;
        }

        /// <summary>
        /// 执行SQL语句返回结果到DataView中
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>包含执行结果的DataView</returns>
        public DataView AccessReadDataToDataView(string sSQLString)
        {
            //如果连接关闭则首先打开连接
            if (mOleDbConnection.State == ConnectionState.Closed)
            {
                if (!AccessOpenConnection())
                {
                    return null;
                }
            }
            //定义数据视图
            DataView mDataView = new DataView();
            //定义数据集
            DataSet mDataSet = new DataSet();
            //定义执行命令
            OleDbCommand mOleDbCommand = new OleDbCommand(sSQLString, mOleDbConnection);
            OleDbDataAdapter mOleDbDataAdapter = new OleDbDataAdapter(mOleDbCommand);

            try
            {
                //填充数据集
                mOleDbDataAdapter.Fill(mDataSet);
                mDataView = mDataSet.Tables[0].DefaultView;
            }
            catch (Exception e)
            {
                mDataView = null;
                ExceptionOutput(e);
            }

            return mDataView;
        }

        /// <summary>
        /// 执行 "SELECT COUNT(*) FROM table name" 类型的SQL语句来计算数据行总数量
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>返回结果行数 -1:无法打开数据库联连接或数据集为空; >=0:返回的行数</returns>
        public int AccessReadDataToCount(string sSQLString)
        {
            int sqlResultCount = 0;
            //如果连接关闭则首先打开连接
            if (mOleDbConnection.State == ConnectionState.Closed)
            {
                if (!AccessOpenConnection())
                {
                    return -1;
                }
            }

            //定义数据集
            DataSet mDataSet = new DataSet();
            //定义执行命令
            OleDbCommand mOleDbCommand = new OleDbCommand(sSQLString, mOleDbConnection);
            OleDbDataAdapter mOleDbDataAdapter = new OleDbDataAdapter(mOleDbCommand);
            try
            {
                mOleDbDataAdapter.Fill(mDataSet, "objDataSet");//填充数据集
            }
            catch (Exception e)
            {
                mDataSet = null;
                ExceptionOutput(e);
            }
            //数据集为空则返回
            if (mDataSet == null)
            {
                return -1;
            }

            //取得第一个字段的值放入sqlResultCount变量
            sqlResultCount = Convert.ToInt32(mDataSet.Tables[0].Rows[0].ItemArray[0].ToString());

            return sqlResultCount;
        }

        /// <summary>
        /// 根据列索引和行索引获取字段值
        /// </summary>
        /// <param name="columnIndex">列索引</param>
        /// <param name="rowIndex">行索引</param>
        /// <param name="mDataSet">数据集</param>
        /// <returns>字段值</returns>
        public object AccessReadDataValue(int rowIndex, int columnIndex, DataSet mDataSet)
        {
            if (mDataSet == null)
            {
                return null;
            }
            //获取指定位置的值
            object resultValue = mDataSet.Tables[0].Rows[rowIndex].ItemArray[columnIndex];

            return resultValue;
        }

        /// <summary>
        /// 根据列名称和行索引获取字段值
        /// </summary>
        /// <param name="columnName">列名称</param>
        /// <param name="rowIndex">行索引</param>
        /// <param name="mDataSet">数据集</param>
        /// <returns>字段值</returns>
        public object AccessReadDataValue(int rowIndex, string columnName, DataSet mDataSet)
        {
            if (mDataSet == null)
            {
                return null;
            }
            //获取指定位置的值
            object resultValue = mDataSet.Tables[0].Rows[rowIndex][columnName];

            return resultValue;
        }
    }
}

/**************************************文档说明************************************
 * Copyright(C), 2010, BUAA, 软件与控制研究室
 * 文件名称:    ExcelDBOperation.cs
 * 作者:        Liuwei
 * 版本:        1.0        
 * 创建日期:    2011.11.14, 22:10
 * 完成日期:    2011
 * 文件描述:    通过数据库连接进行Excel文件操作
 *              
 * 调用关系:    using System.Data.OleDb;
 * 
 * 继承关系:    
 * 其它:        
 *              
 * 属性列表:    略
 * 
 * 修改历史:
 * 1.   修改日期:   
 *      修改人:     
 *      修改功能:   
***********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
//
using System.Data.OleDb;

namespace ExcelDll
{
    /// <summary>
    /// Excel数据库操作类
    /// </summary>
    public class ExcelDBOperation
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string m_sConnectionString = "";

        //数据库连接
        private OleDbConnection m_OleDbConnection = null;
       
        /// <summary>
        /// 数据集
        /// </summary>
        public DataSet m_DataSet = null;

        /// <summary>
        /// 错误输出字符串
        /// </summary>
        private string m_sExceptionString = "";
        /// <summary>
        /// 虚函数:输出错误信息, 在派生类中被重写
        /// </summary>
        /// <returns>错误输出字符串</returns>
        public virtual string ExceptionOutput(Exception ex)
        {
            m_sExceptionString = ex.Message;
            return m_sExceptionString;
        }

//         /// <summary>
//         /// 构造函数
//         /// </summary>
//         public ExcelDBOperation(string sDBConnectionString)
//         {
//             OleDBSetDatabase(sDBConnectionString); 
//         }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ExcelDBOperation(string filename)
        {
            if (!File.Exists(filename))
            {
                return;
            }
            OleDBSetDatabase("Provider=Microsoft.ACE.Oledb.12.0; Extended Properties=\"Excel 8.0; HDR=No\"; Data Source=" + filename);
        }

        /// <summary>
        /// 设置数据库并创建链接对象
        /// </summary>
        /// <param name="sDBConnectionString">数据源完整路径</param>
        public void OleDBSetDatabase(string sDBConnectionString)
        {
            m_sConnectionString = sDBConnectionString;

            //连接不为空时先释放连接资源
            if (m_OleDbConnection != null)
            {
                m_OleDbConnection.Dispose();
            }
            //创建一个连接对象
            m_OleDbConnection = new OleDbConnection(m_sConnectionString);
        }

        /// <summary>
        /// 通过OleDB方式连接数据库
        /// </summary>
        /// <returns>是否连接成功</returns>
        public bool OleDBOpenConnection()
        {
            bool isConnSuccess = false;//是否连接成功
            if (m_OleDbConnection.State == ConnectionState.Open)//如果连接打开
            {
                m_OleDbConnection.Close();
            }
            if (m_OleDbConnection.State == ConnectionState.Closed)//如果连接关闭
            {
                try
                {
                    m_OleDbConnection.Open();
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
        public bool OleDBCloseConnection()
        {
            bool isCloseConnSuccess = false;//是否连接成功
            if (m_OleDbConnection.State == ConnectionState.Open)//如果连接打开
            {
                try
                {
                    m_OleDbConnection.Close();
                    //mOdbcConnection.Dispose();//...
                    isCloseConnSuccess = true;
                }
                catch (Exception e)
                {
                    ExceptionOutput(e);
                }
            }
            else if (m_OleDbConnection.State == ConnectionState.Closed)//如果连接关闭
            {
                isCloseConnSuccess = true;
            }
            return isCloseConnSuccess;
        }

        /// <summary>
        /// 执行SQL语句返回结果到DataSet中 - SQL语句如"SELECT * FROM [Sheet1$]"
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>包含执行结果的DataSet</returns>
        public DataSet OleDBReadDataToDataSet(string sSQLString)
        {
            //如果连接关闭则首先打开连接
            if (m_OleDbConnection.State == ConnectionState.Closed)
            {
                if (!OleDBOpenConnection())
                {
                    return null;
                }
            }
            //定义数据集
            DataSet mDataSet = new DataSet();

            //定义执行命令
            OleDbCommand mOleDbCommand = new OleDbCommand(sSQLString, m_OleDbConnection);
            OleDbDataAdapter mOleDbDataAdapter = new OleDbDataAdapter(mOleDbCommand);
            //定义执行命令 - 下句也可以
            //OleDbDataAdapter mOleDbDataAdapter = new OleDbDataAdapter(sSQLString, m_OleDbConnection);
            //
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

            //新增
            mOleDbDataAdapter.Dispose();
            mOleDbCommand.Dispose();
            //新增end

            return mDataSet;
        }

        /// <summary>
        /// 执行SQL命令字符串,不返回结果
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串,可以为CREATE、UPDATE、INSERT 或 DELETE 等语句</param>
        /// <returns>SQL命令是否成功执行</returns>
        public bool OleDBExecuteSQLString(string sSQLString)
        {
            bool isExecuteSuccess = true;//是否执行成功

            //如果连接关闭则首先打开连接
            if (m_OleDbConnection.State == ConnectionState.Closed)
            {
                if (!OleDBOpenConnection())
                {
                    return false;
                }
            }

            //定义事务
            OleDbTransaction mOleDbTransaction = m_OleDbConnection.BeginTransaction();
            //定义执行命令
            OleDbCommand mOleDbCommand = new OleDbCommand(sSQLString, m_OleDbConnection, mOleDbTransaction);

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
            //
            return isExecuteSuccess;
        }

        /// <summary>
        /// 执行SQL命令字符串,不返回结果
        /// </summary>
        /// <param name="sheetName">Sheet名称</param>
        /// <param name="columnName">列名称</param>
        /// <returns>SQL命令是否成功执行</returns>
        public bool OleDBInsertSheet(string sheetName, string[] columnName)
        {
            bool isExecuteSuccess = true;//是否执行成功

            //如果连接关闭则首先打开连接
            if (m_OleDbConnection.State == ConnectionState.Closed)
            {
                if (!OleDBOpenConnection())
                {
                    return false;
                }
            }

            //定义事务
            OleDbTransaction mOleDbTransaction = m_OleDbConnection.BeginTransaction();
            //定义执行命令 - @"CREATE TABLE SSSSSSS(序号 Integer, 名称 varchar)"
            string sqlString = @"CREATE TABLE ";
            sqlString = sqlString + sheetName;
            if (columnName != null)
            {
                sqlString = sqlString + "(";
                for (int i = 0; i < columnName.Length; ++i)
                {
                    sqlString = sqlString + columnName[i] + " varchar" + ",";
                }
                sqlString = sqlString.Substring(0, sqlString.Length - 1);
                sqlString = sqlString + ")";
            }
            else
            {
                sqlString = sqlString + "(def varchar)";
            }
            OleDbCommand mOleDbCommand = new OleDbCommand(sqlString, m_OleDbConnection, mOleDbTransaction);

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
            //
            return isExecuteSuccess;
        }

        /// <summary>
        /// 执行SQL命令字符串,不返回结果
        /// </summary>
        /// <param name="sheetName">Sheet名称</param>
        /// <param name="columnNames">列名称数组</param>
        /// <param name="mValues">值数组</param>
        /// <returns>SQL命令是否成功执行</returns>
        public bool OleDBInsertData(string sheetName, string[] columnNames, object[,] mValues)
        {
            bool isExecuteSuccess = true;//是否执行成功

            //列数不对
            if (columnNames.Length != mValues.GetLength(1))
            {
                return false;
            }

            //如果连接关闭则首先打开连接
            if (m_OleDbConnection.State == ConnectionState.Closed)
            {
                if (!OleDBOpenConnection())
                {
                    return false;
                }
            }

            OleDbCommand mOleDbCommand = new OleDbCommand();
            //赋值连接
            mOleDbCommand.Connection = m_OleDbConnection;

            //赋值参数和执行命令字符串
            string commandText = "INSERT INTO " + sheetName + "(";
            for (int j = 0; j < columnNames.Length; ++j)//列数量
            {
                commandText = commandText + columnNames[j] + ",";
                mOleDbCommand.Parameters.Add(new OleDbParameter("@" + columnNames[j], OleDbType.VarChar));
            }
            commandText = commandText.Substring(0, commandText.Length - 1);//去掉最后一个逗号
            commandText = commandText + ") VALUES(";
            for (int j = 0; j < columnNames.Length; ++j)//列数量
            {
                commandText = commandText + "@" + columnNames[j] + ",";
            }
            commandText = commandText.Substring(0, commandText.Length - 1);//去掉最后一个逗号
            commandText = commandText + ")";
            mOleDbCommand.CommandText = commandText;
            //赋值参数和执行命令字符串-end
            //定义并赋值事务
            OleDbTransaction mOleDbTransaction = m_OleDbConnection.BeginTransaction();
            mOleDbCommand.Transaction = mOleDbTransaction;

            try
            {
                //赋值
                for (int i = 0; i < mValues.GetLength(0); ++i )
                {
                    for (int j = 0; j < mValues.GetLength(1); ++j)
                    {
                        mOleDbCommand.Parameters[j].Value = mValues[i, j];
                    }
                }

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
            //
            return isExecuteSuccess;
        }
    }
}

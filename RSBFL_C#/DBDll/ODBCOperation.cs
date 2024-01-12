/*****************************************************************
 * Copyright(C), 2010, BUAA, 软件与控制研究室
 * 文件名称:    ODBCOperation.cs
 * 作者:        Liuwei
 * 版本:        1.0        
 * 创建日期:    2010.07.22
 * 完成日期:    2010
 * 文件描述:    数据库操作类。利用ODBC操作数据库。
 *              
 * 引用:        引用系统ODBC空间: System.Data.Odbc
 * 
 * 修改历史:
 * 1.   修改日期:
 *      修改人:
 *      修改功能:
 * 2.   
*****************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Odbc;


namespace DBDll
{
    /// <summary>
    /// ODBC方式操作数据库类
    /// </summary>
    public class ODBCOperation
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        //private string m_sConnectionString = "Provider=MSDAORA.1;Data Source=(DESCRIPTION =(ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.2)(PORT = 1521)))(CONNECT_DATA = (SID = DEV)));Dsn=oracle;pwd=sa;Unicode=true";
        //private string m_sConnectionString = "Dsn=oracle;uid=god;pwd=sa";
        private string m_sConnectionString = "";

        /// <summary>
        /// 存储数据库连接(保护类,只有由它派生的类才能访问)
        /// </summary>
        protected OdbcConnection mOdbcConnection;

        /// <summary>
        /// 存储数据库已用游标数(保护类,只有由它派生的类才能访问)
        /// </summary>
        protected int cursor_used = 0;

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

        /// <summary>
        /// 构造函数：数据库的默认连接
        /// </summary>
        public ODBCOperation()
        {
            ODBCSetDatabase(m_sConnectionString);
        }

        /// <summary>
        /// 构造函数：带有数据源完整地址参数的数据库连接
        /// </summary>
        public ODBCOperation(string sDBConnectionString)
        {
            ODBCSetDatabase(sDBConnectionString);
        }

        /// <summary>
        /// 获取数据源完整地址/数据库连接字符串
        /// </summary>
        /// <returns></returns>
        public string ODBCGetDBConnectionString()
        {
            return m_sConnectionString;
        }

        /// <summary>
        /// 设置数据源完整地址/数据库连接字符串
        /// </summary>
        /// <param name="sDBConnectionString">数据源完整地址</param>
        public void ODBCSetDBConnectionString(string sDBConnectionString)
        {
            m_sConnectionString = sDBConnectionString;
        }

        /// <summary>
        /// 设置数据库并创建链接对象
        /// </summary>
        /// <param name="sDBConnectionString">数据源完整路径</param>
        public void ODBCSetDatabase(string sDBConnectionString)
        {
            m_sConnectionString = sDBConnectionString;

            //连接不为空时先释放连接资源
            if (mOdbcConnection != null)
            {
                mOdbcConnection.Dispose();
            }
            //创建一个连接对象
            mOdbcConnection = new OdbcConnection(m_sConnectionString);/////////////////////////////
        }
        /// <summary>
        /// 打开Oracle数据库连接
        /// </summary>
        /// <returns>是否成功打开数据库连接</returns>
        public bool ODBCOpenConnection()
        {
            bool isConnSuccess = false;//是否连接成功
            if (mOdbcConnection.State == ConnectionState.Open)//如果连接打开
            {
                mOdbcConnection.Close();
            }
            if (mOdbcConnection.State == ConnectionState.Closed)//如果连接关闭
            {
                try
                {
                    mOdbcConnection.Open();
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
        public bool ODBCCloseConnection()
        {
            bool isCloseConnSuccess = false;//是否连接成功
            if (mOdbcConnection.State == ConnectionState.Open)//如果连接打开
            {
                try
                {
                    mOdbcConnection.Close();
                    //mOdbcConnection.Dispose();//...
                    isCloseConnSuccess = true;
                    //游标置零
                    cursor_used = 0;
                }
                catch (Exception e)
                {
                    ExceptionOutput(e);
                }
            }
            else if (mOdbcConnection.State == ConnectionState.Closed)//如果连接关闭
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
        public bool ODBCExecuteSQLString(string sSQLString)
        {
            bool isExecuteSuccess = false;//是否连接成功

            //如果连接关闭则首先打开连接
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return isExecuteSuccess;
                }
            }
            //定义事务
            OdbcTransaction mOdbcTransaction = mOdbcConnection.BeginTransaction();
            //定义执行命令
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection, mOdbcTransaction);
            try
            {
                // 使用 ExecuteNonQuery 执行编录操作（例如查询数据库的结构或创建诸如表等的数据库对象）
                // 或通过执行 UPDATE、INSERT 或 DELETE 语句更改数据库中的数据。
                // 虽然 ExecuteNonQuery 不返回任何行，但是映射到参数的任何输出参数或返回值都会用数据进行填充。
                // 对于 UPDATE、INSERT 和 DELETE 语句，返回值为该命令所影响的行数。对于其他所有类型的语句，返回值为 -1。
                mOdbcCommand.ExecuteNonQuery();//执行命令
                mOdbcTransaction.Commit();//提交事务
                //新增
                mOdbcCommand.Dispose();
                mOdbcCommand = null;
                //新增end
                isExecuteSuccess = true;
            }
            catch (Exception e)
            {
                try
                {
                    mOdbcTransaction.Rollback();//尝试回滚事务
                }
                catch
                {
                    // 什么也不做: transaction 非活动
                }
                ExceptionOutput(e);
            }
            //新增
            mOdbcTransaction.Dispose();
            mOdbcTransaction = null;
            //新增end

            return isExecuteSuccess;
        }

        /// <summary>
        /// 执行SQL命令字符串,不返回结果
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串,可以为UPDATE、INSERT 或 DELETE 等语句</param>
        /// <returns>SQL命令是否成功执行</returns>
        public bool ODBCExecuteSQLStringWithoutTrans(string sSQLString)
        {
            bool isExecuteSuccess = false;//是否连接成功

            //如果连接关闭则首先打开连接
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return isExecuteSuccess;
                }
            }
            //定义执行命令
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection);
            try
            {
                // 使用 ExecuteNonQuery 执行编录操作（例如查询数据库的结构或创建诸如表等的数据库对象）
                // 或通过执行 UPDATE、INSERT 或 DELETE 语句更改数据库中的数据。
                // 虽然 ExecuteNonQuery 不返回任何行，但是映射到参数的任何输出参数或返回值都会用数据进行填充。
                // 对于 UPDATE、INSERT 和 DELETE 语句，返回值为该命令所影响的行数。对于其他所有类型的语句，返回值为 -1。
                mOdbcCommand.ExecuteNonQuery();//执行命令
                //新增
                mOdbcCommand.Dispose();
                mOdbcCommand = null;
                //新增end
                isExecuteSuccess = true;
            }
            catch (Exception e)
            {
                ExceptionOutput(e);
            }

            return isExecuteSuccess;
        }

        /// <summary>
        /// 执行批量SQL命令字符串,不返回结果
        /// </summary>
        /// <param name="sSQLStrings">SQL命令字符串组,可以为UPDATE、INSERT 或 DELETE 等语句</param>
        /// <param name="sSQLStringsCount">SQL命令字符串命令语句组数量</param>
        /// <returns>SQL命令是否成功执行</returns>
        public bool ODBCExecuteSQLString(string[] sSQLStrings, int sSQLStringsCount)
        {
            bool isExecuteSuccess = true;//是否连接成功

            //如果连接关闭则首先打开连接
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return isExecuteSuccess;
                }
            }

            //定义事务
            OdbcTransaction mOdbcTransaction = mOdbcConnection.BeginTransaction();
            //定义执行命令
            OdbcCommand mOdbcCommand = new OdbcCommand();
            mOdbcCommand.Connection = mOdbcConnection;
            mOdbcCommand.CommandType = CommandType.Text;
            mOdbcCommand.Transaction = mOdbcTransaction;
            try
            {
                for (int i = 0; i < sSQLStringsCount; ++i)
                {
                    mOdbcCommand.CommandText = sSQLStrings[i];
                    // 使用 ExecuteNonQuery 执行编录操作（例如查询数据库的结构或创建诸如表等的数据库对象）
                    // 或通过执行 UPDATE、INSERT 或 DELETE 语句更改数据库中的数据。
                    // 虽然 ExecuteNonQuery 不返回任何行，但是映射到参数的任何输出参数或返回值都会用数据进行填充。
                    // 对于 UPDATE、INSERT 和 DELETE 语句，返回值为该命令所影响的行数。对于其他所有类型的语句，返回值为 -1。
                    mOdbcCommand.ExecuteNonQuery();//执行命令
                }
                mOdbcTransaction.Commit();//提交事务
            }
            catch (Exception e)
            {
                mOdbcTransaction.Rollback();//尝试回滚事务
                isExecuteSuccess = false;
                ExceptionOutput(e);
            }

            return isExecuteSuccess;
        }
        
        /// <summary>
        /// 执行SQL语句返回结果到DataReader中
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>包含执行结果的OdbcDataReader数据集合</returns>
        public OdbcDataReader ODBCReadDataToDataReader(string sSQLString)
        {
            //如果连接关闭则首先打开连接
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //定义执行命令
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection);
            OdbcDataReader mOdbcDataReader;
            try
            {
                //执行读取操作
                mOdbcDataReader = mOdbcCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                mOdbcDataReader = null;
                ExceptionOutput(e);
            }

            //新增
            mOdbcCommand.Dispose();
            mOdbcCommand = null;
            //新增end

            return mOdbcDataReader;

            //mOdbcDataReader.Close();//使用时要调用关闭函数
        }

        /// <summary>
        /// 执行 "SELECT COUNT(*) FROM table name" 类型的SQL语句来计算数据行总数量
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>返回结果行数 -1:无法打开数据库联连接或数据集为空; >=0:返回的行数</returns>
        public int ODBCReadDataToCount(string sSQLString)
        {
            int sqlResultCount = 0;
            //如果连接关闭则首先打开连接
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return -1;
                }
            }

            //定义数据集
            DataSet mDataSet = new DataSet();
            //定义执行命令
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection);
            OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter(mOdbcCommand);
            try
            {
                mOdbcDataAdapter.Fill(mDataSet, "objDataSet");//填充数据集
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

            //新增
            mOdbcDataAdapter.Dispose();
            mOdbcCommand.Dispose();
            mDataSet.Clear();
            mDataSet.Reset();
            //新增end

            return sqlResultCount;
        }

        /// <summary>
        /// 执行SQL语句返回结果到DataSet中
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>包含执行结果的DataSet</returns>
        public DataSet ODBCReadDataToDataSet(string sSQLString)
        {
            //如果连接关闭则首先打开连接
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //定义数据集
            DataSet mDataSet = new DataSet();
            //定义执行命令
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection);
            OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter(mOdbcCommand);

            //定义执行命令 - 下句也可以
            //OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter(sSQLString, mOdbcConnection);

            try
            {
                //填充数据集
                mOdbcDataAdapter.Fill(mDataSet, "objDataSet");
            }
            catch (Exception e)
            {
                mDataSet = null;
                ExceptionOutput(e);
            }

            //新增
            mOdbcDataAdapter.Dispose();
            mOdbcCommand.Dispose();
            //新增end

            return mDataSet;
        }

        /// <summary>
        /// 执行SQL语句返回结果到DataSet中
        /// </summary>
        /// <param name="sSQLStrings">SQL命令字符串数组,可以为UPDATE、INSERT 或 DELETE 等语句组</param>
        /// <param name="sSQLStringsCount">SQL命令字符串组执行语句数量</param>
        /// <returns>包含执行结果的DataSet</returns>
        public DataSet[] ODBCReadDataToDataSet(string[] sSQLStrings, int sSQLStringsCount)
        {
            //如果连接关闭则首先打开连接
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //定义数据集
            DataSet[] mDataSet = new DataSet[sSQLStringsCount];
            for (int i = 0; i < sSQLStringsCount; ++i)
            {
                mDataSet[i] = new DataSet();
            }
            //定义执行命令
            OdbcCommand mOdbcCommand = new OdbcCommand();
            mOdbcCommand.Connection = mOdbcConnection;
            OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter();

            //定义执行命令 - 下句也可以
            //OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter(sSQLString, mOdbcConnection);
            try
            {
                for (int i = 0; i < sSQLStringsCount; ++i)
                {
                    mOdbcCommand.CommandText = sSQLStrings[i];
                    mOdbcDataAdapter.SelectCommand = mOdbcCommand;
                    //填充数据集
                    mOdbcDataAdapter.Fill(mDataSet[i], "objDataSet" + i.ToString());
                }
            }
            catch (Exception e)
            {
                mDataSet = null;
                ExceptionOutput(e);
            }

            //新增
            mOdbcDataAdapter.Dispose();
            mOdbcCommand.Dispose();
            //新增end

            return mDataSet;
        }

        /// <summary>
        /// 执行SQL语句返回结果到DataTable中
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>包含执行结果的DataTable</returns>
        public DataTable ODBCReadDataToDataTable(string sSQLString)
        {
            //如果连接关闭则首先打开连接
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //定义数据表
            DataTable mDataTable = new DataTable();
            //定义执行命令
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection);
            OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter(mOdbcCommand);

            try
            {
                //填充数据集
                mOdbcDataAdapter.Fill(mDataTable);
            }
            catch (Exception e)
            {
                mDataTable = null;
                ExceptionOutput(e);
            }

            //新增
            mOdbcDataAdapter.Dispose();
            mOdbcCommand.Dispose();
            //新增

            return mDataTable;
        }

        /// <summary>
        /// 执行SQL语句返回结果到DataView中
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>包含执行结果的DataView</returns>
        public DataView ODBCReadDataToDataView(string sSQLString)
        {
            //如果连接关闭则首先打开连接
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //定义数据视图
            DataView mDataView = new DataView();
            //定义数据集
            DataSet mDataSet = new DataSet();
            //定义执行命令
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection);
            OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter(mOdbcCommand);

            try
            {
                //填充数据集
                mOdbcDataAdapter.Fill(mDataSet);
                mDataView = mDataSet.Tables[0].DefaultView;
            }
            catch (Exception e)
            {
                mDataView = null;
                ExceptionOutput(e);
            }
            
            //新增
            mOdbcCommand.Dispose();
            mOdbcDataAdapter.Dispose();
            mDataSet.Dispose();
            mDataSet.Reset();
            //新增

            return mDataView;
        }

        /// <summary>
        /// 读取数据表的所有列的列名
        /// </summary>
        /// <param name="sTableName">表名</param>
        /// <returns>列名数组</returns>
        public string[] ODBCReadDataColumnsName(string sTableName)
        {
            //如果连接关闭则首先打开连接
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //定义执行命令
            OdbcCommand mOdbcCommand = new OdbcCommand("SELECT * FROM " + sTableName + " WHERE 0=1", mOdbcConnection);
            OdbcDataReader mOdbcDataReader;
            try
            {
                //执行读取操作
                mOdbcDataReader = mOdbcCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                mOdbcDataReader = null;
                ExceptionOutput(e);
                return null;
            }

            string[] sColumns = new string[mOdbcDataReader.FieldCount];
            for (int i = 0; i < mOdbcDataReader.FieldCount; ++i)
            {
                sColumns[i] = mOdbcDataReader.GetName(i);
            }
            
            mOdbcDataReader.Close();//使用时要调用关闭函数
            //新增
            mOdbcCommand.Dispose();
            mOdbcCommand = null;
            mOdbcDataReader.Dispose();
            //新增end

            return sColumns;
        }

        /// <summary>
        /// 读取数据表的所有列的类型
        /// </summary>
        /// <param name="sTableName">表名</param>
        /// <returns>列名数组</returns>
        public string[] ODBCReadDataColumnsType(string sTableName)
        {
            //如果连接关闭则首先打开连接
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //定义执行命令
            OdbcCommand mOdbcCommand = new OdbcCommand("SELECT * FROM " + sTableName + " WHERE 0=1", mOdbcConnection);
            OdbcDataReader mOdbcDataReader;
            try
            {
                //执行读取操作
                mOdbcDataReader = mOdbcCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                mOdbcDataReader = null;
                ExceptionOutput(e);
                return null;
            }

            string[] sColumnsType = new string[mOdbcDataReader.FieldCount];
            for (int i = 0; i < mOdbcDataReader.FieldCount; ++i)
            {
                sColumnsType[i] = mOdbcDataReader.GetFieldType(i).ToString();
            }

            mOdbcDataReader.Close();//使用时要调用关闭函数
            //新增
            mOdbcCommand.Dispose();
            mOdbcCommand = null;
            mOdbcDataReader.Dispose();
            //新增end

            return sColumnsType;

        }

        /// <summary>
        /// 根据列索引和行索引获取字段值
        /// </summary>
        /// <param name="columnIndex">列索引</param>
        /// <param name="rowIndex">行索引</param>
        /// <param name="mDataSet">数据集</param>
        /// <returns>字段值</returns>
        public static object ODBCReadDataValue(int rowIndex, int columnIndex, DataSet mDataSet)
        {
            if ((mDataSet == null) || (0 == mDataSet.Tables[0].Rows.Count))
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
        public static object ODBCReadDataValue(int rowIndex, string columnName, DataSet mDataSet)
        {
            if (mDataSet == null || (0 == mDataSet.Tables[0].Rows.Count))
            {
                return null;
            }
            //获取指定位置的值
            object resultValue = mDataSet.Tables[0].Rows[rowIndex][columnName];

            return resultValue;
        }
    }
}

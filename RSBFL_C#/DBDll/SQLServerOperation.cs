using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;


namespace DBDll
{
    /// <summary>
    /// Microsoft SQL Server 操作类
    /// </summary>
    public class SQLServerOperation
    {
        /// <summary>
        /// 连接字符串 
        /// </summary>
        private string m_sConnectionString = "Data Source=127.0.0.1;Initial Catalog=SoftwareFaultLocalization; Persist Security Info=true;User ID=sa;Password=sasasasa";
        //Data Source=USER-PC

        /// <summary>
        /// 存储数据库连接(保护类,只有由它派生的类才能访问,不能被继承)
        /// </summary>
        protected SqlConnection m_SqlServerConnection = null;

        /// <summary>
        /// 错误输出字符串
        /// </summary>
        private string m_sExceptionString = String.Empty;
        /// <summary>
        /// 获取错误字符串
        /// </summary>
        public string ExceptionString
        {
            get { return m_sExceptionString; }
        }

        /// <summary>
        /// 数据库是否处于连接状态
        /// </summary>
        private bool m_bIsConnective = false;

        /// <summary>
        /// 获取数据库是否处于连接状态
        /// </summary>
        /// <returns>错误输出字符串</returns>
        public bool IsConnective
        {
            get
            {
                if (false == m_bIsConnective)
                {
                    return false;
                }
                else
                {
                    if (m_SqlServerConnection.State == ConnectionState.Open)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 虚函数:输出错误信息, 需要在派生类中被重写
        /// </summary>
        /// <returns>错误输出字符串</returns>
        public virtual string ExceptionOutput(Exception ex)
        {
            m_sExceptionString = ex.Message;
            return m_sExceptionString;
        }

        /// <summary>
        /// 虚函数:输出提示信息, 需要在派生类中被重写
        /// </summary>
        /// <returns>错误提示字符串</returns>
        public virtual string InformationOutput(string sInfo)
        {
            return sInfo;
        }

        /// <summary>
        /// 构造函数：创建数据库默认连接
        /// </summary>
        public SQLServerOperation()
        {
            SQLServerCreateDatabase(m_sConnectionString);
        }

        /// <summary>
        /// 构造函数：创建带有连接字符串参数的数据库连接
        /// </summary>
        /// <param name="sConnectionString">数据库连接字符串</param>
        public SQLServerOperation(string sConnectionString)
        {
            SQLServerCreateDatabase(sConnectionString);
        }

        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <returns></returns>
        public string SQLServerGetConnectionString()
        {
            return m_sConnectionString;
        }

        /// <summary>
        /// 设置数据库连接字符串
        /// </summary>
        /// <param name="sConnString">数据库连接字符串</param>
        public void SQLServerSetConnectionString(string sConnString)
        {
            m_sConnectionString = sConnString;
        }

        /// <summary>
        /// 创建SQLServer数据库对象
        /// </summary>
        /// <param name="sConnectionString">数据源完整路径</param>
        public void SQLServerCreateDatabase(string sConnectionString)
        {
            m_sConnectionString = sConnectionString;

            //连接不为空时先释放连接资源
            if (m_SqlServerConnection != null)
            {
                m_SqlServerConnection.Dispose();
            }
            //创建一个连接对象
            try
            {
                m_SqlServerConnection = new SqlConnection(m_sConnectionString);
            }
            catch (Exception e)
            {
                m_SqlServerConnection = null;
                ExceptionOutput(e);
            }
        }

        /// <summary>
        /// 打开SQLServer数据库连接
        /// </summary>
        /// <returns>是否成功打开SQLServer数据库连接</returns>
        public bool SQLServerOpenConnection()
        {
            if (null == m_SqlServerConnection)
            {
                InformationOutput("无法打开数据库. \r\n原因: Microsoft SQL Server数据库连接未创建或创建失败.");
                m_bIsConnective = false;
                return false;
            }

            //
            if (m_SqlServerConnection.State == ConnectionState.Open)//如果连接打开则先关闭(防止重复链接)
            {
                try
                {
                    m_SqlServerConnection.Close();
                    m_bIsConnective = false;
                }
                catch (Exception e)
                {
                    ExceptionOutput(e);
                }
            }
            if (m_SqlServerConnection.State == ConnectionState.Closed)//如果连接关闭
            {
                try
                {
                    m_SqlServerConnection.Open();
                    m_bIsConnective = true;
                }
                catch (Exception e)
                {
                    ExceptionOutput(e);
                }
            }
            return m_bIsConnective;
        }

        /// <summary>
        /// 关闭SQLServer数据库连接
        /// </summary>
        /// <returns>是否成功关闭SQLServer数据库连接</returns>
        public bool SQLServerCloseConnection()
        {
            if (null == m_SqlServerConnection)
            {
                InformationOutput("无法打开数据库. \r\n原因: Microsoft SQL Server数据库连接未创建或创建失败.");
                m_bIsConnective = false;
                return false;
            }

            bool isCloseConnSuccess = false;//是否连接成功
            if (m_SqlServerConnection.State == ConnectionState.Open)//如果连接打开
            {
                try
                {
                    m_SqlServerConnection.Close();
                    //m_SqlServerConnection.Dispose();
                    m_bIsConnective = false;
                    isCloseConnSuccess = true;
                }
                catch (Exception e)
                {
                    isCloseConnSuccess = false;
                    ExceptionOutput(e);
                }
            }
            else if (m_SqlServerConnection.State == ConnectionState.Closed)//如果连接关闭
            {
                m_bIsConnective = false;
                isCloseConnSuccess = true;
            }
            return isCloseConnSuccess;
        }

        /// <summary>
        /// 执行SQLServer的SQL命令字符串,不返回结果
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串,可以为UPDATE、INSERT 或 DELETE 等语句</param>
        /// <returns>SQL命令是否成功执行</returns>
        public bool SQLServerExecuteSQLString(string sSQLString)
        {
            //如果连接关闭
            //if (!m_bIsConnective)
            //{
            //    InformationOutput("无法执行SQL指令. \r\n原因: Microsoft SQL Server数据库连接未打开.");
            //    return false;
            //}
            if (m_SqlServerConnection.State == ConnectionState.Closed)
            {
                SQLServerOpenConnection();
            }

            bool isExecuteSuccess = false;//是否连接成功
            //定义事务
            SqlTransaction mSqlServerTransaction = m_SqlServerConnection.BeginTransaction();
            //定义执行命令
            SqlCommand mSqlServerCommand = new SqlCommand(sSQLString, m_SqlServerConnection, mSqlServerTransaction);
            try
            {
                // 使用 ExecuteNonQuery 执行编录操作（例如查询数据库的结构或创建诸如表等的数据库对象）
                // 或通过执行 UPDATE、INSERT 或 DELETE 语句更改数据库中的数据。
                // 虽然 ExecuteNonQuery 不返回任何行，但是映射到参数的任何输出参数或返回值都会用数据进行填充。
                // 对于 UPDATE、INSERT 和 DELETE 语句，返回值为该命令所影响的行数。对于其他所有类型的语句，返回值为 -1。
                mSqlServerCommand.ExecuteNonQuery();//执行命令
                mSqlServerTransaction.Commit();//提交事务
                isExecuteSuccess = true;
            }
            catch (Exception e)
            {
                try
                {
                    ExceptionOutput(e);
                    mSqlServerTransaction.Rollback();//尝试回滚事务
                }
                catch (Exception ex)
                {
                    // 什么也不做: transaction 非活动
                    ExceptionOutput(ex);
                }
                ExceptionOutput(e);
            }
            return isExecuteSuccess;
        }

        /// <summary>
        /// 执行SQL命令字符串,不返回结果
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串,可以为UPDATE、INSERT 或 DELETE 等语句</param>
        /// <returns>SQL命令是否成功执行</returns>
        public bool SQLServerExecuteSQLStringWithoutTransaction(string sSQLString)
        {
            //如果连接关闭
            if (!m_bIsConnective)
            {
                InformationOutput("无法执行SQL指令. \r\n原因: Microsoft SQL Server数据库连接未打开.");
                return false;
            }
            
            bool isExecuteSuccess = false;//是否连接成功
            //定义执行命令
            SqlCommand mSqlServerCommand = new SqlCommand(sSQLString, m_SqlServerConnection);
            try
            {
                // 使用 ExecuteNonQuery 执行编录操作（例如查询数据库的结构或创建诸如表等的数据库对象）
                // 或通过执行 UPDATE、INSERT 或 DELETE 语句更改数据库中的数据。
                // 虽然 ExecuteNonQuery 不返回任何行，但是映射到参数的任何输出参数或返回值都会用数据进行填充。
                // 对于 UPDATE、INSERT 和 DELETE 语句，返回值为该命令所影响的行数。对于其他所有类型的语句，返回值为 -1。
                mSqlServerCommand.ExecuteNonQuery();//执行命令
                //新增
                mSqlServerCommand.Dispose();
                mSqlServerCommand = null;
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
        public bool SQLServerExecuteSQLString(string[] sSQLStrings, int sSQLStringsCount)
        {
            //如果连接关闭
            if (!m_bIsConnective)
            {
                InformationOutput("无法执行SQL指令. \r\n原因: Microsoft SQL Server数据库连接未打开.");
                return false;
            }

            bool isExecuteSuccess = true;//是否连接成功
            //定义事务
            SqlTransaction mSqlServerTransaction = m_SqlServerConnection.BeginTransaction();
            //定义执行命令
            SqlCommand mSqlServerCommand = new SqlCommand();
            mSqlServerCommand.Connection = m_SqlServerConnection;
            mSqlServerCommand.CommandType = CommandType.Text;
            mSqlServerCommand.Transaction = mSqlServerTransaction;
            try
            {
                for (int i = 0; i < sSQLStringsCount; ++i)
                {
                    mSqlServerCommand.CommandText = sSQLStrings[i];
                    // 使用 ExecuteNonQuery 执行编录操作（例如查询数据库的结构或创建诸如表等的数据库对象）
                    // 或通过执行 UPDATE、INSERT 或 DELETE 语句更改数据库中的数据。
                    // 虽然 ExecuteNonQuery 不返回任何行，但是映射到参数的任何输出参数或返回值都会用数据进行填充。
                    // 对于 UPDATE、INSERT 和 DELETE 语句，返回值为该命令所影响的行数。对于其他所有类型的语句，返回值为 -1。
                    mSqlServerCommand.ExecuteNonQuery();//执行命令
                }
                mSqlServerTransaction.Commit();//提交事务
            }
            catch (Exception e)
            {
                mSqlServerTransaction.Rollback();//尝试回滚事务
                isExecuteSuccess = false;
                ExceptionOutput(e);
            }

            return isExecuteSuccess;
        }
        /*
        /// <summary>
        /// 获取存储过程的参数列表。
        /// </summary>
        /// <returns></returns>
        private ArrayList GetParas(string sStoredProcedureName)
        {
            SqlCommand comm = new SqlCommand("dbo.sp_sproc_columns_90", m_SqlServerConnection);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("@procedure_name", (object)sStoredProcedureName);
            SqlDataAdapter sda = new SqlDataAdapter(comm);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            ArrayList al = new ArrayList();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                al.Add(dt.Rows[i][3].ToString());
            }
            return al;
        }

        /// <summary>
        /// 为 SqlCommand 添加参数及赋值。
        /// </summary>
        /// <param name="mSqlServerCommand"></param>
        /// <param name="paraValues"></param>
        private void AddInParaValues(SqlCommand mSqlServerCommand, string sStoredProcedureName, params object[] paraValues) 
        {
            mSqlServerCommand.Parameters.Add(new SqlParameter("@RETURN_VALUE", SqlDbType.Int));
            mSqlServerCommand.Parameters["@RETURN_VALUE"].Direction = ParameterDirection.ReturnValue;
            if (paraValues != null) 
            {
                ArrayList al = GetParas(sStoredProcedureName);
                for (int i = 0; i < paraValues.Length; i++) 
                {
                    mSqlServerCommand.Parameters.AddWithValue(al[i + 1].ToString(), paraValues[i]);
                }
            }
        }

        /// <summary>
        /// 执行存储过程，返回值
        /// </summary>
        /// <param name="sStoredProcedureName">SQL命令字符串,可以为UPDATE、INSERT 或 DELETE 等语句</param>
        /// <returns></returns>
        public string SQLServerExecuteStoredProcedure(string sStoredProcedureName)
        {
            //如果连接关闭
            if (!m_bIsConnective)
            {
                InformationOutput("无法执行存储过程. \r\n原因: Microsoft SQL Server数据库连接未打开.");
                return "";
            }

            //bool isExecuteSuccess = false;//是否连接成功

            //定义数据集
            DataSet mDataSet = new DataSet();
            //定义执行命令
            SqlCommand mSqlServerCommand = new SqlCommand(sStoredProcedureName, m_SqlServerConnection);
            mSqlServerCommand.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter mSqlServerDataAdapter = new SqlDataAdapter(mSqlServerCommand);
            try
            {
                mSqlServerDataAdapter.Fill(mDataSet, "objDataSet");//填充数据集
            }
            catch (Exception e)
            {
                mDataSet = null;
                ExceptionOutput(e);
            }
            //数据集为空则返回
            if (mDataSet == null)
            {
                return "";
            }
            //取得第一个字段的值放入sqlResultCount变量
            string s = mDataSet.Tables[0].Rows[0].ItemArray[0].ToString();

            return s;
        }

        /// <summary>
        /// 执行存储过程，不返回值
        /// </summary>
        /// <param name="sStoredProcedureName">SQL命令字符串,可以为UPDATE、INSERT 或 DELETE 等语句</param>
        /// <returns>SQL命令是否成功执行</returns>
        public bool SQLServerExecuteStoredProcedureNoQuery(string sStoredProcedureName, params object[] paraValues)
        {
            //如果连接关闭
            if (!m_bIsConnective)
            {
                InformationOutput("无法执行存储过程. \r\n原因: Microsoft SQL Server数据库连接未打开.");
                return false;
            }

            bool isExecuteSuccess = false;//是否连接成功
            //定义执行命令
            SqlCommand mSqlServerCommand = new SqlCommand(sStoredProcedureName, m_SqlServerConnection);
            mSqlServerCommand.CommandType = CommandType.StoredProcedure;
            try
            {
                AddInParaValues(mSqlServerCommand, sStoredProcedureName, paraValues);
                mSqlServerCommand.ExecuteNonQuery();//执行命令
                //新增
                mSqlServerCommand.Dispose();
                mSqlServerCommand = null;
                //新增end
                isExecuteSuccess = true;
            }
            catch (Exception e)
            {
                ExceptionOutput(e);
            }

            return isExecuteSuccess;
        }
        //*/

        /// <summary>
        /// 执行SQL语句返回结果到DataReader中
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>包含执行结果的SqlDataReader数据集合</returns>
        public SqlDataReader SQLServerReadDataToDataReader(string sSQLString)
        {
            //如果连接关闭
            if (!m_bIsConnective)
            {
                InformationOutput("无法执行SQL指令. \r\n原因: Microsoft SQL Server数据库连接未打开.");
                return null;
            }

            //定义执行命令
            SqlCommand mSqlServerCommand = new SqlCommand(sSQLString, m_SqlServerConnection);
            SqlDataReader mSqlServerDataReader = null;
            try
            {
                //执行读取操作
                mSqlServerDataReader = mSqlServerCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                mSqlServerDataReader = null;
                ExceptionOutput(e);
            }

            return mSqlServerDataReader;

            //mOracleDataReader.Close();//使用时要调用关闭函数
        }

        /// <summary>
        /// 执行 "SELECT COUNT(*) FROM tablename [WHERE conditions]" 类型的SQL语句来计算数据行总数量
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>返回结果行数 -1:无法打开数据库联连接或数据集为空; >=0:返回的行数</returns>
        public int SqlServerReadDataToCount(string sSQLString)
        {
            //如果连接关闭
            if (!m_bIsConnective)
            {
                InformationOutput("无法执行SQL指令. \r\n原因: Microsoft SQL Server数据库连接未打开.");
                return -1;
            }

            int sqlResultCount = 0;
            //定义数据集
            DataSet mDataSet = new DataSet();
            //定义执行命令
            SqlCommand mSqlServerCommand = new SqlCommand(sSQLString, m_SqlServerConnection);
            SqlDataAdapter mSqlServerDataAdapter = new SqlDataAdapter(mSqlServerCommand);
            try
            {
                mSqlServerDataAdapter.Fill(mDataSet, "objDataSet");//填充数据集
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
        /// 执行SQL语句返回结果到DataSet中(使用灵活,但效率相对较低)
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>包含执行结果的DataSet</returns>
        public DataSet SqlServerReadDataToDataSet(string sSQLString)
        {
            //如果连接关闭
            if (!m_bIsConnective)
            {
                InformationOutput("无法执行SQL指令. \r\n原因: Microsoft SQL Server数据库连接未打开.");
                return null;
            }

            //定义数据集
            DataSet mDataSet = new DataSet();
            //定义执行命令
            SqlCommand mSqlServerCommand = new SqlCommand(sSQLString, m_SqlServerConnection);
            SqlDataAdapter mSqlServerDataAdapter = new SqlDataAdapter(mSqlServerCommand);

            //定义执行命令 - 下句也可以
            //OracleDataAdapter mOracleDataAdapter = new OracleDataAdapter(sSQLString, mOracleConnection);

            try
            {
                //填充数据集
                mSqlServerDataAdapter.Fill(mDataSet, "objDataSet");
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
        public DataTable SqlServerReadDataToDataTable(string sSQLString)
        {
            //如果连接关闭
            if (!m_bIsConnective)
            {
                InformationOutput("无法执行SQL指令. \r\n原因: Microsoft SQL Server数据库连接未打开.");
                return null;
            }

            //定义数据表
            DataTable mDataTable = new DataTable();
            //定义执行命令
            SqlCommand mSqlServerCommand = new SqlCommand(sSQLString, m_SqlServerConnection);
            SqlDataAdapter mSqlServerDataAdapter = new SqlDataAdapter(mSqlServerCommand);

            try
            {
                //填充数据集
                mSqlServerDataAdapter.Fill(mDataTable);
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
        public DataView SqlServerReadDataToDataView(string sSQLString)
        {
            //如果连接关闭
            if (!m_bIsConnective)
            {
                InformationOutput("无法执行SQL指令. \r\n原因: Microsoft SQL Server数据库连接未打开.");
                return null;
            }

            //定义数据视图
            DataView mDataView = new DataView();
            //定义数据集
            DataSet mDataSet = new DataSet();

            //定义执行命令
            SqlCommand mSqlServerCommand = new SqlCommand(sSQLString, m_SqlServerConnection);
            SqlDataAdapter mSqlServerDataAdapter = new SqlDataAdapter(mSqlServerCommand);

            try
            {
                //填充数据集
                mSqlServerDataAdapter.Fill(mDataSet);
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
        /// 读取数据表的所有列的列名
        /// </summary>
        /// <param name="sTableName">表名</param>
        /// <returns>列名数组</returns>
        public string[] SqlServerReadDataColumnsName(string sTableName)
        {
            //如果连接关闭
            if (!m_bIsConnective)
            {
                InformationOutput("无法执行SQL指令. \r\n原因: Microsoft SQL Server数据库连接未打开.");
                return null;
            }

            //定义执行命令
            SqlCommand mSqlServerCommand = new SqlCommand("SELECT * FROM " + sTableName + " WHERE 0=1", m_SqlServerConnection);
            SqlDataReader mSqlServeDataReader;
            try
            {
                //执行读取操作
                mSqlServeDataReader = mSqlServerCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                mSqlServeDataReader = null;
                ExceptionOutput(e);
                return null;
            }

            string[] sColumns = new string[mSqlServeDataReader.FieldCount];
            for (int i = 0; i < mSqlServeDataReader.FieldCount; ++i)
            {
                sColumns[i] = mSqlServeDataReader.GetName(i);
            }

            mSqlServeDataReader.Close();//使用时要调用关闭函数
            //新增
            mSqlServerCommand.Dispose();
            mSqlServerCommand = null;
            mSqlServeDataReader.Dispose();
            //新增end

            return sColumns;
        }

        /// <summary>
        /// 读取数据表的所有列的类型
        /// </summary>
        /// <param name="sTableName">表名</param>
        /// <returns>列名数组</returns>
        public string[] SqlServerReadDataColumnsType(string sTableName)
        {
            //如果连接关闭
            if (!m_bIsConnective)
            {
                InformationOutput("无法执行SQL指令. \r\n原因: Microsoft SQL Server数据库连接未打开.");
                return null;
            }

            //定义执行命令
            SqlCommand mSqlServerCommand = new SqlCommand("SELECT * FROM " + sTableName + " WHERE 0=1", m_SqlServerConnection);
            SqlDataReader mSqlServeDataReader = null;
            try
            {
                //执行读取操作
                mSqlServeDataReader = mSqlServerCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                mSqlServeDataReader = null;
                ExceptionOutput(e);
                return null;
            }

            string[] sColumnsType = new string[mSqlServeDataReader.FieldCount];
            for (int i = 0; i < mSqlServeDataReader.FieldCount; ++i)
            {
                sColumnsType[i] = mSqlServeDataReader.GetFieldType(i).ToString();
            }

            mSqlServeDataReader.Close();//使用时要调用关闭函数
            //新增
            mSqlServerCommand.Dispose();
            mSqlServerCommand = null;
            mSqlServeDataReader.Dispose();
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
        public static object SqlServerReadDataValue(int rowIndex, int columnIndex, DataSet mDataSet)
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
        public static object SqlServerReadDataValue(int rowIndex, string columnName, DataSet mDataSet)
        {
            if ((mDataSet == null) || (0 == mDataSet.Tables[0].Rows.Count))
            {
                return null;
            }
            //获取指定位置的值
            object resultValue = mDataSet.Tables[0].Rows[rowIndex][columnName];

            return resultValue;
        }
    }
}
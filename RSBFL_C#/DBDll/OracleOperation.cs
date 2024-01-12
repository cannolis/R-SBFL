using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OracleClient;

namespace DBDll
{
    /// <summary>
    /// Oracle数据库操作类
    /// </summary>
    public class OracleOperation
    {
        /// <summary>
        /// 数据源的完整地址
        /// </summary>
        private string sDatabaseFullname = "Data Source=sa;Persist Security Info=True;User ID=liuwei;Password=sa;Unicode=True";
        //private string sDatabaseFullname = "Dsn=oracle;uid=sh;pwd=sa;dbq=ORACLE;dba=W;apa=T;exc=F;fen=T;qto=T;frc=10;fdl=10;lob=T;rst=T;btd=F;bnf=F;bam=IfAllSuccessful;num=NLS;dpm=F;mts=T;mdi=F;csr=F;fwc=F;fbs=64000;tlo=O";

        /// <summary>
        /// 连接字符串
        /// </summary>
        private string sConnectionString = "Data Source=sa;Persist Security Info=True;User ID=liuwei;Password=sa;Unicode=True";
        //private string sConnectionString = "Dsn=oracle;uid=sh;pwd=sa;dbq=ORACLE;dba=W;apa=T;exc=F;fen=T;qto=T;frc=10;fdl=10;lob=T;rst=T;btd=F;bnf=F;bam=IfAllSuccessful;num=NLS;dpm=F;mts=T;mdi=F;csr=F;fwc=F;fbs=64000;tlo=O";


        /// <summary>
        /// 存储数据库连接(保护类,只有由它派生的类才能访问)
        /// </summary>
        protected OracleConnection mOracleConnection;

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
        public OracleOperation()
        {
            OracleSetDatabase(sDatabaseFullname);
        }

        /// <summary>
        /// 构造函数：带有数据源完整地址参数的数据库连接
        /// </summary>
        public OracleOperation(string sDBFullname)
        {
            OracleSetDatabase(sDBFullname);
        }

        /// <summary>
        /// 获取数据源完整地址
        /// </summary>
        /// <returns></returns>
        public string OracleGetDBFullname()
        {
            return sDatabaseFullname;
        }

        /// <summary>
        /// 设置数据源完整地址
        /// </summary>
        /// <param name="sDBFullname">数据源完整地址</param>
        public void OracleSetDBFullname(string sDBFullname)
        {
            sDatabaseFullname = sDBFullname;
        }

        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <returns></returns>
        public string OracleGetConnectionString()
        {
            return sConnectionString;
        }

        /// <summary>
        /// 设置数据库连接字符串
        /// </summary>
        /// <param name="sConnString">数据库连接字符串</param>
        public void OracleSetConnectionString(string sConnString)
        {
            sConnectionString = sConnString;
        }

        /// <summary>
        /// 设置数据库并创建链接对象
        /// </summary>
        /// <param name="sDBFullname">数据源完整路径</param>
        public void OracleSetDatabase(string sDBFullname)
        {
            sDatabaseFullname = sDBFullname;
            sConnectionString = sDatabaseFullname;

            //连接不为空时先释放连接资源
            if (mOracleConnection != null)
            {
                mOracleConnection.Dispose();
            }
            //创建一个连接对象
            mOracleConnection = new OracleConnection(sConnectionString);
        }

        /// <summary>
        /// 打开Oracle数据库连接
        /// </summary>
        /// <returns>是否成功打开数据库连接</returns>
        public bool OracleOpenConnection()
        {
            bool isConnSuccess = false;//是否连接成功
            if (mOracleConnection.State == ConnectionState.Open)//如果连接打开
            {
                mOracleConnection.Close();
            }
            if (mOracleConnection.State == ConnectionState.Closed)//如果连接关闭
            {
                try
                {
                    mOracleConnection.Open();
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
        public bool OracleCloseConnection()
        {
            bool isCloseConnSuccess = false;//是否连接成功
            if (mOracleConnection.State == ConnectionState.Open)//如果连接打开
            {
                try
                {
                    mOracleConnection.Close();
                    mOracleConnection.Dispose();
                    isCloseConnSuccess = true;
                }
                catch (Exception e)
                {
                    ExceptionOutput(e);
                }
            }
            else if (mOracleConnection.State == ConnectionState.Closed)//如果连接关闭
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
        public bool OracleExecuteSQLString(string sSQLString)
        {
            bool isExecuteSuccess = false;//是否连接成功

            //如果连接关闭则首先打开连接
            if (mOracleConnection.State == ConnectionState.Closed)
            {
                if (!OracleOpenConnection())
                {
                    return isExecuteSuccess;
                }
            }
            //定义事务
            OracleTransaction mOracleTransaction = mOracleConnection.BeginTransaction();
            //定义执行命令
            OracleCommand mOracleCommand = new OracleCommand(sSQLString, mOracleConnection, mOracleTransaction);

            try
            {
                // 使用 ExecuteNonQuery 执行编录操作（例如查询数据库的结构或创建诸如表等的数据库对象）
                // 或通过执行 UPDATE、INSERT 或 DELETE 语句更改数据库中的数据。
                // 虽然 ExecuteNonQuery 不返回任何行，但是映射到参数的任何输出参数或返回值都会用数据进行填充。
                // 对于 UPDATE、INSERT 和 DELETE 语句，返回值为该命令所影响的行数。对于其他所有类型的语句，返回值为 -1。
                mOracleCommand.ExecuteNonQuery();//执行命令
                mOracleTransaction.Commit();//提交事务
                isExecuteSuccess = true;
            }
            catch (Exception e)
            {
                try
                {
                    mOracleTransaction.Rollback();//尝试回滚事务
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
        /// 执行SQL语句返回结果到DataReader中
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>包含执行结果的OracleDataReader数据集合</returns>
        public OracleDataReader OracleReadDataToDataReader(string sSQLString)
        {
            //如果连接关闭则首先打开连接
            if (mOracleConnection.State == ConnectionState.Closed)
            {
                if (!OracleOpenConnection())
                {
                    return null;
                }
            }
            //定义执行命令
            OracleCommand mOracleCommand = new OracleCommand(sSQLString, mOracleConnection);
            OracleDataReader mOracleDataReader;
            try
            {
                //执行读取操作
                mOracleDataReader = mOracleCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                mOracleDataReader = null;
                ExceptionOutput(e);
            }

            return mOracleDataReader;

            //mOracleDataReader.Close();//使用时要调用关闭函数
        }

        /// <summary>
        /// 执行 "SELECT COUNT(*) FROM table name" 类型的SQL语句来计算数据行总数量
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>返回结果行数 -1:无法打开数据库联连接或数据集为空; >=0:返回的行数</returns>
        public int OracleReadDataToCount(string sSQLString)
        {
            int sqlResultCount = 0;
            //如果连接关闭则首先打开连接
            if (mOracleConnection.State == ConnectionState.Closed)
            {
                if (!OracleOpenConnection())
                {
                    return -1;
                }
            }

            //定义数据集
            DataSet mDataSet = new DataSet();
            //定义执行命令
            OracleCommand mOracleCommand = new OracleCommand(sSQLString, mOracleConnection);
            OracleDataAdapter mOracleDataAdapter = new OracleDataAdapter(mOracleCommand);
            try
            {
                mOracleDataAdapter.Fill(mDataSet, "objDataSet");//填充数据集
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
        /// 执行SQL语句返回结果到DataSet中
        /// </summary>
        /// <param name="sSQLString">SQL命令字符串</param>
        /// <returns>包含执行结果的DataSet</returns>
        public DataSet OracleReadDataToDataSet(string sSQLString)
        {
            //如果连接关闭则首先打开连接
            if (mOracleConnection.State == ConnectionState.Closed)
            {
                if (!OracleOpenConnection())
                {
                    return null;
                }
            }
            //定义数据集
            DataSet mDataSet = new DataSet();
            //定义执行命令
            OracleCommand mOracleCommand = new OracleCommand(sSQLString, mOracleConnection);
            OracleDataAdapter mOracleDataAdapter = new OracleDataAdapter(mOracleCommand);

            //定义执行命令 - 下句也可以
            //OracleDataAdapter mOracleDataAdapter = new OracleDataAdapter(sSQLString, mOracleConnection);

            try
            {
                //填充数据集
                mOracleDataAdapter.Fill(mDataSet, "objDataSet");
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
        public DataTable OracleReadDataToDataTable(string sSQLString)
        {
            //如果连接关闭则首先打开连接
            if (mOracleConnection.State == ConnectionState.Closed)
            {
                if (!OracleOpenConnection())
                {
                    return null;
                }
            }
            //定义数据表
            DataTable mDataTable = new DataTable();
            //定义执行命令
            OracleCommand mOracleCommand = new OracleCommand(sSQLString, mOracleConnection);
            OracleDataAdapter mOracleDataAdapter = new OracleDataAdapter(mOracleCommand);

            try
            {
                //填充数据集
                mOracleDataAdapter.Fill(mDataTable);
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
        public DataView OracleReadDataToDataView(string sSQLString)
        {
            //如果连接关闭则首先打开连接
            if (mOracleConnection.State == ConnectionState.Closed)
            {
                if (!OracleOpenConnection())
                {
                    return null;
                }
            }
            //定义数据视图
            DataView mDataView = new DataView();
            //定义数据集
            DataSet mDataSet = new DataSet();
            //定义执行命令
            OracleCommand mOracleCommand = new OracleCommand(sSQLString, mOracleConnection);
            OracleDataAdapter mOracleDataAdapter = new OracleDataAdapter(mOracleCommand);

            try
            {
                //填充数据集
                mOracleDataAdapter.Fill(mDataSet);
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
        /// 根据列索引和行索引获取字段值
        /// </summary>
        /// <param name="columnIndex">列索引</param>
        /// <param name="rowIndex">行索引</param>
        /// <param name="mDataSet">数据集</param>
        /// <returns>字段值</returns>
        public object OracleReadDataValue(int rowIndex, int columnIndex, DataSet mDataSet)
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
        public object OracleReadDataValue(int rowIndex, string columnName, DataSet mDataSet)
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

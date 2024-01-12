/*****************************************************************
 * Copyright(C), 2010, BUAA, ���������о���
 * �ļ�����:    ODBCOperation.cs
 * ����:        Liuwei
 * �汾:        1.0        
 * ��������:    2010.07.22
 * �������:    2010
 * �ļ�����:    ���ݿ�����ࡣ����ODBC�������ݿ⡣
 *              
 * ����:        ����ϵͳODBC�ռ�: System.Data.Odbc
 * 
 * �޸���ʷ:
 * 1.   �޸�����:
 *      �޸���:
 *      �޸Ĺ���:
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
    /// ODBC��ʽ�������ݿ���
    /// </summary>
    public class ODBCOperation
    {
        /// <summary>
        /// �����ַ���
        /// </summary>
        //private string m_sConnectionString = "Provider=MSDAORA.1;Data Source=(DESCRIPTION =(ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.2)(PORT = 1521)))(CONNECT_DATA = (SID = DEV)));Dsn=oracle;pwd=sa;Unicode=true";
        //private string m_sConnectionString = "Dsn=oracle;uid=god;pwd=sa";
        private string m_sConnectionString = "";

        /// <summary>
        /// �洢���ݿ�����(������,ֻ����������������ܷ���)
        /// </summary>
        protected OdbcConnection mOdbcConnection;

        /// <summary>
        /// �洢���ݿ������α���(������,ֻ����������������ܷ���)
        /// </summary>
        protected int cursor_used = 0;

        /// <summary>
        /// ��������ַ���
        /// </summary>
        private string m_sExceptionString = "";

        /// <summary>
        /// �麯��:���������Ϣ, ���������б���д
        /// </summary>
        /// <returns>��������ַ���</returns>
        public virtual string ExceptionOutput(Exception ex)
        {
            m_sExceptionString = ex.Message;
            return m_sExceptionString;
        }

        /// <summary>
        /// ���캯�������ݿ��Ĭ������
        /// </summary>
        public ODBCOperation()
        {
            ODBCSetDatabase(m_sConnectionString);
        }

        /// <summary>
        /// ���캯������������Դ������ַ���������ݿ�����
        /// </summary>
        public ODBCOperation(string sDBConnectionString)
        {
            ODBCSetDatabase(sDBConnectionString);
        }

        /// <summary>
        /// ��ȡ����Դ������ַ/���ݿ������ַ���
        /// </summary>
        /// <returns></returns>
        public string ODBCGetDBConnectionString()
        {
            return m_sConnectionString;
        }

        /// <summary>
        /// ��������Դ������ַ/���ݿ������ַ���
        /// </summary>
        /// <param name="sDBConnectionString">����Դ������ַ</param>
        public void ODBCSetDBConnectionString(string sDBConnectionString)
        {
            m_sConnectionString = sDBConnectionString;
        }

        /// <summary>
        /// �������ݿⲢ�������Ӷ���
        /// </summary>
        /// <param name="sDBConnectionString">����Դ����·��</param>
        public void ODBCSetDatabase(string sDBConnectionString)
        {
            m_sConnectionString = sDBConnectionString;

            //���Ӳ�Ϊ��ʱ���ͷ�������Դ
            if (mOdbcConnection != null)
            {
                mOdbcConnection.Dispose();
            }
            //����һ�����Ӷ���
            mOdbcConnection = new OdbcConnection(m_sConnectionString);/////////////////////////////
        }
        /// <summary>
        /// ��Oracle���ݿ�����
        /// </summary>
        /// <returns>�Ƿ�ɹ������ݿ�����</returns>
        public bool ODBCOpenConnection()
        {
            bool isConnSuccess = false;//�Ƿ����ӳɹ�
            if (mOdbcConnection.State == ConnectionState.Open)//������Ӵ�
            {
                mOdbcConnection.Close();
            }
            if (mOdbcConnection.State == ConnectionState.Closed)//������ӹر�
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
        /// �ر����ݿ�����
        /// </summary>
        /// <returns>�Ƿ�ɹ��ر����ݿ�����</returns>
        public bool ODBCCloseConnection()
        {
            bool isCloseConnSuccess = false;//�Ƿ����ӳɹ�
            if (mOdbcConnection.State == ConnectionState.Open)//������Ӵ�
            {
                try
                {
                    mOdbcConnection.Close();
                    //mOdbcConnection.Dispose();//...
                    isCloseConnSuccess = true;
                    //�α�����
                    cursor_used = 0;
                }
                catch (Exception e)
                {
                    ExceptionOutput(e);
                }
            }
            else if (mOdbcConnection.State == ConnectionState.Closed)//������ӹر�
            {
                isCloseConnSuccess = true;
            }
            return isCloseConnSuccess;
        }

        /// <summary>
        /// ִ��SQL�����ַ���,�����ؽ��
        /// </summary>
        /// <param name="sSQLString">SQL�����ַ���,����ΪUPDATE��INSERT �� DELETE �����</param>
        /// <returns>SQL�����Ƿ�ɹ�ִ��</returns>
        public bool ODBCExecuteSQLString(string sSQLString)
        {
            bool isExecuteSuccess = false;//�Ƿ����ӳɹ�

            //������ӹر������ȴ�����
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return isExecuteSuccess;
                }
            }
            //��������
            OdbcTransaction mOdbcTransaction = mOdbcConnection.BeginTransaction();
            //����ִ������
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection, mOdbcTransaction);
            try
            {
                // ʹ�� ExecuteNonQuery ִ�б�¼�����������ѯ���ݿ�Ľṹ�򴴽������ȵ����ݿ����
                // ��ͨ��ִ�� UPDATE��INSERT �� DELETE ���������ݿ��е����ݡ�
                // ��Ȼ ExecuteNonQuery �������κ��У�����ӳ�䵽�������κ���������򷵻�ֵ���������ݽ�����䡣
                // ���� UPDATE��INSERT �� DELETE ��䣬����ֵΪ��������Ӱ������������������������͵���䣬����ֵΪ -1��
                mOdbcCommand.ExecuteNonQuery();//ִ������
                mOdbcTransaction.Commit();//�ύ����
                //����
                mOdbcCommand.Dispose();
                mOdbcCommand = null;
                //����end
                isExecuteSuccess = true;
            }
            catch (Exception e)
            {
                try
                {
                    mOdbcTransaction.Rollback();//���Իع�����
                }
                catch
                {
                    // ʲôҲ����: transaction �ǻ
                }
                ExceptionOutput(e);
            }
            //����
            mOdbcTransaction.Dispose();
            mOdbcTransaction = null;
            //����end

            return isExecuteSuccess;
        }

        /// <summary>
        /// ִ��SQL�����ַ���,�����ؽ��
        /// </summary>
        /// <param name="sSQLString">SQL�����ַ���,����ΪUPDATE��INSERT �� DELETE �����</param>
        /// <returns>SQL�����Ƿ�ɹ�ִ��</returns>
        public bool ODBCExecuteSQLStringWithoutTrans(string sSQLString)
        {
            bool isExecuteSuccess = false;//�Ƿ����ӳɹ�

            //������ӹر������ȴ�����
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return isExecuteSuccess;
                }
            }
            //����ִ������
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection);
            try
            {
                // ʹ�� ExecuteNonQuery ִ�б�¼�����������ѯ���ݿ�Ľṹ�򴴽������ȵ����ݿ����
                // ��ͨ��ִ�� UPDATE��INSERT �� DELETE ���������ݿ��е����ݡ�
                // ��Ȼ ExecuteNonQuery �������κ��У�����ӳ�䵽�������κ���������򷵻�ֵ���������ݽ�����䡣
                // ���� UPDATE��INSERT �� DELETE ��䣬����ֵΪ��������Ӱ������������������������͵���䣬����ֵΪ -1��
                mOdbcCommand.ExecuteNonQuery();//ִ������
                //����
                mOdbcCommand.Dispose();
                mOdbcCommand = null;
                //����end
                isExecuteSuccess = true;
            }
            catch (Exception e)
            {
                ExceptionOutput(e);
            }

            return isExecuteSuccess;
        }

        /// <summary>
        /// ִ������SQL�����ַ���,�����ؽ��
        /// </summary>
        /// <param name="sSQLStrings">SQL�����ַ�����,����ΪUPDATE��INSERT �� DELETE �����</param>
        /// <param name="sSQLStringsCount">SQL�����ַ����������������</param>
        /// <returns>SQL�����Ƿ�ɹ�ִ��</returns>
        public bool ODBCExecuteSQLString(string[] sSQLStrings, int sSQLStringsCount)
        {
            bool isExecuteSuccess = true;//�Ƿ����ӳɹ�

            //������ӹر������ȴ�����
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return isExecuteSuccess;
                }
            }

            //��������
            OdbcTransaction mOdbcTransaction = mOdbcConnection.BeginTransaction();
            //����ִ������
            OdbcCommand mOdbcCommand = new OdbcCommand();
            mOdbcCommand.Connection = mOdbcConnection;
            mOdbcCommand.CommandType = CommandType.Text;
            mOdbcCommand.Transaction = mOdbcTransaction;
            try
            {
                for (int i = 0; i < sSQLStringsCount; ++i)
                {
                    mOdbcCommand.CommandText = sSQLStrings[i];
                    // ʹ�� ExecuteNonQuery ִ�б�¼�����������ѯ���ݿ�Ľṹ�򴴽������ȵ����ݿ����
                    // ��ͨ��ִ�� UPDATE��INSERT �� DELETE ���������ݿ��е����ݡ�
                    // ��Ȼ ExecuteNonQuery �������κ��У�����ӳ�䵽�������κ���������򷵻�ֵ���������ݽ�����䡣
                    // ���� UPDATE��INSERT �� DELETE ��䣬����ֵΪ��������Ӱ������������������������͵���䣬����ֵΪ -1��
                    mOdbcCommand.ExecuteNonQuery();//ִ������
                }
                mOdbcTransaction.Commit();//�ύ����
            }
            catch (Exception e)
            {
                mOdbcTransaction.Rollback();//���Իع�����
                isExecuteSuccess = false;
                ExceptionOutput(e);
            }

            return isExecuteSuccess;
        }
        
        /// <summary>
        /// ִ��SQL��䷵�ؽ����DataReader��
        /// </summary>
        /// <param name="sSQLString">SQL�����ַ���</param>
        /// <returns>����ִ�н����OdbcDataReader���ݼ���</returns>
        public OdbcDataReader ODBCReadDataToDataReader(string sSQLString)
        {
            //������ӹر������ȴ�����
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //����ִ������
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection);
            OdbcDataReader mOdbcDataReader;
            try
            {
                //ִ�ж�ȡ����
                mOdbcDataReader = mOdbcCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                mOdbcDataReader = null;
                ExceptionOutput(e);
            }

            //����
            mOdbcCommand.Dispose();
            mOdbcCommand = null;
            //����end

            return mOdbcDataReader;

            //mOdbcDataReader.Close();//ʹ��ʱҪ���ùرպ���
        }

        /// <summary>
        /// ִ�� "SELECT COUNT(*) FROM table name" ���͵�SQL���������������������
        /// </summary>
        /// <param name="sSQLString">SQL�����ַ���</param>
        /// <returns>���ؽ������ -1:�޷������ݿ������ӻ����ݼ�Ϊ��; >=0:���ص�����</returns>
        public int ODBCReadDataToCount(string sSQLString)
        {
            int sqlResultCount = 0;
            //������ӹر������ȴ�����
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return -1;
                }
            }

            //�������ݼ�
            DataSet mDataSet = new DataSet();
            //����ִ������
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection);
            OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter(mOdbcCommand);
            try
            {
                mOdbcDataAdapter.Fill(mDataSet, "objDataSet");//������ݼ�
            }
            catch (Exception e)
            {
                mDataSet = null;
                ExceptionOutput(e);
            }
            //���ݼ�Ϊ���򷵻�
            if (mDataSet == null)
            {
                return -1;
            }

            //ȡ�õ�һ���ֶε�ֵ����sqlResultCount����
            sqlResultCount = Convert.ToInt32(mDataSet.Tables[0].Rows[0].ItemArray[0].ToString());

            //����
            mOdbcDataAdapter.Dispose();
            mOdbcCommand.Dispose();
            mDataSet.Clear();
            mDataSet.Reset();
            //����end

            return sqlResultCount;
        }

        /// <summary>
        /// ִ��SQL��䷵�ؽ����DataSet��
        /// </summary>
        /// <param name="sSQLString">SQL�����ַ���</param>
        /// <returns>����ִ�н����DataSet</returns>
        public DataSet ODBCReadDataToDataSet(string sSQLString)
        {
            //������ӹر������ȴ�����
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //�������ݼ�
            DataSet mDataSet = new DataSet();
            //����ִ������
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection);
            OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter(mOdbcCommand);

            //����ִ������ - �¾�Ҳ����
            //OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter(sSQLString, mOdbcConnection);

            try
            {
                //������ݼ�
                mOdbcDataAdapter.Fill(mDataSet, "objDataSet");
            }
            catch (Exception e)
            {
                mDataSet = null;
                ExceptionOutput(e);
            }

            //����
            mOdbcDataAdapter.Dispose();
            mOdbcCommand.Dispose();
            //����end

            return mDataSet;
        }

        /// <summary>
        /// ִ��SQL��䷵�ؽ����DataSet��
        /// </summary>
        /// <param name="sSQLStrings">SQL�����ַ�������,����ΪUPDATE��INSERT �� DELETE �������</param>
        /// <param name="sSQLStringsCount">SQL�����ַ�����ִ���������</param>
        /// <returns>����ִ�н����DataSet</returns>
        public DataSet[] ODBCReadDataToDataSet(string[] sSQLStrings, int sSQLStringsCount)
        {
            //������ӹر������ȴ�����
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //�������ݼ�
            DataSet[] mDataSet = new DataSet[sSQLStringsCount];
            for (int i = 0; i < sSQLStringsCount; ++i)
            {
                mDataSet[i] = new DataSet();
            }
            //����ִ������
            OdbcCommand mOdbcCommand = new OdbcCommand();
            mOdbcCommand.Connection = mOdbcConnection;
            OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter();

            //����ִ������ - �¾�Ҳ����
            //OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter(sSQLString, mOdbcConnection);
            try
            {
                for (int i = 0; i < sSQLStringsCount; ++i)
                {
                    mOdbcCommand.CommandText = sSQLStrings[i];
                    mOdbcDataAdapter.SelectCommand = mOdbcCommand;
                    //������ݼ�
                    mOdbcDataAdapter.Fill(mDataSet[i], "objDataSet" + i.ToString());
                }
            }
            catch (Exception e)
            {
                mDataSet = null;
                ExceptionOutput(e);
            }

            //����
            mOdbcDataAdapter.Dispose();
            mOdbcCommand.Dispose();
            //����end

            return mDataSet;
        }

        /// <summary>
        /// ִ��SQL��䷵�ؽ����DataTable��
        /// </summary>
        /// <param name="sSQLString">SQL�����ַ���</param>
        /// <returns>����ִ�н����DataTable</returns>
        public DataTable ODBCReadDataToDataTable(string sSQLString)
        {
            //������ӹر������ȴ�����
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //�������ݱ�
            DataTable mDataTable = new DataTable();
            //����ִ������
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection);
            OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter(mOdbcCommand);

            try
            {
                //������ݼ�
                mOdbcDataAdapter.Fill(mDataTable);
            }
            catch (Exception e)
            {
                mDataTable = null;
                ExceptionOutput(e);
            }

            //����
            mOdbcDataAdapter.Dispose();
            mOdbcCommand.Dispose();
            //����

            return mDataTable;
        }

        /// <summary>
        /// ִ��SQL��䷵�ؽ����DataView��
        /// </summary>
        /// <param name="sSQLString">SQL�����ַ���</param>
        /// <returns>����ִ�н����DataView</returns>
        public DataView ODBCReadDataToDataView(string sSQLString)
        {
            //������ӹر������ȴ�����
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //����������ͼ
            DataView mDataView = new DataView();
            //�������ݼ�
            DataSet mDataSet = new DataSet();
            //����ִ������
            OdbcCommand mOdbcCommand = new OdbcCommand(sSQLString, mOdbcConnection);
            OdbcDataAdapter mOdbcDataAdapter = new OdbcDataAdapter(mOdbcCommand);

            try
            {
                //������ݼ�
                mOdbcDataAdapter.Fill(mDataSet);
                mDataView = mDataSet.Tables[0].DefaultView;
            }
            catch (Exception e)
            {
                mDataView = null;
                ExceptionOutput(e);
            }
            
            //����
            mOdbcCommand.Dispose();
            mOdbcDataAdapter.Dispose();
            mDataSet.Dispose();
            mDataSet.Reset();
            //����

            return mDataView;
        }

        /// <summary>
        /// ��ȡ���ݱ�������е�����
        /// </summary>
        /// <param name="sTableName">����</param>
        /// <returns>��������</returns>
        public string[] ODBCReadDataColumnsName(string sTableName)
        {
            //������ӹر������ȴ�����
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //����ִ������
            OdbcCommand mOdbcCommand = new OdbcCommand("SELECT * FROM " + sTableName + " WHERE 0=1", mOdbcConnection);
            OdbcDataReader mOdbcDataReader;
            try
            {
                //ִ�ж�ȡ����
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
            
            mOdbcDataReader.Close();//ʹ��ʱҪ���ùرպ���
            //����
            mOdbcCommand.Dispose();
            mOdbcCommand = null;
            mOdbcDataReader.Dispose();
            //����end

            return sColumns;
        }

        /// <summary>
        /// ��ȡ���ݱ�������е�����
        /// </summary>
        /// <param name="sTableName">����</param>
        /// <returns>��������</returns>
        public string[] ODBCReadDataColumnsType(string sTableName)
        {
            //������ӹر������ȴ�����
            if (mOdbcConnection.State == ConnectionState.Closed)
            {
                if (!ODBCOpenConnection())
                {
                    return null;
                }
            }
            //����ִ������
            OdbcCommand mOdbcCommand = new OdbcCommand("SELECT * FROM " + sTableName + " WHERE 0=1", mOdbcConnection);
            OdbcDataReader mOdbcDataReader;
            try
            {
                //ִ�ж�ȡ����
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

            mOdbcDataReader.Close();//ʹ��ʱҪ���ùرպ���
            //����
            mOdbcCommand.Dispose();
            mOdbcCommand = null;
            mOdbcDataReader.Dispose();
            //����end

            return sColumnsType;

        }

        /// <summary>
        /// ��������������������ȡ�ֶ�ֵ
        /// </summary>
        /// <param name="columnIndex">������</param>
        /// <param name="rowIndex">������</param>
        /// <param name="mDataSet">���ݼ�</param>
        /// <returns>�ֶ�ֵ</returns>
        public static object ODBCReadDataValue(int rowIndex, int columnIndex, DataSet mDataSet)
        {
            if ((mDataSet == null) || (0 == mDataSet.Tables[0].Rows.Count))
            {
                return null;
            }
            //��ȡָ��λ�õ�ֵ
            object resultValue = mDataSet.Tables[0].Rows[rowIndex].ItemArray[columnIndex];

            return resultValue;
        }

        /// <summary>
        /// ���������ƺ���������ȡ�ֶ�ֵ
        /// </summary>
        /// <param name="columnName">������</param>
        /// <param name="rowIndex">������</param>
        /// <param name="mDataSet">���ݼ�</param>
        /// <returns>�ֶ�ֵ</returns>
        public static object ODBCReadDataValue(int rowIndex, string columnName, DataSet mDataSet)
        {
            if (mDataSet == null || (0 == mDataSet.Tables[0].Rows.Count))
            {
                return null;
            }
            //��ȡָ��λ�õ�ֵ
            object resultValue = mDataSet.Tables[0].Rows[rowIndex][columnName];

            return resultValue;
        }
    }
}

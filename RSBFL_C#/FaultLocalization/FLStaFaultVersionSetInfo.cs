using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaultLocalization
{
    public struct FLStaFaultVersionName
    {
        public string suiteName;
        public string programName;
        public string versionName;
    }

    public class FLStaFaultVersionSetInfo
    {
        #region 名称
        protected FLStaFaultVersionName m_Name;

        /// <summary>
        /// 获取或设置试验包名称
        /// </summary>
        public string SuiteName
        {
            get { return m_Name.suiteName; }
            set { m_Name.suiteName = value; }
        }

        /// <summary>
        /// 获取或设置实验程序名称
        /// </summary>
        public string ProgramName
        {
            get { return m_Name.programName; }
            set { m_Name.programName = value; }
        }

        /// <summary>
        /// 获取或设置缺陷版本名称
        /// </summary>
        public string VersionName
        {
            get { return m_Name.versionName; }
            set { m_Name.versionName = value; }
        }

        public string FullName
        {
            get { return GetFullName("\\"); }
        }
        /// <summary>
        /// 获取缺陷版本的全称
        /// </summary>
        /// <param name="saparator">分隔符</param>
        /// <returns>全称</returns>
        public string GetFullName(string saparator)
        {
            return SuiteName + saparator + ProgramName + saparator + VersionName;
        }
        #endregion

        protected List<FLStaFault> m_Faults = new List<FLStaFault>();
        /// <summary>
        /// 获取或设置缺陷列表
        /// </summary>
        public List<FLStaFault> Faults
        {
            get { return m_Faults; }
            set { m_Faults = value; }
        }
        /// <summary>
        /// 缺陷数量
        /// </summary>
        public int NumFaults
        {
            get { return m_Faults.Count; }
        }

        // 构造函数
        public FLStaFaultVersionSetInfo(string suiteName, string programName, string versionName, List<FLStaFault>  faultList)
        {
            SuiteName = suiteName;
            ProgramName = programName;
            VersionName = versionName;

            m_Faults = faultList;
        }

        public void MapLineNumber2ID(string sLineMap)
        {
            for (int faultIndex = 0; faultIndex < NumFaults; faultIndex++)
            {
                for (int statementIndex = 0; statementIndex < m_Faults[faultIndex].FaultyStatements.Count; statementIndex++)
                {
                    m_Faults[faultIndex].FaultyStatements[statementIndex].LineNumber2ID(sLineMap);
                }
            }
        }

        public List<int> GetAllFaultyStatementsID()
        {
            List<int> result = new List<int>();

            for (int i = 0; i < NumFaults; i++)
            {
                result.AddRange(GetFaultyStatementsIDof(m_Faults[i]));
            }

            return result;
        }

        public List<int> GetFaultyStatementsIDof(FLStaFault fault)
        {
            List<int> result = new List<int>();

            for (int i = 0; i < fault.FaultyStatements.Count; i++)
            {
                result.Add(fault.FaultyStatements[i].ID);
            }

            return result;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaultLocalization
{
    public class FLStaFault
    {
        private string m_FaultName = string.Empty;
        /// <summary>
        /// 缺陷名称
        /// </summary>
        public string FaultName
        {
            get { return m_FaultName; }
            set { m_FaultName = value; }
        }

        private List<FLStatement> m_FaultyStatements = new List<FLStatement>();
        /// <summary>
        /// 缺陷语句列表
        /// </summary>
        public List<FLStatement> FaultyStatements
        {
            get { return m_FaultyStatements; }
            set { m_FaultyStatements = value; }
        }

        private string m_Description = string.Empty;
        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }
    }
}

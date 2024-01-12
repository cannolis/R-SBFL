/**********************************************************************
 * 
 *  SPStaLocationEffort.cs
 *  
 *  功能： 记录了计算定位效率的必要信息包括
 *         1) 算法名称
 *         2) 最好，最差，平均和绝对排位
 *         3) 以及和排位相对性计算出的expense
 * 
 * **********************************************GaoYichao.2013.08.12**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaultLocalization
{
    /// <summary>
    /// 实验结果
    /// </summary>
    public class FLStaLocationEffort
    {
        /// <summary>
        /// 算法名称
        /// </summary>
        private string m_AlgorithmName = null;
        /// <summary>
        /// 获取或设置算法名称
        /// </summary>
        public string AlgorithmName
        {
            get { return m_AlgorithmName; }
            set { m_AlgorithmName = value; }
        }

        private string m_ExperimentDiscription = string.Empty;
        /// <summary>
        /// 获取或设置实验模式
        /// </summary>
        public string ExperimentDiscription
        {
            get { return m_ExperimentDiscription; }
            set { m_ExperimentDiscription = value; }
        }

        #region 缺陷语句的排位
        /// <summary>
        /// 最优排位
        /// </summary>
        private int m_BestSort = -1;
        /// <summary>
        /// 获取或设置最优排位
        /// </summary>
        public int BestSort
        {
            get { return m_BestSort; }
            set { m_BestSort = value; }
        }

        /// <summary>
        /// 最次排位
        /// </summary>
        private int m_WorstSort = -1;
        /// <summary>
        /// 获取或设置最次排位
        /// </summary>
        public int WorstSort
        {
            get { return m_WorstSort; }
            set { m_WorstSort = value; }
        }

        /// <summary>
        /// 平均排位
        /// </summary>
        private int m_AveSort = -1;
        /// <summary>
        /// 获取或设置平均排位
        /// </summary>
        public int AveSort
        {
            get { return m_AveSort; }
            set { m_AveSort = value; }
        }

        /// <summary>
        /// 绝对排位
        /// </summary>
        private int m_AbsSort = -1;
        /// <summary>
        /// 获取或设置绝对排位
        /// </summary>
        public int AbsSort
        {
            get { return m_AbsSort; }
            set { m_AbsSort = value; }
        }
        #endregion

        #region 定位expense
        /// <summary>
        /// 至少expense
        /// </summary>
        private double m_LeastExpense = -1.0;
        /// <summary>
        /// 获取或设置至少expense
        /// </summary>
        public double LeastExpense
        {
            get { return m_LeastExpense; }
            set { m_LeastExpense = value; }
        }
        /// <summary>
        /// 最多expense
        /// </summary>
        private double m_MostExpense = -1.0;
        /// <summary>
        /// 获取或设置最多expense
        /// </summary>
        public double MostExpense
        {
            get { return m_MostExpense; }
            set { m_MostExpense = value; }
        }
        /// <summary>
        /// 平均expense
        /// </summary>
        private double m_AveExpense = -1.0;
        /// <summary>
        /// 获取或设置平均expense
        /// </summary>
        public double AveExpense
        {
            get { return m_AveExpense; }
            set { m_AveExpense = value; }
        }
        /// <summary>
        /// 绝对计算expense
        /// </summary>
        private double m_AbsExpense = -1.0;
        /// <summary>
        /// 获取或设置绝对计算的expense
        /// </summary>
        public double AbsExpense
        {
            get { return m_AbsExpense; }
            set { m_AbsExpense = value; }
        }
        #endregion 
    }

    /// <summary>
    /// 多次重复实验的统计结果
    /// </summary>
    public class FLStaLocationEffortStatic
    {
        /// <summary>
        /// 算法名称
        /// </summary>
        private string m_AlgorithmName = string.Empty;
        /// <summary>
        /// 获取或设置算法名称
        /// </summary>
        public string AlgorithmName
        {
            get { return m_AlgorithmName; }
            set { m_AlgorithmName = value; }
        }

        private string m_ExperimentDiscription = string.Empty;
        /// <summary>
        /// 获取或设置实验模式
        /// </summary>
        public string ExperimentDiscription
        {
            get { return m_ExperimentDiscription; }
            set { m_ExperimentDiscription = value; }
        }

        #region 定位expense
        /// <summary>
        /// 至少expense
        /// </summary>
        private double m_LeastExpense = -1.0;
        /// <summary>
        /// 获取或设置至少expense
        /// </summary>
        public double LeastExpense
        {
            get { return m_LeastExpense; }
            set { m_LeastExpense = value; }
        }
        /// <summary>
        /// 最多expense
        /// </summary>
        private double m_MostExpense = -1.0;
        /// <summary>
        /// 获取或设置最多expense
        /// </summary>
        public double MostExpense
        {
            get { return m_MostExpense; }
            set { m_MostExpense = value; }
        }
        /// <summary>
        /// 平均expense
        /// </summary>
        private double m_AveExpense = -1.0;
        /// <summary>
        /// 获取或设置平均expense
        /// </summary>
        public double AveExpense
        {
            get { return m_AveExpense; }
            set { m_AveExpense = value; }
        }
        /// <summary>
        /// 绝对计算expense
        /// </summary>
        private double m_AbsExpense = -1.0;
        /// <summary>
        /// 获取或设置绝对计算的expense
        /// </summary>
        public double AbsExpense
        {
            get { return m_AbsExpense; }
            set { m_AbsExpense = value; }
        }
        #endregion 

        #region 定位expense方差

        /// <summary>
        /// 至少expenseVariance
        /// </summary>
        private double m_LeastExpenseVariance = -1.0;
        /// <summary>
        /// 获取或设置至少expenseVariance
        /// </summary>
        public double LeastExpenseVariance
        {
            get { return m_LeastExpenseVariance; }
            set { m_LeastExpenseVariance = value; }
        }
        /// <summary>
        /// 最多expenseVariance
        /// </summary>
        private double m_MostExpenseVariance = -1.0;
        /// <summary>
        /// 获取或设置最多expenseVariance
        /// </summary>
        public double MostExpenseVariance
        {
            get { return m_MostExpenseVariance; }
            set { m_MostExpenseVariance = value; }
        }
        /// <summary>
        /// 平均expenseVariance
        /// </summary>
        private double m_AveExpenseVariance = -1.0;
        /// <summary>
        /// 获取或设置平均expenseVariance
        /// </summary>
        public double AveExpenseVariance
        {
            get { return m_AveExpenseVariance; }
            set { m_AveExpenseVariance = value; }
        }
        /// <summary>
        /// 绝对计算expenseVariance
        /// </summary>
        private double m_AbsExpenseVariance = -1.0;
        /// <summary>
        /// 获取或设置绝对计算的expenseVariance
        /// </summary>
        public double AbsExpenseVariance
        {
            get { return m_AbsExpenseVariance; }
            set { m_AbsExpenseVariance = value; }
        }

        #endregion
    }

}

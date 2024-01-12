using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using PlanningAlgorithmInterface.Socket4DataTrans;

namespace FaultLocalization
{
    public class FLConfigure
    {
        /// <summary>
        /// 数据源
        /// </summary>
        public DirectoryInfo DataRootInfo = null;

        public string Experiment = null;
        /// <summary>
        /// 程序包名称
        /// </summary>
        public string SuiteName = null;
        /// <summary>
        /// 程序名称
        /// </summary>
        public string ProgramName = null;
        /// <summary>
        /// 缺陷版本名称
        /// </summary>
        public string VersionName = null;

        #region 变更部分测试用例类别相关配置
        /// <summary>
        /// 变更类别的用例比例
        /// </summary>
        public double ClassChangeRatio = 0.0;
        /// <summary>
        /// 成例占所有变更用例的比例,若为负数则不区分成例失例
        /// </summary>
        public double ChangeSucRatio = -0.5;
        /// <summary>
        /// 变更为失例的用例数量最大占原失例数的比例
        /// </summary>
        public double MaxClassChangeRatioInOriginal = Double.MaxValue;
        /// <summary>
        /// 变更类别用例的选取策略描述
        /// </summary>
        public string ClassChangeSelectStrategy = null;
        /// <summary>
        /// 最少的测试用例数量
        /// </summary>
        public int MinRuns = 1;
        /// <summary>
        /// 最少的失例数量
        /// </summary>
        public int MinSucRuns = 1;
        /// <summary>
        /// 最少的成例数量
        /// </summary>
        public int MinFalRuns = 1;
        /// <summary>
        /// 最小类别比例(成例数量:失例数量)
        /// </summary>
        public double MinClassRatio = -1;
        /// <summary>
        /// 最大类别比例(成例数量:失例数量)
        /// </summary>
        public double MaxClassRatio = -1;
        #endregion

        #region 选取部分测试用例相关配置
        /// <summary>
        /// 使用测试用例比例,(0,1]
        /// </summary>
        public double TestCasePersent = 1.0;
        /// <summary>
        /// 用例选取策略描述
        /// </summary>
        public string TestCaseSelectStrategy = null;
        #endregion

        #region 集成相关配置
        /// <summary>
        /// 拆分类别比例(成例数量:失例数量)，用于集成算法
        /// </summary>
        public double ClassRatio = 1.0;
        /// <summary>
        /// 按类别比例拆分用例策略
        /// </summary>
        public string ClassRatioDivideStrategy = null;
        /// <summary>
        /// 集成时权值公式索引
        /// </summary>
        public int WeightFormulaId = 0;
        /// <summary>
        /// 集成加权核
        /// </summary>
        public string IntegrateKernel = null;
        #endregion

        /// <summary>
        /// 重复实验次数
        /// </summary>
        public int RepeatTimes = 10;

        #region 李成龙添加 客户端服务端通信相关配置
        public IPAddress serverIP { get; set; }
        public Int32 port { get; set; }

        public SocketClient client;
        #endregion

        /// <summary>
        /// 李成龙添加 源数据中缺陷语句是否被插桩
        /// </summary>
        public bool isInstrumented = true;
    }
}

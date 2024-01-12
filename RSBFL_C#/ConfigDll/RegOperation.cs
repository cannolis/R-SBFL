/**************************************************************************************************
 * Copyright(C), 2010, BUAA, 软件与控制研究室
 * 文件名称:    RegOperation.cs
 * 作者:        Liuwei
 * 版本:        1.0        
 * 创建日期:    2010.08.11
 * 完成日期:    2010
 * 文件描述:    注册表操作类。
 *              
 * 引用:        Microsoft.Win32;
 * 说明:        NET框架在Microsoft.Win32名字空间中提供了两个类来操作注册表：Registry和RegistryKey。
 *              这两个类都是密封类不允许被继承。
 *              Registry类提供了7个公共的静态域,分别代表7个基本主键(其中两个在XP系统中没有)分别是：
 *              Registry.ClassesRoot 对应于HKEY_CLASSES_ROOT主键
 *              Registry.CurrentUser 对应于HKEY_CURRENT_USER主键
 *              Registry.LocalMachine 对应于 HKEY_LOCAL_MACHINE主键
 *              Registry.User 对应于 HKEY_USER主键
 *              Registry.CurrentConfig 对应于HEKY_CURRENT_CONFIG主键
 *              Registry.DynDa 对应于HKEY_DYN_DATA主键  (XP系统中没有)
 *              Registry.PerformanceData 对应于HKEY_PERFORMANCE_DATA主键    (XP系统中没有)
 *              RegistryKey类中提供了对注册表操作的方法。要注意的是操作注册表必须符合系统权限，否则
 *                  将会抛出错误。
 * 
 * 修改历史:
 * 1.   修改日期:
 *      修改人:
 *      修改功能:
 * 2.   
***************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.ComponentModel;

namespace ConfigDll
{
    /// <summary>
    /// 注册表操作类
    /// </summary>
    public class RegOperation
    {
        /// <summary>
        /// 注册表键枚举值
        /// </summary>
        public enum RegistryKeyEnum : byte 
        {
            /// <summary>
            /// ClassesRoot
            /// </summary>
            [Description("ClassesRoot")]
            ClassesRoot,
            /// <summary>
            /// CurrentUser
            /// </summary>
            [Description("CurrentUser")]
            CurrentUser,
            /// <summary>
            /// LocalMachine
            /// </summary>
            [Description("LocalMachine")]
            LocalMachine,
            /// <summary>
            /// Users
            /// </summary>
            [Description("Users")]
            Users,
            /// <summary>
            /// CurrentConfig
            /// </summary>
            [Description("CurrentConfig")]
            CurrentConfig,
            /// <summary>
            /// DynData
            /// </summary>
            [Description("DynData")]
            DynData,
            /// <summary>
            /// PerformanceData
            /// </summary>
            [Description("PerformanceData")]
            PerformanceData 
        };

        /// <summary>
        /// 获取注册表主键
        /// </summary>
        /// <param name="eRegistryKeyEnum"></param>
        /// <returns></returns>
        public static RegistryKey GetRegistryKey(RegistryKeyEnum eRegistryKeyEnum)
        {
            RegistryKey regk = null;
            switch (eRegistryKeyEnum)
            {
                case RegistryKeyEnum.ClassesRoot: regk = Registry.ClassesRoot; break;
                case RegistryKeyEnum.CurrentUser: regk = Registry.CurrentUser; break;
                case RegistryKeyEnum.LocalMachine: regk = Registry.LocalMachine; break;
                case RegistryKeyEnum.Users: regk = Registry.Users; break;
                case RegistryKeyEnum.CurrentConfig: regk = Registry.CurrentConfig; break;
                case RegistryKeyEnum.DynData: regk = Registry.DynData; break;
                case RegistryKeyEnum.PerformanceData: regk = Registry.PerformanceData; break;
            }
            return regk;
        }

        /// <summary>
        /// 判断指定注册表项是否存在
        /// </summary>
        /// <param name="mRegistryKey">要在其中搜索的主键</param>
        /// <param name="sSubKey">要在其中搜索的子键,格式如:"SOFTWARE\\SUBKEY\\SUBKEY"</param>
        /// <param name="sRegName">要搜索的项名称</param>
        public static bool RegeditKeyIsExist(RegistryKey mRegistryKey, string sSubKey, string sRegName)
        {
            try
            {
                bool exit = false;
                string[] subkeyNames;
                RegistryKey subkey = mRegistryKey.OpenSubKey(sSubKey, true); 
                subkeyNames = subkey.GetValueNames();
                foreach (string keyName in subkeyNames)
                {
                    if (keyName == sRegName)
                    {
                        exit = true;
                        return exit;
                    }
                }
                return exit;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断指定注册表子目录项是否存在
        /// </summary>
        /// <param name="mRegistryKey">要在其中搜索的主键</param>
        /// <param name="sSubKey">要在其中搜索的子键,格式如:"SOFTWARE\\SUBKEY\\SUBKEY"</param>
        public static bool RegeditSubKeyIsExist(RegistryKey mRegistryKey, string sSubKey)
        {
            try
            {
                RegistryKey subkey = mRegistryKey.OpenSubKey(sSubKey, true);
                if ( null == subkey)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 创建新注册表项
        /// </summary>
        /// <param name="mRegistryKey">主键</param>
        /// <param name="sSubKey">要创建的子键,格式如:"SOFTWARE\\SUBKEY\\SUBKEY"</param>
        /// <param name="sRegName">项名称</param>
        /// <param name="sRegValue">项值</param>
        public static bool AddRegeditKey(RegistryKey mRegistryKey, string sSubKey, string sRegName, string sRegValue)
        {
            try
            {
                if (!RegeditSubKeyIsExist(mRegistryKey, sSubKey))
                {
                    mRegistryKey.CreateSubKey(sSubKey);
                }
                if (!RegeditKeyIsExist(mRegistryKey, sSubKey, sRegName))
                {
                    RegistryKey subkey = mRegistryKey.OpenSubKey(sSubKey, true);//true表示可以修改
                    subkey.SetValue(sRegName, sRegValue);
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 修改注册表项
        /// </summary>
        /// <param name="mRegistryKey">主键</param>
        /// <param name="sSubKey">要修改的子键,格式如:"SOFTWARE\\SUBKEY\\SUBKEY"</param>
        /// <param name="sRegName">项名称</param>
        /// <param name="sRegValue">项值</param>
        public static bool ModifyRegeditKey(RegistryKey mRegistryKey, string sSubKey, string sRegName, string sRegValue)
        {
            try
            {
                if (RegeditKeyIsExist(mRegistryKey, sSubKey, sRegName))
                {
                    RegistryKey subkey = mRegistryKey.OpenSubKey(sSubKey, true);//true表示可以修改
                    subkey.SetValue(sRegName, sRegValue);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 删除注册表项
        /// </summary>
        /// <param name="mRegistryKey">主键</param>
        /// <param name="sSubKey">要删除的子键,格式如:"SOFTWARE\\SUBKEY\\SUBKEY"</param>
        /// <param name="sRegName">项名称</param>
        /// <param name="sRegValue">项值</param>
        public static bool DeleteRegeditKey(RegistryKey mRegistryKey, string sSubKey, string sRegName, string sRegValue)
        {
            try
            {
                RegistryKey subkey = mRegistryKey.OpenSubKey(sSubKey, true);//true表示可以修改
                subkey.DeleteValue(sRegName, true);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

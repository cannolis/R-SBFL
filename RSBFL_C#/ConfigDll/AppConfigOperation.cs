/**************************************************************
 * Copyright(C), 2010, BUAA, 软件与控制研究室
 * 文件名称:    AppConfigOperation.cs
 * 作者:        Liuwei
 * 版本:        1.0        
 * 创建日期:    2010.08.11
 * 完成日期:    2010
 * 文件描述:    应用程序配置项操作类。
 *              
 * 引用:        using System.Configuration
 * 
 * 修改历史:
 * 1.   修改日期:
 *      修改人:
 *      修改功能:
 * 2.   
***************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ConfigDll
{
    /// <summary>
    /// 应用程序配置操作类
    /// </summary>
    public class AppConfigOperation
    {
        /// <summary>
        /// 增加配置项
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">键值</param>
        /// <returns>是否增加成功</returns>
        public static bool AddConfigurationItem(string key, string value)
        {
            //打开默认应用程序配置文件
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //读取appSettings的所有键
            AppSettingsSection appSettingSection = config.AppSettings; //(AppSettingsSection)config.GetSection("appSettings");

            //如果无法读取则返回
            if (null == appSettingSection)
            {
                return false;
            }

            //判断键是否存在
            bool isExist = false;
            foreach (string tmpKey in appSettingSection.Settings.AllKeys)
            {
                if (tmpKey == key)
                {
                    isExist = true;
                    break;
                }
            }
            if (!isExist)
            {
                //增加一项
                appSettingSection.Settings.Add(key, value);
                //保存增加内容
                config.Save(ConfigurationSaveMode.Modified);
                //刷新
                ConfigurationManager.RefreshSection("appSettings");

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 修改配置项
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="newValue">更新的键值</param>
        /// <returns>是否更新成功</returns>
        public static bool UpdateConfigurationItemValue(string key, string newValue)
        {
            //打开默认应用程序配置文件
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //读取appSettings的所有键
            AppSettingsSection appSettingSection = config.AppSettings; //(AppSettingsSection)config.GetSection("appSettings");

            //如果无法读取则返回
            if (null == appSettingSection)
            {
                return false;
            }

            //判断键是否存在
            bool isExist = false;
            foreach (string tmpKey in appSettingSection.Settings.AllKeys)
            {
                if (tmpKey == key)
                {
                    isExist = true;
                    break;
                }
            }
            if (isExist)
            {
                //修改指定键的值
                appSettingSection.Settings[key].Value = newValue;
                //保存增加内容
                config.Save(ConfigurationSaveMode.Modified);
                //刷新
                ConfigurationManager.RefreshSection("appSettings");

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 删除配置项
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public static bool DeleteConfigurationItem(string key)
        {
            //打开默认应用程序配置文件
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //读取appSettings的所有键
            AppSettingsSection appSettingSection = config.AppSettings; //(AppSettingsSection)config.GetSection("appSettings");

            //如果无法读取则返回
            if (null == appSettingSection)
            {
                return false;
            }

            //判断键是否存在
            bool isExist = false;
            foreach (string tmpKey in appSettingSection.Settings.AllKeys)
            {
                if (tmpKey == key)
                {
                    isExist = true;
                    break;
                }
            }
            if (isExist)
            {
                appSettingSection.Settings.Remove(key);
                //保存增加内容
                config.Save(ConfigurationSaveMode.Modified);
                //刷新
                ConfigurationManager.RefreshSection("appSettings");

                return true;
            }
            else
            {
                return false;
            }
            
        }

        /// <summary>
        /// 取得配置文件appSettings里的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public static string GetConfigurationValue(string key)
        {
            //打开默认应用程序配置文件
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //读取appSettings的所有键
            AppSettingsSection appSettingSection = config.AppSettings; //(AppSettingsSection)config.GetSection("appSettings");

            //如果无法读取则返回
            if (null == appSettingSection)
            {
                return "";
            }

            //判断键是否存在
            bool isExist = false;
            foreach (string tmpKey in appSettingSection.Settings.AllKeys)
            {
                if (tmpKey == key)
                {
                    isExist = true;
                    break;
                }
            }
            if (isExist)
            {
                //返回读取值
                return ConfigurationManager.AppSettings[key];//or: return appSettingSection.Settings[key].Value;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 取得配置文件appSettings里所有的键(用数组表示)
        /// </summary>
        /// <returns>字符串数组</returns>
        public static string[] GetConfigurationItemList()
        {
            //打开默认应用程序配置文件
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //读取appSettings的所有键
            AppSettingsSection appSettingSection = config.AppSettings; //(AppSettingsSection)config.GetSection("appSettings");

            //如果无法读取则返回
            if (null == appSettingSection)
            {
                return null;
            }

            KeyValueConfigurationCollection kvCongigCollection = appSettingSection.Settings;

            if (null == kvCongigCollection)
            {
                return null;
            }

            return kvCongigCollection.AllKeys;
        }


    }
}

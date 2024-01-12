/************************************************************************************************
 * Copyright(C), 2010, BUAA, 软件与控制研究室
 * 文件名称:    IniOperation.cs
 * 作者:        Liuwei
 * 版本:        1.0        
 * 创建日期:    2010.08.11
 * 完成日期:    2010
 * 文件描述:    。
 *              
 * 引用:        System.Runtime.InteropServices
 * 函数:        1, WritePrivateProfileString:
 *                  功能：将信息写入ini文件
 *                  返回值：long,如果为0则表示写入失败，反之成功。
 *                  参数1(section):写入ini文件的某个小节名称（不区分大小写）。
 *                  参数2(key):上面section下某个项的键名(不区分大小写)。
 *                  参数3(val):上面key对应的value
 *                  参数4(filePath):ini的文件名，包括其路径(example: "c:\config.ini")。
 *                      如果没有指定路径，仅有文件名，系统会自动在windows目录中查找是否有
 *                      对应的ini文件，如果没有则会自动在当前应用程序运行的根目录下创建ini文件。
 *              2, GetPrivateProfileString
 *                  功能：从ini文件中读取相应信息
 *                  返回值：返回所取信息字符串的字节长度
 *                  参数1(section):某个小节名(不区分大小写)，如果为空，则将在retVal内装载
 *                      这个ini文件的所有小节列表。 
 *                  参数2(key):欲获取信息的某个键名(不区分大小写)，如果为空，则将在retVal
 *                      内装载指定小节下的所有键列表。 
 *                  参数3(def):当指定信息，未找到时，则返回def，可以为空。 
 *                  参数4(retVal):一个字串缓冲区，所要获取的字符串将被保存在其中，其缓冲区
 *                      大小至少为size。
 *                  参数5(size):retVal的缓冲区大小(最大字符数量)。 
 *                  参数6(filePath):指定的ini文件路径，如果没有路径，则在windows目录下查找，
 *                      如果还是没有则在应用程序目录下查找，再没有，就只能返回def了。 
 * 
 * 修改历史:
 * 1.   修改日期:
 *      修改人:
 *      修改功能:
 * 2.   
************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ConfigDll
{
    /// <summary>
    /// 配置文件操作类
    /// </summary>
    public class IniOperation
    {
        //导入WinAPI
        [DllImport("kernel32")] 
        private static extern long WritePrivateProfileString(string section,string key,string val,string filePath); 
        [DllImport("kernel32")] 
        private static extern long GetPrivateProfileString(string section,string key,string def,StringBuilder retVal,int size,string filePath);

        /// <summary>
        /// 写配置信息, 如果文件不存在则新建一个
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="val">值</param>
        /// <param name="filePath">指定的ini文件路径，如果没有路径，则在windows目录下查找. </param>
        public static void WriteProfileString(string section, string key, string val, string filePath)
        {
            WritePrivateProfileString(section,key,val,filePath); 
        }

        /// <summary>
        /// 读配置信息
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="def">当指定信息，未找到时，则返回def，可以为空。</param>
        /// <param name="retVal">值</param>
        /// <param name="size">retVal的缓冲区大小(最大字符数量)。 </param>
        /// <param name="filePath">指定的ini文件路径，如果没有路径，则在windows目录下查找. </param>
        public static string GetProfileString(string section, string key, string def, string filePath)
        {
            int size = 65535;
            StringBuilder returnVal = new StringBuilder(65535);
            GetPrivateProfileString(section, key, def, returnVal, size, filePath);
            return returnVal.ToString();
        }
    }
}

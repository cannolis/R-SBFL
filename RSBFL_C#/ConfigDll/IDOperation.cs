using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfigDll
{
    /// <summary>
    /// 唯一标志符操作类
    /// </summary>
    public class IDOperation
    {
        /// <summary>
        /// 生成长字符串形式的全局唯一标识符(GUID) - 如:0f8fad5b-d9cb-469f-a165-70867728950e
        /// </summary>
        /// <returns></returns>
        public static string GenerateLongStringID()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 生成短字符串形式的全局唯一标识符(GUID) - 如:
        /// </summary>
        /// <returns></returns>
        public static string GenerateShortStringID()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i = i * ((int)b + 1);
            }

            //
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

        /// <summary>
        /// 生成数字(长整型)形式的全局唯一标识符(GUID) - 如:
        /// </summary>
        /// <returns></returns>
        public static long GenerateLongIntID()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }
    }
}

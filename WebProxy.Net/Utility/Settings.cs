using System.Collections.Specialized;
using System.Configuration;

namespace WebProxy.Net.Utility
{
    public class Settings
    {
        /// <summary>
        /// 获取验签密钥
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetDesKey(string key)
        {
            NameValueCollection webDesKeys = (NameValueCollection)ConfigurationManager.GetSection("webDesKey");
            string originalKey = webDesKeys[key.ToLower()];
            string md5 = EncryptHelper.GetMd5Hash(originalKey);

            return md5.Substring(0, 8);
        }

        /// <summary>
        /// 程序根目录
        /// </summary>
        public static string RootPath { get; set; }

        /// <summary>
        /// 多指令请求分隔字符
        /// </summary>
        public static char MultiCommandSplitChar = '|';
    }
}
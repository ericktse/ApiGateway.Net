using System.Collections.Specialized;
using System.Configuration;

namespace ApiGateway.Net.Utility
{
    public class Settings
    {
        /// <summary>
        /// 获取验签密钥
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetSignKey(string key)
        {
            NameValueCollection webDesKeys = (NameValueCollection)ConfigurationManager.GetSection("signKey");
            string originalKey = webDesKeys[key.ToLower()];
            string md5 = EncryptHelper.GetMd5Hash(originalKey);

            return md5.Substring(0, 8);
        }
    }
}
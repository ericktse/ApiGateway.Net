﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ApiGateway.Net.Utility
{
    public class EncryptHelper
    {
        #region DES加密/解密

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="srcData">待加密的字符串</param>
        /// <param name="encryPwd">加密密钥</param>
        /// <returns>加密成功返回加密后的字符串,失败返回源串</returns>
        public static string DESEncrypt(string srcData, string encryPwd)
        {
            if (string.IsNullOrEmpty(srcData) || string.IsNullOrEmpty(encryPwd))
            {
                return string.Empty;
            }
            string key = encryPwd.Substring(0, 8);
            var byteKey = Encoding.UTF8.GetBytes(key);
            var byteIV = Encoding.UTF8.GetBytes(key);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(srcData);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byteKey, byteIV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="encryptData">待解密的字符串</param>
        /// <param name="encryPwd">解密密钥</param>
        /// <returns>解密成功返回解密后的字符串,失败返源串</returns>
        public static string DESDecrypt(string encryptData, string encryPwd)
        {
            if (string.IsNullOrEmpty(encryptData) || string.IsNullOrEmpty(encryPwd))
            {
                return string.Empty;
            }
            string key = encryPwd.Substring(0, 8);
            var byteKey = Encoding.UTF8.GetBytes(key);
            var byteIV = Encoding.UTF8.GetBytes(key);
            byte[] inputByteArray = new byte[encryptData.Length / 2];
            for (int x = 0; x < encryptData.Length / 2; x++)
            {
                int i = (Convert.ToInt32(encryptData.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }
            try
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byteKey, byteIV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                Encoding encoding = new UTF8Encoding();
                return encoding.GetString(ms.ToArray());
            }
            catch
            {
                return "";
            }
        }

        #endregion

        #region MD5

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input">需加密字符串</param>
        /// <returns></returns>
        public static string GetMd5Hash(string input)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        #endregion

        #region 数字证书签名/验证签名

        private static string _password = "ucs2013";
        /// <summary>
        ///  证书加密 
        /// </summary>
        /// <param name="srcData">源数据</param>
        /// <param name="xpath">证书路径</param>
        /// <returns></returns>
        public static string SignDataForCert(string srcData, string xpath)
        {
            //创建证书文件
            X509Certificate2 objx5092 = new X509Certificate2(xpath, _password);
            //取出私钥
            RSACryptoServiceProvider rsa = objx5092.PrivateKey as RSACryptoServiceProvider;
            //将要签名的要素转化为byte[]
            byte[] data = Encoding.UTF8.GetBytes(srcData);
            //md5 你懂的 返回byte[]
            byte[] sigData = rsa.SignData(data, "SHA1");
            //将md5 之后的数据进行base64编码 必须的 返回的就是已签名的数据 
            string strSigData = Convert.ToBase64String(sigData);
            return strSigData;
        }

        /// <summary>
        /// 证书验签名
        /// </summary>
        /// <param name="srcData">明文</param>
        /// <param name="token">签名后Base64密文</param>
        /// <param name="xpath">证书路径</param>
        /// <returns></returns>
        public static bool VerifyDataForCert(string srcData, string token, string xpath)
        {
            //将要签名的要素转化为byte[]
            byte[] data = Encoding.UTF8.GetBytes(srcData);
            //创建证书文件
            X509Certificate2 objx5092 = new X509Certificate2(xpath);
            //取出公钥
            RSACryptoServiceProvider rsa = objx5092.PublicKey.Key as RSACryptoServiceProvider;
            byte[] sigDataByte = Convert.FromBase64String(token);
            return rsa.VerifyData(data, new SHA1CryptoServiceProvider(), sigDataByte);

        }

        #endregion

        #region 证书加密/解密
        /// <summary>
        /// 证书加密(失败请检查证书是否正确或者明文是否有效)
        /// </summary>
        /// <param name="srcData">明文</param>
        /// <param name="cerPath">公钥证书路径(.cer)</param>
        /// <returns>加密后Base64</returns>
        public static string CertEncrypt(string srcData, string cerPath)
        {
            try
            {
                //创建证书文件
                X509Certificate2 objx5092 = new X509Certificate2(cerPath);
                //取出公钥
                RSACryptoServiceProvider rsa = objx5092.PublicKey.Key as RSACryptoServiceProvider;
                //将要签名的要素转化为byte[]
                byte[] data = Encoding.UTF8.GetBytes(srcData);
                byte[] sigData = rsa.Encrypt(data, true);
                //数据进行base64编码 必须的 返回的就是已签名的数据 
                string enData = Convert.ToBase64String(sigData);
                return enData;
            }
            catch
            {
                return string.Empty;

            }
        }

        /// <summary>
        /// 证书解密(失败请检查证书密码是否正确或者密文是否有效)
        /// </summary>
        /// <param name="enData">密文</param>
        /// <param name="cerPath">私钥证书路径(.pfx)</param>
        /// <param name="cerPwd">访问证书密码</param>
        /// <returns></returns>
        public static string CertDecrypt(string enData, string cerPath, string cerPwd)
        {
            try
            {
                //将要签名的要素转化为byte[]
                byte[] data = Convert.FromBase64String(enData);
                //创建证书文件
                X509Certificate2 objx5092 = new X509Certificate2(cerPath, cerPwd);
                //取出私钥
                RSACryptoServiceProvider rsa = objx5092.PrivateKey as RSACryptoServiceProvider;
                if (rsa != null)
                {
                    byte[] srcDataByte = rsa.Decrypt(data, true);
                    return Encoding.UTF8.GetString(srcDataByte);
                }
                return null;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region Base64位编解码
        /// <summary>
        /// Base64编码
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Base64Encode(byte[] input)
        {
            string encode = Convert.ToBase64String(input);
            return encode;
        }

        /// <summary> 
        /// Base64解码
        /// </summary> 
        /// <param name="input">已用base64算法加密的字符串</param> 
        /// <returns>解密后的字符串</returns> 
        public static byte[] Base64Decode(string input)
        {
            byte[] bytes = Convert.FromBase64String(input);
            return bytes;
        }
        #endregion
    }
}
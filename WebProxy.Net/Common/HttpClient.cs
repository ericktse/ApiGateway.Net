using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace WebProxy.Net.Common
{
    public class HttpClient
    {
        public static async Task<string> PostAsync(string url, RequestHead head, Dictionary<string, object> body)
        {
            //-- Head
            dynamic headData = new ExpandoObject();
            headData.SerialNumber = head.SerialNumber;
            headData.Channel = head.Channel;
            headData.RequestHost = head.RequestHost;
            //channel: web,wap,app
            headData.RequestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string headStr = JsonConvert.SerializeObject(headData);
            string postHead = EncryptHelper.Base64Encode(Encoding.UTF8.GetBytes(headStr));

            //-- Body
            string bodyStr = JsonConvert.SerializeObject(body);
            string postBody = EncryptHelper.Base64Encode(Encoding.UTF8.GetBytes(bodyStr));

            //-- Post Data
            RestClient client = new RestClient(url);
            client.Proxy = null;
            client.Timeout = 60000;
            client.CookieContainer = null;
            client.FollowRedirects = false;
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("head", postHead);
            request.AddParameter("body", postBody);

            var respones = await client.ExecuteTaskAsync(request);
            return respones.Content;
        }
    }
}
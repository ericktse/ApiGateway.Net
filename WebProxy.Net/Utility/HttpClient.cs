using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using ApiGateway.Net.Model;

namespace ApiGateway.Net.Utility
{
    public class HttpClient
    {

        public static async Task<string> PostAsync(string url, RequestHead head, Dictionary<string, object> body)
        {
            RestRequest request = CreateRestRequest(head, body);
            var client = new RestClient(url)
            {
                Proxy = null,
                CookieContainer = null,
                FollowRedirects = false,
                Timeout = 60000
            };

            var respones = await client.ExecuteTaskAsync(request);
            return respones.Content;
        }

        private static RestRequest CreateRestRequest(RequestHead head, Dictionary<string, object> body)
        {
            //-- Head
            dynamic headData = new ExpandoObject();
            headData.SerialNumber = head.SerialNumber;
            headData.Channel = head.Channel;
            headData.RequestHost = head.RequestHost;
            headData.RequestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string headStr = JsonConvert.SerializeObject(headData);
            string postHead = EncryptHelper.Base64Encode(Encoding.UTF8.GetBytes(headStr));

            //-- Body
            string bodyStr = JsonConvert.SerializeObject(body);
            string postBody = EncryptHelper.Base64Encode(Encoding.UTF8.GetBytes(bodyStr));

            //-- Post Data
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("head", postHead);
            request.AddParameter("body", postBody);


            return request;
        }
    }
}
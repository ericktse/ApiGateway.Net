using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using WebProxy.Net.Model;

namespace WebProxy.Net.Utility
{
    public class HttpClient
    {
        public static async Task<string> PostAsync(Dictionary<string, string> handles, RequestHead head, Dictionary<string, object> body)
        {
            Dictionary<string, Task> requestDic = new Dictionary<string, Task>();
            foreach (var handle in handles)
            {
                var name = handle.Key;
                var url = handle.Value;

                //-- Head
                dynamic headData = new ExpandoObject();
                headData.SerialNumber = head.SerialNumber;
                //channel: web,wap,app
                headData.Channel = head.Channel;
                headData.RequestHost = head.RequestHost;
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
                request.AddParameter("head", postHead);
                request.AddParameter("body", postBody);

                Task<IRestResponse> task = client.ExecuteTaskAsync(request);

                requestDic.Add(name, task);
            }

            var taskList = requestDic.Select(x => x.Value).ToArray();
            await Task.WhenAll(taskList);

            var responseDatas = requestDic.Select(x => new ResponseData() { Name = x.Key, Content = ((Task<IRestResponse>)x.Value).Result.Content });
            //单请求直接返回请求内容，多请求返回name-content的数组
            if (responseDatas.Count() == 1)
            {
                return JsonConvert.SerializeObject(responseDatas.First().Content);
            }

            return JsonConvert.SerializeObject(responseDatas);
        }

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
            request.AddParameter("head", postHead);
            request.AddParameter("body", postBody);

            var respones = await client.ExecuteTaskAsync(request);
            return respones.Content;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Text;
using System.Web;
using ApiGateway.Net.Utility;
using Nancy;
using Newtonsoft.Json;
using RestSharp;
using ApiGateway.Net.Model;

namespace ApiGateway.Net.Modules
{
    public class TestModule : BaseModule
    {
        public TestModule()
        {
            Get["/Test"] = _ =>
            {
                return View["Test"];
            };

            Post["/Test"] = _ =>
            {
                //Get Data
                RequestHead head = new RequestHead();
                head.Command = Request.Form["command"];
                head.Version = Request.Form["version"];
                head.System = Request.Form["system"];
                head.Channel = Request.Form["channel"];
                head.UseCache = Request.Form["usecache"];
                head.MultiRequestMode = Request.Form["multirequestmode"];

                head.SerialNumber = Guid.NewGuid().ToString();
                head.RequestHost = "127.0.0.1";
                head.RequestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string headData = JsonConvert.SerializeObject(head);
                headData = EncryptHelper.Base64Encode(Encoding.UTF8.GetBytes(headData));

                string bodyData = Request.Form["body"];
                bodyData = EncryptHelper.Base64Encode(Encoding.UTF8.GetBytes(bodyData));
                string encryptBody = EncryptHelper.DESEncrypt(bodyData, Settings.GetSignKey(head.Channel));

                string url = Request.Url.SiteBase + "/Api";

                //-- Post
                RestClient client = new RestClient(url);
                client.Proxy = null;
                client.Timeout = 60000;
                RestRequest request = new RestRequest(Method.POST);
                request.AddParameter("head", headData);
                request.AddParameter("body", encryptBody);
                string result = client.Execute(request).Content;
                return result;
            };
        }
    }
}
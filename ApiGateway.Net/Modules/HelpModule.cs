using System;
using ApiGateway.Net.Utility;
using Nancy;
using Newtonsoft.Json;

namespace ApiGateway.Net.Modules
{
    public class HelpModule : NancyModule
    {
        public HelpModule()
        {
            Get["/"] = _ =>
            {
                return string.Format("Server Time:{0}", DateTime.Now);
            };

            Get["/Help"] = _ =>
            {
                var routeDic = RouteHelper.GetRouteDatas();
                return JsonConvert.SerializeObject(routeDic, Formatting.Indented);
            };
        }
    }
}
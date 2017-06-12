using System;
using WebProxy.Net.Utility;
using Nancy;
using Newtonsoft.Json;

namespace WebProxy.Net.Modules
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
                var routeDic = RouteHelper.RouteDatas;
                return JsonConvert.SerializeObject(routeDic, Formatting.Indented);
            };
        }
    }
}
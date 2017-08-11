using Nancy;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using WebProxy.Net.Utility;
using WebProxy.Net.Modules;
using Nancy.Bootstrapper;
using Nancy.Json;
using Nancy.TinyIoc;
using Newtonsoft.Json;

namespace WebProxy.Net
{

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        public static string RootPath { get; set; }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            if (string.IsNullOrWhiteSpace(Settings.RootPath))
            {
                Settings.RootPath = RootPathProvider.GetRootPath();
            }

            // 默认情况下，nancy在序列化时将对json key进行大小写装换，效果如下
            // Serialize: NotificationId->notificationId
            // Deserialize: notificationId->NotificationId
            // 如需保存大小写，设置RetainCasing为true（默认为false）
            JsonSettings.RetainCasing = true;
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            pipelines.OnError += (ctx, ex) =>
            {
                LogHelper.Error("Route request error[Global]", string.Format("Route request error，Message:{0}", ex.Message), ex);
                dynamic response = new ExpandoObject();
                response.Code = "500";
                response.ErrorMessage = ex.Message;
                return JsonConvert.SerializeObject(response);
            };
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using WebProxy.Net.Utility;
using Nancy;
using Newtonsoft.Json;
using WebProxy.Net.Model;

namespace WebProxy.Net.Modules
{
    public class BaseModule : NancyModule
    {
        protected RequestHead HeadData;
        protected string HeadOriginalStr;
        protected Dictionary<string, object> BodyData;
        protected RouteData OptimalRoute;
        private bool _useCache = false;

        public BaseModule()
        {
            DateTime elapsedTime = DateTime.Now;

            Before += ctx =>
            {
                GetRequestData(ctx.Request);

                VerifyData(HeadData, BodyData, OptimalRoute);

                return null;
            };

            After += ctx =>
            {
                string response;
                using (MemoryStream respData = new MemoryStream())
                {
                    ctx.Response.Contents(respData);
                    response = Encoding.UTF8.GetString(respData.ToArray());
                }

                LogHelper.Info(HeadData.Command, string.Format("Route request successfully,Address:{0},Time:{1}(s),Head:{2},Body:{3},RouteData:{4},Response:{5},UseCache:{6}", Request.Url, (DateTime.Now - elapsedTime).TotalSeconds, HeadOriginalStr, JsonConvert.SerializeObject(BodyData), JsonConvert.SerializeObject(OptimalRoute), response, _useCache));
            };

            OnError += (ctx, ex) =>
            {
                LogHelper.Error(string.Format("Route request Error,Command[{0}]", HeadData == null ? "" : HeadData.Command), string.Format("Route request error,Address:{0},End time:{1},Head:{2},Body:{3},RouteData:{4},Error Message:{5}", Request.Url, DateTime.Now, HeadOriginalStr, JsonConvert.SerializeObject(BodyData), JsonConvert.SerializeObject(OptimalRoute), ex.Message), ex);

                dynamic response = new ExpandoObject();
                response.Code = "500";
                response.ErrorMessage = string.Format("Route request Error,Message:{0}", ex.Message);
                return JsonConvert.SerializeObject(response);
            };
        }

        /// <summary>
        /// 判断是否使用缓存
        /// </summary>
        /// <returns></returns>
        protected bool UseCache
        {
            get
            {
                //启用缓存条件
                //- 请求Head参数UserCache:true
                //- 路由缓存时间配置大于0
                //- 满足请求条件，满足其一即可：
                //-- 请求Body无参数且路由缓存条件不存在
                //-- 请求body含参数且路由缓存存在条件且请求body所有非空字段都包含在路由缓存条件中
                if (!string.IsNullOrEmpty(HeadData.UseCache)
                    && HeadData.UseCache.ToLower() == "true"
                    && OptimalRoute.CacheTime != 0)
                {
                    if ((BodyData == null && OptimalRoute.CacheCondition == null)
                        || (BodyData != null && OptimalRoute.CacheCondition != null && BodyData.Count(x => x.Value != null) == BodyData.Count(x => OptimalRoute.CacheCondition.ContainsKey(x.Key)) && BodyData.Count(x => x.Value != null) == OptimalRoute.CacheCondition.Count(x => x.Value.Contains(BodyData.First(y => string.Equals(y.Key, x.Key, StringComparison.OrdinalIgnoreCase)).Value.ToString()))))
                    {
                        _useCache = true;
                    }
                    else
                    {
                        _useCache = false;
                    }
                }
                else
                {
                    _useCache = false;
                }

                return _useCache;
            }
            set
            {
                _useCache = value;
            }
        }

        /// <summary>
        /// 生成缓存Key
        /// </summary>
        /// <returns></returns>
        protected string GeneralCacheKey()
        {
            //缓存Key构建逻辑
            //- 参数以“-”连接
            //- 拼接Command,Version,System
            //- 如果存在缓存条件则以key=value的方式拼接
            string key = string.Join("_", HeadData.Command, HeadData.Version, HeadData.System);
            if (OptimalRoute.CacheCondition != null)
            {
                var userCondition = new List<string>();
                foreach (var condition in OptimalRoute.CacheCondition)
                {
                    if (BodyData != null && BodyData.Keys.Contains(condition.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        var bodyVal = BodyData.First(x => string.Equals(x.Key, condition.Key, StringComparison.OrdinalIgnoreCase)).Value.ToString();
                        if (condition.Value.Contains(bodyVal))
                        {
                            string val = string.Format("{0}={1}", condition.Key, bodyVal);
                            userCondition.Add(val);
                        }
                    }
                }

                if (userCondition.Count > 0)
                {
                    key = string.Join("_", key, string.Join("_", userCondition.ToArray()));
                }
            }

            return key.ToLower();
        }

        /// <summary>
        /// 获取请求信息
        /// </summary>
        private void GetRequestData(Request request)
        {
            //- Head
            var head = request.Form["head"];
            if (head == null)
            {
                throw new Exception("Request head data not exist or format error");
            }
            HeadOriginalStr = Encoding.UTF8.GetString(EncryptHelper.Base64Decode(head));
            HeadData = JsonConvert.DeserializeObject<RequestHead>(HeadOriginalStr);

            //- Body
            var bodyForm = request.Form["body"];
            if (bodyForm != null)
            {
                string key = Settings.GetDesKey(HeadData.Channel);
                bodyForm = EncryptHelper.DESDecrypt(bodyForm, key);
                bodyForm = Encoding.UTF8.GetString(EncryptHelper.Base64Decode(bodyForm));
                BodyData = JsonConvert.DeserializeObject<Dictionary<string, object>>(bodyForm);
            }

            //- Route
            OptimalRoute = RouteHelper.GetOptimalRoute(HeadData);
        }

        /// <summary>
        /// 校验数据
        /// </summary>
        /// <param name="head">报文头参数</param>
        /// <param name="body">报文参数</param>
        /// <param name="route">最优路由</param>
        private void VerifyData(RequestHead head, Dictionary<string, object> body, RouteData route)
        {
            if (head == null)
                throw new ArgumentNullException(nameof(head), "Request head data not exist");

            if (route == null)
                throw new ArgumentNullException(nameof(route), "Request route not exist");
        }
    }
}
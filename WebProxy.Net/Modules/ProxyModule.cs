using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Newtonsoft.Json;
using WebProxy.Net.Model;
using WebProxy.Net.Utility;

namespace WebProxy.Net.Modules
{
    public class HomeModule : NancyModule
    {
        protected RequestHead HeadData;
        protected List<Dictionary<string, object>> BodyDatas;
        protected Dictionary<string, RouteData> OptimalRoutes;
        protected bool FinalUseCache;

        public HomeModule()
        {
            #region pipelines
            DateTime elapsedTime = DateTime.Now;
            Before += ctx =>
            {
                GetRequestData(ctx.Request);
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

                LogHelper.Info(HeadData.Command, string.Format("Route request successfully,Address:{0},Time:{1}(s),Head:{2},Body:{3},RouteData:{4},Response:{5},UseCache:{6}", Request.Url, (DateTime.Now - elapsedTime).TotalSeconds, JsonConvert.SerializeObject(HeadData), JsonConvert.SerializeObject(BodyDatas), JsonConvert.SerializeObject(OptimalRoutes), response, FinalUseCache));
            };

            OnError += (ctx, ex) =>
            {
                LogHelper.Error(string.Format("Route request Error,Command[{0}]", HeadData == null ? "" : HeadData.Command), string.Format("Route request error,Address:{0},End time:{1},Head:{2},Body:{3},RouteData:{4},Error Message:{5}", Request.Url, DateTime.Now, JsonConvert.SerializeObject(HeadData), JsonConvert.SerializeObject(BodyDatas), JsonConvert.SerializeObject(OptimalRoutes), ex.Message), ex);

                dynamic response = new ExpandoObject();
                response.Code = "500";
                response.ErrorMessage = string.Format("Route request Error,Message:{0}", ex.Message);
                return JsonConvert.SerializeObject(response);
            };
            #endregion

            #region api
            Post["/Api", true] = async (x, ct) =>
             {
                 Dictionary<string, string> responseDic = new Dictionary<string, string>();

                 MultiRequestMode mode;
                 if (!Enum.TryParse(HeadData.MultiRequestMode, true, out mode))
                 {
                     mode = MultiRequestMode.Serial;
                 }
                 switch (mode)
                 {
                     //并行请求
                     case MultiRequestMode.Parallel:
                         {
                             int i = 0;
                             Dictionary<string, Task> asyncResponseDic = new Dictionary<string, Task>();
                             foreach (var optimalRoute in OptimalRoutes)
                             {
                                 var cmd = optimalRoute.Key;
                                 var route = optimalRoute.Value;
                                 var body = BodyDatas == null ? null : BodyDatas[i];
                                 var requestResult = HandleRequest(cmd, HeadData, route, body);
                                 asyncResponseDic.Add(cmd, requestResult);
                                 i++;
                             }
                             var taskList = asyncResponseDic.Select(y => y.Value).ToArray();
                             await Task.WhenAll(taskList);

                             responseDic = asyncResponseDic.ToDictionary(z => z.Key, z => ((Task<string>)z.Value).Result);
                         }
                         break;
                     //串行请求
                     case MultiRequestMode.Serial:
                     default:
                         {
                             int i = 0;
                             foreach (var optimalRoute in OptimalRoutes)
                             {
                                 var cmd = optimalRoute.Key;
                                 var route = optimalRoute.Value;
                                 var body = BodyDatas == null ? null : BodyDatas[i];
                                 var requestResult = await HandleRequest(cmd, HeadData, route, body);
                                 responseDic.Add(cmd, requestResult);
                                 i++;
                             }
                         }
                         break;
                 }

                 //单请求直接返回请求内容，多请求返回name-content的字典
                 if (responseDic.Count() == 1)
                 {
                     return responseDic.First().Value;
                 }

                 return responseDic;
             };
            #endregion
        }

        #region cache
        /// <summary>
        /// 校验是否启用缓存
        /// </summary>
        /// <param name="userCache">请求启用缓存字段</param>
        /// <param name="route">最优路由</param>
        /// <param name="body">请求参数</param>
        /// <returns></returns>
        private bool CheckUseCache(bool? userCache, RouteData route, Dictionary<string, object> body)
        {
            //启用缓存条件
            //- 请求Head参数UserCache:true
            //- 路由缓存时间配置大于0
            //- 渠道不为null，且渠道不在忽略的列表（IgnoreCacheChannel）中
            //- 满足请求条件，满足其一即可：
            //-- 请求Body无参数且路由缓存条件不存在
            //-- 请求body含参数且路由缓存存在条件且请求body所有非空字段都包含在路由缓存条件中

            if (!userCache.HasValue || userCache.Value == false)
                return false;
            if (route.CacheTime == 0)
                return false;

            if (body == null)
                return true;

            if (route.CacheCondition == null)
                return false;

            List<Tuple<string, bool>> parms = new List<Tuple<string, bool>>();
            foreach (var p in body)
            {
                if (p.Value != null)
                {
                    string cValue = string.Empty;
                    if (!route.CacheCondition.TryGetValue(p.Key, out cValue))
                    {
                        cValue = string.Empty;
                    }
                    var parm = Tuple.Create(p.Key, cValue.Split(',').Contains(p.Value.ToString()));
                    parms.Add(parm);
                }
            }
            if (parms.Count(o => o.Item2 == true) == parms.Count) return true;

            return false;
        }

        /// <summary>
        /// 生成缓存Key
        /// </summary>
        /// <param name="command">请求命令</param>
        /// <param name="version">请求版本</param>
        /// <param name="system">请求系统</param>
        /// <param name="route">最优路由</param>
        /// <param name="body">请求参数</param>
        /// <returns></returns>
        private string GeneralCacheKey(string command, string version, string system, RouteData route, Dictionary<string, object> body)
        {
            // 缓存Key构建逻辑
            //- 参数以“-”连接
            //- 拼接Command,Version,System
            //- 如果存在缓存条件则以key=value的方式拼接
            //- demo:home.banner_1.0.0_pc_condition1=value1_condition2=value2

            string key = string.Join("_", command, version, system);
            if (route.CacheCondition != null && body != null)
            {
                List<string> userCondition = new List<string>();
                foreach (var condition in route.CacheCondition)
                {
                    if (body.Keys.Contains(condition.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        var bodyVal = body.First(x => string.Equals(x.Key, condition.Key, StringComparison.OrdinalIgnoreCase)).Value.ToString();
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

        #endregion

        #region request
        /// <summary>
        /// 请求处理
        /// </summary>
        /// <param name="command">请求指令</param>
        /// <param name="head">请求报文头</param>
        /// <param name="route">最优路由</param>
        /// <param name="body">请求参数</param>
        /// <returns></returns>
        private async Task<string> HandleRequest(string command, RequestHead head, RouteData route, Dictionary<string, object> body)
        {
            string response;
            // 根据请求参数判断是否启用缓存
            // 启用-生成缓存KEY,并尝试读取缓存，成功则返回缓存值，失败则转发请求并更新缓存
            // 不启用-转发请求
            bool isUseCache = CheckUseCache(head.UseCache, route, body);
            if (isUseCache)
            {
                string key = GeneralCacheKey(command, head.Version, head.System, route, body);
                var cacheValue = CacheHelper.Get(key);
                if (cacheValue != null)
                {
                    response = cacheValue;
                    FinalUseCache = true;
                }
                else
                {
                    response = await HttpClient.PostAsync(route.Handle, head, body);
                    CacheHelper.Set(key, response, new TimeSpan(0, 0, route.CacheTime));
                    FinalUseCache = false;
                }
            }
            else
            {
                response = await HttpClient.PostAsync(route.Handle, head, body);
                FinalUseCache = false;
            }

            return response;
        }

        /// <summary>
        /// 获取请求信息
        /// </summary>
        /// <param name="request"></param>
        private void GetRequestData(Request request)
        {
            //- Head
            var head = request.Form["head"];
            if (head == null)
            {
                throw new Exception("Request head data not exist or format error");
            }
            head = Encoding.UTF8.GetString(EncryptHelper.Base64Decode(head));
            HeadData = JsonConvert.DeserializeObject<RequestHead>(head);
            if (HeadData == null)
                throw new ArgumentNullException("head", "Request head data not exist");
            if (string.IsNullOrEmpty(HeadData.Command))
                throw new ArgumentNullException("command", "Request command is null or empty");

            //- Body
            var bodyForm = request.Form["body"];
            if (!string.IsNullOrWhiteSpace(bodyForm))
            {
                string key = Settings.GetDesKey(HeadData.Channel);
                bodyForm = EncryptHelper.DESDecrypt(bodyForm, key);
                bodyForm = Encoding.UTF8.GetString(EncryptHelper.Base64Decode(bodyForm));
                BodyDatas = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(bodyForm);
            }

            //- Route
            // Command参数如果不是json数组装换为数组处理
            if (!HeadData.Command.StartsWith("[") && !HeadData.Command.EndsWith("]"))
            {
                HeadData.Command = string.Format("[\"{0}\"]", HeadData.Command);
            }
            string[] cmds = JsonConvert.DeserializeObject<string[]>(HeadData.Command);

            if (BodyDatas != null && cmds.Count() != BodyDatas.Count)
                throw new Exception("Request body number of parameters error");

            Dictionary<string, RouteData> routeDatas = new Dictionary<string, RouteData>();
            foreach (var cmd in cmds)
            {
                RouteData route = RouteHelper.GetOptimalRoute(cmd, HeadData.Version, HeadData.System);
                if (route == null)
                    throw new ArgumentNullException("route", "Route data not exist");

                routeDatas.Add(cmd, route);
            }

            //路由负载
            OptimalRoutes = RouteHelper.RoutingLoadBalance(routeDatas);
        }
        #endregion
    }
}
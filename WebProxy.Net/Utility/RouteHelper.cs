using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Caching;
using Newtonsoft.Json;
using WebProxy.Net.Model;

namespace WebProxy.Net.Utility
{
    public class RouteHelper
    {
        private static readonly string RouteDataPath = Path.Combine(Settings.RootPath, "App_Data/route");
        private static readonly string RouteDataCacheKey = "routes.json";
        private static readonly string HostDataPath = Path.Combine(Settings.RootPath, "App_Data/host");
        private static readonly string HostDataCacheKey = "hosts.json";

        /// <summary>
        /// 获取路由配置
        /// </summary>        
        public static Dictionary<string, List<RouteData>> GetRouteDatas()
        {
            var routeDic = HttpRuntime.Cache[RouteDataCacheKey] as Dictionary<string, List<RouteData>>;
            if (routeDic == null)
            {
                string[] files = Directory.GetFiles(RouteDataPath, "*.json", SearchOption.AllDirectories);

                routeDic = new Dictionary<string, List<RouteData>>();
                foreach (var file in files)
                {
                    var routeContent = File.ReadAllText(file);
                    var routeSet = JsonConvert.DeserializeObject<List<RouteData>>(routeContent);

                    var singleDic = routeSet.GroupBy(o => o.Command).ToDictionary(
                        k => k.Key,
                        v => v.Select(o => o).ToList()
                        );

                    // 跨配置文件Command必须保持唯一
                    foreach (var route in singleDic)
                        routeDic.Add(route.Key, route.Value);
                }

                CacheHelper.Set(RouteDataCacheKey, routeDic, files);
            }
            return routeDic;
        }

        /// <summary>
        /// 获取Host配置
        /// </summary>
        /// <returns></returns>
        public static List<HostData> GetHostDatas()
        {
            var hostDatas = HttpRuntime.Cache[HostDataCacheKey] as List<HostData>;
            if (hostDatas == null)
            {
                //加载配置目录下所有的json文件
                string[] files = Directory.GetFiles(HostDataPath, "*.json", SearchOption.AllDirectories);

                hostDatas = new List<HostData>();
                foreach (var file in files)
                {
                    var content = File.ReadAllText(file);
                    var data = JsonConvert.DeserializeObject<List<HostData>>(content);
                    hostDatas.AddRange(data);
                }

                CacheHelper.Set(HostDataCacheKey, hostDatas, files);
            }
            return hostDatas;
        }

        /// <summary>
        /// 路由负载均衡
        /// </summary>
        /// <param name="routeDatas"></param>
        /// <returns></returns>
        public static Dictionary<string, RouteData> RoutingLoadBalance(
            Dictionary<string, RouteData> routeDatas)
        {
            if (routeDatas == null)
                return null;

            var hostDatas = GetHostDatas().Select(x => new HostData()
            {
                Name = "${" + x.Name.ToLower() + "}",
                Hosts = x.Hosts
            }).Where(x => routeDatas.Values.Any(y => y.Handle.StartsWith(x.Name)));

            Dictionary<string, string> hostDic = new Dictionary<string, string>();
            foreach (var host in hostDatas)
            {
                var randomHost = RandomHelper.GetRandomList(host.Hosts.ToList(), 1);

                hostDic.Add(host.Name, randomHost.First().ServiceUrl);
            }

            Dictionary<string, RouteData> newData = new Dictionary<string, RouteData>();
            foreach (var route in routeDatas)
            {
                var routedata = route.Value;
                var host = hostDic.FirstOrDefault(x => routedata.Handle.ToLower().StartsWith(x.Key));
                if (host.Key != null && host.Value != null)
                {
                    routedata.Handle = routedata.Handle.Replace(host.Key, host.Value);
                }

                newData.Add(route.Key, routedata);
            }

            return newData;
        }

        /// <summary>
        /// 获取最优路由
        /// </summary>
        /// <param name="command">命令名称</param>
        /// <param name="version">版本号</param>
        /// <param name="system">系统</param>
        /// <returns></returns>
        public static RouteData GetOptimalRoute(string command, string version, string system)
        {
            var routeDatas = GetRouteDatas();
            var routes = routeDatas.FirstOrDefault(x => string.Equals(x.Key, command, StringComparison.OrdinalIgnoreCase));
            if (routes.Value == null)
                return null;

            if (routes.Value.Count == 1)
                return routes.Value.First();

            IEnumerable<RouteData> routeList = routes.Value;

            List<Expression<Func<RouteData, bool>>> expressions = new List<Expression<Func<RouteData, bool>>>();
            if (!string.IsNullOrEmpty(version))
            {
                expressions.Add(x => string.Equals(x.Version, version, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(system))
            {
                expressions.Add(x => string.Equals(x.System.ToString(), system, StringComparison.OrdinalIgnoreCase));
            }

            routeList = expressions.Aggregate(routeList, (current, item) => current.Where(item.Compile()));

            if (routeList.Any())
            {
                return routeList.OrderBy(x => x.Version).ThenBy(x => x.System).FirstOrDefault();
            }

            return routes.Value
                    .Where(x => string.IsNullOrEmpty(x.Version) || x.System == SytemType.None)
                    .OrderBy(x => x.Version).ThenBy(x => x.System)
                    .FirstOrDefault();
        }
    }
}
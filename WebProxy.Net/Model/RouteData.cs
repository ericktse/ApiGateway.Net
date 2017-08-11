using System.Collections.Generic;

namespace WebProxy.Net.Model
{
    public class RouteData
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public string Version { get; set; }
        public SytemType System { get; set; }
        public string Handle { get; set; }
        public int CacheTime { get; set; }
        public Dictionary<string, string> CacheCondition { get; set; }
    }

    /// <summary>
    /// 请求系统类型
    /// </summary>
    public enum SytemType
    {
        None = 0,
        PC = 1,
        Android = 2,
        IOS = 3
    }
}
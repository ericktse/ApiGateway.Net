namespace ApiGateway.Net.Model
{
    public class RequestHead
    {
        /// <summary>
        /// 流水号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 请求地址
        /// </summary>
        public string RequestHost { get; set; }
        /// <summary>
        /// 请求时间
        /// </summary>
        public string RequestTime { get; set; }
        /// <summary>
        /// 指令名称
        /// </summary>
        public string Command { get; set; }
        /// <summary>
        /// 请求版本
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 系统
        /// </summary>
        public string System { get; set; }
        /// <summary>
        /// 渠道
        /// </summary>
        public string Channel { get; set; }
        /// <summary>
        /// 是否使用缓存
        /// </summary>
        public bool? UseCache { get; set; }
        /// <summary>
        /// 多命令请求方式（serial:同步；parallel:异步）
        /// </summary>
        public string MultiRequestMode { get; set; }
    }
}
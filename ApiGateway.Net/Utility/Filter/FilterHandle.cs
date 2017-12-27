using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;

namespace ApiGateway.Net.Utility.Filter
{
    public class FilterHandle
    {
        /// <summary>
        /// FilterAttribute 的 Execute 方法名称
        /// </summary>
        private static readonly string Execute = "Execute";

        /// <summary>
        /// Module拦截器执行
        /// </summary>
        /// <param name="moduleType"></param>
        /// <param name="filterType">BeforeRequestFilterAttribute、AfterRequestFilterAttribute</param>
        /// <param name="filterContext"></param>
        /// <param name="filterTime"></param>
        public static object ModuleInvoke(Type moduleType, Type filterType, NancyContext filterContext, FilterTime filterTime)
        {
            // 获取Module的拦截器标签，并执行标签的Execute方法
            var attributeList = moduleType.GetCustomAttributes(filterType, true);
            foreach (var attribute in attributeList)
            {
                var attributeType = attribute.GetType();
                var methodInfo = attributeType.GetMethod(Execute);
                if (methodInfo != null)
                {
                    object result = methodInfo.Invoke(attribute, new object[] { filterContext, filterType, filterTime });

                    if (result != null)
                        return result;
                }
            }
            return null;
        }
    }
}
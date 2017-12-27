using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;

namespace ApiGateway.Net.Utility.Filter
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public abstract class FilterAttribute : Attribute
    {
        /// <summary>
        /// ModuleFilter拦截入口点
        /// </summary>
        /// <param name="filterContext"></param>
        /// <param name="filterType"></param>
        /// <param name="filterTime"></param>
        public abstract object Execute(NancyContext filterContext, Type filterType, FilterTime filterTime);
    }
}
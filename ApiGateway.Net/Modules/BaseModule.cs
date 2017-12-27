using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ApiGateway.Net.Utility.Filter;
using Nancy;

namespace ApiGateway.Net.Modules
{
    public class BaseModule : NancyModule
    {
        public BaseModule()
        {
            // Filter
            var moduleType = this.GetType();
            Before += ctx =>
            {
                object result = FilterHandle.ModuleInvoke(moduleType, typeof(IBeforeRequestFilterAttribute), ctx, FilterTime.Before);
                if (result != null)
                {
                    return Response.AsText(result.ToString());
                }

                return null;
            };
            After += ctx =>
            {
                FilterHandle.ModuleInvoke(moduleType, typeof(IAfterRequestFilterAttribute), ctx, FilterTime.After);
            };
        }
    }
}
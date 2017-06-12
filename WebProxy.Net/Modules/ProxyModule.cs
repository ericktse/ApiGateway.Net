﻿using System;
using System.Collections.Generic;
using System.Linq;
using WebProxy.Net.Utility;
using Newtonsoft.Json;
using WebProxy.Net.Utility;

namespace WebProxy.Net.Modules
{
    public class HomeModule : BaseModule
    {
        public HomeModule()
        {
            Post["/Api", true] = async (x, ct) =>
             {
                 string result;
                 if (UseCache)
                 {
                     string key = GeneralCacheKey();
                     var cacheValue = CacheHelper.Get(key);
                     if (cacheValue != null)
                     {
                         result = cacheValue;
                     }
                     else
                     {
                         string postResult = await HttpClient.PostAsync(OptimalRoute.Handle, HeadData, BodyData);
                         result = postResult;

                         CacheHelper.Set(key, postResult, new TimeSpan(0, 0, OptimalRoute.CacheTime));
                         UseCache = false;
                     }
                 }
                 else
                 {
                     result = await HttpClient.PostAsync(OptimalRoute.Handle, HeadData, BodyData);
                 }
                 return result;
             };
        }
    }
}
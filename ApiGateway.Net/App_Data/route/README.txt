﻿//说明
// 1.1- 完整范例
// {
//    "Name": "请求1",
//    "Command": "ControllerName.ActionName",
//    "Handle": "http://service.test.com/ControllerName/ActionName1",
//    "Version": "1.0.0",
//    "System": "PC",
//    "CacheTime": 10,
//    "CacheCondition": {
//      "Condition1": "1,2,3",
//      "Condition2": "a,b,c"
//    }
// }
// 1.2- 最简范例
// {
//    "Name": "请求2",
//    "Command": "ControllerName.ActionName",
//    "Handle": "http://service.test.com/ControllerName/ActionName1"
// }
//
// 2- 字段说明
// Name:名称
// Command:命令名称（必填）
// Handle:命令处理URL,对于微服务的请求地址（必填）.单地址直接返回结果，多地址返回name-content字典
// Version:请求版本号（选填）
// System:请求系统类型,[None(等同空值或不传值),PC,Android,IOS]（选填）
// -- Version和System字段用于Route端筛选最优路由的条件
// CacheTime:缓存时间，单位秒（选填）
// CacheCondition:缓存条件（选填）
// -- CacheTime和CacheCondition用于判断请求是否使用缓存和构建缓存Key
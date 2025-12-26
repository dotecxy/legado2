---
trigger: always_on
alwaysApply: true
---
1.始终用中文回答
2.这是一个Blazor hybrid项目
3.异步方法始终Async结尾，要添加Task返回值或者valuetask
4.Legado.Core 是规则解析库等核心数据
5.Legado.Shared 是一个razor组件使用的是mudblazor库做UI
6.Legado.Windows 是一个承载razor组件的windows blazor 项目
7.这个项目更倾向于Windows和手机app项目，布局优先app
8.项目界面参考阅读3.0 app 移植过来的， github地址为https://github.com/gedoor/legado
9.方法和非私有属性都是大驼峰命名法，只读和私有属性使用_+小写
10.json字段注解使用小驼峰命名法
11.数据注解使用下划线命名法 
12.数据model的属性格式如下： /// <summary>
            /// 中文注释
            /// </summary>
            [Column("column_name")]
            [JsonProperty("jsonPropertyName")]
            public Type PropertyName { get; set; } 
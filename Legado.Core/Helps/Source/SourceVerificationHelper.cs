using Legado.Core.App;
using Legado.Core.Data.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Legado.Core.Helps.Source
{
    /// <summary>
    /// 源验证帮助类
    /// 用于处理图片验证码、防爬、滑动验证码、点击字符等验证场景
    /// </summary>
    public static class SourceVerificationHelper
    {
        private static readonly TimeSpan WaitTime = TimeSpan.FromMinutes(1);

        /// <summary>
        /// 获取验证结果键
        /// </summary>
        private static string GetVerificationResultKey(IBaseSource source)
        {
            return GetVerificationResultKey(source.GetKey());
        }

        /// <summary>
        /// 获取验证结果键
        /// </summary>
        private static string GetVerificationResultKey(string sourceKey)
        {
            return $"{sourceKey}_verificationResult";
        }

        /// <summary>
        /// 获取书源验证结果
        /// 支持图片验证码、防爬、滑动验证码、点击字符等
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="url">验证URL</param>
        /// <param name="title">验证页面标题</param>
        /// <param name="useBrowser">是否使用浏览器</param>
        /// <param name="refetchAfterSuccess">成功后是否重新获取</param>
        /// <returns>验证结果</returns>
        public static string GetVerificationResult(
            IBaseSource source,
            string url,
            string title,
            bool useBrowser,
            bool refetchAfterSuccess = true)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "getVerificationResult parameter source cannot be null");

            if (url.Length >= 64 * 1024)
                throw new ArgumentException("getVerificationResult parameter url too long");

            // TODO: 检查是否在主线程
            // if (IsMainThread())
            //     throw new InvalidOperationException("getVerificationResult must be called on a background thread");

            var sourceKey = source.GetKey();
            ClearResult(sourceKey); 
            if (!useBrowser)
            {
                // TODO: 启动验证码Activity
                // appCtx.StartActivity<VerificationCodeActivity>(intent =>
                // {
                //     intent.PutExtra("imageUrl", url);
                //     intent.PutExtra("sourceOrigin", sourceKey);
                //     intent.PutExtra("sourceName", source.GetTag());
                //     intent.PutExtra("sourceType", source.GetSourceType());
                //     IntentData.Put(GetVerificationResultKey(source), Thread.CurrentThread);
                // });
            }
            else
            {
                 StartBrowser(source, url, title, true, refetchAfterSuccess);
            } 

            var result = GetResult(sourceKey);
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new Exception("验证结果为空");
            }

            return result;
        }

        /// <summary>
        /// 启动内置浏览器
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="url">URL地址</param>
        /// <param name="title">标题</param>
        /// <param name="saveResult">保存网页源代码到数据库</param>
        /// <param name="refetchAfterSuccess">成功后是否重新获取</param>
        public static (bool?, IntentData) StartBrowser(
            IBaseSource source,
            string url,
            string title,
            bool saveResult = false,
            bool refetchAfterSuccess = true)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "startBrowser parameter source cannot be null");

            if (url.Length >= 64 * 1024)
                throw new ArgumentException("startBrowser parameter url too long");

            var result = QApplication.Context.StartBrowserDialog(intent =>
            {
                intent.Put("title", title);
                intent.Put("url", url);
                intent.Put("sourceOrigin", source.GetKey());
                intent.Put("sourceName", source.GetTag());
                intent.Put("sourceType", source.GetSourceType());
                intent.Put("sourceVerificationEnable", saveResult);
                intent.Put("refetchAfterSuccess", refetchAfterSuccess);
                intent.Put(GetVerificationResultKey(source), Thread.CurrentThread);
                intent.Put("resultKey", GetVerificationResultKey(source));
            });
            return result;
        }

        /// <summary>
        /// 检查并唤醒等待验证结果的线程
        /// </summary>
        public static void CheckResult(string sourceKey)
        {
            var result = GetResult(sourceKey);
            if (result == null)
            {
                SetResult(sourceKey, "");
            }

            // TODO: 唤醒等待的线程
            // var thread = IntentData.Get<Thread>(GetVerificationResultKey(sourceKey));
            // if (thread != null)
            // {
            //     LockSupport.Unpark(thread);
            // }
        }

        /// <summary>
        /// 设置验证结果
        /// </summary>
        public static void SetResult(string sourceKey, string result)
        {
            CacheManager.Instance.Put(GetVerificationResultKey(sourceKey), result ?? "");
        }

        /// <summary>
        /// 获取验证结果
        /// </summary>
        public static string GetResult(string sourceKey)
        {
            return CacheManager.Instance.Get(GetVerificationResultKey(sourceKey));
        }

        /// <summary>
        /// 清除验证结果
        /// </summary>
        public static void ClearResult(string sourceKey)
        {
            CacheManager.Instance.Delete(GetVerificationResultKey(sourceKey));
        }
    }
}

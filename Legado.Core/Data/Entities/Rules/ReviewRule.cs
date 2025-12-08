using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Data.Entities.Rules
{
    public class ReviewRule
    {
        public string ReviewUrl { get; set; }           // 段评URL
        public string AvatarRule { get; set; }          // 段评发布者头像
        public string ContentRule { get; set; }         // 段评内容
        public string PostTimeRule { get; set; }         // 段评发布时间
        public string ReviewQuoteUrl { get; set; }      // 获取段评回复URL

        // 这些功能将在以上功能完成以后实现
        public string VoteUpUrl { get; set; }           // 点赞URL
        public string VoteDownUrl { get; set; }         // 点踩URL
        public string PostReviewUrl { get; set; }        // 发送回复URL
        public string PostQuoteUrl { get; set; }         // 发送回复段评URL
        public string DeleteUrl { get; set; }           // 删除段评URL

        // 如果需要提供默认值，可以使用以下构造函数
        public ReviewRule() { }

        // 可选：如果需要复制构造函数
        public ReviewRule(ReviewRule other)
        {
            if (other == null) return;

            ReviewUrl = other.ReviewUrl;
            AvatarRule = other.AvatarRule;
            ContentRule = other.ContentRule;
            PostTimeRule = other.PostTimeRule;
            ReviewQuoteUrl = other.ReviewQuoteUrl;
            VoteUpUrl = other.VoteUpUrl;
            VoteDownUrl = other.VoteDownUrl;
            PostReviewUrl = other.PostReviewUrl;
            PostQuoteUrl = other.PostQuoteUrl;
            DeleteUrl = other.DeleteUrl;
        }
    }
}

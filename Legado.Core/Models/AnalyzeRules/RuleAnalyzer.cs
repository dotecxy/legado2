using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Models.AnalyzeRules
{
    /// <summary>
    /// 通用的规则切分处理
    /// </summary>
    public class RuleAnalyzer
    {
        private string queue; // 被处理字符串
        private int pos = 0; // 当前处理到的位置
        private int start = 0; // 当前处理字段的开始
        private int startX = 0; // 当前规则的开始
        private int step = 0; // 分割字符的长度

        private List<string> rule = new List<string>(); // 分割出的规则列表
        public string ElementsType { get; private set; } = ""; // 当前分割字符串
        private readonly bool code; // 是否为代码模式

        // 设置平衡组函数，json或JavaScript时设置成chompCodeBalanced，否则为chompRuleBalanced
        private Func<char, char, bool> chompBalanced;

        /// <summary>
        /// 转义字符
        /// </summary>
        private const char ESC = '\\';

        public RuleAnalyzer(string data, bool code = false)
        {
            this.queue = data;
            this.code = code;
            this.chompBalanced = code ? ChompCodeBalanced : ChompRuleBalanced;
        }

        /// <summary>
        /// 修剪当前规则之前的"@"或者空白符
        /// </summary>
        public void Trim()
        {
            // 在while里重复设置start和startX会拖慢执行速度，所以先来个判断是否存在需要修剪的字段，最后再一次性设置start和startX
            if (pos < queue.Length && (queue[pos] == '@' || queue[pos] < '!'))
            {
                pos++;
                while (pos < queue.Length && (queue[pos] == '@' || queue[pos] < '!'))
                {
                    pos++;
                }
                start = pos; // 开始点推移
                startX = pos; // 规则起始点推移
            }
        }

        /// <summary>
        /// 将pos重置为0，方便复用
        /// </summary>
        public void ResetPos()
        {
            pos = 0;
            startX = 0;
        }

        /// <summary>
        /// 从剩余字串中拉出一个字符串，直到但不包括匹配序列
        /// </summary>
        /// <param name="seq">查找的字符串（区分大小写）</param>
        /// <returns>是否找到相应字段</returns>
        private bool ConsumeTo(string seq)
        {
            start = pos; // 将处理到的位置设置为规则起点
            int offset = queue.IndexOf(seq, pos, StringComparison.Ordinal);
            if (offset != -1)
            {
                pos = offset;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 从剩余字串中拉出一个字符串，直到但不包括匹配序列（匹配参数列表中一项即为匹配），或剩余字串用完
        /// </summary>
        /// <param name="seq">匹配字符串序列</param>
        /// <returns>成功返回true并设置间隔，失败则直接返回false</returns>
        private bool ConsumeToAny(params string[] seq)
        {
            int tempPos = pos; // 声明新变量记录匹配位置，不更改类本身的位置

            while (tempPos != queue.Length)
            {
                foreach (string s in seq)
                {
                    if (tempPos + s.Length <= queue.Length &&
                        queue.Substring(tempPos, s.Length) == s)
                    {
                        step = s.Length; // 间隔数
                        this.pos = tempPos; // 匹配成功, 同步处理位置到类
                        return true; // 匹配就返回 true
                    }
                }
                tempPos++; // 逐个试探
            }
            return false;
        }

        /// <summary>
        /// 从剩余字串中拉出一个字符串，直到但不包括匹配序列（匹配参数列表中一项即为匹配），或剩余字串用完
        /// </summary>
        /// <param name="seq">匹配字符序列</param>
        /// <returns>返回匹配位置</returns>
        private int FindToAny(params char[] seq)
        {
            int tempPos = pos; // 声明新变量记录匹配位置，不更改类本身的位置

            while (tempPos != queue.Length)
            {
                foreach (char s in seq)
                {
                    if (queue[tempPos] == s)
                        return tempPos; // 匹配则返回位置
                }
                tempPos++; // 逐个试探
            }
            return -1;
        }

        /// <summary>
        /// 拉出一个非内嵌代码平衡组，存在转义文本
        /// </summary>
        private bool ChompCodeBalanced(char open, char close)
        {
            int tempPos = pos; // 声明临时变量记录匹配位置，匹配成功后才同步到类的pos
            int depth = 0; // 嵌套深度
            int otherDepth = 0; // 其他对称符号嵌套深度
            bool inSingleQuote = false; // 单引号
            bool inDoubleQuote = false; // 双引号

            do
            {
                if (tempPos == queue.Length) break;
                char c = queue[tempPos++];
                if (c != ESC)
                { // 非转义字符
                    if (c == '\'' && !inDoubleQuote) inSingleQuote = !inSingleQuote; // 匹配具有语法功能的单引号
                    else if (c == '"' && !inSingleQuote) inDoubleQuote = !inDoubleQuote; // 匹配具有语法功能的双引号

                    if (inSingleQuote || inDoubleQuote) continue; // 语法单元未匹配结束，直接进入下个循环

                    if (c == '[') depth++; // 开始嵌套一层
                    else if (c == ']') depth--; // 闭合一层嵌套
                    else if (depth == 0)
                    {
                        // 处于默认嵌套中的非默认字符不需要平衡，仅depth为0时默认嵌套全部闭合，此字符才进行嵌套
                        if (c == open) otherDepth++;
                        else if (c == close) otherDepth--;
                    }
                }
                else
                {
                    tempPos++;
                }
            } while (depth > 0 || otherDepth > 0); // 拉出一个平衡字串

            if (depth > 0 || otherDepth > 0)
                return false;

            this.pos = tempPos; // 同步位置
            return true;
        }

        /// <summary>
        /// 拉出一个规则平衡组，经过仔细测试xpath和jsoup中，引号内转义字符无效
        /// </summary>
        private bool ChompRuleBalanced(char open, char close)
        {
            int tempPos = pos; // 声明临时变量记录匹配位置，匹配成功后才同步到类的pos
            int depth = 0; // 嵌套深度
            bool inSingleQuote = false; // 单引号
            bool inDoubleQuote = false; // 双引号

            do
            {
                if (tempPos == queue.Length) break;
                char c = queue[tempPos++];
                if (c == '\'' && !inDoubleQuote) inSingleQuote = !inSingleQuote; // 匹配具有语法功能的单引号
                else if (c == '"' && !inSingleQuote) inDoubleQuote = !inDoubleQuote; // 匹配具有语法功能的双引号

                if (inSingleQuote || inDoubleQuote) continue; // 语法单元未匹配结束，直接进入下个循环
                else if (c == '\\')
                { // 不在引号中的转义字符才将下个字符转义
                    tempPos++;
                    continue;
                }

                if (c == open) depth++; // 开始嵌套一层
                else if (c == close) depth--; // 闭合一层嵌套
            } while (depth > 0); // 拉出一个平衡字串

            if (depth > 0)
                return false;

            this.pos = tempPos; // 同步位置
            return true;
        }

        /// <summary>
        /// 不用正则,不到最后不切片也不用中间变量存储,只在序列中标记当前查找字段的开头结尾,到返回时才切片,高效快速准确切割规则
        /// 解决jsonPath自带的"&&"和"||"与阅读的规则冲突,以及规则正则或字符串中包含"&&"、"||"、"%%"、"@"导致的冲突
        /// </summary>
        public List<string> SplitRule(params string[] split)
        {
            rule.Clear();

            if (split.Length == 1)
            {
                ElementsType = split[0]; // 设置分割字串
                if (!ConsumeTo(ElementsType))
                {
                    rule.Add(queue.Substring(startX));
                    return rule;
                }
                step = ElementsType.Length; // 设置分隔符长度
                return SplitRuleNext(); // 递归匹配
            }
            else if (!ConsumeToAny(split))
            { // 未找到分隔符
                rule.Add(queue.Substring(startX));
                return rule;
            }

            int end = pos; // 记录分隔位置
            pos = start; // 重回开始，启动另一种查找

            do
            {
                int st = FindToAny('[', '('); // 查找筛选器位置

                if (st == -1)
                {
                    rule = new List<string> { queue.Substring(startX, end - startX) }; // 压入分隔的首段规则到数组

                    ElementsType = queue.Substring(end, step); // 设置组合类型
                    pos = end + step; // 跳过分隔符

                    while (ConsumeTo(ElementsType))
                    { // 循环切分规则压入数组
                        rule.Add(queue.Substring(start, pos - start));
                        pos += step; // 跳过分隔符
                    }

                    rule.Add(queue.Substring(pos)); // 将剩余字段压入数组末尾
                    return rule;
                }

                if (st > end)
                { // 先匹配到st1pos，表明分隔字串不在选择器中，将选择器前分隔字串分隔的字段依次压入数组
                    rule = new List<string> { queue.Substring(startX, end - startX) }; // 压入分隔的首段规则到数组
                    ElementsType = queue.Substring(end, step); // 设置组合类型
                    pos = end + step; // 跳过分隔符

                    while (ConsumeTo(ElementsType) && pos < st)
                    { // 循环切分规则压入数组
                        rule.Add(queue.Substring(start, pos - start));
                        pos += step; // 跳过分隔符
                    }

                    if (pos > st)
                    {
                        startX = start;
                        return SplitRuleNext(); // 首段已匹配,但当前段匹配未完成ORNESTEDMETHOD,调用二段匹配
                    }
                    else
                    { // 执行到此，证明后面再无分隔字符
                        rule.Add(queue.Substring(pos)); // 将剩余字段压入数组末尾
                        return rule;
                    }
                }

                pos = st; // 位置推移到筛选器处
                char next = queue[pos] == '[' ? ']' : ')'; // 平衡组末尾字符

                if (!chompBalanced(queue[pos], next))
                    throw new RuleExcepion(queue.Substring(0, start) + "后未平衡"); // 拉出一个筛选器,不平衡则报错

            } while (end > pos);

            start = pos; // 设置开始查找筛选器位置的起始位置
            return SplitRule(split); // 递归调用首段匹配
        }

        /// <summary>
        /// 二段匹配被调用,elementsType非空(已在首段赋值),直接按elementsType查找,比首段采用的方式更快
        /// </summary>
        private List<string> SplitRuleNext()
        {
            int end = pos; // 记录分隔位置
            pos = start; // 重回开始，启动另一种查找

            do
            {
                int st = FindToAny('[', '('); // 查找筛选器位置

                if (st == -1)
                {
                    rule.Add(queue.Substring(startX, end - startX)); // 压入分隔的首段规则到数组
                    pos = end + step; // 跳过分隔符

                    while (ConsumeTo(ElementsType))
                    { // 循环切分规则压入数组
                        rule.Add(queue.Substring(start, pos - start));
                        pos += step; // 跳过分隔符
                    }

                    rule.Add(queue.Substring(pos)); // 将剩余字段压入数组末尾
                    return rule;
                }

                if (st > end)
                { // 先匹配到st1pos，表明分隔字串不在选择器中，将选择器前分隔字串分隔的字段依次压入数组
                    rule.Add(queue.Substring(startX, end - startX)); // 压入分隔的首段规则到数组
                    pos = end + step; // 跳过分隔符

                    while (ConsumeTo(ElementsType) && pos < st)
                    { // 循环切分规则压入数组
                        rule.Add(queue.Substring(start, pos - start));
                        pos += step; // 跳过分隔符
                    }

                    if (pos > st)
                    {
                        startX = start;
                        return SplitRuleNext(); // 首段已匹配,但当前段匹配未完成ORNESTEDMETHOD,调用二段匹配
                    }
                    else
                    { // 执行到此，证明后面再无分隔字符
                        rule.Add(queue.Substring(pos)); // 将剩余字段压入数组末尾
                        return rule;
                    }
                }

                pos = st; // 位置推移到筛选器处
                char next = queue[pos] == '[' ? ']' : ')'; // 平衡组末尾字符

                if (!chompBalanced(queue[pos], next))
                    throw new RuleExcepion(queue.Substring(0, start) + "后未平衡"); // 拉出一个筛选器,不平衡则报错

            } while (end > pos);

            start = pos; // 设置开始查找筛选器位置的起始位置

            if (!ConsumeTo(ElementsType))
            {
                rule.Add(queue.Substring(startX));
                return rule;
            }

            return SplitRuleNext(); // 递归匹配
        }

        /// <summary>
        /// 替换内嵌规则
        /// </summary>
        /// <param name="inner">起始标志,如{$.</param>
        /// <param name="startStep">不属于规则部分的前置字符长度，如{$.中{不属于规则的组成部分，故startStep为1</param>
        /// <param name="endStep">不属于规则部分的后置字符长度</param>
        /// <param name="fr">查找到内嵌规则时，用于解析的函数</param>
        /// <returns>处理后的字符串</returns>
        public string InnerRule(
            string inner,
            int startStep = 1,
            int endStep = 1,
            Func<string, string> fr = null)
        {
            StringBuilder st = new StringBuilder();

            while (ConsumeTo(inner))
            { // 拉取成功返回true，ruleAnalyzes里的字符序列索引变量pos后移相应位置，否则返回false,且isEmpty为true
                int posPre = pos; // 记录consumeTo匹配位置
                if (ChompCodeBalanced('{', '}'))
                {
                    string frv = fr(queue.Substring(posPre + startStep, pos - endStep - (posPre + startStep)));
                    if (!string.IsNullOrEmpty(frv))
                    {
                        st.Append(queue.Substring(startX, posPre - startX) + frv); // 压入内嵌规则前的内容，及内嵌规则解析得到的字符串
                        startX = pos; // 记录下次规则起点
                        continue; // 获取内容成功，继续选择下个内嵌规则
                    }
                }
                pos += inner.Length; // 拉出字段不平衡，inner只是个普通字串，跳到此inner后继续匹配
            }

            if (startX == 0)
                return "";

            st.Append(queue.Substring(startX));
            return st.ToString();
        }

        /// <summary>
        /// 替换内嵌规则
        /// </summary>
        /// <param name="fr">查找到内嵌规则时，用于解析的函数</param>
        /// <returns>处理后的字符串</returns>
        public string InnerRule(
            string startStr,
            string endStr,
            Func<string, string> fr)
        {
            StringBuilder st = new StringBuilder();
            while (ConsumeTo(startStr))
            { // 拉取成功返回true，ruleAnalyzes里的字符序列索引变量pos后移相应位置，否则返回false,且isEmpty为true
                pos += startStr.Length; // 跳过开始字符串
                int posPre = pos; // 记录consumeTo匹配位置
                if (ConsumeTo(endStr))
                {
                    string frv = fr(queue.Substring(posPre, pos - posPre));
                    st.Append(
                        queue.Substring(
                            startX,
                            posPre - startStr.Length - startX
                        ) + frv
                    ); // 压入内嵌规则前的内容，及内嵌规则解析得到的字符串
                    pos += endStr.Length; // 跳过结束字符串
                    startX = pos; // 记录下次规则起点
                }
            }

            if (startX == 0)
                return queue;

            st.Append(queue.Substring(startX));
            return st.ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core
{
    public interface IChineseToPinyin
    {
        string ToPinYin(string txt);
        string ToPinYinMulti(string txt);
        List<string> ToPinYinArray(string txt);
        string PinYinToFirstChar(string strOrgPinYin, bool zcsDouble);
        bool IsQuickSearchMatch(string keyword, string toCompoareText);
    }
}

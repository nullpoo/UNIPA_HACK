using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace UNIPAHACK
{
    static class ExtensionMethods
    {
        public static string ToHankaku(this string fromStr)
        {
            string ToStr = Strings.StrConv(fromStr, VbStrConv.Narrow);
            ToStr = ToStr.Replace(" ", "");
            return ToStr;
        }
    }
}

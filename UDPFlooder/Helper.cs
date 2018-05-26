using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPFlooder
{
    static class Helper
    {
        public static bool IsEmpty(this string str) => string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
    }
}

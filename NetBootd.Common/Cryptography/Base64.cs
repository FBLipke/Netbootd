using Netboot.Common.Cryptography.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Common.Cryptography
{
    public static class Base64
    {
        public static string FromBase64(string input, Encoding encoding)
            => encoding.GetString(Convert.FromBase64String(input));

        public static string ToBase64(string input, Encoding encoding)
            => Convert.ToBase64String(encoding.GetBytes(input));
    }
}

using System.Text;

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

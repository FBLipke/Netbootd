using System.Security.Cryptography;

namespace Netboot.Cryptography
{
    public class DSASHA1 :IDisposable
    {
        private SHA1 sha1;
        public DSASHA1()
        {
            sha1 = new SHA1CryptoServiceProvider();
        }

        public void Dispose()
        {
            Clear();
            sha1.Dispose();
        }

        public byte[] Hash(byte[] data)
        {
            return sha1.ComputeHash(data);
        }

        public void Clear()
        {
            sha1.Clear();
        }

        public void Transform(byte[] inputBuffer, int inputOffset, int inputCount, out byte[] target, int offset = 0)
        {
            var tgt = new byte[inputCount];
            sha1.TransformBlock(inputBuffer, inputOffset, inputCount, tgt, offset);
            target = tgt;
        }
    }
}

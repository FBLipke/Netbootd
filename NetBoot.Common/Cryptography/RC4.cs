/*
 * RC4 Encryption Engine
 * Based on RSA Security's RC4 algorithm
 */

using System;

namespace Netboot.Common.Cryptography
{
    /// <summary>
    /// RC4 encryption engine for NTLM sealing/unsealing and general purpose encryption
    /// </summary>
    public class RC4
    {
        private byte[] _state = new byte[256];
        private int _x;
        private int _y;

        /// <summary>
        /// Initializes the RC4 state with a key
        /// </summary>
        public void Init(byte[] key)
        {
            for (int i = 0; i < 256; i++)
                _state[i] = (byte)i;

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + _state[i] + key[i % key.Length]) % 256;
                (_state[i], _state[j]) = (_state[j], _state[i]);
            }
            _x = _y = 0;
        }

        /// <summary>
        /// Processes data (encrypts or decrypts - RC4 is self-inverting)
        /// </summary>
        public void Process(byte[] input, byte[] output)
        {
            for (int i = 0; i < input.Length; i++)
            {
                _x = (_x + 1) % 256;
                _y = (_y + _state[_x]) % 256;
                (_state[_x], _state[_y]) = (_state[_y], _state[_x]);
                output[i] = (byte)(input[i] ^ _state[(_state[_x] + _state[_y]) % 256]);
            }
        }

        /// <summary>
        /// Encrypts or decrypts data using RC4 (self-inverting operation)
        /// </summary>
        public static byte[] Crypt(byte[] key, byte[] data)
        {
            var rc4 = new RC4();
            rc4.Init(key);
            var output = new byte[data.Length];
            rc4.Process(data, output);
            return output;
        }
    }
}
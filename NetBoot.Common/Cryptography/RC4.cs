namespace NetBoot.Common.Cryptography
{

    public class RC4
    {
        private byte[] _state = new byte[256];
        private int _x;
        private int _y;

        void Init(byte[] key)
        {
            for (var i = 0; i < _state.Length; i++)
                _state[i] = (byte)i;

            int j = 0;
            for (var i = 0; i < _state.Length; i++)
            {
                j = (j + _state[i] + key[i % key.Length]) % _state.Length;
                (_state[i], _state[j]) = (_state[j], _state[i]);
            }

            _x = _y = 0;
        }

        void Process(byte[] input, byte[] output)
        {
            for (var i = 0; i < input.Length; i++)
            {
                _x = (_x + 1) % _state.Length;
                _y = (_y + _state[_x]) % _state.Length;
                (_state[_x], _state[_y]) = (_state[_y], _state[_x]);
                output[i] = (byte)(input[i] ^ _state[(_state[_x] + _state[_y]) % _state.Length]);
            }
        }

        public byte[] Crypt(byte[] key, byte[] data)
        {
            Init(key);
            var output = new byte[data.Length];
            Process(data, output);
            return output;
        }
    }
}
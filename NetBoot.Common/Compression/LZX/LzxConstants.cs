using System;

namespace Netboot.Common.Compression.LZX
{
	public static class LzxConstants
	{
		public const int NUM_CHARS = 256;
		public const int NUM_PRIMARY_LENGTHS = 8;
		public const int NUM_SECONDARY_LENGTHS = 249;
		
		public const int PRETREE_MAXSYMBOLS = 20;
		public const int PRETREE_TABLEBITS = 6;
		
		public const int MAINTREE_MAXSYMBOLS = 256 + (50 << 3);
		public const int MAINTREE_TABLEBITS = 10;
		
		public const int LENGTH_MAXSYMBOLS = NUM_SECONDARY_LENGTHS + (1 << 4);
		public const int LENGTH_TABLEBITS = 6;
		
		public const int ALIGNED_MAXSYMBOLS = 8;
		public const int ALIGNED_TABLEBITS = 3;
		
		public const int MIN_MATCH = 2;
		public const int MAX_MATCH = 257;
		
		public const int LENTABLE_SAFETY = 64;

		public enum BLOCKTYPE : int
		{
			INVALID = 0,
			VERBATIM = 1,
			ALIGNED = 2,
			UNCOMPRESSED = 3
		}
	}
}
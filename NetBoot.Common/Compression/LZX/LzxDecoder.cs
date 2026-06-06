using Netboot.Common.Compression.LZX;
using System;
using System.IO;

namespace Netboot.Common.Compression
{
	public class LzxDecoder
	{
		LzxState m_state;
		int window;

		static byte[] extra_bits = Array.Empty<byte>();
		static uint[] position_base = Array.Empty<uint>();

		public LzxDecoder(int windowOrder)
		{
			window = windowOrder;
			uint wndsize = (uint)(1 << window);

			if (window < 15 || window > 21)
				throw new ArgumentException("Window size must be between 15 and 21");

			m_state = new LzxState();
			m_state.actual_size = 0;
			m_state.window = new byte[wndsize];
			m_state.actual_size = wndsize;
			m_state.window_size = wndsize;
			m_state.window_posn = 0;

			if (extra_bits.Length == 0)
			{
				extra_bits = new byte[52];
				for (int i = 0, j = 0; i <= 50; i += 2)
				{
					extra_bits[i] = extra_bits[i + 1] = (byte)j;
					if ((i != 0) && (j < 17)) j++;
				}
			}

			if (position_base.Length == 0)
			{
				position_base = new uint[51];

				for (int i = 0, j = 0; i <= 50; i++)
				{
					position_base[i] = (uint)j;
					j += 1 << extra_bits[i];
				}
			}

			int posn_slots;
			if (window == 20)
				posn_slots = 42;
			else if (window == 21)
				posn_slots = 50;
			else
				posn_slots = window << 1;

			m_state.R0 = m_state.R1 = m_state.R2 = 1;
			m_state.main_elements = (ushort)(LzxConstants.NUM_CHARS + (posn_slots << 3));
			m_state.header_read = 0;
			m_state.frames_read = 0;
			m_state.block_remaining = 0;
			m_state.block_type = LzxConstants.BLOCKTYPE.INVALID;
			m_state.intel_curpos = 0;
			m_state.intel_started = 0;

			int table_size = (1 << LzxConstants.PRETREE_TABLEBITS) + (LzxConstants.PRETREE_MAXSYMBOLS << 1);
			m_state.PRETREE_table = new ushort[table_size];
			m_state.PRETREE_len = new byte[LzxConstants.PRETREE_MAXSYMBOLS + LzxConstants.LENTABLE_SAFETY];

			table_size = (1 << LzxConstants.MAINTREE_TABLEBITS) + (LzxConstants.MAINTREE_MAXSYMBOLS << 1);
			m_state.MAINTREE_table = new ushort[table_size];
			m_state.MAINTREE_len = new byte[LzxConstants.MAINTREE_MAXSYMBOLS + LzxConstants.LENTABLE_SAFETY];

			table_size = (1 << LzxConstants.LENGTH_TABLEBITS) + (LzxConstants.LENGTH_MAXSYMBOLS << 1);
			m_state.LENGTH_table = new ushort[table_size];
			m_state.LENGTH_len = new byte[LzxConstants.LENGTH_MAXSYMBOLS + LzxConstants.LENTABLE_SAFETY];

			table_size = (1 << LzxConstants.ALIGNED_TABLEBITS) + (LzxConstants.ALIGNED_MAXSYMBOLS << 1);
			m_state.ALIGNED_table = new ushort[table_size];
			m_state.ALIGNED_len = new byte[LzxConstants.ALIGNED_MAXSYMBOLS + LzxConstants.LENTABLE_SAFETY];

			for (var i = 0; i < LzxConstants.MAINTREE_MAXSYMBOLS; i++)
				m_state.MAINTREE_len[i] = 0;

			for (var i = 0; i < LzxConstants.LENGTH_MAXSYMBOLS; i++)
				m_state.LENGTH_len[i] = 0;
		}

		public static (LZXCompression compression, int windowOrder) ParseCompressionType(ushort typeCompress)
		{
			byte variant = (byte)(typeCompress & 0xFF);
			int windowOrder = (typeCompress >> 8) & 0xFF;

			LZXCompression compression = variant switch
			{
				0 => LZXCompression.None,
				1 => LZXCompression.MSZIP,
				2 or 3 => LZXCompression.LZX,
				_ => LZXCompression.Unknown
			};

			return (compression, windowOrder);
		}

		public byte[] Decode(byte[] compressedData, int uncompressedSize)
		{
			using var inStream = new MemoryStream(compressedData);
			using var outStream = new MemoryStream(uncompressedSize);

			var bitbuf = new BitBuffer(inStream);
			bitbuf.InitBitStream();

			uint R0 = m_state.R0, R1 = m_state.R1, R2 = m_state.R2;

			if (m_state.header_read == 0)
			{
				var intel = bitbuf.ReadBits(1);
				if (intel != 0)
				{
					var i = bitbuf.ReadBits(16);
					var j = bitbuf.ReadBits(16);
					m_state.intel_filesize = (int)((i << 16) | j);
				}
				m_state.header_read = 1;
			}

			var togo = uncompressedSize;

			while (togo > 0)
			{
				if (m_state.block_remaining == 0)
				{
					if (m_state.block_type == LzxConstants.BLOCKTYPE.UNCOMPRESSED)
					{
						if ((m_state.block_length & 1) == 1)
							inStream.ReadByte();

						bitbuf.InitBitStream();
					}

					m_state.block_type = (LzxConstants.BLOCKTYPE)bitbuf.ReadBits(3);

					var i = bitbuf.ReadBits(16);
					var j = bitbuf.ReadBits(8);

					m_state.block_remaining = m_state.block_length = (i << 8) | j;

					switch (m_state.block_type)
					{
						case LzxConstants.BLOCKTYPE.ALIGNED:
							for (var k = 0; k < 8; k++)
								m_state.ALIGNED_len[k] = (byte)bitbuf.ReadBits(3);

							MakeDecodeTable(LzxConstants.ALIGNED_MAXSYMBOLS, LzxConstants.ALIGNED_TABLEBITS,
								m_state.ALIGNED_len, m_state.ALIGNED_table);

							goto case LzxConstants.BLOCKTYPE.VERBATIM;

						case LzxConstants.BLOCKTYPE.VERBATIM:
							#region "PRETREE"
							// Read PRETREE lengths first (20 symbols Ã— 4 bits each)
							for (var k = 0; k < 20; k++)
								m_state.PRETREE_len[k] = (byte)bitbuf.ReadBits(4);

						   var resP = MakeDecodeTable(LzxConstants.PRETREE_MAXSYMBOLS, LzxConstants.PRETREE_TABLEBITS,
								m_state.PRETREE_len, m_state.PRETREE_table);

						   // Functions._ASSERT_(resP != 0, $"MakeDecodeTable failed for PRETREE: rc={resP}");

							Console.WriteLine($"[D] After PRETREE MakeDecodeTable: table[0]={m_state.PRETREE_table[0]}," +
								$" table[1]={m_state.PRETREE_table[1]}, table[2]={m_state.PRETREE_table[2]}, ...");

							// DEBUG
							for (var x = 0; x < 32; x++)
								Console.Write($"{m_state.PRETREE_table[x]} ");

							Console.Write("MAINTREE lens: ");
							for (var y = 0; y < 32; y++)
								Console.Write($"{m_state.PRETREE_len[y]} ");

							Console.WriteLine();
							#endregion

							#region "MAINTREE"
							// Read MAINTREE lengths using PRETREE
							ReadLengths(m_state.MAINTREE_len, 0, 256, bitbuf);
							ReadLengths(m_state.MAINTREE_len, 256, m_state.main_elements, bitbuf);


						   var res = MakeDecodeTable(LzxConstants.MAINTREE_MAXSYMBOLS, LzxConstants.MAINTREE_TABLEBITS,
								m_state.MAINTREE_len, m_state.MAINTREE_table);

							Functions._ASSERT_(res != 0, $"MakeDecodeTable failed for MAINTREE: rc={res}");

							// DEBUG
							for (var x = 0; x < 32; x++)
								Console.Write($"{m_state.MAINTREE_table[x]} ");

							Console.Write("MAINTREE lens: ");
							for (var y = 0; y < 32; y++)
								Console.Write($"{m_state.MAINTREE_len[y]} ");

							Console.WriteLine();

							if (m_state.MAINTREE_len[232] != 0)
								m_state.intel_started = 1;
							#endregion

							#region "LENGTH"
							// Read LENGTH tree lengths using PRETREE
							ReadLengths(m_state.LENGTH_len, 0, LzxConstants.NUM_SECONDARY_LENGTHS, bitbuf);
						   var resL = MakeDecodeTable(LzxConstants.LENGTH_MAXSYMBOLS, LzxConstants.LENGTH_TABLEBITS,
								m_state.LENGTH_len, m_state.LENGTH_table);

							Functions._ASSERT_(resL != 0, $"MakeDecodeTable failed for LENGTHTREE: rc={resL}");
							#endregion
							break;

						case LzxConstants.BLOCKTYPE.UNCOMPRESSED:
							m_state.intel_started = 1;
							bitbuf.EnsureBits(16);

							if (bitbuf.GetBitsLeft() > 16)
								inStream.Seek(-2, SeekOrigin.Current);

							R0 = ReadUInt32LE(inStream);
							R1 = ReadUInt32LE(inStream);
							R2 = ReadUInt32LE(inStream);
							break;

						default:
							throw new InvalidDataException("Invalid LZX block type");
					}
				}

				var this_run = (int)m_state.block_remaining;
				if (this_run > togo)
					this_run = togo;

				togo -= this_run;
				m_state.block_remaining -= (uint)this_run;

				m_state.window_posn &= m_state.window_size - 1;

				if ((m_state.window_posn + (uint)this_run) > m_state.window_size)
					throw new InvalidDataException("LZX: Window overflow");

				switch (m_state.block_type)
				{
					case LzxConstants.BLOCKTYPE.VERBATIM:
					case LzxConstants.BLOCKTYPE.ALIGNED:
						while (this_run > 0)
						{
							var main_element = ReadHuffSym(m_state.MAINTREE_table, m_state.MAINTREE_len,
								LzxConstants.MAINTREE_MAXSYMBOLS, LzxConstants.MAINTREE_TABLEBITS, bitbuf);

							if (main_element < LzxConstants.NUM_CHARS)
							{
								m_state.window[m_state.window_posn++] = (byte)main_element;
								this_run--;
							}
							else
							{
								main_element -= LzxConstants.NUM_CHARS;

								var match_length = (int)(main_element & LzxConstants.NUM_PRIMARY_LENGTHS);
								if (match_length == LzxConstants.NUM_PRIMARY_LENGTHS)
								{
									var length_footer = ReadHuffSym(m_state.LENGTH_table, m_state.LENGTH_len,
										LzxConstants.LENGTH_MAXSYMBOLS, LzxConstants.LENGTH_TABLEBITS, bitbuf);

									match_length += (int)length_footer;
								}

								match_length += LzxConstants.MIN_MATCH;

								var match_offset = (int)(main_element >> 3);

								if (match_offset > 2)
								{
									var extra = extra_bits[match_offset];
									match_offset = (int)position_base[match_offset] - 2;

									if (extra > 3 && m_state.block_type == LzxConstants.BLOCKTYPE.ALIGNED)
									{
										extra -= 3;
										var verbatim_bits = (int)bitbuf.ReadBits(extra);
										match_offset += verbatim_bits << 3;

										var aligned_bits = (int)ReadHuffSym(m_state.ALIGNED_table, m_state.ALIGNED_len,
											LzxConstants.ALIGNED_MAXSYMBOLS, LzxConstants.ALIGNED_TABLEBITS, bitbuf);

										match_offset += aligned_bits;
									}
									else if (extra == 3 && m_state.block_type == LzxConstants.BLOCKTYPE.ALIGNED)
									{
										var aligned_bits = (int)ReadHuffSym(m_state.ALIGNED_table, m_state.ALIGNED_len,
											LzxConstants.ALIGNED_MAXSYMBOLS, LzxConstants.ALIGNED_TABLEBITS, bitbuf);

										match_offset += aligned_bits;
									}
									else if (extra > 0)
									{
										var verbatim_bits = (int)bitbuf.ReadBits(extra);
										match_offset += verbatim_bits;
									}
									else
									{
										match_offset = 1;
									}

									R2 = R1; R1 = R0; R0 = (uint)match_offset;
								}
								else if (match_offset == 0)
								{
									match_offset = (int)R0;
								}
								else if (match_offset == 1)
								{
									match_offset = (int)R1;
									R1 = R0; R0 = (uint)match_offset;
								}
								else
								{
									match_offset = (int)R2;
									R2 = R0; R0 = (uint)match_offset;
								}

								var rundest = (int)m_state.window_posn;
								this_run -= match_length;

								var runsrc = rundest - match_offset;
								if (runsrc < 0)
								{
									runsrc += (int)m_state.window_size;

									var copy_length = match_offset - (int)m_state.window_posn;
									if (copy_length < match_length)
									{
										match_length -= copy_length;
										m_state.window_posn += (uint)copy_length;

										while (copy_length-- > 0)
											m_state.window[rundest++] = m_state.window[runsrc++];

										runsrc = 0;
									}
								}

								m_state.window_posn += (uint)match_length;

								while (match_length-- > 0)
									m_state.window[rundest++] = m_state.window[runsrc++];
							}
						}
						break;

					case LzxConstants.BLOCKTYPE.UNCOMPRESSED:
						var pos = (int)m_state.window_posn;
						var read = inStream.Read(m_state.window, pos, this_run);

						if (read < this_run)
							throw new EndOfStreamException("Unexpected end of stream");

						m_state.window_posn += (uint)read;
						break;
				}
			}

			var start_window_pos = (int)m_state.window_posn;
			if (start_window_pos == 0)
				start_window_pos = (int)m_state.window_size;

			start_window_pos -= uncompressedSize;

			var result = new byte[uncompressedSize];
			Array.Copy(m_state.window, start_window_pos, result, 0, uncompressedSize);

			m_state.R0 = R0;
			m_state.R1 = R1;
			m_state.R2 = R2;

			return result;
		}

		uint ReadUInt32LE(Stream s)
		{
			int b0 = s.ReadByte();
			int b1 = s.ReadByte();
			int b2 = s.ReadByte();
			int b3 = s.ReadByte();
			return (uint)((b3 << 24) | (b2 << 16) | (b1 << 8) | b0);
		}

		uint ReadHuffSym(ushort[] table, byte[] length, uint maxsym, uint tablebits, BitBuffer bitbuf)
		{
			bitbuf.EnsureBits(16);

			uint idx = bitbuf.bit_buffer & ((1u << (int)tablebits) - 1);
			uint sym = table[idx];

			Console.WriteLine($"[D] ReadHuffSym: idx={idx}, sym={sym}, maxsym={maxsym}");

			if (sym >= maxsym)
			{
				// Start at position tablebits (NOT tablebits-1!)
				// Because the initial lookup consumed bits 0 to (tablebits-1)
				// The NEXT bit to read is at position tablebits
				int bit_pos = (int)tablebits;
				do
				{
					if (++bit_pos > 16)
						throw new InvalidDataException("[D] Invalid Huffman tree traversal");

					sym = table[(int)(sym << 1) | ((bitbuf.bit_buffer >> bit_pos) & 1u)];
					Console.WriteLine($"[D] Traverse: bit_pos={bit_pos}, sym={sym}");
				} while (sym >= maxsym);

				int consumed = 1 + (bit_pos - (int)tablebits); // 1 for first tree bit + tree depth
				bitbuf.bit_buffer >>= consumed;
				bitbuf.bits_left -= consumed;
			}
			else
			{
				bitbuf.bit_buffer >>= (int)tablebits;
				bitbuf.bits_left -= (int)tablebits;
			}

			return sym;
		}


		int MakeDecodeTable(uint nsyms, uint nbits, byte[] length, ushort[] table)
		{
			ushort sym;
			ushort next_symbol;
			uint leaf;
			uint fill;
			byte bit_num;
			uint pos = 0;
			uint table_mask = (uint)(1 << (int)nbits);
			uint bit_mask = table_mask >> 1;

			// Phase 1: fill entries for codes short enough for a direct mapping
			for (bit_num = 1; bit_num <= nbits; bit_num++)
			{
				for (sym = 0; sym < nsyms; sym++)
				{
					if (length[sym] != bit_num) continue;

					leaf = pos;
					if ((pos += bit_mask) > table_mask) return 1; // table overrun

					// fill all possible lookups of this symbol with the symbol itself
					for (fill = bit_mask; fill-- > 0;) table[leaf++] = sym;
				}
				bit_mask >>= 1;
			}

			// exit with success if table is now complete
			if (pos == table_mask) return 0;

			// mark all remaining table entries as unused (0xFFFF)
			for (sym = (ushort)pos; sym < table_mask; sym++)
				table[sym] = 0xFFFF;

			// next_symbol = base of allocation for long codes
			next_symbol = (ushort)(((table_mask >> 1) < nsyms) ? nsyms : (table_mask >> 1));

			// give ourselves room for codes to grow by up to 16 more bits.
			// codes now start at bit nbits+16 and end at (nbits+16-codelength)
			pos <<= 16;
			table_mask <<= 16;
			bit_mask = 1u << 15;

			for (bit_num = (byte)(nbits + 1); bit_num <= 16; bit_num++)
			{
				for (sym = 0; sym < nsyms; sym++)
				{
					if (length[sym] != bit_num) continue;
					if (pos >= table_mask) return 1; // table overflow

					leaf = pos >> 16;
					for (fill = 0; fill < (bit_num - nbits); fill++)
					{
						if (table[leaf] == 0xFFFF)
						{
							var newNode = next_symbol++;
							table[(int)(newNode << 1)] = 0xFFFF;
							table[(int)(newNode << 1) + 1] = 0xFFFF;
							table[(int)leaf] = newNode;
							Console.WriteLine($"[D] ALLOC: leaf={leaf} -> node={newNode}, children at {newNode << 1},{(newNode << 1) + 1}");
						}

						leaf = (uint)(table[leaf] << 1);
						Console.WriteLine($"[D] TRAVERSE: leaf={leaf}, pos_bit={((pos >> (15 - (int)fill)) & 1u)}");
						if (((pos >> (15 - (int)fill)) & 1u) != 0) leaf++;
					}
					table[leaf] = sym;
					pos += bit_mask;
				}
				bit_mask >>= 1;
			}

			// full table?
			return (pos == table_mask) ? 0 : 1;
		}



		void ReadLengths(byte[] lens, uint first, uint last, BitBuffer bitbuf)
		{
			var i = first;

			while (i < last)
			{
				var z = ReadHuffSym(m_state.PRETREE_table, m_state.PRETREE_len,
					LzxConstants.PRETREE_MAXSYMBOLS, LzxConstants.PRETREE_TABLEBITS, bitbuf);

				if (z == 17)
				{
					var y = bitbuf.ReadBits(4) + 4;
					while (y-- > 0 && i < last)
						lens[i++] = 0;
				}
				else if (z == 18)
				{
					var y = bitbuf.ReadBits(5) + 20;
					while (y-- > 0 && i < last)
						lens[i++] = 0;
				}
				else if (z == 19)
				{
					var y = bitbuf.ReadBits(1) + 4;

					var z2 = ReadHuffSym(m_state.PRETREE_table, m_state.PRETREE_len,
						LzxConstants.PRETREE_MAXSYMBOLS, LzxConstants.PRETREE_TABLEBITS, bitbuf);

					var value = lens[i] - (int)z2;
					if (value < 0)
						value += 17;

					while (y-- > 0 && i < last)
						lens[i++] = (byte)value;
				}
				else
				{
					var value = lens[i] - (int)z;
					if (value < 0)
						value += 17;

					lens[i++] = (byte)value;
				}
			}
		}
	}
}
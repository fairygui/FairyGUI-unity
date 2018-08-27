using System;
using System.Text;
using UnityEngine;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class ByteBuffer
	{
		/// <summary>
		/// 
		/// </summary>
		public bool littleEndian;

		/// <summary>
		/// 
		/// </summary>
		public string[] stringTable;

		/// <summary>
		/// 
		/// </summary>
		public int version;

		int _pointer;
		int _offset;
		int _length;
		byte[] _data;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		public ByteBuffer(byte[] data, int offset = 0, int length = -1)
		{
			_data = data;
			_pointer = 0;
			_offset = offset;
			if (length < 0)
				_length = data.Length - offset;
			else
				_length = length;
			littleEndian = false;
		}

		/// <summary>
		/// 
		/// </summary>
		public int position
		{
			get { return _pointer; }
			set { _pointer = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int length
		{
			get { return _length; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool bytesAvailable
		{
			get { return _pointer < _length; }
		}

		/// <summary>
		/// 
		/// </summary>
		public byte[] buffer
		{
			get { return _data; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public int Skip(int count)
		{
			_pointer += count;
			return _pointer;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public byte ReadByte()
		{
			return _data[_offset + _pointer++];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="output"></param>
		/// <param name="destIndex"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public byte[] ReadBytes(byte[] output, int destIndex, int count)
		{
			if (count > _length - _pointer)
				throw new ArgumentOutOfRangeException();

			Array.Copy(_data, _offset + _pointer, output, destIndex, count);
			_pointer += count;
			return output;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public byte[] ReadBytes(int count)
		{
			if (count > _length - _pointer)
				throw new ArgumentOutOfRangeException();

			byte[] result = new byte[count];
			Array.Copy(_data, _offset + _pointer, result, 0, count);
			_pointer += count;
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public ByteBuffer ReadBuffer()
		{
			int count = ReadInt();
			ByteBuffer ba = new ByteBuffer(_data, _pointer, count);
			ba.stringTable = stringTable;
			ba.version = version;
			_pointer += count;
			return ba;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public char ReadChar()
		{
			return (char)ReadShort();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool ReadBool()
		{
			bool result = _data[_offset + _pointer] == 1;
			_pointer++;
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public unsafe short ReadShort()
		{
			int startIndex = _offset + _pointer;
			_pointer += 2;
			fixed (byte* pbyte = &_data[startIndex])
			{
				if (littleEndian)
				{
					return (short)((*pbyte) | (*(pbyte + 1) << 8));
				}
				else
				{
					return (short)((*pbyte << 8) | (*(pbyte + 1)));
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public ushort ReadUshort()
		{
			return (ushort)ReadShort();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public unsafe int ReadInt()
		{
			int startIndex = _offset + _pointer;
			_pointer += 4;
			fixed (byte* pbyte = &_data[startIndex])
			{
				if (littleEndian)
				{
					return (*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24);
				}
				else
				{
					return (*pbyte << 24) | (*(pbyte + 1) << 16) | (*(pbyte + 2) << 8) | (*(pbyte + 3));
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public uint ReadUint()
		{
			return (uint)ReadInt();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public unsafe float ReadFloat()
		{
			int val = ReadInt();
			return *(float*)&val;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public unsafe long ReadLong()
		{
			int startIndex = _offset + _pointer;
			_pointer += 8;
			fixed (byte* pbyte = &_data[startIndex])
			{
				if (littleEndian)
				{
					int i1 = (*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24);
					int i2 = (*(pbyte + 4)) | (*(pbyte + 5) << 8) | (*(pbyte + 6) << 16) | (*(pbyte + 7) << 24);
					return (uint)i1 | ((long)i2 << 32);
				}
				else
				{
					int i1 = (*pbyte << 24) | (*(pbyte + 1) << 16) | (*(pbyte + 2) << 8) | (*(pbyte + 3));
					int i2 = (*(pbyte + 4) << 24) | (*(pbyte + 5) << 16) | (*(pbyte + 6) << 8) | (*(pbyte + 7));
					return (uint)i2 | ((long)i1 << 32);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public unsafe double ReadDouble()
		{
			long val = ReadLong();
			return *(double*)&val;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string ReadString()
		{
			ushort len = ReadUshort();
			string result = Encoding.UTF8.GetString(_data, _offset + _pointer, len);
			_pointer += len;
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="len"></param>
		/// <returns></returns>
		public string ReadString(int len)
		{
			string result = Encoding.UTF8.GetString(_data, _offset + _pointer, len);
			_pointer += len;
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string ReadS()
		{
			int index = ReadUshort();
			if (index == 65534) //null
				return null;
			else if (index == 65533)
				return string.Empty;
			else
				return stringTable[index];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void WriteS(string value)
		{
			int index = ReadUshort();
			if (index != 65534 && index != 65533)
				stringTable[index] = value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Color ReadColor()
		{
			int startIndex = _offset + _pointer;
			byte r = _data[startIndex];
			byte g = _data[startIndex + 1];
			byte b = _data[startIndex + 2];
			byte a = _data[startIndex + 3];
			_pointer += 4;

			return new Color32(r, g, b, a);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="indexTablePos"></param>
		/// <param name="blockIndex"></param>
		/// <returns></returns>
		public bool Seek(int indexTablePos, int blockIndex)
		{
			int tmp = _pointer;
			_pointer = indexTablePos;
			int segCount = _data[_offset + _pointer++];
			if (blockIndex < segCount)
			{
				bool useShort = _data[_offset + _pointer++] == 1;
				int newPos;
				if (useShort)
				{
					_pointer += 2 * blockIndex;
					newPos = ReadShort();
				}
				else
				{
					_pointer += 4 * blockIndex;
					newPos = ReadInt();
				}

				if (newPos > 0)
				{
					_pointer = indexTablePos + newPos;
					return true;
				}
				else
				{
					_pointer = tmp;
					return false;
				}
			}
			else
			{
				_pointer = tmp;
				return false;
			}
		}
	}
}

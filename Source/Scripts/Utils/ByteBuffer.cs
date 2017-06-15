using System;
using System.Text;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class ByteBuffer
	{
		public enum Endian
		{
			BIG_ENDIAN,
			LITTLE_ENDIAN
		}

		public Endian endian;
		public int length { get; private set; }

		public int position
		{
			get { return this._pointer; }
			set
			{
				int v = this.length == 0 ? 0 : this.length - 1;
				this._pointer = value > v ? v : (value < 0 ? 0 : value);
			}
		}

		public bool bytesAvailable
		{
			get { return this._pointer < this.length; }
		}

		public byte[] buffer
		{
			get { return _data; }
		}

		int _pointer;
		byte[] _tmp;
		byte[] _data;

		public ByteBuffer(byte[] data)
		{
			if (data == null || data.Length == 0)
				throw new ArgumentException("Parameter is empty or zero length.");

			this._data = data;
			this._tmp = new byte[8];
			this._pointer = 0;
			this.length = data.Length;
		}

		public int SkipBytes(int numberBytes)
		{
			position += numberBytes;
			return position;
		}

		public byte ReadByte()
		{
			return this._data[this._pointer++];
		}

		public byte[] ReadBytes(ref byte[] output, int startIndex, int destIndex, int length)
		{
			Array.Copy(this._data, startIndex, output, destIndex, length);
			return output;
		}

		public byte[] ReadBytes(ref byte[] output, int destIndex, int length)
		{
			Array.Copy(this._data, this._pointer, output, destIndex, length);
			this._pointer += length;
			return output;
		}

		public int ReadInt()
		{
			Array.Copy(this._data, this._pointer, this._tmp, 0, 4);
			if (endian != Endian.LITTLE_ENDIAN)
				Array.Reverse(this._tmp, 0, 4);
			int result = BitConverter.ToInt32(this._tmp, 0);
			this._pointer += 4;
			return result;
		}

		public uint ReadUint()
		{
			Array.Copy(this._data, this._pointer, this._tmp, 0, 4);
			if (endian != Endian.LITTLE_ENDIAN)
				Array.Reverse(this._tmp, 0, 4);
			uint result = BitConverter.ToUInt32(this._tmp, 0);
			this._pointer += 4;
			return result;
		}

		public float ReadFloat()
		{
			Array.Copy(this._data, this._pointer, this._tmp, 0, 4);
			if (endian != Endian.LITTLE_ENDIAN)
				Array.Reverse(this._tmp, 0, 4);
			float result = BitConverter.ToSingle(this._tmp, 0);
			this._pointer += 4;
			return result;
		}

		public long ReadLong()
		{
			Array.Copy(this._data, this._pointer, this._tmp, 0, 8);
			if (endian != Endian.LITTLE_ENDIAN)
				Array.Reverse(this._tmp, 0, 8);
			long result = BitConverter.ToInt64(this._tmp, 0);
			this._pointer += 8;
			return result;
		}

		public double ReadDouble()
		{
			Array.Copy(this._data, this._pointer, this._tmp, 0, 8);
			if (endian != Endian.LITTLE_ENDIAN)
				Array.Reverse(this._tmp, 0, 8);
			double result = BitConverter.ToDouble(this._tmp, 0);
			this._pointer += 8;
			return result;
		}

		public short ReadShort()
		{
			Array.Copy(this._data, this._pointer, this._tmp, 0, 2);
			if (endian != Endian.LITTLE_ENDIAN)
				Array.Reverse(this._tmp, 0, 2);
			short result = BitConverter.ToInt16(this._tmp, 0);
			this._pointer += 2;
			return result;
		}

		public ushort ReadUshort()
		{
			Array.Copy(this._data, this._pointer, this._tmp, 0, 2);
			if (endian != Endian.LITTLE_ENDIAN)
				Array.Reverse(this._tmp, 0, 2);
			ushort result = BitConverter.ToUInt16(this._tmp, 0);
			this._pointer += 2;
			return result;
		}

		public char ReadChar()
		{
			Array.Copy(this._data, this._pointer, this._tmp, 0, 2);
			char result = BitConverter.ToChar(this._tmp, 0);
			if (endian != Endian.LITTLE_ENDIAN)
				Array.Reverse(this._tmp, 0, 2);
			this._pointer += 2;
			return result;
		}

		public bool ReadBool()
		{
			bool result = BitConverter.ToBoolean(this._data, this._pointer);
			this._pointer++;
			return result;
		}

		public string ReadString()
		{
			ushort len = this.ReadUshort();
			string result = Encoding.UTF8.GetString(this._data, this._pointer, len);
			this._pointer += len;
			return result;
		}

		public string ReadString(int len)
		{
			if (len <= 0)
				throw new ArgumentException("argument less than 0");
			string result = Encoding.UTF8.GetString(this._data, this._pointer, len);
			this._pointer += len;
			return result;
		}
	}
}

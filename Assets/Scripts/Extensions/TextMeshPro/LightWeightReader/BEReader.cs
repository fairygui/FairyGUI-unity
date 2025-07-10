#if FAIRYGUI_TMPRO
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace FairyGUI
{

    internal unsafe class BEReader
    {
        [ThreadStatic] static byte[] tls_buffer = new byte[16];

        private Stream m_stream;

        public long Position
        {
            get => m_stream.Position;
            set => m_stream.Position = value;
        }

        public BEReader(Stream stream)
        {
            this.m_stream = stream;
        }

        public string ReadString(int numBytes, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;

            if (numBytes <= 16)
            {
                tls_buffer ??= new byte[16];
                m_stream.Read(tls_buffer, 0, numBytes);
                return encoding.GetString(tls_buffer, 0, numBytes);
            }
            else
            {
                var largeBytes = ArrayPool<byte>.Shared.Rent(numBytes);
                try
                {
                    m_stream.Read(largeBytes, 0, numBytes);
                    return encoding.GetString(largeBytes, 0, numBytes);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(largeBytes);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ReadChar() => ReadPrimite<char>(m_stream);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte() => ReadPrimite<byte>(m_stream);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32() => ReadPrimite<int>(m_stream);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64() => ReadPrimite<long>(m_stream);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte() => ReadPrimite<sbyte>(m_stream);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16() => ReadPrimite<short>(m_stream);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16() => ReadPrimite<ushort>(m_stream);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32() => ReadPrimite<uint>(m_stream);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64() => ReadPrimite<ulong>(m_stream);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T ReadPrimite<T>(Stream stream) where T : unmanaged
        {
            tls_buffer ??= new byte[16];
            stream.Read(tls_buffer, 0, sizeof(T));
            fixed (byte* p = tls_buffer)
            {
                var raw = *(T*)p;
                if (BitConverter.IsLittleEndian)
                {
                    for (int i = 0; i < sizeof(T) >> 1; i++)
                    {
                        int j = sizeof(T) - 1 - i;
                        *(p + i) ^= *(p + j);
                        *(p + j) ^= *(p + i);
                        *(p + i) ^= *(p + j);
                    }
                }

                return *(T*)p;
            }
        }

    }

}

#endif

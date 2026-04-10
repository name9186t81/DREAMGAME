using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Networking
{
    public static class NetworkUtils
    {
        public static readonly int PackageHeaderSize;

        static NetworkUtils()
        {
            PackageHeaderSize = sizeof(PackageType);
        }

        public static byte CompressRotation(float rotation)
        {
            return (byte)(255 * ((((rotation % 360) + 360) % 360) / 360f));
        }

        public static float GetRotation(byte compressedRotation)
        {
            return (float)(compressedRotation / 255f * 360);
        }

        public static byte CompressRotationRadians(float rotation)
        {
            const float PI2 = Mathf.PI * 2;
            return (byte)(255 * (((rotation % PI2) + PI2) % PI2));
        }

        public static int GetFullSize(this IPackage package)
        {
            int add = 0;
            if (package.NeedACK) add += sizeof(short);
            return PackageHeaderSize + add + package.DataSize;
        }

        public static int GetOffset(this IPackage package)
        {
            if (package.NeedACK) return PackageHeaderSize + sizeof(short);
            return PackageHeaderSize;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddVector2ToBuffer(this Vector2 vector, byte[] buffer, int offset)
        {
            Convert(BitConverter.SingleToInt32Bits(vector.x), buffer, offset);
            Convert(BitConverter.SingleToInt32Bits(vector.y), buffer, offset + sizeof(float));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetVector2FromBuffer(byte[] buffer, int offset)
        {
            return new Vector2(BitConverter.Int32BitsToSingle(BitConverter.ToInt32(buffer, offset)), BitConverter.Int32BitsToSingle(BitConverter.ToInt32(buffer, offset + sizeof(float))));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetVector2FromBuffer(ReadOnlySpan<byte> buffer, int offset)
        {
            return new Vector2(BitConverter.ToSingle(buffer.Slice(offset, sizeof(float))), BitConverter.ToSingle(buffer.Slice(offset + sizeof(float), sizeof(float))));
        }

        public static PackageType GetPackageType(ReadOnlySpan<byte> data)
        {
            if (data.Length < sizeof(PackageType))
            {
                return PackageType.Invalid;
            }

            try
            {
                var type = Enum.GetUnderlyingType(typeof(PackageType));
                object enumObj = null;

                if (type == typeof(byte))
                {
                    enumObj = data[0];
                }
                else if (type == typeof(short))
                {
                    enumObj = BitConverter.ToInt16(data);
                }
                else if (type == typeof(int))
                {
                    enumObj = BitConverter.ToInt32(data);
                }
                else if (type == typeof(long))
                {
                    enumObj = BitConverter.ToInt64(data);
                }
                else
                {
                    return PackageType.Invalid;
                }

                return (PackageType)Enum.ToObject(typeof(PackageType), enumObj);
            }
            catch (Exception ex)
            {
                return PackageType.Invalid;
            }
        }

        public static void PackageTypeToByteArray(PackageType type, ref byte[] data)
        {
            byte[] buffer = new byte[sizeof(PackageType)];
            try
            {
                var ttype = Enum.GetUnderlyingType(typeof(PackageType));

                if (ttype == typeof(byte))
                {
                    buffer = BitConverter.GetBytes((byte)type);
                }
                else if (ttype == typeof(short))
                {
                    buffer = BitConverter.GetBytes((short)type);
                }
                else if (ttype == typeof(int))
                {
                    buffer = BitConverter.GetBytes((int)type);
                }
                else if (ttype == typeof(long))
                {
                    buffer = BitConverter.GetBytes((long)type);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"SERVER UTILS: Cannot convert package to byte array {ex.Message}");
            }

            Array.Copy(buffer, data, sizeof(PackageType));
        }

        public static void Convert(this byte value, byte[] array, int offset)
        {
            array[offset] = (byte)(value & 0xFF);
        }

        public static void Convert(this int value, byte[] array, int offset)
        {
            array[offset + 0] = (byte)(value & 0xFF);
            array[offset + 1] = (byte)((value >> 8) & 0xFF);
            array[offset + 2] = (byte)((value >> 16) & 0xFF);
            array[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        public static void Convert(this int value, byte[] array, int offset, int intSize = 4)
        {
            for (int i = 0; i < intSize && i < 4; i++)
            {
                array[offset + i] = (byte)((value >> (8 * i)) & 0xFF);
            }
        }

        public static void Convert(this short value, byte[] array, int offset)
        {
            array[offset + 0] = (byte)(value & 0xFF);
            array[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        public static void Convert(this ushort value, byte[] array, int offset)
        {
            array[offset + 0] = (byte)(value & 0xFF);
            array[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        public static void Convert(this uint value, byte[] array, int offset)
        {
            array[offset + 0] = (byte)(value & 0xFF);
            array[offset + 1] = (byte)((value >> 8) & 0xFF);
            array[offset + 2] = (byte)((value >> 16) & 0xFF);
            array[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        public static void Convert(this long value, byte[] array, int offset)
        {
            array[offset + 0] = (byte)(value & 0xFF);
            array[offset + 1] = (byte)((value >> 8) & 0xFF);
            array[offset + 2] = (byte)((value >> 16) & 0xFF);
            array[offset + 3] = (byte)((value >> 24) & 0xFF);
            array[offset + 4] = (byte)((value >> 32) & 0xFF);
            array[offset + 5] = (byte)((value >> 40) & 0xFF);
            array[offset + 6] = (byte)((value >> 48) & 0xFF);
            array[offset + 7] = (byte)((value >> 56) & 0xFF);
        }

        public static void Convert(this ulong value, byte[] array, int offset)
        {
            array[offset + 0] = (byte)(value & 0xFF);
            array[offset + 1] = (byte)((value >> 8) & 0xFF);
            array[offset + 2] = (byte)((value >> 16) & 0xFF);
            array[offset + 3] = (byte)((value >> 24) & 0xFF);
            array[offset + 4] = (byte)((value >> 32) & 0xFF);
            array[offset + 5] = (byte)((value >> 40) & 0xFF);
            array[offset + 6] = (byte)((value >> 48) & 0xFF);
            array[offset + 7] = (byte)((value >> 56) & 0xFF);
        }
    }
}
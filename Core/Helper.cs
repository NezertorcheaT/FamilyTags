using System;
using System.Text;

namespace Core
{
    public static partial class Helper
    {
        /// <summary>
        /// Coverts byte array with length of 4 into Int32
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Length of byte array must be only 4, to be convertable to Int32</exception>
        public static int ToInt(this byte[] bytes)
        {
            if (bytes.Length != 4)
                throw new ArgumentOutOfRangeException(
                    $"Length of {nameof(bytes)} must be only 4, to be convertable to Int32");
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes);
        }

        /// <summary>
        /// Converts Int32 to byte[] with length of 4
        /// </summary>
        /// <param name="i"></param>
        /// <returns>byte[] with length of 4</returns>
        public static byte[] ToBytes(this int i)
        {
            var intBytes = BitConverter.GetBytes(i);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            return intBytes;
        }

        /// <summary>
        /// Converts byte[] to string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToUTF8(this byte[] data) => Encoding.UTF8.GetString(data);

        /// <summary>
        /// Converts String to byte[]
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string i) => Encoding.UTF8.GetBytes(i);

        public static double Repeat(double inp, double max)
        {
            if (inp <= max)
                return inp < 0 ? Repeat(max - inp, max) : inp;
            return inp > max ? Repeat(inp - max, max) : inp;
        }

        public static float Repeat(float inp, float max) => (float) Repeat((double) inp, max);
    }
}
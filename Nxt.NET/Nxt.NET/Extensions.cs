using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nxt.NET
{
    public static class Extensions
    {
        public static byte[] ToByteArray(this string hex)
        {
            if (hex == null)
                return new byte[0];

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => System.Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static void Write(this MemoryStream stream, sbyte value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, 1);
        }

        public static void Write(this MemoryStream stream, byte value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, 1);
        }

        public static void Write(this MemoryStream stream, short value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, 2);
        }

        public static void Write(this MemoryStream stream, int value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        public static void Write(this MemoryStream stream, long value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, 8);
        }

        public static void Write(this MemoryStream stream, ulong value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, 8);
        }

        public static void Write(this MemoryStream stream, sbyte[] value)
        {
            stream.Write((byte[])(Array)value);
        }

        public static void Write(this MemoryStream stream, byte[] value)
        {
            stream.Write(value, 0, value.Length);
        }

        public static TSource RandomizedSingleOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            var sourceList = source.ToList();
            return RandomizedSingleOrDefault(sourceList);
        }

        public static TSource RandomizedSingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            var sourceList = source.Where(predicate).ToList();
            return RandomizedSingleOrDefault(sourceList);
        }

        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> source, TKey key)
        {
            TValue toBeRemoved;
            return source.TryRemove(key, out toBeRemoved);
        }

        private static TSource RandomizedSingleOrDefault<TSource>(IList<TSource> sourceList)
        {
            if (!sourceList.Any())
                return default(TSource);

            var random = new Random();
            var skip = random.Next(sourceList.Count() - 1);
            return sourceList.Skip(skip).First();
        }
    }
}

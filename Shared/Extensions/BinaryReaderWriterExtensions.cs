using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LegendsOfDescent
{
    public static class BinaryReaderWriterExtensions
    {
        public static void Write(this BinaryWriter writer, Guid guid)
        {
            writer.Write(guid.ToByteArray());
        }

        public static void Write(this BinaryWriter writer, DateTime dateTime)
        {
            writer.Write(dateTime.ToUniversalTime().Ticks);
        }

        public static void Write(this BinaryWriter writer, ISaveable o)
        {
            o.Persist(writer);
        }

        public static void Write(this BinaryWriter writer, IEnumerable<string> list)
        {
            writer.Write(list.Count());
            foreach (var s in list)
            {
                writer.Write(s);
            }
        }

        public static void Write<T>(this BinaryWriter writer, IEnumerable<T> list) where T : ISaveable
        {
            writer.Write(list.Count());
            foreach (var o in list)
            {
                writer.Write(o);
            }
        }

        public static Guid ReadGuid(this BinaryReader reader)
        {
            return new Guid(reader.ReadBytes(16));
        }

        public static DateTime ReadDateTime(this BinaryReader reader)
        {
            return new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
        }

        public static T Read<T>(this BinaryReader reader, int dataVersion) where T : ISaveable, new()
        {
            T t = new T();
            t.Load(reader, dataVersion);
            return t;
        }

        public static List<T> ReadList<T>(this BinaryReader reader, int dataVersion) where T : ISaveable, new()
        {
            int count = reader.ReadInt32();
            List<T> list = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(reader.Read<T>(dataVersion));
            }
            return list;
        }

        public static string[] ReadArrayOfString(this BinaryReader reader)
        {
            int count = reader.ReadInt32();
            var list = new string[count];
            for (int i = 0; i < count; i++)
            {
                list[i] = reader.ReadString();
            }
            return list;
        }

        public static List<T> ReadListWithoutSize<T>(this BinaryReader reader, int dataVersion) where T : ISaveable, new()
        {
            List<T> list = new List<T>();
            while (reader.PeekChar() != -1)
            {
                list.Add(reader.Read<T>(dataVersion));
            }
            return list;
        }

    }
}

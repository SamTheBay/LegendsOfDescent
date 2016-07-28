using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace LegendsOfDescent
{
    static class PersistorExtensions
    {
        public static void Write(this BinaryWriter writer, Point point)
        {
            writer.Write((Int32)point.X);
            writer.Write((Int32)point.Y);
        }

        public static void Write(this BinaryWriter writer, Rectangle rec)
        {
            writer.Write((Int32)rec.X);
            writer.Write((Int32)rec.Y);
            writer.Write((Int32)rec.Width);
            writer.Write((Int32)rec.Height);
        }

        public static void Write(this BinaryWriter writer, Vector2 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
        }

        public static Point ReadPoint(this BinaryReader reader)
        {
            return new Point(reader.ReadInt32(), reader.ReadInt32());
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Rectangle ReadRectangle(this BinaryReader reader)
        {
            return new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        }
    }
}

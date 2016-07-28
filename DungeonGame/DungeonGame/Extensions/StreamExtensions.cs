using System.IO;
using System.Text;

namespace LegendsOfDescent
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Straight from the BCL
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyTo(this Stream source, Stream destination)
        {
            byte[] buffer = new byte[4096];
            int count;
            while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, count);
            }
        }

        public static void Write(this Stream destination, byte[] bytes)
        {
            destination.Write(bytes, 0, bytes.Length);
        }

        public static void Write(this Stream destination, string text)
        {
            destination.Write(Encoding.UTF8.GetBytes(text));
        }

        public static void Write(this Stream destination, Stream source)
        {
            source.CopyTo(destination);
        }
    }
}

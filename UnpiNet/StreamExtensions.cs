using System;
using System.IO;
using System.Threading.Tasks;

namespace UnpiNet
{
    static partial class StreamExtensions
    {
        public static async Task ReadAsyncExact(this Stream stream, byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            var read = 0;
            while (read < count)
            {
                read += await stream.ReadAsync(buffer, offset + read, count - read);
            }
        }
    }
}
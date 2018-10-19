using System.IO;
using System.Threading.Tasks;

namespace Ocelog.Transport.Test
{
    public static class Extensions
    {
        public static Task<string> ReadAsString(this Stream stream)
        {
            var reader = new StreamReader(stream);
            return reader.ReadToEndAsync();
        }
    }
}

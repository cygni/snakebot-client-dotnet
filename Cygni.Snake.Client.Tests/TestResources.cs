using System.IO;
using System.Reflection;
using System.Text;

namespace Cygni.Snake.Client.Tests
{
    public static class TestResources
    {
        private static Stream GetResourceStream(string file)
        {
            return File.Open($"compiler/resources/{file}", FileMode.Open);
        }

        public static string GetResourceText(string file, Encoding encoding)
        {
            using (var stream = GetResourceStream(file))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Could not find the specified resource: {file}");
                }

                using (var streamReader = new StreamReader(stream, encoding))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
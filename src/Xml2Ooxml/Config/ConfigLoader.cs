using System;
using System.IO;
using System.Xml.Serialization;

namespace Xml2Ooxml.Config
{
    public class ConfigLoader
    {
        public Configuration Load(string filename)
        {
            try
            {
                var result = new Configuration();
                using (var tr = new StreamReader(filename))
                {
                    result = Load(tr);
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error when trying to open {filename}: {ex.Message}");
                throw;
            }
        }

        public Configuration Load(TextReader file)
        {
            var xs = new XmlSerializer(typeof(Configuration));
            var result = (Configuration)xs.Deserialize(file);
            return result;
        }

        internal void Save(Configuration config, string path)
        {
            using (var stream = new FileStream(path,
    FileMode.Create, FileAccess.Write, FileShare.None,
    0x1000, FileOptions.WriteThrough))

            using (var tw = new StreamWriter(stream))
            {
                Save(config, tw);
            }
        }

        private void Save(Configuration config, StreamWriter tw)
        {
            var xs = new XmlSerializer(typeof(Configuration));
            xs.Serialize(tw, config);
        }
    }
}

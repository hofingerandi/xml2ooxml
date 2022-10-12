using System;
using System.IO;
using System.Xml.Linq;
using Xml2Ooxml.CommandLine;
using Xml2Ooxml.Config;

namespace Xml2OoXml
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var opts = ParseArgs(args);
            if (opts != null)
            {
                return HandleOptions(opts);
            }
            else
            {
                Console.Error.WriteLine($"Error parsing command line {args}");
                Console.WriteLine("Options:");
                Console.WriteLine("     --config <configFile.xml>");
                Console.WriteLine("     --createsample <configFile.xml>");
                return 1;
            }
        }

        static int HandleOptions(Options opts)
        {
            if (!opts.Convert)
                return 0;

            var loader = new Xml2Ooxml.Config.ConfigLoader();
            var config = loader.Load(opts.ConfigFile);

            var doc = XDocument.Load(config.DocumentPath);
            try
            {
                var converter = new Xml2OoXmlConverter();
                converter.LocalNameAsPrefix = config.LocalNameAsPrefix;
                foreach (var ns in config.NamespaceEntries)
                {
                    converter.RegisterNamespace(ns.Abbreviation, ns.Namespace);
                }
                foreach (var nr in config.NameReplacements)
                {
                    converter.RegisterNameReplacement(nr.FullName, nr.Abbreviation);
                }
                foreach (var xpath in config.XPathsToExternalize)
                {
                    converter.RegisterTypeForExternalization(xpath);
                }

                DirectoryInfo targetFolder = new DirectoryInfo(config.TargetFolder);
                converter.MaxDepth = config.MaxDepth;
                converter.ConvertDocument(doc, targetFolder);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        private static Options ParseArgs(string[] args)
        {
            if (args.Length < 2)
                return null;

            if (args[0] == "--config")
            {

                var configFile = args[1];
                if (!File.Exists(configFile))
                {
                    Console.WriteLine($"File <{configFile}> not found");
                    return null;
                }
                return new() { ConfigFile = configFile, Convert = true };
            }
            else if (args[0] == "--createsample")
            {
                ConfigLoader loader = new ConfigLoader();
                loader.Save(Configuration.CreateDefault(), args[1]);
                return new() { Convert = false };
            }
            return null;
        }
    }
}



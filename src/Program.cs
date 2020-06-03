using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace dotnet_csproj
{
    public class Program
    {
        private const string COMMAND_NAME = "dotnet csproj";

        private static string command;
        private static string path;
        private static string key;
        private static string value;

        private static string[] AvailableKeys
        {
            get
            {
                List<string> result = new List<string>();
                result.Add("Authors");
                result.Add("Company");
                result.Add("Description");
                result.Add("Copyright");
                result.Add("Version");
                result.Add("AssemblyVersion");
                result.Add("FileVersion");
                result.Add("PackageTags");

                return result.ToArray();
            }
        }

        static void Main(string[] args)
        {
            if (RequireHelp(args) || args.Length == 0)
            {
                DisplayHelp();
                return;
            }

            Init(args);
            Run();
        }

        private static void Init(string[] args)
        {
            // Get the command first and validate it
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Not enough arguments provided. Please see the help for more info.");
                Environment.Exit(1);
            }
            else
            {
                command = args[0].Replace("--", "");
            }

            if (command != "get" && command != "set")
            {
                Console.Error.WriteLine("Invalid command provided. See the help for more info.");
                Environment.Exit(1);
            }

            // Followed by retrieving the key (and value)
            if (command == "get")
            {
                key = args[1];
            }
            if (command == "set")
            {
                if (!args[1].Contains("="))
                {
                    Console.Error.WriteLine("Invalid key/value provided. See the help for more info.");
                    Environment.Exit(1);
                }

                key = args[1].Split('=')[0];
                value = args[1].Split('=')[1].Replace("\"", "");
            }

            if (!AvailableKeys.Any(x => x.ToLower() == key.ToLower()))
            {
                Console.Error.WriteLine("Provided key is not valid. See the help for a list of available keys.");
                Environment.Exit(1);
            }
            else
            {
                // Correct the key name in case the user provided in in lowercase
                key = AvailableKeys.First(x => x.ToLower() == key.ToLower());
            }

            // And the path
            if (args.Length == 3)
            {
                path = args[2];

                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"File {path} not found");
                    Environment.Exit(1);
                }
                if (!path.EndsWith(".csproj"))
                {
                    Console.Error.WriteLine($"Provided path is not a csproj file");
                    Environment.Exit(1);
                }
            }
            else
            {
                string folder = Directory.GetCurrentDirectory();
                string[] files = Directory.GetFiles(folder, "*.csproj");

                if (files.Length > 1)
                {
                    Console.Error.WriteLine("Multiple csproj files found in current folder, please specify one.");
                    Environment.Exit(1);
                }
                if (files.Length == 0)
                {
                    Console.Error.WriteLine("No csproj files were found in the current folder");
                    Environment.Exit(1);
                }

                path = files[0];
            }
        }

        private static void Run()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(path);

            XmlNodeList nodeListPropertyGroup = xml.GetElementsByTagName("PropertyGroup");
            XmlNode[] nodesPropertyGroup = nodeListPropertyGroup.Cast<XmlNode>().ToArray();

            if (nodesPropertyGroup.Length == 0)
            {
                Console.Error.WriteLine("No propertygroups where found in the provided csproj file");
                Environment.Exit(1);
            }

            if (command == "get")
            {
                foreach (XmlNode nodePropertyGroup in nodesPropertyGroup)
                {
                    XmlNode[] childNodes = nodePropertyGroup.ChildNodes.Cast<XmlNode>().ToArray();
                    XmlNode node = childNodes.FirstOrDefault(x => x.Name == key);

                    if (node != null)
                    {
                        Console.Write(node.InnerText);
                        Environment.Exit(0);
                    }
                }
            }

            if (command == "set")
            {
                int occurences = 0;

                foreach (XmlNode nodePropertyGroup in nodesPropertyGroup)
                {
                    XmlNode[] childNodes = nodePropertyGroup.ChildNodes.Cast<XmlNode>().ToArray();
                    XmlNode node = childNodes.FirstOrDefault(x => x.Name == key);

                    if (node != null)
                    {
                        node.InnerText = value;
                        occurences++;
                    }
                }

                if (occurences == 0)
                {
                    XmlNode node = xml.CreateElement(key);
                    node.InnerText = value;

                    XmlNode firstNodePropertyGroup = nodesPropertyGroup.First();
                    firstNodePropertyGroup.AppendChild(node);
                }

                xml.Save(path);
            }
        }

        private static void DisplayHelp()
        {
            Console.WriteLine($"Usage: {COMMAND_NAME} --get <KEY> [PATH_TO_CSPROJ]");
            Console.WriteLine($"       {COMMAND_NAME} --set <KEY>=\"<VALUE>\" [PATH_TO_CSPROJ]");
            Console.WriteLine();
            Console.WriteLine("ABOUT");
            Console.WriteLine("This tool allows you to easily set and get csproj xml values.");
            Console.WriteLine();
            Console.WriteLine("KEYS");
            Console.WriteLine("To make life a little easier the following keys are available to set and get:");
            Console.WriteLine();

            foreach (string key in AvailableKeys)
            {
                Console.WriteLine($"-{key}");
            }
        }

        private static bool RequireHelp(string[] args)
        {
            return args.Any(x => x.ToLower() == "-h" || x.ToLower() == "--help");
        }
    }
}

using System;
using System.IO;

namespace Totem.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            TotemGrammar tg = new TotemGrammar();
            Irony.Parsing.Parser parser = new Irony.Parsing.Parser(tg);
            var tree = parser.Parse(new StreamReader(File.OpenRead("test.pole")).ReadToEnd(), "test.pole");
            if (!tree.HasErrors())
            {
                var generator = new Generator("test", Path.Combine(Environment.CurrentDirectory, "test.exe"), tg);
                var rootNode = tree.Root;
                generator.GenerateProgram(rootNode);
                generator.Save();
            }
            else
            {
                Console.WriteLine("Error in input");
                Console.ReadLine();
            }
        }
    }
}

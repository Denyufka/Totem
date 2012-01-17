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
            var tree = parser.Parse(new StreamReader(File.OpenRead("test.totem")).ReadToEnd(), "test.totem");
            if (!tree.HasErrors())
            {
                var generator = new Generator("test");
                var rootNode = tree.Root;
                generator.GenerateProgram(rootNode);
                generator.Save(Environment.CurrentDirectory + "\\test.exe");
            }
        }
    }
}

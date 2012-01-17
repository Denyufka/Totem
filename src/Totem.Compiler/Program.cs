
using System;
namespace Totem.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            TotemGrammar tg = new TotemGrammar();
            Irony.Parsing.Parser parser = new Irony.Parsing.Parser(tg);
            var tree = parser.Parse("var per = 5, navn = 'test', p = null, t = undefined, s = per; var knut = 10; per = knut + 1; knut -= 5;");
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

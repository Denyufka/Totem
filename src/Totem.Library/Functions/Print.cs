using System;
using System.Linq;

namespace Totem.Library.Functions
{
    public class Print : TotemFunction
    {
        public Print(TotemEnvironment env)
            : base(env, "print", new TotemParameter[] { })
        {

        }

        public override TotemValue Execute(TotemArguments arguments)
        {
            var str = arguments.First().Value.ToString();
            var rest = arguments.Skip(1);
            int i = 0;
            foreach (var a in rest)
            {
                string name = null;
                if (a.Name != null)
                    name = a.Name;
                else
                    name = i.ToString();

                string value = a.Value.ToString();
                str = str.Replace("{" + name + "}", value);
                i++;
            }
            Console.WriteLine(str);
            return TotemValue.Undefined;
        }
    }
}

using System;
using System.Linq;

namespace Totem.Library.Types
{
    public class Totem : TotemType
    {
        public override string Name
        {
            get { return "Totem"; }
        }

        public Totem()
        {
            MapProperty("parent", GetParent, null);

            MapMethod("toString", ToString);
            MapMethod("implement", Implement);
        }

        public static TotemValue GetParent(TotemValue type)
        {
            return ((TotemType)type).Parent ?? TotemValue.Null;
        }

        public static TotemString ToString(TotemValue type, TotemArguments args)
        {
            return new TotemString("[Totem " + ((TotemType)type).Name + "]");
        }

        public static TotemValue Implement(TotemValue type, TotemArguments args)
        {
            var fArg = args.First().Value;
            var aArg = args.ElementAt(1).Value;
            if (fArg.Type.GetType() == typeof(Types.String) && aArg.Type.GetType() == typeof(Types.Function))
            {
                var tt = (TotemType)type;
                var fn = (TotemFunction)aArg;
                tt.MapMethod(fArg.ToString(), (@this, arguments) =>
                {
                    var a = new TotemArguments();
                    a.Add(null, @this);
                    foreach (var ar in arguments)
                        a.Add(ar.Name, ar.Value);
                    return fn.Execute(a);
                });
                return TotemValue.Undefined;
            }
            else
            {
                throw new InvalidOperationException("Can't implement anything but functions.");
            }
        }
    }
}

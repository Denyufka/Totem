
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
            var tt = (TotemType)type;
            foreach (var arg in args)
            {
                if (!string.IsNullOrEmpty(arg.Name) && arg.Value.Type.GetType() == typeof(Types.Function))
                {
                    var fn = (TotemFunction)arg.Value;
                    tt.MapMethod(arg.Name, (@this, arguments) =>
                    {
                        var a = new TotemArguments();
                        a.Add(null, @this);
                        foreach (var ar in arguments)
                            a.Add(ar.Name, ar.Value);
                        return fn.Execute(a);
                    });
                }
            }
            return TotemValue.Undefined;
        }
    }
}

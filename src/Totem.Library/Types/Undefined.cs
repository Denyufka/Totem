
namespace Totem.Library.Types
{
    public class Undefined : TotemType
    {
        public override string Name
        {
            get { return "Null"; }
        }

        public Undefined()
        {
            MapMethod("toString", ToString);
        }

        public static TotemString ToString(TotemValue @null, TotemArguments args)
        {
            return new TotemString("<undefined>");
        }
    }
}


namespace Totem.Library.Types
{
    public class Null : TotemType
    {
        public override string Name
        {
            get { return "Null"; }
        }

        public Null()
        {
            MapMethod("toString", ToString);
        }

        public static TotemString ToString(TotemValue @null, TotemArguments args)
        {
            return new TotemString("<null>");
        }
    }
}


namespace Totem.Library.Types
{
    public class Object : TotemType
    {
        public override TotemType Parent
        {
            get
            {
                return null;
            }
        }

        public override string Name
        {
            get { return "Object"; }
        }

        public Object()
        {
            MapMethod("toString", ToString);

            MapProperty("totem", GetType, null);
        }

        public static TotemString ToString(TotemValue @this, TotemArguments args)
        {
            return new TotemString("[" + @this.Type.Name + "]");
        }

        public static TotemType GetType(TotemValue @this)
        {
            return @this.Type;
        }
    }
}

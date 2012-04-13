
namespace Totem.Library.Types
{
    public class String : TotemType
    {
        public override string Name
        {
            get { return "String"; }
        }

        public String()
        {
            MapProperty("length", GetLength, null);

            MapMethod("toUpperCase", ToUpperCase);
            MapMethod("toLowerCase", ToLowerCase);
            MapMethod("toString", ToString);
        }

        public static TotemNumber GetLength(TotemValue str)
        {
            return new TotemNumber(((TotemString)str).Value.Length);
        }

        public static TotemString ToUpperCase(TotemValue str, TotemArguments args)
        {
            return new TotemString(((TotemString)str).Value.ToUpper());
        }

        public static TotemString ToLowerCase(TotemValue str, TotemArguments args)
        {
            return new TotemString(((TotemString)str).Value.ToLower());
        }

        public static TotemString ToString(TotemValue str, TotemArguments args)
        {
            return new TotemString(((TotemString)str).Value);
        }
    }
}

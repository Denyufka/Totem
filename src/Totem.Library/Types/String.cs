
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

            MapFunction("toUpperCase", ToUpperCase);
            MapFunction("toLowerCase", ToLowerCase);
        }

        public static TotemNumber GetLength(TotemValue str)
        {
            return new TotemNumber(((TotemString)str).Value.Length);
        }

        public static TotemString ToUpperCase(TotemArguments args)
        {
            return new TotemString(((TotemString)args.ThisObject).Value.ToUpper());
        }

        public static TotemString ToLowerCase(TotemArguments args)
        {
            return new TotemString(((TotemString)args.ThisObject).Value.ToLower());
        }
    }
}

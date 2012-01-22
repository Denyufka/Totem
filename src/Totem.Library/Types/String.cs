
namespace Totem.Library.Types
{
    public class String : TotemType
    {
        private static String type = new String();
        internal static String Type { get { return type; } }

        private String()
        {
            MapProperty("length", GetLength, null);

            MapFunction("toUpperCase", ToUpperCase);
        }

        public static TotemNumber GetLength(TotemValue str)
        {
            return new TotemNumber(((TotemString)str).Value.Length);
        }

        public static TotemString ToUpperCase(TotemArguments args)
        {
            return new TotemString(((TotemString)args.ThisObject).Value.ToUpper());
        }
    }
}


namespace Totem.Library.Types
{
    public class Array : TotemType
    {
        public override string Name
        {
            get { return "Array"; }
        }

        public Array()
        {
            MapProperty("length", GetLength, null);
            MapMethod("push", Push);
            MapMethod("filter", Filter);
        }

        public static TotemValue GetLength(TotemValue @this)
        {
            return new TotemNumber(((TotemArray)@this).value.Count);
        }

        public static TotemValue Push(TotemValue array, TotemArguments parameters)
        {
            TotemArray arr = (TotemArray)array;
            arr.value.Add(parameters.Value(0));
            return arr;
        }

        public static TotemValue Filter(TotemValue array, TotemArguments parameters)
        {
            TotemArray newArr = new TotemArray();
            TotemArray arr = (TotemArray)array;
            TotemValue fn = parameters.Value(0);
            foreach (var itm in arr.value)
            {
                var arguments = new TotemArguments();
                arguments.Add(null, itm);
                if ((bool)fn.Execute(arguments))
                    newArr.AddItem(itm);
            }
            return newArr;
        }
    }
}

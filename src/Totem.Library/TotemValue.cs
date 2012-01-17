
namespace Totem.Library
{
    public abstract class TotemValue
    {
        public abstract TotemValue ByTotemValue { get; }

        public static TotemValue Undefined { get { return TotemUndefined.Value; } }
        public static TotemValue Null { get { return TotemNull.Value; } }

        public static TotemValue Add(TotemValue left, TotemValue right)
        {
            return TotemValue.Undefined;
        }

        public static TotemValue Subtract(TotemValue left, TotemValue right)
        {
            return TotemValue.Undefined;
        }
    }
}

using System;

namespace Totem.Library
{
    public abstract class TotemValue
    {
        public abstract TotemValue ByTotemValue { get; }

        public virtual TotemValue Execute(TotemArguments arguments)
        {
            throw new InvalidOperationException("Can't execute on a " + GetType());
        }

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

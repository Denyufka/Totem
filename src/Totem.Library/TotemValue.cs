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

        public virtual TotemValue Add(TotemValue other)
        {
            throw new InvalidOperationException("Can't add a " + other.GetType().Name + " to a " + GetType().Name);
        }

        public virtual TotemValue Subtract(TotemValue other)
        {
            throw new InvalidOperationException("Can't subtract a " + other.GetType().Name + " to a " + GetType().Name);
        }

        public static TotemValue Undefined { get { return TotemUndefined.Value; } }
        public static TotemValue Null { get { return TotemNull.Value; } }

        public static TotemValue Add(TotemValue left, TotemValue right)
        {
            return left.Add(right);
        }

        public static TotemValue Subtract(TotemValue left, TotemValue right)
        {
            return left.Subtract(right);
        }
    }
}

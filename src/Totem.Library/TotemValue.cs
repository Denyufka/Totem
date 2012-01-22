using System;

namespace Totem.Library
{
    public abstract class TotemValue
    {
        public abstract TotemValue ByTotemValue { get; }

        public abstract TotemType TotemType { get; }

        public virtual TotemValue Execute(TotemArguments arguments)
        {
            throw new InvalidOperationException("Can't execute on a " + GetType());
        }

        public virtual TotemValue GetProp(string name)
        {
            return TotemType.GetProp(this, name);
        }

        public virtual TotemValue Add(TotemValue other)
        {
            throw new InvalidOperationException("Can't add a " + other.GetType().Name + " to a " + GetType().Name);
        }

        public virtual TotemValue Subtract(TotemValue other)
        {
            throw new InvalidOperationException("Can't subtract a " + other.GetType().Name + " to a " + GetType().Name);
        }

        public virtual TotemValue LessThen(TotemValue other)
        {
            throw new InvalidOperationException("Can't compare a " + other.GetType().Name + " to a " + GetType().Name);
        }

        public virtual TotemValue GreaterThen(TotemValue other)
        {
            throw new InvalidOperationException("Can't compare a " + other.GetType().Name + " to a " + GetType().Name);
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

        public static TotemValue LessThen(TotemValue left, TotemValue right)
        {
            return left.LessThen(right);
        }

        public static TotemValue GreaterThen(TotemValue left, TotemValue right)
        {
            return left.GreaterThen(right);
        }

        public static bool IsTrue(TotemValue value)
        {
            return !(value is TotemUndefined)
                && !(value is TotemNull)
                && !(value is TotemNumber && ((TotemNumber)value).IntValue == 0)
                && !(value is TotemBool && !((TotemBool)value).Value);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Totem.Library
{
    public abstract class TotemValue
    {
        private Dictionary<string, TotemValue> properties = new Dictionary<string, TotemValue>();

        public static TotemValue Undefined { get { return TotemUndefined.Value; } }
        public static TotemValue Null { get { return TotemNull.Value; } }

        public abstract TotemValue ByTotemValue { get; }

        public abstract TotemType Type { get; }

        public virtual TotemValue Execute(TotemArguments arguments)
        {
            throw new InvalidOperationException("Can't execute on a " + GetType());
        }

        public virtual TotemValue GetProp(string name)
        {
            TotemValue ret;
            if (!properties.TryGetValue(name, out ret))
            {
                ret = Type.GetTypeProp(this, name);
                if (!Object.ReferenceEquals(ret, TotemValue.Undefined) && !Object.ReferenceEquals(ret, TotemValue.Null))
                    properties.Add(name, ret);
            }
            return ret;
        }

        public virtual void SetProp(string name, TotemValue value)
        {
            if (!Type.SetTypeProp(this, name, value))
            {
                properties[name] = value;
            }
        }

        public virtual TotemValue Add(TotemValue other)
        {
            throw new InvalidOperationException("Can't add a " + other.GetType().Name + " to a " + GetType().Name);
        }

        public virtual TotemValue Subtract(TotemValue other)
        {
            throw new InvalidOperationException("Can't subtract a " + other.GetType().Name + " to a " + GetType().Name);
        }

        public virtual TotemValue MultiplyWith(TotemValue other)
        {
            throw new InvalidOperationException("Can't multiply a " + other.GetType().Name + " with a " + GetType().Name);
        }

        public virtual TotemValue DivideBy(TotemValue other)
        {
            throw new InvalidOperationException("Can't divide a " + GetType().Name + " with a " + other.GetType().Name);
        }

        public virtual TotemValue LessThan(TotemValue other)
        {
            throw new InvalidOperationException("Can't compare a " + other.GetType().Name + " to a " + GetType().Name);
        }

        public virtual TotemValue GreaterThan(TotemValue other)
        {
            throw new InvalidOperationException("Can't compare a " + other.GetType().Name + " to a " + GetType().Name);
        }

        public virtual TotemValue LessThanOrEqual(TotemValue other)
        {
            throw new InvalidOperationException("Can't compare a " + other.GetType().Name + " to a " + GetType().Name);
        }

        public virtual TotemValue GreaterThanOrEqual(TotemValue other)
        {
            throw new InvalidOperationException("Can't compare a " + other.GetType().Name + " to a " + GetType().Name);
        }

        public virtual TotemValue Increment()
        {
            throw new InvalidOperationException("Can't increment a " + GetType().Name);
        }

        public virtual TotemValue Decrement()
        {
            throw new InvalidOperationException("Can't decrement a " + GetType().Name);
        }

        public static TotemValue operator +(TotemValue left, TotemValue right)
        {
            return left.Add(right);
        }

        public static TotemValue operator -(TotemValue left, TotemValue right)
        {
            return left.Subtract(right);
        }

        public static TotemValue operator *(TotemValue left, TotemValue right)
        {
            return left.MultiplyWith(right);
        }

        public static TotemValue operator /(TotemValue left, TotemValue right)
        {
            return left.DivideBy(right);
        }

        public static TotemValue operator ==(TotemValue left, TotemValue right)
        {
            return new TotemBool(left.Equals(right));
        }

        public static TotemValue operator !=(TotemValue left, TotemValue right)
        {
            return new TotemBool(!left.Equals(right));
        }

        public static TotemValue operator <(TotemValue left, TotemValue right)
        {
            return left.LessThan(right);
        }

        public static TotemValue operator >(TotemValue left, TotemValue right)
        {
            return left.GreaterThan(right);
        }

        public static TotemValue operator <=(TotemValue left, TotemValue right)
        {
            return left.LessThanOrEqual(right);
        }

        public static TotemValue operator >=(TotemValue left, TotemValue right)
        {
            return left.GreaterThanOrEqual(right);
        }

        public static TotemValue operator !(TotemValue value)
        {
            return (bool)value ? new TotemBool(false) : new TotemBool(true);
        }

        public static TotemValue operator ++(TotemValue value)
        {
            return value.Increment();
        }

        public static TotemValue operator --(TotemValue value)
        {
            return value.Decrement();
        }

        public static explicit operator bool(TotemValue value)
        {
            return !(value is TotemUndefined)
                && !(value is TotemNull)
                && !(value is TotemNumber && ((TotemNumber)value).IntValue == 0)
                && !(value is TotemBool && !((TotemBool)value).Value);
        }

        public override string ToString()
        {
            return ((TotemString)Type.GetTypeProp(this, "toString").Execute(new TotemArguments())).Value;
        }
    }
}

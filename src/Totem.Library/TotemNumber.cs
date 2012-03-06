
using System;
namespace Totem.Library
{
    public class TotemNumber : TotemValue
    {
        private long lValue;
        private double fValue;
        private bool isFloatingPoint;

        public TotemNumber(long value)
        {
            lValue = value;
            isFloatingPoint = false;
        }

        public TotemNumber(double value)
        {
            fValue = value;
            isFloatingPoint = true;
        }

        public override TotemValue ByTotemValue
        {
            get
            {
                if (isFloatingPoint)
                    return new TotemNumber(fValue);
                else
                    return new TotemNumber(lValue);
            }
        }

        private object Value
        {
            get { return isFloatingPoint ? fValue : lValue; }
        }

        public double IntValue
        {
            get
            {
                if (isFloatingPoint)
                    return int.MaxValue;
                else
                    return lValue;
            }
        }

        public override TotemValue Add(TotemValue other)
        {
            if (other is TotemNumber)
            {
                var n = (TotemNumber)other;
                if (n.isFloatingPoint || isFloatingPoint)
                {
                    return new TotemNumber(Convert.ToDouble(Value) + Convert.ToDouble(n.Value));
                }
                else
                {
                    return new TotemNumber(lValue + n.lValue);
                }
            }
            return base.Add(other);
        }

        public override TotemValue Subtract(TotemValue other)
        {
            if (other is TotemNumber)
            {
                var n = (TotemNumber)other;
                if (n.isFloatingPoint || isFloatingPoint)
                {
                    return new TotemNumber(ToDouble(this) - ToDouble(n));
                }
                else
                {
                    return new TotemNumber(lValue - n.lValue);
                }
            }
            return base.Subtract(other);
        }

        public override TotemValue MultiplyWith(TotemValue other)
        {
            if (other is TotemNumber)
            {
                var n = (TotemNumber)other;
                if (n.isFloatingPoint || isFloatingPoint)
                {
                    return new TotemNumber(ToDouble(this) * ToDouble(n));
                }
                else
                {
                    return new TotemNumber(lValue * n.lValue);
                }
            }
            return base.MultiplyWith(other);
        }

        public override TotemValue DivideBy(TotemValue other)
        {
            if (other is TotemNumber)
            {
                return new TotemNumber(ToDouble(this) / ToDouble((TotemNumber)other));
            }
            return base.DivideBy(other);
        }

        public override TotemValue LessThan(TotemValue other)
        {
            if (other is TotemNumber)
            {
                return new TotemBool(ToDouble(this) < ToDouble((TotemNumber)other));
            }
            return base.LessThan(other);
        }

        public override TotemValue GreaterThan(TotemValue other)
        {
            if (other is TotemNumber)
            {
                return new TotemBool(ToDouble(this) > ToDouble((TotemNumber)other));
            }
            return base.LessThan(other);
        }

        public override TotemValue LessThanOrEqual(TotemValue other)
        {
            if (other is TotemNumber)
            {
                return new TotemBool(ToDouble(this) <= ToDouble((TotemNumber)other));
            }
            return base.LessThan(other);
        }

        public override TotemValue GreaterThanOrEqual(TotemValue other)
        {
            if (other is TotemNumber)
            {
                return new TotemBool(ToDouble(this) >= ToDouble((TotemNumber)other));
            }
            return base.LessThan(other);
        }

        public override TotemValue Increment()
        {
            if (isFloatingPoint)
                return new TotemNumber(fValue + 1);
            else
                return new TotemNumber(lValue + 1);
        }

        public override TotemValue Decrement()
        {
            if (isFloatingPoint)
                return new TotemNumber(fValue - 1);
            else
                return new TotemNumber(lValue - 1);
        }

        public override bool Equals(object obj)
        {
            if (obj is TotemNumber)
            {
                return Value.Equals(((TotemNumber)obj).Value);
            }
            return false;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        private static Double ToDouble(TotemNumber num)
        {
            if (num.isFloatingPoint)
                return num.fValue;
            else
                return (double)num.lValue;
        }

        public override TotemType Type
        {
            get { throw new NotImplementedException(); }
        }
    }
}

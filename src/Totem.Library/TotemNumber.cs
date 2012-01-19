
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
                    return new TotemNumber(Convert.ToDouble(Value) - Convert.ToDouble(n.Value));
                }
                else
                {
                    return new TotemNumber(lValue - n.lValue);
                }
            }
            return base.Subtract(other);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}

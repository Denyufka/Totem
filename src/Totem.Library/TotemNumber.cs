
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
    }
}

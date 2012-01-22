
namespace Totem.Library
{
    public class TotemBool : TotemValue
    {
        private bool value;

        public TotemBool(bool value)
        {
            this.value = value;
        }

        public bool Value { get { return value; } }

        public override TotemValue ByTotemValue
        {
            get { return new TotemBool(value); }
        }

        public override TotemType TotemType
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}

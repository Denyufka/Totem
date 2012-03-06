
namespace Totem.Library
{
    public class TotemString : TotemValue
    {
        private string value;

        public TotemString(string value)
        {
            this.value = value;
        }

        public string Value
        {
            get { return value; }
        }

        public override TotemType Type
        {
            get { return TotemType.Resolve<Types.String>(); }
        }

        public override TotemValue ByTotemValue
        {
            get { return new TotemString(value); }
        }

        public override string ToString()
        {
            return value;
        }
    }
}

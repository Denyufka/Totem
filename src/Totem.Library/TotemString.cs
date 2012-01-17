
namespace Totem.Library
{
    public class TotemString : TotemValue
    {
        private string value;

        public TotemString(string value)
        {
            this.value = value;
        }

        public override TotemValue ByTotemValue
        {
            get { return new TotemString(value); }
        }
    }
}

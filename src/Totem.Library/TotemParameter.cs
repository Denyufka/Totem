
namespace Totem.Library
{
    public class TotemParameter
    {
        private readonly string name;
        private readonly TotemValue defaultValue;

        public string Name
        {
            get { return name; }
        }

        public TotemValue DefaultValue
        {
            get { return defaultValue; }
        }

        public TotemParameter(string name, TotemValue defaultValue)
        {
            this.defaultValue = defaultValue ?? TotemValue.Undefined;
            this.name = string.IsNullOrWhiteSpace(name) ? null : name;
        }
    }
}

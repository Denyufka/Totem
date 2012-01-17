using System.Collections.Generic;

namespace Totem.Library
{
    public class TotemEnvironment
    {
        private readonly TotemEnvironment parent;
        private readonly Dictionary<string, TotemValue> values;
        public TotemEnvironment(TotemEnvironment parent)
        {
            this.parent = parent;
            this.values = new Dictionary<string, TotemValue>();
        }

        internal void Declare(string name)
        {
            if (!values.ContainsKey(name))
                values.Add(name, TotemValue.Undefined);
        }

        internal void Set(string name, TotemValue value)
        {
            if (parent == null || values.ContainsKey(name))
                values[name] = value;
            else
                parent.Set(name, value);
        }

        internal TotemValue Get(string name)
        {
            if (values.ContainsKey(name))
                return values[name];
            else if (parent == null)
                return null;
            return parent.Get(name);
        }
    }
}

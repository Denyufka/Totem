using System;
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

        internal virtual void Declare(string name)
        {
            if (!values.ContainsKey(name))
                values.Add(name, TotemValue.Undefined);
        }

        internal virtual void Set(string name, TotemValue value)
        {
            if (parent == null || values.ContainsKey(name))
                values[name] = value;
            else
                parent.Set(name, value);
        }

        internal virtual TotemValue Get(string name)
        {
            if (values.ContainsKey(name))
                return values[name];
            else if (parent == null)
                return TotemValue.Undefined;
            return parent.Get(name);
        }

        private static TotemGlobal global = new TotemGlobal();
        public static TotemEnvironment Global
        {
            get { return global; }
        }

        private class TotemGlobal : TotemEnvironment
        {
            public TotemGlobal()
                : base(null)
            {
                values.Add("print", new Functions.Print(this));
            }

            internal override void Declare(string name)
            {
                throw new InvalidOperationException("Can't declare variable on global scope.");
            }

            internal override void Set(string name, TotemValue value)
            {
                throw new InvalidOperationException("Can't change global values.");
            }

            internal override TotemValue Get(string name)
            {
                if (values.ContainsKey(name))
                    return values[name];
                throw new InvalidOperationException("Undefined variable " + name);
            }
        }
    }
}

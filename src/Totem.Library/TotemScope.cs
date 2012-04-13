using System;
using System.Collections.Generic;

namespace Totem.Library
{
    public class TotemScope
    {
        private readonly TotemScope parent;
        private readonly Dictionary<string, TotemValue> values;
        public TotemScope(TotemScope parent)
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
        public static TotemScope Global
        {
            get { return global; }
        }

        private class TotemGlobal : TotemScope
        {
            public TotemGlobal()
                : base(null)
            {
                values.Add("print", new Functions.Print(this));
                values.Add("String", TotemType.Resolve<Types.String>());
            }

            internal override void Declare(string name)
            {
                throw new InvalidOperationException("Can't declare variable " + name + " on global scope.");
            }

            internal override void Set(string name, TotemValue value)
            {
                throw new InvalidOperationException("Can't change global value " + name + ".");
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

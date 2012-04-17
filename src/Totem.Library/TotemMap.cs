using System;
using System.Collections.Generic;

namespace Totem.Library
{
    public class TotemMap : TotemValue
    {
        public Dictionary<TotemValue, TotemValue> value;

        public TotemMap()
        {
            value = new Dictionary<TotemValue, TotemValue>();
        }

        public override TotemType Type
        {
            get { return TotemType.Resolve<Types.Map>(); }
        }

        public void AddItem(TotemValue key, TotemValue value)
        {
            this.value.Add(key, value);
        }

        public override TotemValue ByTotemValue
        {
            get { return this; }
        }

        public override TotemValue GetProp(string name)
        {
            var ret = base.GetProp(name);
            if (Object.ReferenceEquals(ret, TotemValue.Undefined) || Object.ReferenceEquals(ret, TotemValue.Null))
            {
                if (!value.TryGetValue(new TotemString(name), out ret))
                    ret = TotemValue.Undefined;
            }
            return ret;
        }

        public override TotemValue this[TotemValue key]
        {
            get
            {
                TotemValue ret;
                return value.TryGetValue(key, out ret) ? ret : TotemValue.Undefined;
            }
            set
            {
                this.value[key] = value;
            }
        }
    }
}

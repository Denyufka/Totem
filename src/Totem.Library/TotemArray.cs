using System;
using System.Collections.Generic;

namespace Totem.Library
{
    public class TotemArray : TotemValue
    {
        public List<TotemValue> value;

        public TotemArray()
        {
            value = new List<TotemValue>();
        }

        public override TotemType Type
        {
            get { return TotemType.Resolve<Types.Array>(); }
        }

        public void AddItem(TotemValue value)
        {
            this.value.Add(value);
        }

        public override TotemValue ByTotemValue
        {
            get { return this; }
        }

        public override TotemValue this[TotemValue key]
        {
            get
            {
                if (key.GetType() == typeof(TotemNumber))
                    return value[(int)((TotemNumber)key).IntValue];
                throw new InvalidOperationException("Invalid array key.");
            }
            set
            {
                if (key.GetType() == typeof(TotemNumber))
                    this.value[(int)((TotemNumber)key).IntValue] = value;
                throw new InvalidOperationException("Invalid array key.");
            }
        }
    }
}

using System.Collections.Generic;

namespace Totem.Library
{
    public class TotemType
    {
        private Dictionary<string, TotemProperty> properties = new Dictionary<string, TotemProperty>();

        protected TotemType()
        {

        }

        protected void MapProperty(string propName, PropertyGetter getter, PropertySetter setter)
        {
            var prop = new TotemProperty();
            prop.Type = TotemPropertyType.Property;
            prop.Getter = getter;
            prop.Setter = setter;
            properties.Add(propName, prop);
        }

        protected void MapFunction(string propName, Function function)
        {
            var prop = new TotemProperty();
            prop.Type = TotemPropertyType.Value;
            var clrFunction = new ClrFunction(propName, function);
            prop.Value = clrFunction;
            prop.Flags = TotemPropertyFlags.ReadOnly;
            properties.Add(propName, prop);
        }

        public virtual TotemValue GetProp(TotemValue @this, string propName)
        {
            if (properties.ContainsKey(propName))
            {
                var prop = properties[propName];
                if (prop.Type == TotemPropertyType.Property)
                    return prop.Getter(@this);
                else
                    return prop.Value;
            }
            return TotemValue.Undefined;
        }
    }
}

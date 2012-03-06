using System;
using System.Collections.Generic;

namespace Totem.Library
{
    public abstract class TotemType : TotemValue
    {
        private Dictionary<string, TotemProperty> properties = new Dictionary<string, TotemProperty>();

        public abstract string Name { get; }

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
            var clrFunction = new ClrMethod(propName, function);
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

        public override TotemValue ByTotemValue
        {
            get { return this; }
        }

        public override TotemType Type
        {
            get
            {
                return TotemTypeType.Instance;
            }
        }

        #region TypeType
        private class TotemTypeType : TotemType
        {
            public static TotemTypeType Instance = new TotemTypeType();

            public override string Name
            {
                get { return "TotemType"; }
            }

            public TotemTypeType()
            {

            }
        }
        #endregion

        private static Dictionary<Type, TotemType> types = new Dictionary<Type, TotemType>();
        public static TotemType Resolve<TType>()
            where TType : TotemType, new()
        {
            TotemType type;
            if (!types.TryGetValue(typeof(TType), out type))
            {
                type = new TType();
                types.Add(typeof(TType), type);
            }
            return type;
        }
    }
}

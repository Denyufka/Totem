using System;
using System.Collections.Generic;

namespace Totem.Library
{
    public abstract class TotemType : TotemValue
    {
        private Dictionary<string, TotemProperty> properties = new Dictionary<string, TotemProperty>();

        public abstract string Name { get; }
        public virtual TotemType Parent
        {
            get
            {
                return TotemType.Resolve<Types.Object>();
            }
        }

        protected TotemType()
        {

        }

        protected internal void MapProperty(string propName, PropertyGetter getter, PropertySetter setter)
        {
            var prop = new TotemProperty();
            prop.Type = TotemPropertyType.Property;
            prop.Getter = getter;
            prop.Setter = setter;
            properties.Add(propName, prop);
        }

        protected internal void MapMethod(string propName, Method function)
        {
            var prop = new TotemProperty();
            prop.Type = TotemPropertyType.Property;
            prop.Getter = @this => new ClrMethod(propName, @this, function);
            prop.Flags = TotemPropertyFlags.ReadOnly;
            properties.Add(propName, prop);
        }

        public virtual TotemValue GetTypeProp(TotemValue @this, string propName)
        {
            TotemProperty prop;
            if (!properties.TryGetValue(propName, out prop))
                return Object.ReferenceEquals(Parent, null) ? TotemValue.Undefined : Parent.GetTypeProp(@this, propName);

            if (prop.Type == TotemPropertyType.Property)
                return prop.Getter(@this);
            else
                return prop.Value;
        }

        public virtual bool SetTypeProp(TotemValue @this, string propName, TotemValue value)
        {
            TotemProperty prop;
            if (!properties.TryGetValue(propName, out prop))
                return !Object.ReferenceEquals(Parent, null) && Parent.SetTypeProp(@this, propName, value);

            if (prop.Type == TotemPropertyType.Property)
                prop.Setter(@this, value);
            else
                return false;
            return true;
        }

        public override TotemValue ByTotemValue
        {
            get { return this; }
        }

        public override TotemType Type
        {
            get
            {
                return TotemType.Resolve<Types.Totem>();
            }
        }

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

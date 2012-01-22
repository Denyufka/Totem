
namespace Totem.Library
{
    internal enum TotemPropertyType
    {
        Property,
        Value
    }

    internal enum TotemPropertyFlags
    {
        None = 0,
        ReadOnly = 1 << 0
    }

    public delegate TotemValue PropertyGetter(TotemValue @this);
    public delegate void PropertySetter(TotemValue @this, TotemValue value);
    public delegate TotemValue Function(TotemArguments args);

    internal class TotemProperty
    {
        internal TotemPropertyType Type { get; set; }

        internal PropertyGetter Getter { get; set; }

        internal PropertySetter Setter { get; set; }

        internal TotemValue Value { get; set; }

        internal TotemPropertyFlags Flags { get; set; }
    }
}

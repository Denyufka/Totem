
namespace Totem.Library
{
    internal sealed class TotemUndefined : TotemValue
    {
        public static TotemUndefined Value
        {
            get
            {
                return value;
            }
        }

        static TotemUndefined value = new TotemUndefined();
        private TotemUndefined()
        { }

        public override TotemValue ByTotemValue
        {
            get { return this; }
        }
    }
}

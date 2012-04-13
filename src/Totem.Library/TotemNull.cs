
namespace Totem.Library
{
    internal class TotemNull : TotemValue
    {
        public static TotemNull Value
        {
            get
            {
                return value;
            }
        }

        static TotemNull value = new TotemNull();
        private TotemNull()
        { }

        public override TotemValue ByTotemValue
        {
            get { return this; }
        }

        public override TotemType Type
        {
            get { return TotemType.Resolve<Types.Null>(); }
        }
    }
}

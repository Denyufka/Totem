
namespace Totem.Library
{
    public class TotemString : TotemValue
    {
        private string value;

        public TotemString(string value)
        {
            this.value = value;
        }

        public string Value
        {
            get { return value; }
        }

        public override TotemType Type
        {
            get { return TotemType.Resolve<Types.String>(); }
        }

        public override TotemValue ByTotemValue
        {
            get { return new TotemString(value); }
        }

        public override TotemValue Add(TotemValue other)
        {
            if (other.GetType() == typeof(TotemString))
                return new TotemString(value + ((TotemString)other).Value);
            return base.Add(other);
        }

        public override bool Equals(object obj)
        {
            if (!object.ReferenceEquals(obj, null) && obj.GetType() == typeof(TotemString))
                return ((TotemString)obj).Value == value;
            return base.Equals(obj);
        }
    }
}

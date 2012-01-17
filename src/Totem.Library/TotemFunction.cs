
namespace Totem.Library
{
    public abstract class TotemFunction : TotemValue
    {
        public TotemValue Execute()
        {
            // Generate local scope
            return TotemRun();
            // Destroy generated scope
        }

        protected void LocalSet(string name, TotemValue value)
        {
            // Set value
        }

        protected TotemValue LocalGet(string name)
        {
            return TotemUndefined.Value;
        }

        protected abstract TotemValue TotemRun();

        protected TotemFunction()
        {

        }

        public override TotemValue ByTotemValue
        {
            get { return this; }
        }
    }
}


namespace Totem.Library
{
    public class ClrMethod : TotemMethod
    {
        private Method method;

        public ClrMethod(string name, TotemValue @this, Method method)
            : base(@this, TotemScope.Global, name, new TotemParameter[0])
        {
            this.method = method;
        }

        public override TotemValue Execute(TotemArguments arguments)
        {
            return method(@this, arguments);
        }
    }
}

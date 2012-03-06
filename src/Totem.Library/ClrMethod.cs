
namespace Totem.Library
{
    public class ClrMethod : TotemMethod
    {
        private Function function;

        public ClrMethod(string name, Function function)
            : base(TotemScope.Global, name, new TotemParameter[0])
        {
            this.function = function;
        }

        protected override TotemValue TotemRun()
        {
            var args = (TotemArguments)LocalGet("arguments");
            return function(args);
        }
    }
}

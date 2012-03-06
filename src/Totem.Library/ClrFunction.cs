
namespace Totem.Library
{
    public class ClrFunction : TotemFunction
    {
        private Function function;

        public ClrFunction(string name, Function function)
            : base(TotemScope.Global, name, new TotemParameter[0])
        {
            this.function = function;
        }

        public override TotemValue Execute(TotemArguments arguments)
        {
            return function(arguments);
        }
    }
}

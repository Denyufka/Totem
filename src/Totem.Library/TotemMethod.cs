
namespace Totem.Library
{
    public abstract class TotemMethod : TotemFunction
    {
        protected TotemValue @this;
        public TotemMethod(TotemValue @this, TotemScope env, string name, TotemParameter[] parametersDefinition)
            : base(env, name, parametersDefinition)
        {
            this.@this = @this;
        }
    }
}

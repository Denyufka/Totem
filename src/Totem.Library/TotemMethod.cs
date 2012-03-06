using System;

namespace Totem.Library
{
    public abstract class TotemMethod : TotemFunction
    {
        public TotemMethod(TotemScope env, string name, TotemParameter[] parametersDefinition)
            : base(env, name, parametersDefinition)
        {

        }

        public override TotemValue Execute(TotemArguments arguments)
        {
            if (object.ReferenceEquals(null, arguments.ThisObject) && arguments.IsSet("this"))
            {
                var nArgs = new TotemArguments(arguments.Value("this"));
                foreach (var val in arguments)
                    if (val.Name != "this")
                        nArgs.Add(val.Name, val.Value);
                arguments = nArgs;
            }
            if (object.ReferenceEquals(arguments.ThisObject, null))
                throw new InvalidOperationException("Can't call a method without specifying a this-object.");

            return base.Execute(arguments);
        }
    }
}

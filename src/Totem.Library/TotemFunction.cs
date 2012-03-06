using System;
using System.Collections.Generic;

namespace Totem.Library
{
    public abstract class TotemFunction : TotemValue
    {
        private readonly TotemParameter[] parametersDefinition;
        private readonly string name;
        private readonly TotemScope environment;
        private Stack<TotemScope> scopes = new Stack<TotemScope>();

        public override TotemValue Execute(TotemArguments arguments)
        {
            arguments = arguments ?? new TotemArguments(null);
            using (var scope = new ScopeWrapper(this))
            {
                scope.Declare("arguments");
                scope.Set("arguments", arguments);
                for (int i = 0; i < parametersDefinition.Length; i++)
                {
                    var param = parametersDefinition[i];
                    if (arguments.IsSet(i))
                    {
                        scope.Declare(param.Name);
                        scope.Set(param.Name, arguments.Value(i));
                    }
                    else if (arguments.IsSet(param.Name))
                    {
                        scope.Declare(param.Name);
                        scope.Set(param.Name, arguments.Value(param.Name));
                    }
                }
                try
                {
                    return TotemRun();
                }
                catch (Exception e)
                {
                    throw;// new TotemException(e);
                }
            }
        }

        public string Name { get { return name; } }

        protected TotemScope Scope { get { return scopes.Count == 0 ? environment : scopes.Peek(); } }

        protected void LocalDeclare(string name)
        {
            Scope.Declare(name);
        }

        protected void LocalSet(string name, TotemValue value)
        {
            Scope.Set(name, value);
        }

        protected TotemValue LocalGet(string name)
        {
            return Scope.Get(name) ?? TotemValue.Undefined;
        }

        protected void LocalDec(string name, TotemValue value)
        {
            Scope.Declare(name);
            Scope.Set(name, value);
        }

        protected virtual TotemValue TotemRun()
        {
            throw new NotImplementedException("Either Execute or TotemRun needs to be implemented in a subclass");
        }

        protected TotemFunction(TotemScope env, string name, TotemParameter[] parametersDefinition)
        {
            this.environment = env;
            this.name = name;
            this.parametersDefinition = parametersDefinition ?? new TotemParameter[0];
        }

        public override TotemValue ByTotemValue
        {
            get { return this; }
        }

        public override TotemType Type
        {
            get { throw new NotImplementedException(); }
        }

        public class ScopeWrapper : TotemScope, IDisposable
        {
            private TotemFunction function;
            public ScopeWrapper(TotemFunction function)
                : base(function.Scope)
            {
                this.function = function;
                function.scopes.Push(this);
            }

            public void Dispose()
            {
                if (function.Scope != this)
                {
                    throw new InvalidOperationException("Scope-stack invalid");
                }
                function.scopes.Pop();
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Totem.Library
{
    public abstract class TotemFunction : TotemValue
    {
        private readonly TotemParameter[] parametersDefinition;
        private readonly string name;
        private readonly TotemEnvironment environment;
        private Stack<TotemEnvironment> executionEnvironments = new Stack<TotemEnvironment>();

        public override TotemValue Execute(TotemArguments arguments)
        {
            arguments = arguments ?? new TotemArguments(null);
            var executionEnvironment = new TotemEnvironment(environment);
            executionEnvironments.Push(executionEnvironment);
            executionEnvironment.Declare("arguments");
            executionEnvironment.Set("arguments", arguments);
            for (int i = 0; i < parametersDefinition.Length; i++)
            {
                var param = parametersDefinition[i];
                if (arguments.IsSet(i))
                {
                    executionEnvironment.Declare(param.Name);
                    executionEnvironment.Set(param.Name, arguments.Value(i));
                }
                else if (arguments.IsSet(param.Name))
                {
                    executionEnvironment.Declare(param.Name);
                    executionEnvironment.Set(param.Name, arguments.Value(param.Name));
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
            finally
            {
                executionEnvironment = null;
                executionEnvironments.Pop();
            }
        }

        public string Name { get { return name; } }

        protected TotemEnvironment Environment { get { return executionEnvironments.Peek(); } }

        protected void LocalDeclare(string name)
        {
            Environment.Declare(name);
        }

        protected void LocalSet(string name, TotemValue value)
        {
            Environment.Set(name, value);
        }

        protected TotemValue LocalGet(string name)
        {
            return Environment.Get(name) ?? TotemValue.Undefined;
        }

        protected void LocalDec(string name, TotemValue value)
        {
            Environment.Declare(name);
            Environment.Set(name, value);
        }

        protected virtual TotemValue TotemRun()
        {
            throw new NotImplementedException("Either Execute or TotemRun needs to be implemented in a subclass");
        }

        protected TotemFunction(TotemEnvironment env, string name, TotemParameter[] parametersDefinition)
        {
            this.environment = env;
            this.name = name;
            this.parametersDefinition = parametersDefinition ?? new TotemParameter[0];
        }

        public override TotemValue ByTotemValue
        {
            get { return this; }
        }

        public override TotemType TotemType
        {
            get { throw new NotImplementedException(); }
        }
    }
}

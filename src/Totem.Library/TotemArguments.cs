using System.Collections.Generic;
using System.Linq;

namespace Totem.Library
{
    public class TotemArguments : TotemValue, IEnumerable<TotemArguments.TotemArgument>
    {
        public struct TotemArgument
        {
            public string Name { get; set; }
            public TotemValue Value { get; set; }
        }
        private readonly List<TotemArgument> args = new List<TotemArgument>();

        public TotemArguments()
        {
        }

        public bool IsSet(int pos)
        {
            return args.Where(a => a.Name == null).Skip(pos).Take(1).Any();
        }

        public bool IsSet(string name)
        {
            return args.Any(a => a.Name == name);
        }

        public TotemValue Value(int pos)
        {
            return args.Where(a => a.Name == null).Skip(pos).First().Value;
        }

        public TotemValue Value(string name)
        {
            return args.Where(a => a.Name == name).First().Value;
        }

        public void Add(string name, TotemValue value)
        {
            args.Add(new TotemArgument
            {
                Name = name,
                Value = value
            });
        }

        public override TotemValue ByTotemValue
        {
            get { return this; }
        }

        #region IEnumerable<TotemArguments> Members

        IEnumerator<TotemArguments.TotemArgument> IEnumerable<TotemArguments.TotemArgument>.GetEnumerator()
        {
            return args.Where(a => a.Name == null).Concat(args.Where(a => a.Name != null)).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TotemArguments.TotemArgument>)this).GetEnumerator();
        }

        #endregion

        public override TotemType Type
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}

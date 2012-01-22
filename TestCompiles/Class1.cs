
namespace TestCompiles
{
    public class Class1
    {
        string[] Method1()
        {
            return new string[] { "1", "2", "3" };
        }

        string Method2(object t)
        {
            string te;
            if (t == null)
                te = "true";
            else if (t != null)
                te = "false";
            else
                te = "elseif";
            return te;
        }

        public Class1()
            : this(new string[] { "1", "2", "3" })
        {

        }
        public Class1(string[] args)
        {

        }
    }
}

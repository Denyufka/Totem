
namespace TestCompiles
{
    public class Class1
    {
        string[] Method1()
        {
            return new string[] { "1", "2", "3" };
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

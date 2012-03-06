
namespace TestCompiles
{
    public class MyBaseType
    {
        private int value;

        public MyBaseType(int value)
        {
            this.value = value;
        }

        public static MyBaseType operator ++(MyBaseType mba)
        {
            return new MyBaseType(mba.value + 1);
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}

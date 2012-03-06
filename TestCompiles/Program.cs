using System;
using Totem.Library;

namespace TestCompiles
{
    public static class Program
    {
        public static void Main()
        {
            MyBaseType mba1 = new MyBaseType(2);
            var mba2 = mba1++;
            var mba3 = ++mba1;
            Console.WriteLine(mba1);
            Console.WriteLine(mba2);
            Console.WriteLine(mba3);

            var a1 = 2;
            var a2 = a1++;
            var a3 = ++a1;
            Console.WriteLine(a1);
            Console.WriteLine(a2);
            Console.WriteLine(a3);

            Console.ReadLine();
        }

        static void Test(params string[] str)
        {
            Console.WriteLine(String.Join(", ", str));
        }

        static void ForLoops()
        {
            for (int i = 0; i < 10; i++)
                Console.WriteLine(i);
        }

        static void Using()
        {
            using (IDisposable id = null)
            {
                Console.WriteLine(id);
            }
        }

        static void TotemFor()
        {
            for (TotemValue i = new TotemNumber(0L); (bool)(i < new TotemNumber(10L)); i++)
                Console.WriteLine(i);
        }
    }
}

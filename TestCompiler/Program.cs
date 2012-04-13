using IKVM.Reflection;
using IKVM.Reflection.Emit;
using Totem.Library;

namespace TestCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Universe u = new Universe();
            var asm = u.DefineDynamicAssembly(new AssemblyName("test2"), IKVM.Reflection.Emit.AssemblyBuilderAccess.Save);
            var mod = asm.DefineDynamicModule("test2", "test2.exe");

            System.Func<System.Type, Type> L = ty => u.Load(ty.Assembly.FullName).GetType(ty.FullName);

            var t = mod.DefineType("test2.Test", TypeAttributes.Public, L(typeof(Totem.Library.Function)));
            var md = t.DefineMethod("Main", MethodAttributes.Static | MethodAttributes.Public, L(typeof(void)), Type.EmptyTypes);
            asm.SetEntryPoint(md);

            var ctor = t.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);

            var il = md.GetILGenerator();
            il.Emit(OpCodes.Ldstr, "Click enter to start");
            il.Emit(OpCodes.Call, L(typeof(System.Console)).GetMethod("WriteLine", new Type[] { L(typeof(string)) }));
            il.Emit(OpCodes.Call, L(typeof(System.Console)).GetMethod("ReadLine", Type.EmptyTypes));
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Callvirt, L(typeof(TotemFunction)).GetMethod("Execute"));
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Call, L(typeof(System.Console)).GetMethod("ReadLine", Type.EmptyTypes));
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);

            var ctorIl = ctor.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, L(typeof(TotemScope)).GetProperty("Global").GetGetMethod());
            ctorIl.Emit(OpCodes.Ldstr, "Main");
            ctorIl.Emit(OpCodes.Ldnull);
            ctorIl.Emit(OpCodes.Call, L(typeof(TotemFunction)).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { L(typeof(TotemScope)), L(typeof(string)), L(typeof(TotemParameter[])) }, null));
            ctorIl.Emit(OpCodes.Ret);

            t.CreateType();

            asm.Save("test2.exe");
        }
    }
}

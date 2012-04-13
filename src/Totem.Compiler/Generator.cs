using System;
using System.Collections.Generic;
using IKVM.Reflection.Emit;
using Irony.Parsing;
using Totem.Library;
using IKCtor = IKVM.Reflection.ConstructorInfo;
using IKMethod = IKVM.Reflection.MethodInfo;
using IKType = IKVM.Reflection.Type;
using r = IKVM.Reflection;

namespace Totem.Compiler
{
    internal class Generator
    {
        private static Dictionary<Type, IKType> typeRegister = new Dictionary<Type, IKType>();
        private static Dictionary<System.Reflection.Assembly, r.Assembly> assemblyRegister = new Dictionary<System.Reflection.Assembly, r.Assembly>();
        private static r.Universe universe = new r.Universe();

        AssemblyBuilder assembly;
        ModuleBuilder module;
        string nsp, path;
        TotemGrammar grammar;


        internal static IKType function;
        internal static IKType value;
        internal static IKType tstring;
        internal static IKType arguments;
        internal static IKType parameter;
        internal static IKType @bool;
        internal static IKType scope;

        internal static IKType arr_parameters;
        internal static IKType sys_bool;

        internal static IKMethod value_val;

        internal static IKMethod undefined;
        internal static IKMethod @null;
        internal static IKMethod function_run;
        internal static IKMethod execute;
        internal static IKCtor function_ctor;
        internal static IKMethod function_local_set;
        internal static IKMethod function_local_get;
        internal static IKMethod function_local_dec;
        internal static IKMethod function_env;
        internal static IKCtor number_ctor_long;
        internal static IKCtor string_ctor;
        internal static IKCtor parameter_ctor;
        internal static IKCtor arguments_ctor;
        internal static IKMethod arguments_add;

        internal static IKMethod value_add;
        internal static IKMethod value_sub;
        internal static IKMethod value_mul;
        internal static IKMethod value_div;
        internal static IKMethod value_eq;
        internal static IKMethod value_neq;
        internal static IKMethod value_gt;
        internal static IKMethod value_lt;
        internal static IKMethod value_lte;
        internal static IKMethod value_gte;
        internal static IKMethod value_incr;
        internal static IKMethod value_istrue;

        internal static IKCtor scope_ctor;

        internal static IKMethod get_prop;
        internal static IKMethod set_prop;

        internal static IKCtor bool_ctor;

        internal static IKMethod dispose;

        HashSet<string> fn_names = new HashSet<string>() { "Main" };
        Stack<FunctionPoints> functionPoints = new Stack<FunctionPoints>();

        private class FunctionPoints
        {
            public HashSet<LocalBuilder> Avail = new HashSet<LocalBuilder>();
            public HashSet<LocalBuilder> SVars = new HashSet<LocalBuilder>();
            public MethodBuilder MethodDefinition { get; set; }
            public IlProcessor Il { get; set; }
            public Label EndOfStatementLabel { get; set; }
            public Stack<LoopDec> LoopDecs = new Stack<LoopDec>();
        }

        private class LoopDec
        {
            public Label ContinueLabel;
            public Label BreakLabel;
        }

        internal Generator(string nsp, string path, TotemGrammar grammar)
        {
            assembly = universe.DefineDynamicAssembly(new IKVM.Reflection.AssemblyName(nsp), AssemblyBuilderAccess.Save);
            module = assembly.DefineDynamicModule(nsp, path);
            LoadAll();
            this.nsp = nsp;
            this.path = path;
            this.grammar = grammar;
        }

        private static IKType Load(Type type)
        {
            IKType reference = null;
            if (!typeRegister.TryGetValue(type, out reference))
            {
                r.Assembly assembly = null;
                if (!assemblyRegister.TryGetValue(type.Assembly, out assembly))
                {
                    assembly = universe.Load(type.Assembly.FullName);
                }
                reference = assembly.GetType(type.FullName, true);
                //reference = module.
                typeRegister.Add(type, reference);
            }
            return reference;
        }

        private static void LoadAll()
        {
            // Types
            function = Load(typeof(TotemFunction));
            value = Load(typeof(TotemValue));
            tstring = Load(typeof(TotemString));
            arguments = Load(typeof(TotemArguments));
            parameter = Load(typeof(TotemParameter));
            @bool = Load(typeof(TotemBool));
            scope = Load(typeof(TotemFunction.ScopeWrapper));

            arr_parameters = Load(typeof(TotemParameter[]));
            sys_bool = Load(typeof(bool));

            // Methods
            value_val = Load(typeof(TotemValue)).GetProperty("ByTotemValue").GetGetMethod();

            function_run = Load(typeof(TotemFunction)).GetMethod("TotemRun", r.BindingFlags.NonPublic | r.BindingFlags.Instance);
            execute = Load(typeof(TotemValue)).GetMethod("Execute");
            function_ctor = Load(typeof(TotemFunction)).GetConstructor(r.BindingFlags.NonPublic | r.BindingFlags.Instance, null, new IKType[] { Load(typeof(TotemScope)), Load(typeof(string)), Load(typeof(TotemParameter[])) }, null);
            function_local_set = Load(typeof(TotemFunction)).GetMethod("LocalSet", r.BindingFlags.NonPublic | r.BindingFlags.Instance);
            function_local_get = Load(typeof(TotemFunction)).GetMethod("LocalGet", r.BindingFlags.NonPublic | r.BindingFlags.Instance);
            function_local_dec = Load(typeof(TotemFunction)).GetMethod("LocalDec", r.BindingFlags.NonPublic | r.BindingFlags.Instance);
            function_env = Load(typeof(TotemFunction)).GetProperty("Scope", r.BindingFlags.NonPublic | r.BindingFlags.Instance).GetGetMethod(true);

            arguments_ctor = Load(typeof(TotemArguments)).GetConstructor(IKType.EmptyTypes);
            arguments_add = Load(typeof(TotemArguments)).GetMethod("Add", r.BindingFlags.Public | r.BindingFlags.Instance | r.BindingFlags.DeclaredOnly);

            number_ctor_long = Load(typeof(TotemNumber)).GetConstructor(new IKType[] { Load(typeof(long)) });

            string_ctor = Load(typeof(TotemString)).GetConstructor(new IKType[] { Load(typeof(string)) });

            parameter_ctor = Load(typeof(TotemParameter)).GetConstructor(new IKType[] { Load(typeof(string)), Load(typeof(TotemValue)) });

            undefined = Load(typeof(TotemValue)).GetProperty("Undefined").GetGetMethod();
            @null = Load(typeof(TotemValue)).GetProperty("Null").GetGetMethod();

            value_add = Load(typeof(TotemValue)).GetMethod("op_Addition", r.BindingFlags.Static | r.BindingFlags.Public);
            value_sub = Load(typeof(TotemValue)).GetMethod("op_Subtraction", r.BindingFlags.Static | r.BindingFlags.Public);
            value_mul = Load(typeof(TotemValue)).GetMethod("op_Multiply", r.BindingFlags.Static | r.BindingFlags.Public);
            value_div = Load(typeof(TotemValue)).GetMethod("op_Division", r.BindingFlags.Static | r.BindingFlags.Public);
            value_eq = Load(typeof(TotemValue)).GetMethod("op_Equality", r.BindingFlags.Static | r.BindingFlags.Public);
            value_neq = Load(typeof(TotemValue)).GetMethod("op_Inequality", r.BindingFlags.Static | r.BindingFlags.Public);
            value_lt = Load(typeof(TotemValue)).GetMethod("op_LessThan", r.BindingFlags.Static | r.BindingFlags.Public);
            value_gt = Load(typeof(TotemValue)).GetMethod("op_GreaterThan", r.BindingFlags.Static | r.BindingFlags.Public);
            value_lte = Load(typeof(TotemValue)).GetMethod("op_LessThanOrEqual", r.BindingFlags.Static | r.BindingFlags.Public);
            value_gte = Load(typeof(TotemValue)).GetMethod("op_GreaterThanOrEqual", r.BindingFlags.Static | r.BindingFlags.Public);
            value_incr = Load(typeof(TotemValue)).GetMethod("op_Increment", r.BindingFlags.Static | r.BindingFlags.Public);
            value_istrue = Load(typeof(TotemValue)).GetMethod("op_Explicit", r.BindingFlags.Static | r.BindingFlags.Public, null, r.CallingConventions.Standard, new IKType[] { Load(typeof(TotemValue)) }, null);

            scope_ctor = Load(typeof(TotemFunction.ScopeWrapper)).GetConstructor(new IKType[] { Load(typeof(TotemFunction)) });

            dispose = Load(typeof(IDisposable)).GetMethod("Dispose");

            get_prop = Load(typeof(TotemValue)).GetMethod("GetProp");
            set_prop = Load(typeof(TotemValue)).GetMethod("SetProp");

            bool_ctor = Load(typeof(TotemBool)).GetConstructor(new IKType[] { Load(typeof(bool)) });
        }

        internal void GenerateProgram(ParseTreeNode rootNode)
        {
            if (rootNode.Term.Name != "Prog")
                throw new InvalidOperationException("Can't compile from the middle of a tree");

            TypeBuilder td = module.DefineType(nsp + ".Program", r.TypeAttributes.Public, function);

            MethodBuilder md = td.DefineMethod("Main", r.MethodAttributes.Static | r.MethodAttributes.Public, Load(typeof(void)), IKType.EmptyTypes);

            assembly.SetEntryPoint(md);

            var ctor = td.DefineConstructor(r.MethodAttributes.Public, r.CallingConventions.Standard, IKType.EmptyTypes);
            var ctorIl = ctor.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, Load(typeof(TotemScope)).GetProperty("Global").GetGetMethod());
            ctorIl.Emit(OpCodes.Ldstr, "Main");
            ctorIl.Emit(OpCodes.Ldnull);
            ctorIl.Emit(OpCodes.Call, function_ctor);
            ctorIl.Emit(OpCodes.Ret);

            using (var il = md.GetILProcessor())
            {
                il.Emit(OpCodes.Ldstr, "Click enter to start.");
                il.Emit(OpCodes.Call, Load(typeof(Console)).GetMethod("WriteLine", new IKType[] { Load(typeof(string)) }));
                il.Emit(OpCodes.Call, Load(typeof(Console)).GetMethod("ReadLine", IKType.EmptyTypes));
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Newobj, ctor);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Callvirt, Load(typeof(TotemFunction)).GetMethod("Execute"));
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ldstr, "Done.");
                il.Emit(OpCodes.Call, Load(typeof(Console)).GetMethod("WriteLine", new IKType[] { Load(typeof(string)) }));
                il.Emit(OpCodes.Call, Load(typeof(Console)).GetMethod("ReadLine", IKType.EmptyTypes));
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ret);
            }

            md = td.DefineMethod("TotemRun", r.MethodAttributes.Family | r.MethodAttributes.HideBySig | r.MethodAttributes.Virtual, value, IKType.EmptyTypes);
            using (var il = md.GetILProcessor())
            {

                functionPoints.Push(new FunctionPoints
                {
                    Avail = new HashSet<LocalBuilder>(),
                    SVars = new HashSet<LocalBuilder>(),
                    MethodDefinition = md,
                    Il = il
                });

                var element_list = rootNode.ChildNodes[0];
                GenerateFunction(il, element_list.ChildNodes);
                var next = Instruction.Create(OpCodes.Nop);
                il.Append(next);

                il.Emit(OpCodes.Call, undefined);
                il.Emit(OpCodes.Ret);
            }
            var t = td.CreateType();
            return;
        }

        private void GenerateFunction(IlProcessor il, IEnumerable<ParseTreeNode> statement_nodes)
        {
            Instruction current = Instruction.Create(OpCodes.Nop);
            il.Append(current);
            foreach (var node in statement_nodes)
            {
                GenerateStatement(il, ref current, node);
            }
        }

        private void GenerateFunction(IlProcessor il, ref Instruction start, ref Instruction current, string name, string mName, ParseTreeNodeList parameters, IEnumerable<ParseTreeNode> body)
        {
            var fn = module.DefineType(nsp + "." + mName, r.TypeAttributes.Public | r.TypeAttributes.Sealed, function);
            var ctor = fn.DefineConstructor(r.MethodAttributes.Public, r.CallingConventions.Standard, new IKType[] { Load(typeof(TotemScope)), Load(typeof(string)), arr_parameters });
            ctor.DefineParameter(0, r.ParameterAttributes.In, "env");
            ctor.DefineParameter(1, r.ParameterAttributes.In, "name");
            ctor.DefineParameter(2, r.ParameterAttributes.In, "parameters");
            var ctorIl = ctor.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldarg_1);
            ctorIl.Emit(OpCodes.Ldarg_2);
            ctorIl.Emit(OpCodes.Ldarg_3);
            ctorIl.Emit(OpCodes.Call, function_ctor);
            ctorIl.Emit(OpCodes.Ret);

            var fnc = fn.DefineMethod("TotemRun", r.MethodAttributes.Family | r.MethodAttributes.HideBySig | r.MethodAttributes.Virtual, r.CallingConventions.Standard, value, IKType.EmptyTypes);
            using (var fnil = fnc.GetILProcessor())
            {
                functionPoints.Push(new FunctionPoints
                {
                    Avail = new HashSet<LocalBuilder>(),
                    SVars = new HashSet<LocalBuilder>(),
                    MethodDefinition = fnc,
                    Il = fnil
                });
                GenerateFunction(fnil, body);
                fnil.Emit(OpCodes.Nop);
                fnil.Emit(OpCodes.Call, undefined);
                fnil.Emit(OpCodes.Ret);
                functionPoints.Pop();
            }
            fn.CreateType();

            var pn = GetSVar(arr_parameters);
            var prev = start;
            il.Add(ref prev, Instruction.Create(OpCodes.Ldc_I4, parameters.Count));
            il.Add(ref prev, Instruction.Create(OpCodes.Newarr, Generator.parameter));
            il.Add(ref prev, Instruction.Create(OpCodes.Stloc, pn));
            for (var i = 0; i < parameters.Count; i++)
            {
                var param = parameters[i];
                il.Add(ref prev, Instruction.Create(OpCodes.Ldloc, pn));
                il.Add(ref prev, Instruction.Create(OpCodes.Ldc_I4, i));
                il.Add(ref prev, Instruction.Create(OpCodes.Ldstr, param.ChildNodes[0].Token.ValueString));
                if (param.ChildNodes.Count > 1)
                {
                    GenerateExpression(il, ref start, ref prev, param.ChildNodes[1].ChildNodes[1]);
                }
                else
                {
                    il.Add(ref prev, Instruction.Create(OpCodes.Call, undefined));
                }
                il.Add(ref prev, Instruction.Create(OpCodes.Newobj, parameter_ctor));
                il.Add(ref prev, Instruction.Create(OpCodes.Stelem_Ref));
            }
            start = prev;
            il.Add(ref current, Instruction.Create(OpCodes.Ldarg_0));
            il.Add(ref current, Instruction.Create(OpCodes.Callvirt, function_env));
            il.Add(ref current, Instruction.Create(OpCodes.Ldstr, name));
            il.Add(ref current, Instruction.Create(OpCodes.Ldloc, pn));
            il.Add(ref current, Instruction.Create(OpCodes.Newobj, ctor));
        }

        private void GenerateStatement(IlProcessor il, ref Instruction current, ParseTreeNode node)
        {
            var label = CreateStatementLabel(il);
            var start = current;
            LocalBuilder pn = null, pn2 = null;
            il.Add(ref current, Instruction.Create(OpCodes.Nop));
            switch (node.Term.Name)
            {
                case "VarStmt":
                    GenerateDeclaration(il, ref start, ref current, node);
                    break;
                case "FuncDefStmt":
                    var name = node.ChildNodes[1].Token.ValueString;
                    var mName = MakeFunctionName(name);
                    il.Add(ref current, Instruction.Create(OpCodes.Ldarg_0));
                    il.Add(ref current, Instruction.Create(OpCodes.Ldstr, name));
                    GenerateFunction(il, ref start, ref current, name, mName, node.ChildNodes[2].ChildNodes, node.ChildNodes[3].ChildNodes[0].ChildNodes);
                    il.Add(ref current, Instruction.Create(OpCodes.Callvirt, function_local_dec));
                    break;
                case "FlowControlStmt":
                    var keyword = node.ChildNodes[0].Token.ValueString;
                    switch (keyword)
                    {
                        case "return":
                            if (node.ChildNodes.Count > 1)
                            {
                                GenerateExpression(il, ref start, ref current, node.ChildNodes[1]);
                            }
                            else
                            {
                                il.Add(ref current, Instruction.Create(OpCodes.Call, undefined));
                            }
                            il.Add(ref current, Instruction.Create(OpCodes.Ret));
                            break;
                        case "continue":
                            il.Add(ref current, Instruction.Create(OpCodes.Br, GetLoopDec().ContinueLabel));
                            break;
                        case "break":
                            il.Add(ref current, Instruction.Create(OpCodes.Br, GetLoopDec().BreakLabel));
                            break;
                        default:
                            throw new InvalidOperationException("Unknown flow control keyword " + keyword);
                    }
                    break;
                case "ExprStmt":
                    GenerateExpression(il, ref start, ref current, node.ChildNodes[0]);
                    il.Add(ref current, Instruction.Create(OpCodes.Pop));
                    break;
                case "IfElseStmt":
                    pn = GetSVar(sys_bool);
                    var endLabel = il.DefineLabel();
                    GenerateExpression(il, ref start, ref current, node.ChildNodes[1].ChildNodes[0]);
                    il.Add(ref current, Instruction.Create(OpCodes.Call, value_istrue));
                    il.Add(ref current, Instruction.Create(OpCodes.Ldc_I4_0));
                    il.Add(ref current, Instruction.Create(OpCodes.Ceq));
                    il.Add(ref current, Instruction.Create(OpCodes.Stloc, pn));
                    il.Add(ref current, Instruction.Create(OpCodes.Ldloc, pn));

                    if (node.ChildNodes.Count == 5)
                    {
                        var elseLabel = il.DefineLabel();
                        il.Add(ref current, Instruction.Create(OpCodes.Brtrue, elseLabel));
                        GenerateStatement(il, ref current, node.ChildNodes[2]);
                        il.Add(ref current, Instruction.Create(OpCodes.Br, endLabel));

                        il.Add(ref current, Instruction.Create(Specials.Label, elseLabel));
                        GenerateStatement(il, ref current, node.ChildNodes[4]);
                    }
                    else
                    {
                        il.Add(ref current, Instruction.Create(OpCodes.Brtrue, endLabel));
                        GenerateStatement(il, ref current, node.ChildNodes[2]);
                    }
                    il.Add(ref current, Instruction.Create(Specials.Label, endLabel));

                    break;
                case "ForStmt":
                    AddLoopDec(il);
                    // Push new scope
                    pn = GetVar(scope);

                    il.Add(ref start, Instruction.Create(Specials.BeginTotemScope, pn));

                    // Initializer
                    il.Add(ref current, Instruction.Create(OpCodes.Nop));
                    var initializer = node.ChildNodes[1];
                    if (initializer.ChildNodes.Count != 0)
                    {
                        initializer = initializer.ChildNodes[0];
                        if (initializer.ChildNodes.Count == 1)
                        {
                            GenerateExpression(il, ref start, ref current, initializer.ChildNodes[0]);
                            il.Add(ref current, Instruction.Create(OpCodes.Pop));
                        }
                        else
                        {
                            foreach (var dec in initializer.ChildNodes[1].ChildNodes)
                                GenerateDeclaration(il, ref start, ref current, dec);
                        }
                    }
                    var conditionLabel = il.DefineLabel();
                    var bodyLabel = il.DefineLabel();
                    endLabel = il.DefineLabel();
                    il.Add(ref current, Instruction.Create(OpCodes.Br, conditionLabel));

                    // Body
                    il.Add(ref current, Instruction.Create(Specials.Label, bodyLabel));
                    GenerateStatement(il, ref current, node.ChildNodes[4]);
                    il.Add(ref current, Instruction.Create(OpCodes.Nop));

                    // Increment
                    Instruction incr = il.Add(ref current, Instruction.Create(Specials.Label, GetLoopDec().ContinueLabel));
                    var increment = node.ChildNodes[3];
                    if (increment.ChildNodes.Count != 0)
                    {
                        GenerateExpression(il, ref start, ref current, increment.ChildNodes[0]);
                    }

                    // Condition
                    il.Add(ref current, Instruction.Create(Specials.Label, conditionLabel));
                    var condition = node.ChildNodes[2];
                    if (condition.ChildNodes.Count == 0)
                    {
                        il.Add(ref current, Instruction.Create(OpCodes.Ldc_I4_1));
                        il.Add(ref current, Instruction.Create(OpCodes.Call, bool_ctor)); // Create TotemBool true
                    }
                    else
                    {
                        GenerateExpression(il, ref start, ref current, condition.ChildNodes[0]);
                    }
                    il.Add(ref current, Instruction.Create(OpCodes.Call, value_istrue));
                    il.Add(ref current, Instruction.Create(OpCodes.Brtrue, bodyLabel));
                    il.Add(ref current, Instruction.Create(Specials.Label, GetLoopDec().BreakLabel));
                    il.Add(ref current, Instruction.Create(OpCodes.Leave, endLabel));

                    pn2 = GetSVar(Load(typeof(bool)));

                    // Pop scope
                    il.Add(ref current, Instruction.Create(Specials.EndTotemScope, pn));
                    RemLoopDec();
                    RelVar(pn);
                    il.Add(ref current, Instruction.Create(Specials.Label, endLabel));
                    break;
                case "Block":
                    foreach (var stmt in node.ChildNodes[0].ChildNodes)
                    {
                        GenerateStatement(il, ref current, stmt);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Term " + node.Term.Name + " is not a statement");
            }
            RelSVars();
            il.Add(ref current, Instruction.Create(Specials.Label, label));
        }

        private void GenerateDeclaration(IlProcessor il, ref Instruction start, ref Instruction current, ParseTreeNode node)
        {
            string name;
            switch (node.Term.Name)
            {
                case "VarStmt":
                    var declarators = node.ChildNodes[1].ChildNodes;
                    foreach (var dec in declarators)
                    {
                        GenerateDeclaration(il, ref start, ref current, dec);
                    }
                    break;
                case "VarExpr":
                    name = node.ChildNodes[0].Token.ValueString;
                    il.Add(ref current, Instruction.Create(OpCodes.Ldarg_0));
                    il.Add(ref current, Instruction.Create(OpCodes.Ldstr, name));
                    if (node.ChildNodes.Count > 1)
                    {
                        GenerateExpression(il, ref start, ref current, node.ChildNodes[1].ChildNodes[1]);
                    }
                    else
                    {
                        il.Add(ref current, Instruction.Create(OpCodes.Call, undefined));
                    }
                    il.Add(ref current, Instruction.Create(OpCodes.Callvirt, function_local_dec));
                    break;
                default:
                    throw new InvalidOperationException("Term " + node.Term.Name + " is not a declaration statement");
            }
        }

        private void GenerateExpression(IlProcessor il, ref Instruction start, ref Instruction current, ParseTreeNode node)
        {
            string name;
            LocalBuilder pn;
            var prev = start;
            switch (node.Term.Name)
            {
                case "ConstExpr":
                    GenerateConst(il, start, ref current, node.ChildNodes[0]);
                    break;
                case "BinExpr":
                    GenerateExpression(il, ref start, ref current, node.ChildNodes[0]); // left
                    GenerateExpression(il, ref start, ref current, node.ChildNodes[2]); // right
                    var keySymbol = node.ChildNodes[1].Token.ValueString;
                    switch (keySymbol)
                    {
                        case "+":
                            il.Add(ref current, Instruction.Create(OpCodes.Call, value_add));
                            break;
                        case "-":
                            il.Add(ref current, Instruction.Create(OpCodes.Call, value_sub));
                            break;
                        case "<":
                            il.Add(ref current, Instruction.Create(OpCodes.Call, value_lt));
                            break;
                        case ">":
                            il.Add(ref current, Instruction.Create(OpCodes.Call, value_gt));
                            break;
                        case "==":
                            il.Add(ref current, Instruction.Create(OpCodes.Call, value_eq));
                            break;
                        case "!=":
                            il.Add(ref current, Instruction.Create(OpCodes.Call, value_neq));
                            break;
                        case "*":
                            il.Add(ref current, Instruction.Create(OpCodes.Call, value_mul));
                            break;
                        case "/":
                            il.Add(ref current, Instruction.Create(OpCodes.Call, value_div));
                            break;
                        case "<=":
                            il.Add(ref current, Instruction.Create(OpCodes.Call, value_lte));
                            break;
                        case ">=":
                            il.Add(ref current, Instruction.Create(OpCodes.Call, value_gte));
                            break;
                        default:
                            throw new InvalidOperationException("Unknown bin expression key symbol " + keySymbol);
                    }
                    break;
                case "TerExpr": // a < b ? c : d
                    Label trueLabel = il.DefineLabel(),
                        endLabel = il.DefineLabel();
                    GenerateExpression(il, ref start, ref current, node.ChildNodes[0]); // a < b
                    il.Add(ref current, Instruction.Create(OpCodes.Call, value_istrue));
                    il.Add(ref current, Instruction.Create(OpCodes.Brtrue, trueLabel));

                    GenerateExpression(il, ref start, ref current, node.ChildNodes[3]); // d
                    il.Add(ref current, Instruction.Create(OpCodes.Br, endLabel));

                    il.Add(ref current, Instruction.Create(Specials.Label, trueLabel));
                    GenerateExpression(il, ref start, ref current, node.ChildNodes[2]); // c
                    il.Add(ref current, Instruction.Create(Specials.Label, endLabel));
                    break;
                case "FunctionCallExpr":
                    pn = GetSVar(arguments);
                    il.Add(ref prev, Instruction.Create(OpCodes.Newobj, arguments_ctor));
                    il.Add(ref prev, Instruction.Create(OpCodes.Stloc, pn));
                    foreach (var arg in node.ChildNodes[1].ChildNodes)
                    {
                        il.Add(ref prev, Instruction.Create(OpCodes.Ldloc, pn));
                        if (arg.ChildNodes.Count == 1)
                        {
                            il.Add(ref prev, Instruction.Create(OpCodes.Ldnull));
                        }
                        else
                        {
                            il.Add(ref prev, Instruction.Create(OpCodes.Ldstr, arg.ChildNodes[0].Token.ValueString));
                        }
                        GenerateExpression(il, ref start, ref prev, arg.ChildNodes[arg.ChildNodes.Count - 1]);
                        il.Add(ref prev, Instruction.Create(OpCodes.Callvirt, arguments_add));
                    }
                    start = prev;
                    GenerateExpression(il, ref start, ref current, node.ChildNodes[0]);
                    il.Add(ref current, Instruction.Create(OpCodes.Ldloc, pn));
                    il.Add(ref current, Instruction.Create(OpCodes.Callvirt, execute));
                    break;
                case "AssignExpr":
                    switch (node.ChildNodes[0].Term.Name)
                    {
                        case "identifier":
                            pn = GetSVar(value);
                            if (node.ChildNodes.Count == 3) // a = b / a += b etc.
                            {
                                GenerateExpression(il, ref start, ref start, node.ChildNodes[2]);
                                il.Add(ref start, Instruction.Create(OpCodes.Stloc, pn));
                                il.Add(ref current, Instruction.Create(OpCodes.Ldarg_0));
                                il.Add(ref current, Instruction.Create(OpCodes.Ldstr, node.ChildNodes[0].Token.ValueString));
                                il.Add(ref current, Instruction.Create(OpCodes.Ldloc, pn));
                                il.Add(ref current, Instruction.Create(OpCodes.Callvirt, function_local_set));
                                il.Add(ref current, Instruction.Create(OpCodes.Ldloc, pn));
                            }
                            else
                            {
                                var pn2 = GetSVar(value);
                                il.Add(ref current, Instruction.Create(OpCodes.Ldarg_0));
                                il.Add(ref current, Instruction.Create(OpCodes.Ldstr, node.ChildNodes[0].Token.ValueString));
                                il.Add(ref current, Instruction.Create(OpCodes.Callvirt, function_local_get));
                                il.Add(ref current, Instruction.Create(OpCodes.Dup));
                                il.Add(ref current, Instruction.Create(OpCodes.Stloc, pn));
                                // Increment and restore
                                il.Add(ref current, Instruction.Create(OpCodes.Call, value_incr));
                                il.Add(ref current, Instruction.Create(OpCodes.Stloc, pn2));
                                il.Add(ref current, Instruction.Create(OpCodes.Ldarg_0));
                                il.Add(ref current, Instruction.Create(OpCodes.Ldstr, node.ChildNodes[0].Token.ValueString));
                                il.Add(ref current, Instruction.Create(OpCodes.Ldloc, pn2));
                                il.Add(ref current, Instruction.Create(OpCodes.Callvirt, function_local_set));
                                il.Add(ref current, Instruction.Create(OpCodes.Ldloc, pn));
                            }
                            break;
                        case "MemberExpr":
                            pn = GetSVar(value);
                            if (node.ChildNodes.Count == 3) // a = b / a += b etc.
                            {
                                GenerateExpression(il, ref start, ref start, node.ChildNodes[2]);
                                il.Add(ref start, Instruction.Create(OpCodes.Stloc, pn));
                                GenerateExpression(il, ref start, ref current, node.ChildNodes[0].ChildNodes[0]);
                                il.Add(ref current, Instruction.Create(OpCodes.Ldstr, node.ChildNodes[0].ChildNodes[2].Token.ValueString));
                                il.Add(ref current, Instruction.Create(OpCodes.Ldloc, pn));
                                il.Add(ref current, Instruction.Create(OpCodes.Callvirt, set_prop));
                                il.Add(ref current, Instruction.Create(OpCodes.Ldloc, pn));
                            }
                            else
                            {

                            }
                            break;
                        default:
                            throw new InvalidOperationException("Invalid QualifiedName term " + node.ChildNodes[0].Term.Name);
                    }
                    break;
                case "identifier":
                    name = node.Token.ValueString;
                    il.Add(ref current, Instruction.Create(OpCodes.Ldarg_0));
                    il.Add(ref current, Instruction.Create(OpCodes.Ldstr, name));
                    il.Add(ref current, Instruction.Create(OpCodes.Callvirt, function_local_get));
                    break;
                case "FuncDefExpr":
                    if (node.ChildNodes[1] != null && node.ChildNodes[1].Token != null && node.ChildNodes[1].Token.Text == "=>")
                    {
                        name = "anonymous";
                        var mName = MakeFunctionName(name);
                        IEnumerable<ParseTreeNode> bodyNodes;
                        ParseTreeNodeList parameters;
                        if (node.ChildNodes[2].Term.Name == "Block")
                        {
                            bodyNodes = node.ChildNodes[2].ChildNodes[0].ChildNodes;
                        }
                        else if (node.ChildNodes[2].Term.Name.Contains("Stmt"))
                        {
                            bodyNodes = new ParseTreeNode[] { node.ChildNodes[2] };
                        }
                        else
                        {
                            var flowCtrlStmtNode = new ParseTreeNode(grammar.FlowControlStmt, new SourceSpan());
                            flowCtrlStmtNode.ChildNodes.Add(new ParseTreeNode(new Token(grammar.@return, new SourceLocation(), "return", "return")));
                            flowCtrlStmtNode.ChildNodes.Add(node.ChildNodes[2]);
                            bodyNodes = new ParseTreeNode[] { flowCtrlStmtNode };
                        }
                        if (node.ChildNodes[0].Term.Name == "identifier")
                        {
                            parameters = new ParseTreeNodeList();
                            var param = new ParseTreeNode(grammar.Parameter, node.ChildNodes[0].Span);
                            param.ChildNodes.Add(node.ChildNodes[0]);
                            parameters.Add(param);
                        }
                        else
                        {
                            parameters = node.ChildNodes[0].ChildNodes;
                        }
                        GenerateFunction(il, ref start, ref current, name, mName, parameters, bodyNodes);
                    }
                    else
                    {
                        if (node.ChildNodes[1].ChildNodes.Count == 1)
                            name = node.ChildNodes[1].ChildNodes[0].Token.ValueString;
                        else
                            name = "anonymous";
                        var mName = MakeFunctionName(name);
                        GenerateFunction(il, ref start, ref current, name, mName, node.ChildNodes[2].ChildNodes, node.ChildNodes[3].ChildNodes[0].ChildNodes);
                    }
                    break;
                case "MemberExpr":
                    GenerateExpression(il, ref start, ref current, node.ChildNodes[0]);
                    il.Add(ref current, Instruction.Create(OpCodes.Ldstr, node.ChildNodes[2].Token.ValueString));
                    il.Add(ref current, Instruction.Create(OpCodes.Callvirt, get_prop));
                    break;
                default:
                    throw new InvalidOperationException("Term " + node.Term.Name + " is not a valid expression term");
            }
        }

        private void GenerateConst(IlProcessor il, Instruction start, ref Instruction current, ParseTreeNode node)
        {
            var obj = node.Token.Value;
            if (node.Token.Terminal is KeyTerm)
            {
                switch ((string)obj)
                {
                    case "true":
                        il.Add(ref current, Instruction.Create(OpCodes.Ldc_I4_1));
                        il.Add(ref current, Instruction.Create(OpCodes.Newobj, bool_ctor));
                        break;
                    case "false":
                        il.Add(ref current, Instruction.Create(OpCodes.Ldc_I4_0));
                        il.Add(ref current, Instruction.Create(OpCodes.Newobj, bool_ctor));
                        break;
                    case "null":
                        il.Add(ref current, Instruction.Create(OpCodes.Call, @null));
                        break;
                    case "undefined":
                        il.Add(ref current, Instruction.Create(OpCodes.Call, undefined));
                        break;
                    default:
                        throw new InvalidOperationException("Unknown keyword " + obj);
                }
            }
            else if (obj is string)
            {
                il.Add(ref current, Instruction.Create(OpCodes.Ldstr, (string)obj));
                il.Add(ref current, Instruction.Create(OpCodes.Newobj, string_ctor));
            }
            else if (obj is int || obj is long)
            {
                il.Add(ref current, Instruction.Create(OpCodes.Ldc_I8, Convert.ToInt64(obj)));
                il.Add(ref current, Instruction.Create(OpCodes.Newobj, number_ctor_long));
            }
            else
                throw new InvalidOperationException("Unknown constant type " + obj.GetType());
        }

        private Label CreateStatementLabel(IlProcessor il)
        {
            return functionPoints.Peek().EndOfStatementLabel = il.DefineLabel();
        }

        private LocalBuilder GetVar(IKType type)
        {
            var p = functionPoints.Peek();
            LocalBuilder pn = null;
            foreach (var param in p.Avail)
            {
                if (param.LocalType == type)
                {
                    p.Avail.Remove(param);
                    pn = param;
                    break;
                }
            }
            if (pn == null)
            {
                pn = p.Il.AddLocal(type);
            }

            return pn;
        }

        private void RelVar(LocalBuilder pn)
        {
            var p = functionPoints.Peek();
            p.Avail.Add(pn);
        }

        private LocalBuilder GetSVar(IKType type)
        {
            var p = functionPoints.Peek();
            var pn = GetVar(type);
            p.SVars.Add(pn);
            return pn;
        }

        private void RelSVars()
        {
            var p = functionPoints.Peek();
            foreach (var sv in p.SVars)
                p.Avail.Add(sv);
        }

        private void AddLoopDec(IlProcessor il)
        {
            var p = functionPoints.Peek();
            p.LoopDecs.Push(new LoopDec
            {
                ContinueLabel = il.DefineLabel(),
                BreakLabel = il.DefineLabel()
            });
        }

        private LoopDec GetLoopDec()
        {
            return functionPoints.Peek().LoopDecs.Peek();
        }

        private void RemLoopDec()
        {
            var ld = functionPoints.Peek().LoopDecs.Pop();
        }

        private string MakeFunctionName(string name)
        {
            var mName = name[0].ToString().ToUpper() + name.Substring(1);
            int i = 0;
            while (fn_names.Contains(mName))
            {
                mName = name[0].ToString().ToUpper() + name.Substring(1) + "__" + (++i);
            }
            fn_names.Add(mName);
            return mName;
        }

        internal void Save()
        {
            assembly.Save(path);
        }
    }

    static class IlExtensions
    {
        public static Instruction Add(this IlProcessor il, ref Instruction prev, Instruction insert)
        {
            il.InsertAfter(prev, insert);
            prev = insert;
            return prev;
        }
    }
}

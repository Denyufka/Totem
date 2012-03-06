using System;
using System.Collections.Generic;
using Irony.Parsing;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Totem.Library;
using r = System.Reflection;

namespace Totem.Compiler
{
    internal class Generator
    {
        private Dictionary<Type, TypeReference> typeRegister = new Dictionary<Type, TypeReference>();
        private Dictionary<r.MethodBase, MethodReference> methodRegister = new Dictionary<r.MethodBase, MethodReference>();

        AssemblyDefinition assembly;
        ModuleDefinition module;
        string nsp;

        TypeReference function;
        TypeReference value;
        TypeReference tstring;
        TypeReference arguments;
        TypeReference parameter;
        TypeReference @bool;
        TypeReference scope;

        TypeReference arr_parameters;
        TypeReference sys_bool;

        MethodReference value_val;

        MethodReference undefined;
        MethodReference @null;
        MethodReference function_run;
        MethodReference execute;
        MethodReference function_ctor;
        MethodReference function_local_set;
        MethodReference function_local_get;
        MethodReference function_local_dec;
        MethodReference function_env;
        MethodReference number_ctor_long;
        MethodReference string_ctor;
        MethodReference parameter_ctor;
        MethodReference arguments_ctor;
        MethodReference arguments_add;

        MethodReference value_add;
        MethodReference value_sub;
        MethodReference value_mul;
        MethodReference value_div;
        MethodReference value_eq;
        MethodReference value_neq;
        MethodReference value_gt;
        MethodReference value_lt;
        MethodReference value_lte;
        MethodReference value_gte;
        MethodReference value_incr;
        MethodReference value_istrue;

        MethodReference scope_ctor;

        MethodReference get_prop;

        MethodReference bool_ctor;

        MethodReference dispose;

        HashSet<string> fn_names = new HashSet<string>() { "Main" };
        Stack<FunctionPoints> functionPoints = new Stack<FunctionPoints>();

        private class FunctionPoints
        {
            public HashSet<VariableDefinition> Avail = new HashSet<VariableDefinition>();
            public HashSet<VariableDefinition> SVars = new HashSet<VariableDefinition>();
            public MethodDefinition MethodDefinition { get; set; }
            public List<Func<Instruction, Instruction>> OnNextStatement = new List<Func<Instruction, Instruction>>();
            public Stack<LoopDec> LoopDecs = new Stack<LoopDec>();
        }

        private class LoopDec
        {
            public List<Instruction> ContinueAfter = new List<Instruction>();
            public List<Instruction> BreakAfter = new List<Instruction>();
        }

        internal Generator(string nsp)
        {
            assembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(nsp, new Version(0, 1, 0)), nsp, ModuleKind.Console);
            module = assembly.MainModule;
            LoadAll();
            this.nsp = nsp;
        }

        private TypeReference Load(Type type)
        {
            TypeReference reference = null;
            if (!typeRegister.TryGetValue(type, out reference))
            {
                reference = module.Import(type);
                typeRegister.Add(type, reference);
            }
            return reference;
        }

        private MethodReference Load(r.MethodBase mi)
        {
            MethodReference ret = null;
            if (!methodRegister.TryGetValue(mi, out ret))
            {
                ret = module.Import(mi);
                methodRegister.Add(mi, ret);
            }
            return ret;
        }

        private void LoadAll()
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
            value_val = Load(typeof(TotemValue).GetProperty("ByTotemValue").GetGetMethod());

            function_run = Load(typeof(TotemFunction).GetMethod("TotemRun", r.BindingFlags.NonPublic | r.BindingFlags.Instance));
            execute = Load(typeof(TotemValue).GetMethod("Execute"));
            function_ctor = Load(typeof(TotemFunction).GetConstructor(r.BindingFlags.NonPublic | r.BindingFlags.Instance, null, new Type[] { typeof(TotemScope), typeof(string), typeof(TotemParameter[]) }, null));
            function_local_set = Load(typeof(TotemFunction).GetMethod("LocalSet", r.BindingFlags.NonPublic | r.BindingFlags.Instance));
            function_local_get = Load(typeof(TotemFunction).GetMethod("LocalGet", r.BindingFlags.NonPublic | r.BindingFlags.Instance));
            function_local_dec = Load(typeof(TotemFunction).GetMethod("LocalDec", r.BindingFlags.NonPublic | r.BindingFlags.Instance));
            function_env = Load(typeof(TotemFunction).GetProperty("Scope", r.BindingFlags.NonPublic | r.BindingFlags.Instance).GetGetMethod(true));

            arguments_ctor = Load(typeof(TotemArguments).GetConstructor(new Type[] { typeof(TotemValue) }));
            arguments_add = Load(typeof(TotemArguments).GetMethod("Add", r.BindingFlags.Public | r.BindingFlags.Instance | r.BindingFlags.DeclaredOnly));

            number_ctor_long = Load(typeof(TotemNumber).GetConstructor(new Type[] { typeof(long) }));

            string_ctor = Load(typeof(TotemString).GetConstructor(new Type[] { typeof(string) }));

            parameter_ctor = Load(typeof(TotemParameter).GetConstructor(new Type[] { typeof(string), typeof(TotemValue) }));

            undefined = Load(typeof(TotemValue).GetProperty("Undefined").GetGetMethod());
            @null = Load(typeof(TotemValue).GetProperty("Null").GetGetMethod());

            value_add = Load(typeof(TotemValue).GetMethod("op_Addition", r.BindingFlags.Static | r.BindingFlags.Public));
            value_sub = Load(typeof(TotemValue).GetMethod("op_Subtraction", r.BindingFlags.Static | r.BindingFlags.Public));
            value_mul = Load(typeof(TotemValue).GetMethod("op_Multiply", r.BindingFlags.Static | r.BindingFlags.Public));
            value_div = Load(typeof(TotemValue).GetMethod("op_Division", r.BindingFlags.Static | r.BindingFlags.Public));
            value_eq = Load(typeof(TotemValue).GetMethod("op_Equality", r.BindingFlags.Static | r.BindingFlags.Public));
            value_neq = Load(typeof(TotemValue).GetMethod("op_Inequality", r.BindingFlags.Static | r.BindingFlags.Public));
            value_lt = Load(typeof(TotemValue).GetMethod("op_LessThan", r.BindingFlags.Static | r.BindingFlags.Public));
            value_gt = Load(typeof(TotemValue).GetMethod("op_GreaterThan", r.BindingFlags.Static | r.BindingFlags.Public));
            value_lte = Load(typeof(TotemValue).GetMethod("op_LessThanOrEqual", r.BindingFlags.Static | r.BindingFlags.Public));
            value_gte = Load(typeof(TotemValue).GetMethod("op_GreaterThanOrEqual", r.BindingFlags.Static | r.BindingFlags.Public));
            value_incr = Load(typeof(TotemValue).GetMethod("op_Increment", r.BindingFlags.Static | r.BindingFlags.Public));
            value_istrue = Load(typeof(TotemValue).GetMethod("op_Explicit", r.BindingFlags.Static | r.BindingFlags.Public, null, r.CallingConventions.Standard, new Type[] { typeof(TotemValue) }, null));

            scope_ctor = Load(typeof(TotemFunction.ScopeWrapper).GetConstructor(new Type[] { typeof(TotemFunction) }));

            dispose = Load(typeof(IDisposable).GetMethod("Dispose"));

            get_prop = Load(typeof(TotemValue).GetMethod("GetProp"));

            bool_ctor = Load(typeof(TotemBool).GetConstructor(new Type[] { typeof(bool) }));
        }

        internal void GenerateProgram(ParseTreeNode rootNode)
        {
            if (rootNode.Term.Name != "Prog")
                throw new InvalidOperationException("Can't compile from the middle of a tree");

            TypeDefinition td = new TypeDefinition(nsp, "Program", TypeAttributes.Public, function);
            module.Types.Add(td);

            MethodDefinition md = new MethodDefinition("Main", MethodAttributes.Static | MethodAttributes.Public, Load(typeof(void)));
            td.Methods.Add(md);

            assembly.EntryPoint = md;

            var ctor = new MethodDefinition(".ctor", MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.Public, Load(typeof(void)));
            var ctorIl = ctor.Body.GetILProcessor();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, Load(typeof(TotemScope).GetProperty("Global").GetGetMethod()));
            ctorIl.Emit(OpCodes.Ldstr, "Main");
            ctorIl.Emit(OpCodes.Ldnull);
            ctorIl.Emit(OpCodes.Call, function_ctor);
            ctorIl.Emit(OpCodes.Ret);
            td.Methods.Add(ctor);

            var il = md.Body.GetILProcessor();
            il.Emit(OpCodes.Ldstr, "Click enter to start");
            il.Emit(OpCodes.Call, Load(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) })));
            il.Emit(OpCodes.Call, Load(typeof(Console).GetMethod("ReadLine", Type.EmptyTypes)));
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Callvirt, Load(typeof(TotemFunction).GetMethod("Execute")));
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Call, Load(typeof(Console).GetMethod("ReadLine", Type.EmptyTypes)));
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);

            md = new MethodDefinition("TotemRun", MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual, value);
            il = md.Body.GetILProcessor();

            functionPoints.Push(new FunctionPoints
            {
                Avail = new HashSet<VariableDefinition>(),
                SVars = new HashSet<VariableDefinition>(),
                MethodDefinition = md
            });

            var element_list = rootNode.ChildNodes[0];
            GenerateFunction(il, element_list.ChildNodes);

            var next = il.Create(OpCodes.Nop);
            il.Append(next);
            OnNextStatement(il.Body.Instructions[il.Body.Instructions.Count - 1]);

            il.Emit(OpCodes.Call, undefined);
            il.Emit(OpCodes.Ret);
            td.Methods.Add(md);
        }

        private void GenerateFunction(ILProcessor il, IEnumerable<ParseTreeNode> statement_nodes)
        {
            Instruction current = il.Create(OpCodes.Nop);
            il.Append(current);
            foreach (var node in statement_nodes)
            {
                GenerateStatement(il, ref current, node);
            }
        }

        private void GenerateFunction(ILProcessor il, ref Instruction start, ref Instruction current, string name, string mName, ParseTreeNodeList parameters, IEnumerable<ParseTreeNode> body)
        {
            var fn = new TypeDefinition(nsp, mName, TypeAttributes.Public | TypeAttributes.Sealed, function);
            var ctor = new MethodDefinition(".ctor", MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.Public, Load(typeof(void)));
            ctor.Parameters.Add(new ParameterDefinition("env", ParameterAttributes.In, Load(typeof(TotemScope))));
            ctor.Parameters.Add(new ParameterDefinition("name", ParameterAttributes.In, Load(typeof(string))));
            ctor.Parameters.Add(new ParameterDefinition("parameters", ParameterAttributes.In, arr_parameters));
            var ctorIl = ctor.Body.GetILProcessor();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldarg_1);
            ctorIl.Emit(OpCodes.Ldarg_2);
            ctorIl.Emit(OpCodes.Ldarg_3);
            ctorIl.Emit(OpCodes.Call, function_ctor);
            ctorIl.Emit(OpCodes.Ret);
            fn.Methods.Add(ctor);

            var fnc = new MethodDefinition("TotemRun", MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual, value);
            var fnil = fnc.Body.GetILProcessor();
            functionPoints.Push(new FunctionPoints
            {
                Avail = new HashSet<VariableDefinition>(),
                SVars = new HashSet<VariableDefinition>(),
                MethodDefinition = fnc
            });
            GenerateFunction(fnil, body);
            var next = fnil.Create(OpCodes.Nop);
            fnil.Append(next);
            OnNextStatement(next);
            fnil.Emit(OpCodes.Call, undefined);
            fnil.Emit(OpCodes.Ret);
            functionPoints.Pop();
            fn.Methods.Add(fnc);

            module.Types.Add(fn);

            var pn = GetSVar(arr_parameters);
            var prev = start;
            il.Add(ref prev, il.Create(OpCodes.Ldc_I4, parameters.Count));
            il.Add(ref prev, il.Create(OpCodes.Newarr, this.parameter));
            il.Add(ref prev, il.Create(OpCodes.Stloc, pn));
            for (var i = 0; i < parameters.Count; i++)
            {
                var param = parameters[i];
                il.Add(ref prev, il.Create(OpCodes.Ldloc, pn));
                il.Add(ref prev, il.Create(OpCodes.Ldc_I4, i));
                il.Add(ref prev, il.Create(OpCodes.Ldstr, param.ChildNodes[0].Token.ValueString));
                if (param.ChildNodes.Count > 1)
                {
                    GenerateExpression(il, ref start, ref prev, param.ChildNodes[1].ChildNodes[1]);
                }
                else
                {
                    il.Add(ref prev, il.Create(OpCodes.Call, undefined));
                }
                il.Add(ref prev, il.Create(OpCodes.Newobj, parameter_ctor));
                il.Add(ref prev, il.Create(OpCodes.Stelem_Ref));
            }
            start = prev;
            il.Add(ref current, il.Create(OpCodes.Ldarg_0));
            il.Add(ref current, il.Create(OpCodes.Callvirt, function_env));
            il.Add(ref current, il.Create(OpCodes.Ldstr, name));
            il.Add(ref current, il.Create(OpCodes.Ldloc, pn));
            il.Add(ref current, il.Create(OpCodes.Newobj, ctor));
        }

        private void GenerateStatement(ILProcessor il, ref Instruction current, ParseTreeNode node)
        {
            var start = current;
            il.Add(ref current, il.Create(OpCodes.Nop));
            current = OnNextStatement(current);
            int pn;
            switch (node.Term.Name)
            {
                case "VarStmt":
                    GenerateDeclaration(il, ref start, ref current, node);
                    break;
                case "FuncDefStmt":
                    var name = node.ChildNodes[1].Token.ValueString;
                    var mName = MakeFunctionName(name);
                    il.Add(ref current, il.Create(OpCodes.Ldarg_0));
                    il.Add(ref current, il.Create(OpCodes.Ldstr, name));
                    GenerateFunction(il, ref start, ref current, name, mName, node.ChildNodes[2].ChildNodes, node.ChildNodes[3].ChildNodes[0].ChildNodes);
                    il.Add(ref current, il.Create(OpCodes.Callvirt, function_local_dec));
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
                                il.Add(ref current, il.Create(OpCodes.Call, undefined));
                            }
                            il.Add(ref current, il.Create(OpCodes.Ret));
                            break;
                        case "continue":
                            ContinueAfter(current);
                            break;
                        case "break":
                            BreakAfter(current);
                            break;
                        default:
                            throw new InvalidOperationException("Unknown flow control keyword " + keyword);
                    }
                    break;
                case "ExprStmt":
                    GenerateExpression(il, ref start, ref current, node.ChildNodes[0]);
                    il.Add(ref current, il.Create(OpCodes.Pop));
                    break;
                case "IfElseStmt":
                    pn = GetSVar(sys_bool);
                    GenerateExpression(il, ref start, ref current, node.ChildNodes[1].ChildNodes[0]);
                    il.Add(ref current, il.Create(OpCodes.Call, value_istrue));
                    il.Add(ref current, il.Create(OpCodes.Ldc_I4_0));
                    il.Add(ref current, il.Create(OpCodes.Ceq));
                    il.Add(ref current, il.Create(OpCodes.Stloc, pn));
                    il.Add(ref current, il.Create(OpCodes.Ldloc, pn));

                    Instruction insBreakToFalseAfter = current, insBreakToAfterFalseAfter;

                    if (node.ChildNodes.Count == 5)
                    {
                        GenerateStatement(il, ref current, node.ChildNodes[2]);
                        insBreakToAfterFalseAfter = current;
                        il.Add(ref current, il.Create(OpCodes.Nop));
                        il.Add(ref insBreakToFalseAfter, il.Create(OpCodes.Brtrue, current));
                        GenerateStatement(il, ref current, node.ChildNodes[4]);
                        il.Add(ref current, il.Create(OpCodes.Nop));

                        OnNextStatement(next =>
                        {
                            il.Add(ref insBreakToAfterFalseAfter, il.Create(OpCodes.Br, next));
                            return next;
                        });
                    }
                    else
                    {
                        GenerateStatement(il, ref current, node.ChildNodes[2]);
                        il.Add(ref current, il.Create(OpCodes.Nop));
                        il.Add(ref insBreakToFalseAfter, il.Create(OpCodes.Brtrue, current));
                    }

                    break;
                case "ForStmt":
                    AddLoopDec();
                    // Push new scope
                    pn = GetVar(scope);
                    il.Add(ref start, il.Create(OpCodes.Ldarg_0));
                    il.Add(ref start, il.Create(OpCodes.Newobj, scope_ctor));
                    il.Add(ref start, il.Create(OpCodes.Stloc, pn));

                    // Initializer
                    var initializer = node.ChildNodes[1];
                    if (initializer.ChildNodes.Count != 0)
                    {
                        initializer = initializer.ChildNodes[0];
                        if (initializer.ChildNodes.Count == 1)
                        {
                            GenerateExpression(il, ref start, ref current, initializer.ChildNodes[0]);
                            il.Add(ref current, il.Create(OpCodes.Pop));
                        }
                        else
                        {
                            foreach (var dec in initializer.ChildNodes[1].ChildNodes)
                                GenerateDeclaration(il, ref start, ref current, dec);
                        }
                    }
                    var jumpToConditionAfter = current;

                    // Body
                    GenerateStatement(il, ref current, node.ChildNodes[4]);
                    il.Add(ref current, il.Create(OpCodes.Nop));

                    // Increment
                    Instruction incr = il.Add(ref current, il.Create(OpCodes.Nop));
                    var increment = node.ChildNodes[3];
                    if (increment.ChildNodes.Count != 0)
                    {
                        GenerateExpression(il, ref start, ref current, increment.ChildNodes[0]);
                    }

                    // Condition
                    Instruction cond = il.Add(ref current, il.Create(OpCodes.Nop));
                    il.Add(ref jumpToConditionAfter, il.Create(OpCodes.Br, cond));
                    var condition = node.ChildNodes[2];
                    if (condition.ChildNodes.Count == 0)
                    {
                        il.Add(ref current, il.Create(OpCodes.Ldc_I4_1));
                        il.Add(ref current, il.Create(OpCodes.Call, bool_ctor)); // Create TotemBool true
                    }
                    else
                    {
                        GenerateExpression(il, ref start, ref current, condition.ChildNodes[0]);
                    }
                    il.Add(ref current, il.Create(OpCodes.Call, value_istrue));
                    //ContinueAfter(current);
                    il.Add(ref current, il.Create(OpCodes.Brtrue, jumpToConditionAfter.Next));

                    // Pop scope
                    il.Add(ref current, il.Create(OpCodes.Ldloc, pn));
                    RemLoopDec(il, incr, current);
                    il.Add(ref current, il.Create(OpCodes.Callvirt, dispose));
                    RelVar(pn);
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
        }

        private void GenerateDeclaration(ILProcessor il, ref Instruction start, ref Instruction current, ParseTreeNode node)
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
                    il.Add(ref current, il.Create(OpCodes.Ldarg_0));
                    il.Add(ref current, il.Create(OpCodes.Ldstr, name));
                    if (node.ChildNodes.Count > 1)
                    {
                        GenerateExpression(il, ref start, ref current, node.ChildNodes[1].ChildNodes[1]);
                    }
                    else
                    {
                        il.Add(ref current, il.Create(OpCodes.Call, undefined));
                    }
                    il.Add(ref current, il.Create(OpCodes.Callvirt, function_local_dec));
                    break;
                default:
                    throw new InvalidOperationException("Term " + node.Term.Name + " is not a declaration statement");
            }
        }

        private void GenerateExpression(ILProcessor il, ref Instruction start, ref Instruction current, ParseTreeNode node)
        {
            string name;
            int pn;
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
                            il.Add(ref current, il.Create(OpCodes.Call, value_add));
                            break;
                        case "-":
                            il.Add(ref current, il.Create(OpCodes.Call, value_sub));
                            break;
                        case "<":
                            il.Add(ref current, il.Create(OpCodes.Call, value_lt));
                            break;
                        case ">":
                            il.Add(ref current, il.Create(OpCodes.Call, value_gt));
                            break;
                        case "==":
                            il.Add(ref current, il.Create(OpCodes.Call, value_eq));
                            break;
                        case "!=":
                            il.Add(ref current, il.Create(OpCodes.Call, value_neq));
                            break;
                        case "*":
                            il.Add(ref current, il.Create(OpCodes.Call, value_mul));
                            break;
                        case "/":
                            il.Add(ref current, il.Create(OpCodes.Call, value_div));
                            break;
                        case "<=":
                            il.Add(ref current, il.Create(OpCodes.Call, value_lte));
                            break;
                        case ">=":
                            il.Add(ref current, il.Create(OpCodes.Call, value_gte));
                            break;
                        default:
                            throw new InvalidOperationException("Unknown bin expression key symbol " + keySymbol);
                    }
                    break;
                case "FunctionCallExpr":
                    pn = GetSVar(arguments);
                    var mbr = -1;
                    if (node.ChildNodes[0].Term.Name == "MemberExpr")
                    {
                        mbr = GetSVar(value);
                        GenerateExpression(il, ref start, ref prev, node.ChildNodes[0].ChildNodes[0]);
                        il.Add(ref prev, il.Create(OpCodes.Dup));
                        il.Add(ref prev, il.Create(OpCodes.Stloc, mbr));
                    }
                    else
                    {
                        il.Add(ref prev, il.Create(OpCodes.Ldnull));
                    }
                    il.Add(ref prev, il.Create(OpCodes.Newobj, arguments_ctor));
                    il.Add(ref prev, il.Create(OpCodes.Stloc, pn));
                    foreach (var arg in node.ChildNodes[1].ChildNodes)
                    {
                        il.Add(ref prev, il.Create(OpCodes.Ldloc, pn));
                        if (arg.ChildNodes.Count == 1)
                        {
                            il.Add(ref prev, il.Create(OpCodes.Ldnull));
                        }
                        else
                        {
                            il.Add(ref prev, il.Create(OpCodes.Ldstr, arg.ChildNodes[0].Token.ValueString));
                        }
                        GenerateExpression(il, ref start, ref prev, arg.ChildNodes[arg.ChildNodes.Count - 1]);
                        il.Add(ref prev, il.Create(OpCodes.Callvirt, arguments_add));
                    }
                    start = prev;
                    if (mbr == -1)
                    {
                        GenerateExpression(il, ref start, ref current, node.ChildNodes[0]);
                    }
                    else
                    {
                        il.Add(ref current, il.Create(OpCodes.Ldloc, mbr));
                        il.Add(ref current, il.Create(OpCodes.Ldstr, node.ChildNodes[0].ChildNodes[2].Token.ValueString));
                        il.Add(ref current, il.Create(OpCodes.Callvirt, get_prop));
                    }
                    il.Add(ref current, il.Create(OpCodes.Ldloc, pn));
                    il.Add(ref current, il.Create(OpCodes.Callvirt, execute));
                    break;
                case "AssignExpr":
                    switch (node.ChildNodes[0].Term.Name)
                    {
                        case "identifier":
                            pn = GetSVar(value);
                            if (node.ChildNodes.Count == 3)
                            {
                                GenerateExpression(il, ref start, ref start, node.ChildNodes[2]);
                                il.Add(ref start, il.Create(OpCodes.Stloc, pn));
                                il.Add(ref current, il.Create(OpCodes.Ldarg_0));
                                il.Add(ref current, il.Create(OpCodes.Ldstr, node.ChildNodes[0].Token.ValueString));
                                il.Add(ref current, il.Create(OpCodes.Ldloc, pn));
                                il.Add(ref current, il.Create(OpCodes.Callvirt, function_local_set));
                                il.Add(ref current, il.Create(OpCodes.Ldloc, pn));
                            }
                            else
                            {
                                var pn2 = GetSVar(value);
                                il.Add(ref current, il.Create(OpCodes.Ldarg_0));
                                il.Add(ref current, il.Create(OpCodes.Ldstr, node.ChildNodes[0].Token.ValueString));
                                il.Add(ref current, il.Create(OpCodes.Callvirt, function_local_get));
                                il.Add(ref current, il.Create(OpCodes.Dup));
                                il.Add(ref current, il.Create(OpCodes.Stloc, pn));
                                // Increment and restore
                                il.Add(ref current, il.Create(OpCodes.Call, value_incr));
                                il.Add(ref current, il.Create(OpCodes.Stloc, pn2));
                                il.Add(ref current, il.Create(OpCodes.Ldarg_0));
                                il.Add(ref current, il.Create(OpCodes.Ldstr, node.ChildNodes[0].Token.ValueString));
                                il.Add(ref current, il.Create(OpCodes.Ldloc, pn2));
                                il.Add(ref current, il.Create(OpCodes.Callvirt, function_local_set));
                                il.Add(ref current, il.Create(OpCodes.Ldloc, pn));
                            }
                            break;
                        default:
                            throw new InvalidOperationException("Invalid QualifiedName term " + node.ChildNodes[0].Term.Name);
                    }
                    break;
                case "identifier":
                    name = node.Token.ValueString;
                    il.Add(ref current, il.Create(OpCodes.Ldarg_0));
                    il.Add(ref current, il.Create(OpCodes.Ldstr, name));
                    il.Add(ref current, il.Create(OpCodes.Callvirt, function_local_get));
                    break;
                case "FuncDefExpr":
                    if (node.ChildNodes[1].ChildNodes.Count == 1)
                        name = node.ChildNodes[1].ChildNodes[0].Token.ValueString;
                    else
                        name = "anononymous";
                    var mName = MakeFunctionName(name);
                    GenerateFunction(il, ref start, ref current, name, mName, node.ChildNodes[2].ChildNodes, node.ChildNodes[3].ChildNodes[0].ChildNodes);
                    break;
                case "MemberExpr":
                    GenerateExpression(il, ref start, ref current, node.ChildNodes[0]);
                    il.Add(ref current, il.Create(OpCodes.Ldstr, node.ChildNodes[2].Token.ValueString));
                    il.Add(ref current, il.Create(OpCodes.Callvirt, get_prop));
                    break;
                default:
                    throw new InvalidOperationException("Term " + node.Term.Name + " is not a valid expression term");
            }
        }

        private void GenerateConst(ILProcessor il, Instruction start, ref Instruction current, ParseTreeNode node)
        {
            var obj = node.Token.Value;
            if (node.Token.Terminal is KeyTerm)
            {
                switch ((string)obj)
                {
                    case "true":
                        il.Add(ref current, il.Create(OpCodes.Ldc_I4_1));
                        il.Add(ref current, il.Create(OpCodes.Newobj, bool_ctor));
                        break;
                    case "false":
                        il.Add(ref current, il.Create(OpCodes.Ldc_I4_0));
                        il.Add(ref current, il.Create(OpCodes.Newobj, bool_ctor));
                        break;
                    case "null":
                        il.Add(ref current, il.Create(OpCodes.Call, @null));
                        break;
                    case "undefined":
                        il.Add(ref current, il.Create(OpCodes.Call, undefined));
                        break;
                    default:
                        throw new InvalidOperationException("Unknown keyword " + obj);
                }
            }
            else if (obj is string)
            {
                il.Add(ref current, il.Create(OpCodes.Ldstr, (string)obj));
                il.Add(ref current, il.Create(OpCodes.Newobj, string_ctor));
            }
            else if (obj is int || obj is long)
            {
                il.Add(ref current, il.Create(OpCodes.Ldc_I8, Convert.ToInt64(obj)));
                il.Add(ref current, il.Create(OpCodes.Newobj, number_ctor_long));
            }
            else
                throw new InvalidOperationException("Unknown constant type " + obj.GetType());
        }

        private Instruction OnNextStatement(Instruction current)
        {
            functionPoints.Peek().OnNextStatement.ForEach(ons => current = ons(current));
            functionPoints.Peek().OnNextStatement.Clear();
            return current;
        }

        private void OnNextStatement(Func<Instruction, Instruction> func)
        {
            functionPoints.Peek().OnNextStatement.Add(func);
        }

        private int GetVar(TypeReference type)
        {
            var p = functionPoints.Peek();
            int pn = -1;
            foreach (var param in p.Avail)
            {
                if (param.VariableType == type)
                {
                    p.Avail.Remove(param);
                    pn = p.MethodDefinition.Body.Variables.IndexOf(param);
                    break;
                }
            }
            if (pn == -1)
            {
                p.MethodDefinition.Body.Variables.Add(new VariableDefinition(type));
                pn = p.MethodDefinition.Body.Variables.Count - 1;
            }

            return pn;
        }

        private void RelVar(int pn)
        {
            var p = functionPoints.Peek();
            p.Avail.Add(p.MethodDefinition.Body.Variables[pn]);
        }

        private int GetSVar(TypeReference type)
        {
            var p = functionPoints.Peek();
            var pn = GetVar(type);
            p.SVars.Add(p.MethodDefinition.Body.Variables[pn]);
            return pn;
        }

        private void RelSVars()
        {
            var p = functionPoints.Peek();
            foreach (var sv in p.SVars)
                p.Avail.Add(sv);
        }

        private void AddLoopDec()
        {
            var p = functionPoints.Peek();
            p.LoopDecs.Push(new LoopDec());
        }

        private void ContinueAfter(Instruction instruction)
        {
            functionPoints.Peek().LoopDecs.Peek().ContinueAfter.Add(instruction);
        }

        private void BreakAfter(Instruction instruction)
        {
            functionPoints.Peek().LoopDecs.Peek().BreakAfter.Add(instruction);
        }

        private void RemLoopDec(ILProcessor il, Instruction continuePoint, Instruction breakPoint)
        {
            var ld = functionPoints.Peek().LoopDecs.Pop();
            foreach (var c in ld.ContinueAfter)
                il.InsertAfter(c, il.Create(OpCodes.Br, continuePoint));
            foreach (var c in ld.BreakAfter)
                il.InsertAfter(c, il.Create(OpCodes.Br, breakPoint));
        }

        private string MakeFunctionName(string name)
        {
            var mName = name[0].ToString().ToUpper() + name.Substring(1);
            int i = 0;
            while (fn_names.Contains(mName))
            {
                mName = name[0].ToString().ToUpper() + name.Substring(1) + "__" + (++i);
            }
            return mName;
        }

        internal void Save(string file)
        {
            assembly.Write(file);
        }
    }

    static class IlExtensions
    {
        public static Instruction Add(this ILProcessor il, ref Instruction prev, Instruction insert)
        {
            il.InsertAfter(prev, insert);
            prev = insert;
            return prev;
        }
    }
}

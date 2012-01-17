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
        AssemblyDefinition assembly;
        ModuleDefinition module;
        string nsp;

        TypeReference function;
        TypeReference value;
        TypeReference tstring;
        TypeReference arguments;

        TypeReference arr_parameters;

        MethodReference value_val;

        MethodReference undefined;
        MethodReference null_totem;
        MethodReference function_run;
        MethodReference function_execute;
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

        int fn_count = 0;
        HashSet<string> fn_names = new HashSet<string>();
        Stack<ParameterCount> paramCount = new Stack<ParameterCount>();

        private struct ParameterCount
        {
            public HashSet<VariableDefinition> Avail { get; set; }
            public MethodDefinition MethodDefinition { get; set; }
        }

        internal Generator(string nsp)
        {
            assembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(nsp, new Version(0, 1, 0)), nsp, ModuleKind.Console);
            module = assembly.MainModule;
            LoadAll();
            this.nsp = nsp;
        }

        private void LoadAll()
        {
            // Types
            function = module.Import(typeof(TotemFunction));
            value = module.Import(typeof(TotemValue));
            tstring = module.Import(typeof(TotemString));
            arguments = module.Import(typeof(TotemArguments));

            arr_parameters = module.Import(typeof(TotemParameter[]));

            // Methods
            value_val = module.Import(typeof(TotemValue).GetProperty("ByTotemValue").GetGetMethod());

            function_run = module.Import(typeof(TotemFunction).GetMethod("TotemRun", r.BindingFlags.NonPublic | r.BindingFlags.Instance));
            function_execute = module.Import(typeof(TotemFunction).GetMethod("Execute"));
            function_ctor = module.Import(typeof(TotemFunction).GetConstructor(r.BindingFlags.NonPublic | r.BindingFlags.Instance, null, new Type[] { typeof(TotemEnvironment), typeof(string), typeof(TotemParameter[]) }, null));
            function_local_set = module.Import(typeof(TotemFunction).GetMethod("LocalSet", r.BindingFlags.NonPublic | r.BindingFlags.Instance));
            function_local_get = module.Import(typeof(TotemFunction).GetMethod("LocalGet", r.BindingFlags.NonPublic | r.BindingFlags.Instance));
            function_local_dec = module.Import(typeof(TotemFunction).GetMethod("LocalDec", r.BindingFlags.NonPublic | r.BindingFlags.Instance));
            function_env = module.Import(typeof(TotemFunction).GetProperty("Environment", r.BindingFlags.NonPublic | r.BindingFlags.Instance).GetGetMethod(true));

            arguments_ctor = module.Import(typeof(TotemArguments).GetConstructor(Type.EmptyTypes));
            arguments_add = module.Import(typeof(TotemArguments).GetMethod("Add"));

            number_ctor_long = module.Import(typeof(TotemNumber).GetConstructor(new Type[] { typeof(long) }));

            string_ctor = module.Import(typeof(TotemString).GetConstructor(new Type[] { typeof(string) }));

            parameter_ctor = module.Import(typeof(TotemParameter).GetConstructor(new Type[] { typeof(string), typeof(TotemValue) }));

            undefined = module.Import(typeof(TotemValue).GetProperty("Undefined").GetGetMethod());
            null_totem = module.Import(typeof(TotemValue).GetProperty("Null").GetGetMethod());

            value_add = module.Import(typeof(TotemValue).GetMethod("Add"));
            value_sub = module.Import(typeof(TotemValue).GetMethod("Subtract"));
        }

        internal void GenerateProgram(ParseTreeNode rootNode)
        {
            if (rootNode.Term.Name != "program")
                throw new InvalidOperationException("Can't compile from the middle of a tree");

            TypeDefinition td = new TypeDefinition(nsp, "Program", TypeAttributes.Public, function);
            module.Types.Add(td);

            MethodDefinition md = new MethodDefinition("Main", MethodAttributes.Static | MethodAttributes.Public, module.Import(typeof(void)));
            td.Methods.Add(md);

            assembly.EntryPoint = md;

            var ctor = new MethodDefinition(".ctor", MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.Public, module.Import(typeof(void)));
            var ctorIl = ctor.Body.GetILProcessor();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldnull);
            ctorIl.Emit(OpCodes.Ldstr, "Main");
            ctorIl.Emit(OpCodes.Ldnull);
            ctorIl.Emit(OpCodes.Call, function_ctor);
            ctorIl.Emit(OpCodes.Ret);
            td.Methods.Add(ctor);

            var il = md.Body.GetILProcessor();
            il.Emit(OpCodes.Newobj, module.Import(ctor));
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Callvirt, module.Import(typeof(TotemFunction).GetMethod("Execute")));
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);

            md = new MethodDefinition("TotemRun", MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual, value);
            il = md.Body.GetILProcessor();

            paramCount.Push(new ParameterCount
            {
                Avail = new HashSet<VariableDefinition>(),
                MethodDefinition = md
            });

            var statement_list_opt = rootNode.ChildNodes[0];
            var statement_list = statement_list_opt.ChildNodes[0];
            GenerateFunction(il, statement_list.ChildNodes);

            il.Emit(OpCodes.Call, undefined);
            il.Emit(OpCodes.Ret);
            td.Methods.Add(md);
        }

        private void GenerateFunction(ILProcessor il, IEnumerable<ParseTreeNode> statement_nodes)
        {
            foreach (var node in statement_nodes)
            {
                GenerateStatement(il, node);
            }
        }

        private void GenerateFunction(ILProcessor il, string cilName, string name, ParseTreeNode parameter_list_opt, ParseTreeNode block, int loc)
        {
            var pn = GetVar(arr_parameters);

            if (parameter_list_opt.ChildNodes.Count == 0)
            {
                il.Emit(OpCodes.Ldc_I4, 0);
                il.Emit(OpCodes.Newarr, module.Import(typeof(TotemParameter)));
                il.Emit(OpCodes.Stloc, pn);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, parameter_list_opt.ChildNodes[0].ChildNodes.Count);
                il.Emit(OpCodes.Newarr, module.Import(typeof(TotemParameter)));
                il.Emit(OpCodes.Stloc, pn);
                for (int i = 0; i < parameter_list_opt.ChildNodes[0].ChildNodes.Count; i++)
                {
                    il.Emit(OpCodes.Ldloc, pn);
                    il.Emit(OpCodes.Ldc_I4, i);
                    var pNode = parameter_list_opt.ChildNodes[0].ChildNodes[i];
                    il.Emit(OpCodes.Ldstr, pNode.ChildNodes[0].Token.ValueString); // parameter name
                    if (pNode.ChildNodes.Count == 1)
                    {
                        il.Emit(OpCodes.Call, undefined);
                    }
                    else
                    {
                        GenerateExpression(il, pNode.ChildNodes[2]);
                    }
                    il.Emit(OpCodes.Newobj, parameter_ctor);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }

            TypeDefinition td = new TypeDefinition(nsp, cilName, TypeAttributes.Public, function);
            var ctor = new MethodDefinition(".ctor", MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.Public, module.Import(typeof(void)));
            ctor.Parameters.Add(new ParameterDefinition("env", ParameterAttributes.In, module.Import(typeof(TotemEnvironment))));
            ctor.Parameters.Add(new ParameterDefinition("name", ParameterAttributes.In, module.Import(typeof(string))));
            ctor.Parameters.Add(new ParameterDefinition("parameters", ParameterAttributes.In, module.Import(typeof(TotemParameter[]))));
            var ctorIl = ctor.Body.GetILProcessor();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldarg_1);
            ctorIl.Emit(OpCodes.Ldarg_2);
            ctorIl.Emit(OpCodes.Ldarg_3);
            ctorIl.Emit(OpCodes.Call, function_ctor);
            ctorIl.Emit(OpCodes.Ret);
            td.Methods.Add(ctor);

            var md = new MethodDefinition("TotemRun", MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual, value);
            var nIl = md.Body.GetILProcessor();

            paramCount.Push(new ParameterCount
            {
                Avail = new HashSet<VariableDefinition>(),
                MethodDefinition = md
            });
            var statement_list_opt = block.ChildNodes[0];
            var statement_list = statement_list_opt.ChildNodes[0];
            GenerateFunction(nIl, statement_list.ChildNodes);
            nIl.Emit(OpCodes.Call, undefined);
            nIl.Emit(OpCodes.Ret);
            td.Methods.Add(md);
            paramCount.Pop();

            module.Types.Add(td);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, function_env);
            il.Emit(OpCodes.Ldstr, name);
            il.Emit(OpCodes.Ldloc, pn);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Stloc, loc);

            RelVar(pn);
        }

        private void GenerateStatement(ILProcessor il, ParseTreeNode node)
        {
            switch (node.Term.Name)
            {
                case "declaration_statement":
                    GenerateDeclaration(il, node.ChildNodes[0]);
                    break;
                case "statement_expression":
                    GenerateMemberWriteLoad(il, node.ChildNodes[0].ChildNodes[0]);
                    if (node.ChildNodes[1].ChildNodes[0].Token.ValueString != "=")
                    {
                        var bin_op = node.ChildNodes[1].ChildNodes[0].Token.ValueString[0].ToString();
                        GenerateBinOpExpression(il, bin_op, node.ChildNodes[0], node.ChildNodes[2]);
                    }
                    else
                    {
                        GenerateExpression(il, node.ChildNodes[2]);
                    }
                    GenerateMemberWriteCall(il, node.ChildNodes[0].ChildNodes[0]);
                    break;
                case "return_expression":
                    GenerateExpression(il, node.ChildNodes[1]);
                    il.Emit(OpCodes.Ret);
                    break;
                default:
                    throw new InvalidOperationException("Term " + node.Term.Name + " is not a statement");
            }
        }

        private void GenerateDeclaration(ILProcessor il, ParseTreeNode node)
        {
            string name;
            switch (node.Term.Name)
            {
                case "local_variable_declaration":
                    var declarators = node.ChildNodes[1].ChildNodes;
                    foreach (var dec in declarators)
                    {
                        name = dec.ChildNodes[0].Token.ValueString;
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldstr, name);
                        if (dec.ChildNodes.Count > 1)
                        {
                            GenerateExpression(il, dec.ChildNodes[2].ChildNodes[0]);
                        }
                        else
                        {
                            il.Emit(OpCodes.Call, undefined);
                        }
                        il.Emit(OpCodes.Callvirt, function_local_dec);
                    }
                    break;
                case "local_function_declaration":
                    name = node.ChildNodes[1].Token.ValueString;
                    var cilName = name + "Function";
                    while (fn_names.Contains(cilName))
                    {
                        cilName += "__" + (++fn_count);
                    }
                    fn_names.Add(cilName);

                    var pn = GetVar(function);

                    GenerateFunction(il, cilName, name, node.ChildNodes[2], node.ChildNodes[3], pn);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, name);
                    il.Emit(OpCodes.Ldloc, pn);
                    il.Emit(OpCodes.Callvirt, function_local_dec);

                    RelVar(pn);
                    break;
                default:
                    throw new InvalidOperationException("Term " + node.Term.Name + " is not a declaration_statement");
            }
        }

        private void GenerateExpression(ILProcessor il, ParseTreeNode node)
        {
            switch (node.Term.Name)
            {
                case "primary_expression":
                    GeneratePrimaryExpression(il, node.ChildNodes[0]);
                    break;
                case "bin_op_expression":
                    GenerateBinOpExpression(il, node.ChildNodes[1].Token.ValueString, node.ChildNodes[0], node.ChildNodes[2]);
                    break;
                case "member_access":
                    GenerateMemberRead(il, node);
                    break;
                default:
                    throw new InvalidOperationException("Term " + node.Term.Name + " is not an expression");
            }
        }

        private void GeneratePrimaryExpression(ILProcessor il, ParseTreeNode node)
        {
            object value;
            switch (node.Term.Name)
            {
                case "number":
                    value = node.Token.Value;
                    if (value is int || value is long)
                    {
                        il.Emit(OpCodes.Ldc_I8, Convert.ToInt64(value));
                        il.Emit(OpCodes.Newobj, number_ctor_long);
                    }
                    else
                        throw new InvalidOperationException("Unknown number type");
                    break;
                case "string":
                    string sVal = node.Token.ValueString;
                    il.Emit(OpCodes.Ldstr, sVal);
                    il.Emit(OpCodes.Newobj, string_ctor);
                    break;
                case "null":
                    il.Emit(OpCodes.Call, null_totem);
                    break;
                case "undefined":
                    il.Emit(OpCodes.Call, undefined);
                    break;
                case "member_access":
                    GenerateMemberRead(il, node);
                    break;
                default:
                    throw new InvalidOperationException("Term " + node.Term.Name + " is not a primary expression");
            }
        }

        private void GenerateBinOpExpression(ILProcessor il, string op, ParseTreeNode left, ParseTreeNode right)
        {
            GenerateExpression(il, left);
            GenerateExpression(il, right);
            switch (op)
            {
                case "+":
                    il.Emit(OpCodes.Call, value_add);
                    break;
                case "-":
                    il.Emit(OpCodes.Call, value_sub);
                    break;
                default:
                    throw new InvalidOperationException("Unknown binary operation " + op);
            }
        }

        private void GenerateMemberRead(ILProcessor il, ParseTreeNode node)
        {
            string identifier = node.ChildNodes[0].Token.ValueString;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, identifier);
            il.Emit(OpCodes.Callvirt, function_local_get);
            if (node.ChildNodes[1].ChildNodes.Count == 0)
            {
                il.Emit(OpCodes.Callvirt, value_val);
            }
            else
            {
                foreach (var segment in node.ChildNodes[1].ChildNodes[0].ChildNodes)
                {
                    var n = segment.ChildNodes[0];
                    switch (n.Term.Name)
                    {
                        case "argument_list_par":
                            int pn = GetVar(arguments);
                            il.Emit(OpCodes.Newobj, arguments_ctor);
                            il.Emit(OpCodes.Stloc, pn);
                            if (n.ChildNodes[0].ChildNodes.Count > 0)
                            {
                                foreach (var p in n.ChildNodes[0].ChildNodes[0].ChildNodes)
                                {
                                    il.Emit(OpCodes.Ldloc, pn);
                                    if (p.ChildNodes.Count == 1)
                                        il.Emit(OpCodes.Ldnull);
                                    else
                                        il.Emit(OpCodes.Ldstr, p.ChildNodes[0].Token.ValueString);
                                    GenerateExpression(il, p.ChildNodes[p.ChildNodes.Count - 1]);
                                    il.Emit(OpCodes.Callvirt, arguments_add);
                                }
                            }
                            RelVar(pn);
                            il.Emit(OpCodes.Ldloc, pn);
                            il.Emit(OpCodes.Callvirt, function_execute);
                            break;
                        default:
                            throw new InvalidOperationException("Unknown member access segment " + n.Term.Name);
                    }
                }
            }
        }

        private void GenerateMemberWriteLoad(ILProcessor il, ParseTreeNode node)
        {
            switch (node.Term.Name)
            {
                case "identifier":
                    string identifier = node.Token.ValueString;
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, identifier);
                    break;
                default:
                    throw new InvalidOperationException("Unknown member access " + node.Term.Name);
            }
        }

        private void GenerateMemberWriteCall(ILProcessor il, ParseTreeNode node)
        {
            switch (node.Term.Name)
            {
                case "identifier":
                    string identifier = node.Token.ValueString;
                    il.Emit(OpCodes.Callvirt, function_local_set);
                    break;
                default:
                    throw new InvalidOperationException("Unknown member access " + node.Term.Name);
            }
        }

        private int GetVar(TypeReference type)
        {
            var p = paramCount.Peek();
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
            var p = paramCount.Peek();
            p.Avail.Add(p.MethodDefinition.Body.Variables[pn]);
        }

        internal void Save(string file)
        {
            assembly.Write(file);
        }
    }
}

using System;
using System.Collections.ObjectModel;
using IKVM.Reflection.Emit;
using IKCtor = IKVM.Reflection.ConstructorInfo;
using IKMethod = IKVM.Reflection.MethodInfo;
using IKType = IKVM.Reflection.Type;

namespace Totem.Compiler
{
    class IlProcessor : IDisposable
    {
        MethodBuilder builder;
        Collection<Instruction> instructions;
        ILGenerator il;

        public IlProcessor(MethodBuilder builder)
        {
            this.builder = builder;
            this.instructions = new Collection<Instruction>();
        }

        public void Emit(OpCode opcode)
        {
            Append(Instruction.Create(opcode));
        }

        public void Emit(OpCode opcode, string str)
        {
            Append(Instruction.Create(opcode, str));
        }

        public void Emit(OpCode opcode, IKMethod met)
        {
            Append(Instruction.Create(opcode, met));
        }

        public void Emit(OpCode opcode, IKCtor ctor)
        {
            Append(Instruction.Create(opcode, ctor));
        }

        public void Emit(OpCode opcode, IKType type)
        {
            Append(Instruction.Create(opcode, type));
        }

        public void Append(Instruction instruction)
        {
            instructions.Add(instruction);
        }

        public void InsertAfter(Instruction prev, Instruction instr)
        {
            instructions.Insert(instructions.IndexOf(prev) + 1, instr);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Generate();
        }

        #endregion

        void Generate()
        {
            if (il == null)
                il = builder.GetILGenerator();
            foreach (var i in instructions)
                i.Emit(il);
        }

        public LocalBuilder AddLocal(IKType type)
        {
            if (il == null)
                il = builder.GetILGenerator();

            return il.DeclareLocal(type);
        }

        public Label DefineLabel()
        {
            if (il == null)
                il = builder.GetILGenerator();

            return il.DefineLabel();
        }
    }

    static class ILProcessorExtensions
    {
        public static IlProcessor GetILProcessor(this MethodBuilder mb)
        {
            return new IlProcessor(mb);
        }
    }

    enum InstructionType
    {
        OpCode,
        Special
    }

    enum Specials
    {
        Label,
        BeginTry,
        BeginFinally,
        EndTry,
        BeginTotemScope,
        EndTotemScope
    }

    class Instruction
    {
        OpCode opCode;
        InstructionType type;
        Specials special;
        object operand;
        SequencePoint sequencePoint;
        Type operandType;

        public void Emit(ILGenerator gen)
        {
            switch (type)
            {
                case InstructionType.OpCode:
                    EmitOpCode(gen);
                    break;
                case InstructionType.Special:
                    EmitSpecial(gen);
                    break;
                default:
                    throw new InvalidOperationException("Invalid type");
            }
        }

        private struct Empty { }

        void TypeSwitch(params object[] typeSwitch)
        {
            foreach (var t in typeSwitch)
            {
                if (t.GetType().GetGenericTypeDefinition() == typeof(Action<>))
                {
                    var type = t.GetType().GetGenericArguments()[0];
                    if (type == operandType)
                    {
                        dynamic tt = t;
                        dynamic d = operand;
                        tt(d);
                        return;
                    }
                }
            }

            throw new InvalidOperationException("default:");
        }

        void EmitOpCode(ILGenerator gen)
        {
            TypeSwitch(
                new Action<Empty>(v => gen.Emit(opCode)),
                new Action<IKMethod>(m => gen.Emit(opCode, m)),
                new Action<IKCtor>(ctor => gen.Emit(opCode, ctor)),
                new Action<IKType>(type => gen.Emit(opCode, type)),
                new Action<string>(str => gen.Emit(opCode, str)),
                new Action<long>(num => gen.Emit(opCode, num)),
                new Action<LocalBuilder>(lvar => gen.Emit(opCode, lvar)),
                new Action<Label>(label => gen.Emit(opCode, label))
            );
        }

        void EmitSpecial(ILGenerator gen)
        {
            switch (special)
            {
                case Specials.Label:
                    gen.MarkLabel((Label)operand);
                    break;
                case Specials.BeginTry:
                    gen.BeginExceptionBlock();
                    break;
                case Specials.BeginFinally:
                    gen.BeginFinallyBlock();
                    break;
                case Specials.EndTry:
                    gen.EndExceptionBlock();
                    break;
                case Specials.BeginTotemScope:
                    gen.Emit(OpCodes.Nop);
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Newobj, Generator.scope_ctor);
                    gen.Emit(OpCodes.Stloc, (LocalBuilder)operand);
                    gen.BeginExceptionBlock();
                    break;
                case Specials.EndTotemScope:
                    var endFinallyLabel = gen.DefineLabel();
                    gen.BeginFinallyBlock();
                    gen.Emit(OpCodes.Ldloc, (LocalBuilder)operand);
                    gen.Emit(OpCodes.Ldnull);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, endFinallyLabel);

                    gen.Emit(OpCodes.Ldloc, (LocalBuilder)operand);
                    gen.Emit(OpCodes.Callvirt, Generator.dispose);
                    gen.Emit(OpCodes.Nop);

                    gen.MarkLabel(endFinallyLabel);
                    gen.EndExceptionBlock();
                    break;
                default:
                    throw new InvalidOperationException("default:special");
            }
        }

        public static Instruction Create(OpCode opcode)
        {
            Instruction i = new Instruction();
            i.opCode = opcode;
            i.type = InstructionType.OpCode;
            i.operandType = typeof(Empty);
            i.operand = new Empty();
            return i;
        }

        public static Instruction Create(OpCode opcode, string str)
        {
            Instruction i = new Instruction();
            i.opCode = opcode;
            i.type = InstructionType.OpCode;
            i.operand = str;
            i.operandType = typeof(string);
            return i;
        }

        public static Instruction Create(OpCode opcode, IKMethod met)
        {
            Instruction i = new Instruction();
            i.opCode = opcode;
            i.type = InstructionType.OpCode;
            i.operand = met;
            i.operandType = typeof(IKMethod);
            return i;
        }

        public static Instruction Create(OpCode opcode, IKCtor ctor)
        {
            Instruction i = new Instruction();
            i.opCode = opcode;
            i.type = InstructionType.OpCode;
            i.operand = ctor;
            i.operandType = typeof(IKCtor);
            return i;
        }

        public static Instruction Create(OpCode opcode, IKType type)
        {
            Instruction i = new Instruction();
            i.opCode = opcode;
            i.type = InstructionType.OpCode;
            i.operand = type;
            i.operandType = typeof(IKType);
            return i;
        }

        internal static Instruction Create(OpCode opcode, long number)
        {
            Instruction i = new Instruction();
            i.opCode = opcode;
            i.type = InstructionType.OpCode;
            i.operand = number;
            i.operandType = typeof(long);
            return i;
        }

        internal static Instruction Create(OpCode opcode, LocalBuilder lvar)
        {
            Instruction i = new Instruction();
            i.opCode = opcode;
            i.type = InstructionType.OpCode;
            i.operand = lvar;
            i.operandType = typeof(LocalBuilder);
            return i;
        }

        internal static Instruction Create(OpCode opcode, Label label)
        {
            Instruction i = new Instruction();
            i.opCode = opcode;
            i.type = InstructionType.OpCode;
            i.operand = label;
            i.operandType = typeof(Label);
            return i;
        }

        public static Instruction Create(Specials specials)
        {
            Instruction i = new Instruction();
            i.type = InstructionType.Special;
            i.operand = new Empty();
            i.operandType = typeof(Empty);
            i.special = specials;
            return i;
        }

        public static Instruction Create(Specials specials, Label label)
        {
            Instruction i = new Instruction();
            i.type = InstructionType.Special;
            i.operand = label;
            i.operandType = typeof(Label);
            i.special = specials;
            return i;
        }

        public static Instruction Create(Specials specials, LocalBuilder lvar)
        {
            Instruction i = new Instruction();
            i.type = InstructionType.Special;
            i.operand = lvar;
            i.operandType = typeof(LocalBuilder);
            i.special = specials;
            return i;
        }
    }

    class SequencePoint
    {
        string document;
        int line, column;
    }
}

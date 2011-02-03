using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace reflect_ilc
{
	class Instruction
	{
		public byte[] code;
		public int offset;
		public int length;
		public OpCode oc;
		public TokenResolver resolver;

		public Instruction(byte[] code, int offset, int length, OpCode oc, TokenResolver resolver)
		{
			this.code = code;
			this.offset = offset;
			this.length = length;
			this.oc = oc;
			this.resolver = resolver;
		}

		public override string ToString()
		{
			return ToString2();
			//return ToString2() + string.Format("\n\t\t[ot={0} a={1} b={2}]\n", oc.OpCodeType, oc.StackBehaviourPop, oc.StackBehaviourPush);
		}

		int GetImm32() { return BitConverter.ToInt32(code, offset + length - 4); }
		sbyte GetImm8() { return (sbyte)code[offset + length - 1]; }

		public string ToString2()
		{
			var s = string.Format(".IL_{0:X3}:\t{2}\t{1}", offset, oc, GetCodeBytes());

			if (oc.OperandType == OperandType.InlineMethod)
			{
				var mb = resolver.AsMethod(GetImm32());
				return s + "\t" + mb.DeclaringType + "::" + mb.Name;
			}

			if (oc.OperandType == OperandType.InlineBrTarget)
			{
				return s + string.Format("\t.IL_{0:X3}", GetImm32() - offset );
			}

			if (oc.OperandType == OperandType.ShortInlineBrTarget)
			{
				return s + string.Format("\t.IL_{0:X3}", GetImm8() + offset + length);
			}

			if (oc.OperandType == OperandType.InlineField)
			{
				var f = resolver.AsField(GetImm32());
				return s + string.Format("\t{0}::{1}", f.DeclaringType.FullName, f.Name);
			}

			return s;
		}

		private string GetCodeBytes()
		{
			var s = "";
			for (int i = offset; i < offset + length; i++)
				s += string.Format("{0:X2} ", code[i]);

			if (s.Length < 20)
				s += new string(' ', 20 - s.Length);
			return s;
		}

		public Type GetMethodReturnType()
		{
			if (oc.OperandType == OperandType.InlineMethod)
				return resolver.GetMethodReturnType(GetImm32());

			throw new NotImplementedException();
		}

		public int GetImmediateValue()
		{
			if (oc.Value >= OpCodes.Ldarg_0.Value && oc.Value <= OpCodes.Ldarg_3.Value)
				return oc.Value - OpCodes.Ldarg_0.Value;

			if (oc.Value >= OpCodes.Ldloc_0.Value && oc.Value <= OpCodes.Ldloc_3.Value)
				return oc.Value - OpCodes.Ldloc_0.Value;

			if (oc.Value >= OpCodes.Stloc_0.Value && oc.Value <= OpCodes.Stloc_3.Value)
				return oc.Value - OpCodes.Stloc_0.Value;

			if (oc == OpCodes.Ldc_I4_M1) return -1;

			if (oc.Value >= OpCodes.Ldc_I4_0.Value && oc.Value <= OpCodes.Ldc_I4_8.Value)
				return oc.Value - OpCodes.Ldc_I4_0.Value;

			if (oc.OperandType == OperandType.ShortInlineI)
				return GetImm8();

			if (oc.OperandType == OperandType.ShortInlineVar)
				return GetImm8();

			throw new NotImplementedException();
		}

		static Regex killIndexSpecialization = new Regex(@"\.\d+$");

		public string GetName()
		{
			return killIndexSpecialization.Replace(oc.Name, "");
		}

		public Type GetLocalType(int i)
		{
			return resolver.GetLocalType(i);
		}

		public Type GetArgType(int i)
		{
			return resolver.GetArgType(i);
		}

		public Type[] GetMethodArgs()
		{
			if (oc.OperandType == OperandType.InlineMethod)
				return resolver.GetMethodArgs(GetImm32());

			throw new NotImplementedException();
		}

		public Type GetTokenDeclaringType()
		{
			if (oc.OperandType == OperandType.InlineField)
				return resolver.GetDeclaringType(GetImm32());

			throw new NotImplementedException();
		}

		public Type GetTokenType()
		{
			if (oc.OperandType == OperandType.InlineField)
				return resolver.AsField(GetImm32()).FieldType;

			throw new NotImplementedException();
		}
	}
}

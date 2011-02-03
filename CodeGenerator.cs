using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection.Emit;

namespace reflect_ilc
{
	enum BasicType
	{
		I4,
		I8,
		F,
		Ref,
		Struct,
	}

	static class Helpers
	{
		public static IEnumerable<int> To(this int low, int high)
		{
			while (low < high)
				yield return low++;
		}

		public static BasicType GetBasicType(this Type t)
		{
			if (t == typeof(int)) return BasicType.I4;
			if (t == typeof(byte)) return BasicType.I4;
			if (t.IsClass) return BasicType.Ref;

			if (t.IsPrimitive) throw new NotImplementedException("Don't know BasicTypeOf(" + t + ")");
			if (t.IsValueType) return BasicType.Struct;

			throw new NotImplementedException("Don't know BasicTypeOf(" + t + ")");
		}

		public static Type[] EvalTypesInContext(string s, Instruction i, Type[] stack)
		{
			s = s.Trim().Replace(" ", "");

			if (s.Contains(','))
				return s.Split(',').SelectMany(
					f => EvalTypesInContext(f, i, stack)).ToArray();

			if (s.Contains("%imm"))
				s = s.Replace("%imm", i.GetImmediateValue().ToString());

			if (s == "%top")
				return new Type[] { stack.LastOrDefault() };
			if (s == "()")
				return new Type[] { };
			if (s == "I4")
				return new Type[] { typeof(int) };
			if (s == "Ref")
				return new Type[] { typeof(object) };

			for (int j = 0; j < i.resolver.GetNumArgs(); j++)
				if (s == "%arg_type[" + j + "]")
					return new Type[] { i.resolver.GetArgType(j) };

			for (int j = 0; j < i.resolver.GetNumLocals(); j++)
				if (s == "%local_type[" + j + "]")
					return new Type[] { i.resolver.GetLocalType(j) };

			if (s == "%tok_ret")
				return new Type[] { i.GetMethodReturnType() };

			if (s == "%tok_args")
				return i.GetMethodArgs();

			if (s == "%tok_decltype")
				return new Type[] { i.GetTokenDeclaringType() };

			if (s == "%tok_type")
				return new Type[] { i.GetTokenType() };

			throw new NotImplementedException();
		}
	}

	class CodeGenerator
	{
		static List<InstructionImpl> impls = new List<InstructionImpl>();
		static InstructionImpl current = null;

		static CodeGenerator()
		{
			foreach (var line in File.ReadAllLines("x86_instructions.txt"))
			{
				if (line.Trim().StartsWith("#")) continue;
				if (line.Trim().Length == 0) continue;

				if (line.StartsWith("\t"))
				{
					if (current == null)
						throw new InvalidOperationException();

					current.AddCode(line);
				}
				else
				{
					// it's a definition
					string[] names;
					Func<Instruction, Type[], Type[]> stackBefore, stackAfter;

					if (!ParseInstructionDefinition(line, out names, out stackBefore, out stackAfter))
						throw new InvalidOperationException();

					current = new InstructionImpl(line, names, stackBefore, stackAfter);
					impls.Add(current);
				}
			}

			Console.WriteLine("Done loading instructions.");
		}

		static bool ParseInstructionDefinition(string s, out string[] names,
			out Func<Instruction, Type[], Type[]> stackBefore,
			out Func<Instruction, Type[], Type[]> stackAfter)
		{
			names = null; stackBefore = null; stackAfter = null;

			var parts = s.Split(new string[] { ":", "->" }, StringSplitOptions.None);
			if (parts.Length != 3)
				return false;

			names = parts[0].Split(',').Select(a => a.Trim()).ToArray();
			stackBefore = (i, t) => Helpers.EvalTypesInContext(parts[1], i, t);
			stackAfter = (i, t) => Helpers.EvalTypesInContext(parts[2], i, t);

			return true;
		}

		public static IEnumerable<InstructionImpl> FindMatches(Instruction i, Type[] stack)
		{
			foreach (var impl in impls)
				if (impl.IsApplicable(i, stack))
					yield return impl;
		}
	}
}

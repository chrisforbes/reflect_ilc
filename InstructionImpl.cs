using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection.Emit;

namespace reflect_ilc
{
	class InstructionImpl
	{
		string def;
		string[] names;
		List<string> code = new List<string>();
		Func<Instruction, Type[], Type[]> stackBefore, stackAfter;

		public string Definition { get { return def; } }

		static bool IsCompatible(Type t, Type u)
		{
			return t.GetBasicType() == u.GetBasicType();
		}

		public bool IsApplicable(Instruction i, Type[] stack)
		{
			if (!names.Contains(i.GetName()))
				return false;

			var t = stackBefore(i, stack);

			if (t.Length > stack.Length)
				return false;

			for (int j = 0; j < t.Length; j++)
				if (!IsCompatible(t[j], stack[stack.Length - t.Length + j]))
					return false;

			if (i.oc == OpCodes.Ret)
				if (t.Length != stack.Length)
					return false;

			return true;
		}

		public Type[] AdjustStack(Instruction i, Type[] stack)
		{
			var t = stackBefore(i, stack);
			var u = stackAfter(i, stack);

			return stack
				.Take(stack.Length - t.Length).Concat(u).ToArray();
		}

		public InstructionImpl(string def, string[] names,
			Func<Instruction, Type[], Type[]> stackBefore,
			Func<Instruction, Type[], Type[]> stackAfter)
		{
			this.def = def;
			this.names = names;
			this.stackBefore = stackBefore;
			this.stackAfter = stackAfter;
		}

		public void AddCode(string line)
		{
			code.Add(line);
		}

		public IEnumerable<string> SpecializeCodeForContext(Instruction i, Type[] stack)
		{
			foreach (var s in code)
			{
				var t = s.Contains("%imm") ? s.Replace("%imm", i.GetImmediateValue().ToString()) : s;

				// todo: more substitutions

				yield return t;
			}
		}
	}
}

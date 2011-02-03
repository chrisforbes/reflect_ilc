using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.IO;

namespace reflect_ilc
{
	class CilRecognizer
	{
		static OpCode[] shortOpcodes;
		static OpCode[] longOpcodes;

		static CilRecognizer()
		{
			shortOpcodes = new OpCode[0x100];
			longOpcodes = new OpCode[0x100];

			foreach (var fi in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				var o = (OpCode)fi.GetValue(null);
				var value = (ushort)o.Value;

				if (value < 0x100)
					shortOpcodes[value] = o;
				else if ((value & 0xff00) == 0xfe00)
					longOpcodes[value & 0xff] = o;
			}
		}

		public CilRecognizer(byte[] code, TokenResolver resolver)
		{
			this.code = code;
			this.resolver = resolver;
		}

		byte[] code;
		int position;
		TokenResolver resolver;

		byte ReadByte() { return code[position++]; }

		int GetOperandSize(OpCode o, int pos)
		{
			switch (o.OperandType)
			{
				case OperandType.ShortInlineBrTarget: return 1;
				case OperandType.ShortInlineI: return 1;
				case OperandType.ShortInlineR: return 4;
				case OperandType.ShortInlineVar: return 1;
				case OperandType.InlineVar: return 2;
				case OperandType.InlineType: return 4;
				case OperandType.InlineTok: return 4;
				case OperandType.InlineString: return 4;
				case OperandType.InlineSig: return 4;
				case OperandType.InlineR: return 8;
				case OperandType.InlineNone: return 0;
				case OperandType.InlineMethod: return 4;
				case OperandType.InlineI8: return 8;
				case OperandType.InlineI: return 4;
				case OperandType.InlineField: return 4;
				case OperandType.InlineBrTarget: return 4;
				case OperandType.InlineSwitch:
					return 4 + 4 * BitConverter.ToInt32(code, pos);
			}

			throw new NotImplementedException();
		}

		Instruction ReadNext()
		{
			var o = OpCodes.Nop;

			var oldPos = position;
			var b = ReadByte();
			if (b != 0xfe)
				o = shortOpcodes[b];
			else
				o = longOpcodes[ReadByte()];

			var opsize = GetOperandSize( o, position );
			position = oldPos + o.Size + opsize;
			return new Instruction(code, oldPos, o.Size + opsize, o, resolver);
		}

		public IEnumerable<Instruction> GetInstructions()
		{
			position = 0;

			while (position < code.Length)
				yield return ReadNext();
		}
	}
}

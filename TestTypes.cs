using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace reflect_ilc
{
	public struct Fail
	{
		public int x, y;
	}

	public class Foo
	{
		public int x = 0;
		public void Bar()
		{
			int y = 2; x = 3 + y++ + Baz();
		}

		int Baz()
		{
			return 1;
		}

		int Moo(int a)
		{
			return Baz() + a;
		}

		Foo Woz()
		{
			return new Foo() { x = 7 };
		}

		public byte X( byte Y )
		{
			return (byte)(Y + 2);
		}

		public Fail Fail()
		{
			Fail f;
			f.x = 42;
			f.y = 17;
			return f;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace reflect_ilc
{
	class Program
	{
		static void Main(string[] args)
		{
			var a = Assembly.ReflectionOnlyLoadFrom(@"reflect_ilc.exe");
			if (a == null)
				Console.WriteLine("Failed loading assembly");

			var t = a.GetType("reflect_ilc.Foo");

			foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
				ProcessMethod(m);
		}

		static string TypeName(Type t)
		{
			if (t == null) return null;
			if (t == typeof(int)) return "I4";
			return t.ToString();
		}

		static void ProcessMethod(MethodInfo m)
		{
			Console.WriteLine("Processing Method: {0}::{1}", m.DeclaringType.FullName, m.Name);

			var body = m.GetMethodBody();

			Console.WriteLine("Return type: {0}", m.ReturnType);
			Console.WriteLine("Local variable layout:");
			foreach (var lv in body.LocalVariables)
			{
				Console.WriteLine("\t{0}: type {1} pinned {2}", lv.LocalIndex, lv.LocalType, lv.IsPinned);
			}

			Console.WriteLine("Code bytes:");
			var code = body.GetILAsByteArray();

			var recog = new CilRecognizer(code, new TokenResolver(m));

			var stack = new Type[] { };

			Func<Type[], string> StringOfStack = 
				s => string.Join( " ", s.Select(a=>a.ToString()).ToArray() );

			foreach (var oc in recog.GetInstructions())
			{
				Console.WriteLine("{0}", oc.ToString());

				// try to generate some code
				var impls = CodeGenerator.FindMatches(oc, stack).ToArray();
	
				if (impls.Length == 0)
				{
					Console.WriteLine("--- ERROR: No code generation option ----");
					Console.WriteLine("Stack: " + StringOfStack(stack));
					Console.WriteLine();
				}

				else
				{
					Console.WriteLine("Emitting from: {0}", impls[0].Definition);
					foreach( var s in impls[0].SpecializeCodeForContext( oc, stack ) )
						Console.WriteLine(s);

					stack = impls[0].AdjustStack(oc, stack);
				}
			}

			Console.WriteLine();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace reflect_ilc
{
	static class Extensions
	{
		public static int GetSizeAsField(this Type t)
		{
			if (t.IsPointer) return 4;
			if (t.IsClass) return 4;
			if (t.IsInterface) return 4;
			if (t.IsEnum) return t.UnderlyingSystemType.GetSizeAsField();

			throw new NotImplementedException();
		}

		public static IEnumerable<T> Concat<T>(this IEnumerable<T> seq, params T[] ts)
		{
			foreach (var t in seq) yield return t;
			foreach (var t in ts) yield return t;
		}

		public static Type GetStackType(this Type t)
		{
			if (t == typeof(sbyte)) return typeof(int);
			if (t == typeof(short)) return typeof(int);
			if (t == typeof(byte)) return typeof(int);
			if (t == typeof(ushort)) return typeof(int);
			if (t == typeof(uint)) return typeof(int);
			if (t == typeof(char)) return typeof(int);

			return t;
		}
	}
}

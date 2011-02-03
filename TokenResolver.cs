using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace reflect_ilc
{
	class TokenResolver
	{
		MethodBase containingMethod;

		public TokenResolver(MethodBase containingMethod)
		{
			this.containingMethod = containingMethod;
		}

		public MethodBase AsMethod(int token)
		{
			return containingMethod.Module.ResolveMethod(token);
		}

		public FieldInfo AsField(int token)
		{
			return containingMethod.Module.ResolveField(token);
		}

		public Type GetArgType(int i)
		{
			if (containingMethod.IsStatic)
				return containingMethod.GetParameters()[i].ParameterType;
			else
				return i > 0 ? containingMethod.GetParameters()[i - 1].ParameterType : containingMethod.DeclaringType;
		}

		public Type GetLocalType(int i)
		{
			return containingMethod.GetMethodBody().LocalVariables[i].LocalType;
		}

		public int GetNumArgs()
		{
			if (containingMethod.IsStatic)
				return containingMethod.GetParameters().Length;
			else
				return containingMethod.GetParameters().Length + 1;
		}

		public int GetNumLocals()
		{
			return containingMethod.GetMethodBody().LocalVariables.Count;
		}

		public Type GetMethodReturnType(int token)
		{
			var m = AsMethod(token);
			if (m is MethodInfo)
				return ((MethodInfo)m).ReturnType;
			if (m is ConstructorInfo)
				return m.DeclaringType;

			throw new NotImplementedException();
		}

		public Type[] GetMethodArgs(int token)		// todo: unhax this hax for ctors
		{
			var m = AsMethod(token);

			if (m.IsStatic || m.IsConstructor)
				return m.GetParameters().Select(a => a.ParameterType).ToArray();
			else
				return new Type[] { m.DeclaringType }.Concat(m.GetParameters().Select(a => a.ParameterType)).ToArray();

			throw new NotImplementedException();
		}

		public Type GetDeclaringType(int token)
		{
			var f = AsField(token);
			return f.DeclaringType;
		}
	}
}

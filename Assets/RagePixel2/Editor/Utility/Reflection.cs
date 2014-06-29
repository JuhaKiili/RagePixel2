using System;
using System.Reflection;
using UnityEditor;

namespace RagePixel2
{
	public class Reflection
	{
		public static void SetEditorStatic (string typeName, string propertyName, object[] value)
		{
			Assembly assembly = Assembly.GetAssembly (typeof (EditorUtility));
			System.Type type = assembly.GetType ("UnityEditor." + typeName);
			PropertyInfo propertyInfo = type.GetProperty (propertyName);
			propertyInfo.GetSetMethod ().Invoke (null, value);
		}

		public static object GetEditorStatic (string typeName, string propertyName)
		{
			Assembly assembly = Assembly.GetAssembly (typeof (EditorUtility));
			System.Type type = assembly.GetType ("UnityEditor." + typeName);
			PropertyInfo propertyInfo = type.GetProperty (propertyName);
			return propertyInfo.GetGetMethod ().Invoke (null, new object[0]);
		}

		public static void Set (object instance, string propertyName, object value)
		{
			instance.GetType ()
				.GetProperty (propertyName)
				.SetValue (instance, value, BindingFlags.Default, null, new object[] {0}, null);
		}

		public static object Get (object instance, string propertyName)
		{
			PropertyInfo propertyInfo = instance.GetType ().GetProperty (propertyName);
			return propertyInfo.GetValue (instance, new object[] {0});
		}

		public static System.Type GetEditorType (string typeName)
		{
			Assembly assembly = Assembly.GetAssembly (typeof (EditorUtility));
			return assembly.GetType ("UnityEditor." + typeName);
		}

		public static object InvokeEditorStatic (string typeName, string methodName, object[] paramValues, Type[] types)
		{
			System.Type type = GetEditorType (typeName);
			MethodInfo methodInfo = type.GetMethod (methodName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, types, null);
			return methodInfo.Invoke (methodName, paramValues);
		}

		public static object Invoke (object instance, string methodName, object[] paramValues)
		{
			Type[] parameterTypes = new Type[paramValues.Length];
			for (int i = 0; i < paramValues.Length; i++)
				parameterTypes[i] = paramValues[i].GetType ();

			MethodInfo methodInfo = instance.GetType ().GetMethod (methodName, parameterTypes);
			return methodInfo.Invoke (instance, paramValues);
		}

		public static object ConstructEditor (string typeName, object[] paramValues)
		{
			Assembly assembly = Assembly.GetAssembly (typeof (EditorUtility));
			System.Type type = assembly.GetType (typeName);
			ConstructorInfo ctor = type.GetConstructor (
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance,
				null,
				new[] {type},
				null
				);
			return ctor.Invoke (paramValues);
		}
	}
}

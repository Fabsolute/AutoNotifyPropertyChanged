using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.ComponentModel;

namespace AutoNotifyPropertyChanged
{
    public static class NotifyHelper
    {
        private static Dictionary<Type, Type> ModifiedCache = new Dictionary<Type, Type>();
        private static readonly Lazy<AssemblyBuilder> AssemblyBuilderHolder = new Lazy<AssemblyBuilder>(() => AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("ModifiedAssembly"), AssemblyBuilderAccess.Run));
        private static readonly Lazy<ModuleBuilder> ModuleBuilderHolder = new Lazy<ModuleBuilder>(() => AssemblyBuilder.DefineDynamicModule("ModifiedModule"));

        private static AssemblyBuilder AssemblyBuilder { get { return AssemblyBuilderHolder.Value; } }
        private static ModuleBuilder ModuleBuilder { get { return ModuleBuilderHolder.Value; } }

        public static T CreateInstance<T>() where T : ModelBase
        {
            Type type = typeof(T);
            Type modified = Load(type);
            return (T)Activator.CreateInstance(modified);
        }

        public static Type Load(Type type)
        {
            Type modifiedType;
            // Try load
            if (!ModifiedCache.TryGetValue(type, out modifiedType))
            {
                modifiedType = Create(type);
            }
            return modifiedType;
        }

        private static Type Create(Type type)
        {
            // Create modified type
            TypeBuilder typeBuilder = ModuleBuilder.DefineType(type.Name + "_Modified", TypeAttributes.Public, type);
            // Get raise method
            MethodInfo onPropertyChangedMethodInfo = typeof(ModelBase).GetMethod("OnPropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic, Type.DefaultBinder, new Type[1] { typeof(string) }, null);
            // Get all properties
            type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).
            // Filter virtual and public or protected
            Where(p => p.GetSetMethod().IsVirtual && (p.GetSetMethod().IsPublic || p.GetSetMethod().IsFamily)).ToList().
            // Override all set methods
            ForEach((property) =>
            {
                // Set method
                MethodInfo setMethodInfo = property.GetSetMethod();
                // Create new set method
                MethodBuilder setMethodBuilder = typeBuilder.DefineMethod(setMethodInfo.Name, setMethodInfo.Attributes, setMethodInfo.ReturnType, setMethodInfo.GetParameters().Select(t => t.ParameterType).ToArray());
                // Override set method
                typeBuilder.DefineMethodOverride(setMethodBuilder, setMethodInfo);

                // New method's ilgenerator
                ILGenerator setMethodGenerator = setMethodBuilder.GetILGenerator();
                // Get argument_0 "this"
                setMethodGenerator.Emit(OpCodes.Ldarg_0);
                // Get argument_1 "string parameterName"
                setMethodGenerator.Emit(OpCodes.Ldarg_1);
                // Call "base.set " method
                setMethodGenerator.EmitCall(OpCodes.Call, setMethodInfo, null);
                // Get argument_0 "this"
                setMethodGenerator.Emit(OpCodes.Ldarg_0);
                // Set argument_1 "string property.Name"
                setMethodGenerator.Emit(OpCodes.Ldstr, property.Name);
                // Call this.OnPropertyChanged(property.Name);
                setMethodGenerator.EmitCall(OpCodes.Call, onPropertyChangedMethodInfo, null);
                setMethodGenerator.Emit(OpCodes.Ret);

                /*
                 * public override void set_PropertyName(ParameterType value)
                 * {
                 *      base.set_PropertyName(value);
                 *      OnPropertyChanged("PropertyName");
                 * }
                 * */
            });
            // Create modified type
            Type modifiedType = typeBuilder.CreateType();
            // Cache
            ModifiedCache.Add(type, modifiedType);
            return modifiedType;
        }


    }
}

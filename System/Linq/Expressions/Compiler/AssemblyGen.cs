namespace System.Linq.Expressions.Compiler
{
    using System;
    using System.Dynamic.Utils;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security;
    using System.Text;
    using System.Threading;

    internal sealed class AssemblyGen
    {
        private static AssemblyGen _assembly;
        private int _index;
        private readonly AssemblyBuilder _myAssembly;
        private readonly ModuleBuilder _myModule;

        private AssemblyGen()
        {
            AssemblyName name = new AssemblyName("Snippets");
            CustomAttributeBuilder[] assemblyAttributes = new CustomAttributeBuilder[] { new CustomAttributeBuilder(typeof(SecurityTransparentAttribute).GetConstructor(Type.EmptyTypes), new object[0]) };
            this._myAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run, assemblyAttributes);
            this._myModule = this._myAssembly.DefineDynamicModule(name.Name, false);
            this._myAssembly.DefineVersionInfoResource();
        }

        internal static TypeBuilder DefineDelegateType(string name)
        {
            return Assembly.DefineType(name, typeof(MulticastDelegate), TypeAttributes.AutoClass | TypeAttributes.Sealed | TypeAttributes.Public);
        }

        private TypeBuilder DefineType(string name, Type parent, TypeAttributes attr)
        {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(parent, "parent");
            StringBuilder builder = new StringBuilder(name);
            int num = Interlocked.Increment(ref this._index);
            builder.Append("$");
            builder.Append(num);
            builder.Replace('+', '_').Replace('[', '_').Replace(']', '_').Replace('*', '_').Replace('&', '_').Replace(',', '_').Replace('\\', '_');
            name = builder.ToString();
            return this._myModule.DefineType(name, attr, parent);
        }

        private static AssemblyGen Assembly
        {
            get
            {
                if (_assembly == null)
                {
                    Interlocked.CompareExchange<AssemblyGen>(ref _assembly, new AssemblyGen(), null);
                }
                return _assembly;
            }
        }
    }
}


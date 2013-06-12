namespace System.Text.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    internal class RegexTypeCompiler : RegexCompiler
    {
        private AssemblyBuilder _assembly;
        private MethodBuilder _methbuilder;
        private ModuleBuilder _module;
        private static LocalDataStoreSlot _moduleSlot = Thread.AllocateDataSlot();
        private TypeBuilder _typebuilder;
        private static int _typeCount = 0;

        internal RegexTypeCompiler(AssemblyName an, CustomAttributeBuilder[] attribs, string resourceFile)
        {
            new ReflectionPermission(PermissionState.Unrestricted).Assert();
            try
            {
                List<CustomAttributeBuilder> assemblyAttributes = new List<CustomAttributeBuilder>();
                CustomAttributeBuilder item = new CustomAttributeBuilder(typeof(SecurityTransparentAttribute).GetConstructor(Type.EmptyTypes), new object[0]);
                assemblyAttributes.Add(item);
                CustomAttributeBuilder builder2 = new CustomAttributeBuilder(typeof(SecurityRulesAttribute).GetConstructor(new Type[] { typeof(SecurityRuleSet) }), new object[] { SecurityRuleSet.Level2 });
                assemblyAttributes.Add(builder2);
                this._assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave, assemblyAttributes);
                this._module = this._assembly.DefineDynamicModule(an.Name + ".dll");
                if (attribs != null)
                {
                    for (int i = 0; i < attribs.Length; i++)
                    {
                        this._assembly.SetCustomAttribute(attribs[i]);
                    }
                }
                if (resourceFile != null)
                {
                    this._assembly.DefineUnmanagedResource(resourceFile);
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
        }

        internal void BakeMethod()
        {
            this._methbuilder = null;
        }

        internal Type BakeType()
        {
            Type type = this._typebuilder.CreateType();
            this._typebuilder = null;
            return type;
        }

        internal void DefineMethod(string methname, Type returntype)
        {
            MethodAttributes attributes = MethodAttributes.Virtual | MethodAttributes.Public;
            this._methbuilder = this._typebuilder.DefineMethod(methname, attributes, returntype, null);
            base._ilg = this._methbuilder.GetILGenerator();
        }

        internal void DefineType(string typename, bool ispublic, Type inheritfromclass)
        {
            if (ispublic)
            {
                this._typebuilder = this._module.DefineType(typename, TypeAttributes.Public, inheritfromclass);
            }
            else
            {
                this._typebuilder = this._module.DefineType(typename, TypeAttributes.AnsiClass, inheritfromclass);
            }
        }

        internal Type FactoryTypeFromCode(RegexCode code, RegexOptions options, string typeprefix)
        {
            base._code = code;
            base._codes = code._codes;
            base._strings = code._strings;
            base._fcPrefix = code._fcPrefix;
            base._bmPrefix = code._bmPrefix;
            base._anchors = code._anchors;
            base._trackcount = code._trackcount;
            base._options = options;
            string str3 = Interlocked.Increment(ref _typeCount).ToString(CultureInfo.InvariantCulture);
            string typename = typeprefix + "Runner" + str3;
            string str2 = typeprefix + "Factory" + str3;
            this.DefineType(typename, false, typeof(RegexRunner));
            this.DefineMethod("Go", null);
            base.GenerateGo();
            this.BakeMethod();
            this.DefineMethod("FindFirstChar", typeof(bool));
            base.GenerateFindFirstChar();
            this.BakeMethod();
            this.DefineMethod("InitTrackCount", null);
            base.GenerateInitTrackCount();
            this.BakeMethod();
            Type newtype = this.BakeType();
            this.DefineType(str2, false, typeof(RegexRunnerFactory));
            this.DefineMethod("CreateInstance", typeof(RegexRunner));
            this.GenerateCreateInstance(newtype);
            this.BakeMethod();
            return this.BakeType();
        }

        internal void GenerateCreateHashtable(FieldInfo field, Hashtable ht)
        {
            MethodInfo method = typeof(Hashtable).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            base.Ldthis();
            base.Newobj(typeof(Hashtable).GetConstructor(new Type[0]));
            base.Stfld(field);
            IDictionaryEnumerator enumerator = ht.GetEnumerator();
            while (enumerator.MoveNext())
            {
                base.Ldthisfld(field);
                if (enumerator.Key is int)
                {
                    base.Ldc((int) enumerator.Key);
                    base._ilg.Emit(OpCodes.Box, typeof(int));
                }
                else
                {
                    base.Ldstr((string) enumerator.Key);
                }
                base.Ldc((int) enumerator.Value);
                base._ilg.Emit(OpCodes.Box, typeof(int));
                base.Callvirt(method);
            }
        }

        internal void GenerateCreateInstance(Type newtype)
        {
            base.Newobj(newtype.GetConstructor(new Type[0]));
            base.Ret();
        }

        internal void GenerateRegexType(string pattern, RegexOptions opts, string name, bool ispublic, RegexCode code, RegexTree tree, Type factory)
        {
            FieldInfo ft = this.RegexField("pattern");
            FieldInfo info2 = this.RegexField("roptions");
            FieldInfo info3 = this.RegexField("factory");
            FieldInfo field = this.RegexField("caps");
            FieldInfo info5 = this.RegexField("capnames");
            FieldInfo info6 = this.RegexField("capslist");
            FieldInfo info7 = this.RegexField("capsize");
            Type[] parameterTypes = new Type[0];
            this.DefineType(name, ispublic, typeof(Regex));
            this._methbuilder = null;
            MethodAttributes @public = MethodAttributes.Public;
            base._ilg = this._typebuilder.DefineConstructor(@public, CallingConventions.Standard, parameterTypes).GetILGenerator();
            base.Ldthis();
            base._ilg.Emit(OpCodes.Call, typeof(Regex).GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[0], new ParameterModifier[0]));
            base.Ldthis();
            base.Ldstr(pattern);
            base.Stfld(ft);
            base.Ldthis();
            base.Ldc((int) opts);
            base.Stfld(info2);
            base.Ldthis();
            base.Newobj(factory.GetConstructor(parameterTypes));
            base.Stfld(info3);
            if (code._caps != null)
            {
                this.GenerateCreateHashtable(field, code._caps);
            }
            if (tree._capnames != null)
            {
                this.GenerateCreateHashtable(info5, tree._capnames);
            }
            if (tree._capslist != null)
            {
                base.Ldthis();
                base.Ldc(tree._capslist.Length);
                base._ilg.Emit(OpCodes.Newarr, typeof(string));
                base.Stfld(info6);
                for (int i = 0; i < tree._capslist.Length; i++)
                {
                    base.Ldthisfld(info6);
                    base.Ldc(i);
                    base.Ldstr(tree._capslist[i]);
                    base._ilg.Emit(OpCodes.Stelem_Ref);
                }
            }
            base.Ldthis();
            base.Ldc(code._capsize);
            base.Stfld(info7);
            base.Ldthis();
            base.Call(typeof(Regex).GetMethod("InitializeReferences", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
            base.Ret();
            this._typebuilder.CreateType();
            base._ilg = null;
            this._typebuilder = null;
        }

        private FieldInfo RegexField(string fieldname)
        {
            return typeof(Regex).GetField(fieldname, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        internal void Save()
        {
            this._assembly.Save(this._assembly.GetName().Name + ".dll");
        }
    }
}


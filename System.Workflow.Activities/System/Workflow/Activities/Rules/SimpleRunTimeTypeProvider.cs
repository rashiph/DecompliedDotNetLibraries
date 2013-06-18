namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Threading;
    using System.Workflow.ComponentModel.Compiler;

    internal class SimpleRunTimeTypeProvider : ITypeProvider
    {
        private List<Assembly> references;
        private Assembly root;

        public event EventHandler TypeLoadErrorsChanged;

        public event EventHandler TypesChanged;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal SimpleRunTimeTypeProvider(Assembly startingAssembly)
        {
            this.root = startingAssembly;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Type GetType(string name)
        {
            return this.GetType(name, false);
        }

        public Type GetType(string name, bool throwOnError)
        {
            Type type = this.root.GetType(name, throwOnError, false);
            if (type != null)
            {
                return type;
            }
            type = Type.GetType(name, throwOnError, false);
            if (type != null)
            {
                return type;
            }
            foreach (Assembly assembly in this.ReferencedAssemblies)
            {
                type = assembly.GetType(name, throwOnError, false);
                if (type != null)
                {
                    return type;
                }
            }
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                type = assemblies[i].GetType(name, throwOnError, false);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        public Type[] GetTypes()
        {
            List<Type> list = new List<Type>();
            try
            {
                list.AddRange(this.root.GetTypes());
            }
            catch (ReflectionTypeLoadException exception)
            {
                foreach (Type type in exception.Types)
                {
                    if (type != null)
                    {
                        list.Add(type);
                    }
                }
            }
            foreach (Assembly assembly in this.ReferencedAssemblies)
            {
                try
                {
                    list.AddRange(assembly.GetTypes());
                }
                catch (ReflectionTypeLoadException exception2)
                {
                    foreach (Type type2 in exception2.Types)
                    {
                        if (type2 != null)
                        {
                            list.Add(type2);
                        }
                    }
                }
            }
            return list.ToArray();
        }

        public Assembly LocalAssembly
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.root;
            }
        }

        public ICollection<Assembly> ReferencedAssemblies
        {
            get
            {
                if (this.references == null)
                {
                    List<Assembly> list = new List<Assembly>();
                    foreach (AssemblyName name in this.root.GetReferencedAssemblies())
                    {
                        list.Add(Assembly.Load(name));
                    }
                    this.references = list;
                }
                return this.references;
            }
        }

        public IDictionary<object, Exception> TypeLoadErrors
        {
            get
            {
                this.TypesChanged(this, null);
                this.TypeLoadErrorsChanged(this, null);
                return null;
            }
        }
    }
}


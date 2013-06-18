namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Reflection;
    using System.Runtime;

    internal class AssemblyLoader
    {
        private System.Reflection.Assembly assembly;
        private System.Reflection.AssemblyName assemblyName;
        private bool isLocalAssembly;
        private TypeProvider typeProvider;

        internal AssemblyLoader(TypeProvider typeProvider, string filePath)
        {
            this.isLocalAssembly = false;
            this.typeProvider = typeProvider;
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }
            System.Reflection.AssemblyName assemblyName = System.Reflection.AssemblyName.GetAssemblyName(filePath);
            if (assemblyName != null)
            {
                ITypeResolutionService service = (ITypeResolutionService) typeProvider.GetService(typeof(ITypeResolutionService));
                if (service != null)
                {
                    try
                    {
                        this.assembly = service.GetAssembly(assemblyName);
                        if ((((this.assembly == null) && (assemblyName.GetPublicKeyToken() != null)) && ((assemblyName.GetPublicKeyToken().GetLength(0) == 0) && (assemblyName.GetPublicKey() != null))) && (assemblyName.GetPublicKey().GetLength(0) == 0))
                        {
                            System.Reflection.AssemblyName name = (System.Reflection.AssemblyName) assemblyName.Clone();
                            name.SetPublicKey(null);
                            name.SetPublicKeyToken(null);
                            this.assembly = service.GetAssembly(name);
                        }
                    }
                    catch
                    {
                    }
                }
                if (this.assembly == null)
                {
                    try
                    {
                        if (MultiTargetingInfo.MultiTargetingUtilities.IsFrameworkReferenceAssembly(filePath))
                        {
                            this.assembly = System.Reflection.Assembly.Load(assemblyName.FullName);
                        }
                        else
                        {
                            this.assembly = System.Reflection.Assembly.Load(assemblyName);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            if (this.assembly == null)
            {
                this.assembly = System.Reflection.Assembly.LoadFrom(filePath);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal AssemblyLoader(TypeProvider typeProvider, System.Reflection.Assembly assembly, bool isLocalAssembly)
        {
            this.isLocalAssembly = isLocalAssembly;
            this.typeProvider = typeProvider;
            this.assembly = assembly;
        }

        internal Type GetType(string typeName)
        {
            if (this.assembly != null)
            {
                Type type = null;
                try
                {
                    type = this.assembly.GetType(typeName);
                }
                catch (ArgumentException)
                {
                }
                if ((type != null) && ((type.IsPublic || type.IsNestedPublic) || (this.isLocalAssembly && (type.Attributes != TypeAttributes.NestedPrivate))))
                {
                    return type;
                }
            }
            return null;
        }

        internal Type[] GetTypes()
        {
            List<Type> list = new List<Type>();
            if (this.assembly != null)
            {
                foreach (Type type in this.assembly.GetTypes())
                {
                    if (type.IsPublic || (this.isLocalAssembly && (type.Attributes != TypeAttributes.NestedPrivate)))
                    {
                        list.Add(type);
                    }
                }
            }
            return list.ToArray();
        }

        internal System.Reflection.Assembly Assembly
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.assembly;
            }
        }

        internal System.Reflection.AssemblyName AssemblyName
        {
            get
            {
                if (this.assemblyName == null)
                {
                    this.assemblyName = this.assembly.GetName(true);
                }
                return this.assemblyName;
            }
        }
    }
}


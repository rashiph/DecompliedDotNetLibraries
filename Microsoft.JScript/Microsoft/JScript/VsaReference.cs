namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;

    internal class VsaReference : VsaItem, IJSVsaReferenceItem, IJSVsaItem
    {
        private System.Reflection.Assembly assembly;
        private string assemblyName;
        private bool loadFailed;

        internal VsaReference(VsaEngine engine, string itemName) : base(engine, itemName, JSVsaItemType.Reference, JSVsaItemFlag.None)
        {
            this.assemblyName = itemName;
            this.assembly = null;
            this.loadFailed = false;
        }

        private void CheckCompatibility()
        {
            PortableExecutableKinds kinds;
            ImageFileMachine machine;
            this.assembly.ManifestModule.GetPEKind(out kinds, out machine);
            if ((machine != ImageFileMachine.I386) || (PortableExecutableKinds.ILOnly != (kinds & (PortableExecutableKinds.Required32Bit | PortableExecutableKinds.ILOnly))))
            {
                PortableExecutableKinds pEKindFlags = base.engine.PEKindFlags;
                ImageFileMachine pEMachineArchitecture = base.engine.PEMachineArchitecture;
                if (((pEMachineArchitecture != ImageFileMachine.I386) || (PortableExecutableKinds.ILOnly != (pEKindFlags & (PortableExecutableKinds.Required32Bit | PortableExecutableKinds.ILOnly)))) && (machine != pEMachineArchitecture))
                {
                    JScriptException se = new JScriptException(JSError.IncompatibleAssemblyReference) {
                        value = this.assemblyName
                    };
                    base.engine.OnCompilerError(se);
                }
            }
        }

        internal override void Compile()
        {
            this.Compile(true);
        }

        internal bool Compile(bool throwOnFileNotFound)
        {
            try
            {
                string fileName = Path.GetFileName(this.assemblyName);
                string strA = fileName + ".dll";
                if ((string.Compare(fileName, "mscorlib.dll", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(strA, "mscorlib.dll", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    this.assembly = typeof(object).Assembly;
                }
                if ((string.Compare(fileName, "microsoft.jscript.dll", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(strA, "microsoft.jscript.dll", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    this.assembly = base.engine.JScriptModule.Assembly;
                }
                else if ((base.engine.ReferenceLoaderAPI != LoaderAPI.ReflectionOnlyLoadFrom) && ((string.Compare(fileName, "system.dll", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(strA, "system.dll", StringComparison.OrdinalIgnoreCase) == 0)))
                {
                    this.assembly = typeof(Regex).Module.Assembly;
                }
                if (this.assembly == null)
                {
                    string assemblyFile = base.engine.FindAssembly(this.assemblyName);
                    if (assemblyFile == null)
                    {
                        strA = this.assemblyName + ".dll";
                        bool flag = false;
                        foreach (object obj2 in base.engine.Items)
                        {
                            if ((obj2 is VsaReference) && (string.Compare(((VsaReference) obj2).AssemblyName, strA, StringComparison.OrdinalIgnoreCase) == 0))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            assemblyFile = base.engine.FindAssembly(strA);
                            if (assemblyFile != null)
                            {
                                this.assemblyName = strA;
                            }
                        }
                    }
                    if (assemblyFile == null)
                    {
                        if (throwOnFileNotFound)
                        {
                            throw new JSVsaException(JSVsaError.AssemblyExpected, this.assemblyName, new FileNotFoundException());
                        }
                        return false;
                    }
                    switch (base.engine.ReferenceLoaderAPI)
                    {
                        case LoaderAPI.LoadFrom:
                            this.assembly = System.Reflection.Assembly.LoadFrom(assemblyFile);
                            break;

                        case LoaderAPI.LoadFile:
                            this.assembly = System.Reflection.Assembly.LoadFile(assemblyFile);
                            break;

                        case LoaderAPI.ReflectionOnlyLoadFrom:
                            this.assembly = System.Reflection.Assembly.ReflectionOnlyLoadFrom(assemblyFile);
                            break;
                    }
                    this.CheckCompatibility();
                }
            }
            catch (JSVsaException)
            {
                throw;
            }
            catch (BadImageFormatException exception)
            {
                throw new JSVsaException(JSVsaError.AssemblyExpected, this.assemblyName, exception);
            }
            catch (FileNotFoundException exception2)
            {
                if (throwOnFileNotFound)
                {
                    throw new JSVsaException(JSVsaError.AssemblyExpected, this.assemblyName, exception2);
                }
                return false;
            }
            catch (FileLoadException exception3)
            {
                throw new JSVsaException(JSVsaError.AssemblyExpected, this.assemblyName, exception3);
            }
            catch (ArgumentException exception4)
            {
                throw new JSVsaException(JSVsaError.AssemblyExpected, this.assemblyName, exception4);
            }
            catch (Exception exception5)
            {
                throw new JSVsaException(JSVsaError.InternalCompilerError, exception5.ToString(), exception5);
            }
            if (this.assembly != null)
            {
                return true;
            }
            if (throwOnFileNotFound)
            {
                throw new JSVsaException(JSVsaError.AssemblyExpected, this.assemblyName);
            }
            return false;
        }

        internal Type GetType(string typeName)
        {
            if (this.assembly == null)
            {
                if (!this.loadFailed)
                {
                    try
                    {
                        this.Load();
                    }
                    catch
                    {
                        this.loadFailed = true;
                    }
                }
                if (this.assembly == null)
                {
                    return null;
                }
            }
            Type target = this.assembly.GetType(typeName, false);
            if ((target == null) || (target.IsPublic && !Microsoft.JScript.CustomAttribute.IsDefined(target, typeof(RequiredAttributeAttribute), true)))
            {
                return target;
            }
            return null;
        }

        private void Load()
        {
            try
            {
                if (string.Compare(this.assemblyName, "mscorlib", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.assembly = typeof(object).Module.Assembly;
                }
                else if (string.Compare(this.assemblyName, "Microsoft.JScript", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.assembly = typeof(VsaEngine).Module.Assembly;
                }
                else if (string.Compare(this.assemblyName, "System", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.assembly = typeof(Regex).Module.Assembly;
                }
                else
                {
                    this.assembly = System.Reflection.Assembly.Load(this.assemblyName);
                }
            }
            catch (BadImageFormatException exception)
            {
                throw new JSVsaException(JSVsaError.AssemblyExpected, this.assemblyName, exception);
            }
            catch (FileNotFoundException exception2)
            {
                throw new JSVsaException(JSVsaError.AssemblyExpected, this.assemblyName, exception2);
            }
            catch (ArgumentException exception3)
            {
                throw new JSVsaException(JSVsaError.AssemblyExpected, this.assemblyName, exception3);
            }
            catch (Exception exception4)
            {
                throw new JSVsaException(JSVsaError.InternalCompilerError, exception4.ToString(), exception4);
            }
            if (this.assembly == null)
            {
                throw new JSVsaException(JSVsaError.AssemblyExpected, this.assemblyName);
            }
        }

        internal System.Reflection.Assembly Assembly
        {
            get
            {
                if (base.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                return this.assembly;
            }
        }

        public string AssemblyName
        {
            get
            {
                if (base.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                return this.assemblyName;
            }
            set
            {
                if (base.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                this.assemblyName = value;
                base.isDirty = true;
                base.engine.IsDirty = true;
            }
        }
    }
}


namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Configuration.Assemblies;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security.Policy;
    using System.Threading;

    internal sealed class CompilerGlobals
    {
        internal AssemblyBuilder assemblyBuilder;
        internal Stack BreakLabelStack = new Stack();
        internal TypeBuilder classwriter;
        internal Evidence compilationEvidence;
        internal Stack ContinueLabelStack = new Stack();
        internal SimpleHashtable documents = new SimpleHashtable(8);
        internal int FinallyStackTop;
        internal TypeBuilder globalScopeClassWriter;
        internal bool InsideFinally;
        internal bool InsideProtectedRegion;
        internal ModuleBuilder module;
        internal SimpleHashtable usedNames = new SimpleHashtable(0x20);

        internal CompilerGlobals(VsaEngine engine, string assemName, string assemblyFileName, PEFileKinds PEFileKind, bool save, bool run, bool debugOn, bool isCLSCompliant, Version version, Globals globals)
        {
            string fileName = null;
            string dir = null;
            if (assemblyFileName != null)
            {
                try
                {
                    dir = Path.GetDirectoryName(Path.GetFullPath(assemblyFileName));
                }
                catch (Exception exception)
                {
                    throw new JSVsaException(JSVsaError.AssemblyNameInvalid, assemblyFileName, exception);
                }
                fileName = Path.GetFileName(assemblyFileName);
                if ((assemName == null) || (string.Empty == assemName))
                {
                    assemName = Path.GetFileName(assemblyFileName);
                    if (Path.HasExtension(assemName))
                    {
                        assemName = assemName.Substring(0, assemName.Length - Path.GetExtension(assemName).Length);
                    }
                }
            }
            if ((assemName == null) || (assemName == string.Empty))
            {
                assemName = "JScriptAssembly";
            }
            if (fileName == null)
            {
                if (PEFileKind == PEFileKinds.Dll)
                {
                    fileName = "JScriptModule.dll";
                }
                else
                {
                    fileName = "JScriptModule.exe";
                }
            }
            AssemblyName name = new AssemblyName {
                CodeBase = assemblyFileName
            };
            if (globals.assemblyCulture != null)
            {
                name.CultureInfo = globals.assemblyCulture;
            }
            name.Flags = AssemblyNameFlags.None;
            if ((globals.assemblyFlags & AssemblyFlags.PublicKey) != AssemblyFlags.SideBySideCompatible)
            {
                name.Flags = AssemblyNameFlags.PublicKey;
            }
            AssemblyFlags flags = globals.assemblyFlags & AssemblyFlags.CompatibilityMask;
            if (flags == AssemblyFlags.NonSideBySideAppDomain)
            {
                name.VersionCompatibility = AssemblyVersionCompatibility.SameDomain;
            }
            else if (flags == AssemblyFlags.NonSideBySideProcess)
            {
                name.VersionCompatibility = AssemblyVersionCompatibility.SameProcess;
            }
            else if (flags == AssemblyFlags.NonSideBySideMachine)
            {
                name.VersionCompatibility = AssemblyVersionCompatibility.SameMachine;
            }
            else
            {
                name.VersionCompatibility = (AssemblyVersionCompatibility) 0;
            }
            name.HashAlgorithm = globals.assemblyHashAlgorithm;
            if (globals.assemblyKeyFileName != null)
            {
                try
                {
                    using (FileStream stream = new FileStream(globals.assemblyKeyFileName, FileMode.Open, FileAccess.Read))
                    {
                        StrongNameKeyPair pair = new StrongNameKeyPair(stream);
                        if (globals.assemblyDelaySign)
                        {
                            if (stream.Length == 160L)
                            {
                                byte[] buffer = new byte[160];
                                stream.Seek(0L, SeekOrigin.Begin);
                                stream.Read(buffer, 0, 160);
                                name.SetPublicKey(buffer);
                            }
                            else
                            {
                                name.SetPublicKey(pair.PublicKey);
                            }
                        }
                        else
                        {
                            byte[] publicKey = pair.PublicKey;
                            name.KeyPair = pair;
                        }
                    }
                    goto Label_024E;
                }
                catch
                {
                    globals.assemblyKeyFileNameContext.HandleError(JSError.InvalidAssemblyKeyFile, globals.assemblyKeyFileName);
                    goto Label_024E;
                }
            }
            if (globals.assemblyKeyName != null)
            {
                try
                {
                    StrongNameKeyPair pair2 = new StrongNameKeyPair(globals.assemblyKeyName);
                    byte[] buffer2 = pair2.PublicKey;
                    name.KeyPair = pair2;
                }
                catch
                {
                    globals.assemblyKeyNameContext.HandleError(JSError.InvalidAssemblyKeyFile, globals.assemblyKeyName);
                }
            }
        Label_024E:
            name.Name = assemName;
            if (version != null)
            {
                name.Version = version;
            }
            else if (globals.assemblyVersion != null)
            {
                name.Version = globals.assemblyVersion;
            }
            AssemblyBuilderAccess reflectionOnly = save ? (run ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.Save) : AssemblyBuilderAccess.Run;
            if (engine.ReferenceLoaderAPI == LoaderAPI.ReflectionOnlyLoadFrom)
            {
                reflectionOnly = AssemblyBuilderAccess.ReflectionOnly;
            }
            if (globals.engine.genStartupClass)
            {
                this.assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(name, reflectionOnly, dir, globals.engine.Evidence);
            }
            else
            {
                this.assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(name, reflectionOnly, dir);
            }
            if (save)
            {
                this.module = this.assemblyBuilder.DefineDynamicModule("JScript Module", fileName, debugOn);
            }
            else
            {
                this.module = this.assemblyBuilder.DefineDynamicModule("JScript Module", debugOn);
            }
            if (isCLSCompliant)
            {
                this.module.SetCustomAttribute(new CustomAttributeBuilder(clsCompliantAttributeCtor, new object[] { isCLSCompliant }));
            }
            if (debugOn)
            {
                ConstructorInfo constructor = Typeob.DebuggableAttribute.GetConstructor(new Type[] { Typeob.Boolean, Typeob.Boolean });
                this.assemblyBuilder.SetCustomAttribute(new CustomAttributeBuilder(constructor, new object[] { (globals.assemblyFlags & AssemblyFlags.EnableJITcompileTracking) != AssemblyFlags.SideBySideCompatible, (globals.assemblyFlags & AssemblyFlags.DisableJITcompileOptimizer) != AssemblyFlags.SideBySideCompatible }));
            }
            this.compilationEvidence = globals.engine.Evidence;
            this.classwriter = null;
        }

        internal static ConstructorInfo bitwiseBinaryConstructor
        {
            get
            {
                return Globals.TypeRefs.bitwiseBinaryConstructor;
            }
        }

        internal static ConstructorInfo breakOutOfFinallyConstructor
        {
            get
            {
                return Globals.TypeRefs.breakOutOfFinallyConstructor;
            }
        }

        internal static MethodInfo callMethod
        {
            get
            {
                return Globals.TypeRefs.callMethod;
            }
        }

        internal static MethodInfo callValue2Method
        {
            get
            {
                return Globals.TypeRefs.callValue2Method;
            }
        }

        internal static MethodInfo callValueMethod
        {
            get
            {
                return Globals.TypeRefs.callValueMethod;
            }
        }

        internal static MethodInfo changeTypeMethod
        {
            get
            {
                return Globals.TypeRefs.changeTypeMethod;
            }
        }

        internal static MethodInfo checkIfDoubleIsIntegerMethod
        {
            get
            {
                return Globals.TypeRefs.checkIfDoubleIsIntegerMethod;
            }
        }

        internal static MethodInfo checkIfSingleIsIntegerMethod
        {
            get
            {
                return Globals.TypeRefs.checkIfSingleIsIntegerMethod;
            }
        }

        internal static ConstructorInfo closureConstructor
        {
            get
            {
                return Globals.TypeRefs.closureConstructor;
            }
        }

        internal static FieldInfo closureInstanceField
        {
            get
            {
                return Globals.TypeRefs.closureInstanceField;
            }
        }

        internal static ConstructorInfo clsCompliantAttributeCtor
        {
            get
            {
                return Globals.TypeRefs.clsCompliantAttributeCtor;
            }
        }

        internal static MethodInfo coerce2Method
        {
            get
            {
                return Globals.TypeRefs.coerce2Method;
            }
        }

        internal static MethodInfo coerceTMethod
        {
            get
            {
                return Globals.TypeRefs.coerceTMethod;
            }
        }

        internal static ConstructorInfo compilerGlobalScopeAttributeCtor
        {
            get
            {
                return Globals.TypeRefs.compilerGlobalScopeAttributeCtor;
            }
        }

        internal static MethodInfo constructArrayMethod
        {
            get
            {
                return Globals.TypeRefs.constructArrayMethod;
            }
        }

        internal static MethodInfo constructObjectMethod
        {
            get
            {
                return Globals.TypeRefs.constructObjectMethod;
            }
        }

        internal static FieldInfo contextEngineField
        {
            get
            {
                return Globals.TypeRefs.contextEngineField;
            }
        }

        internal static ConstructorInfo contextStaticAttributeCtor
        {
            get
            {
                return Globals.TypeRefs.contextStaticAttributeCtor;
            }
        }

        internal static ConstructorInfo continueOutOfFinallyConstructor
        {
            get
            {
                return Globals.TypeRefs.continueOutOfFinallyConstructor;
            }
        }

        internal static MethodInfo convertCharToStringMethod
        {
            get
            {
                return Globals.TypeRefs.convertCharToStringMethod;
            }
        }

        internal static MethodInfo createVsaEngine
        {
            get
            {
                return Globals.TypeRefs.createVsaEngine;
            }
        }

        internal static MethodInfo createVsaEngineWithType
        {
            get
            {
                return Globals.TypeRefs.createVsaEngineWithType;
            }
        }

        internal static ConstructorInfo dateTimeConstructor
        {
            get
            {
                return Globals.TypeRefs.dateTimeConstructor;
            }
        }

        internal static MethodInfo dateTimeToInt64Method
        {
            get
            {
                return Globals.TypeRefs.dateTimeToInt64Method;
            }
        }

        internal static MethodInfo dateTimeToStringMethod
        {
            get
            {
                return Globals.TypeRefs.dateTimeToStringMethod;
            }
        }

        internal static MethodInfo debugBreak
        {
            get
            {
                return Globals.TypeRefs.debugBreak;
            }
        }

        internal static ConstructorInfo debuggerHiddenAttributeCtor
        {
            get
            {
                return Globals.TypeRefs.debuggerHiddenAttributeCtor;
            }
        }

        internal static ConstructorInfo debuggerStepThroughAttributeCtor
        {
            get
            {
                return Globals.TypeRefs.debuggerStepThroughAttributeCtor;
            }
        }

        internal static MethodInfo decimalCompare
        {
            get
            {
                return Globals.TypeRefs.decimalCompare;
            }
        }

        internal static ConstructorInfo decimalConstructor
        {
            get
            {
                return Globals.TypeRefs.decimalConstructor;
            }
        }

        internal static MethodInfo decimalToDoubleMethod
        {
            get
            {
                return Globals.TypeRefs.decimalToDoubleMethod;
            }
        }

        internal static MethodInfo decimalToInt32Method
        {
            get
            {
                return Globals.TypeRefs.decimalToInt32Method;
            }
        }

        internal static MethodInfo decimalToInt64Method
        {
            get
            {
                return Globals.TypeRefs.decimalToInt64Method;
            }
        }

        internal static MethodInfo decimalToStringMethod
        {
            get
            {
                return Globals.TypeRefs.decimalToStringMethod;
            }
        }

        internal static MethodInfo decimalToUInt32Method
        {
            get
            {
                return Globals.TypeRefs.decimalToUInt32Method;
            }
        }

        internal static MethodInfo decimalToUInt64Method
        {
            get
            {
                return Globals.TypeRefs.decimalToUInt64Method;
            }
        }

        internal static FieldInfo decimalZeroField
        {
            get
            {
                return Globals.TypeRefs.decimalZeroField;
            }
        }

        internal static ConstructorInfo defaultMemberAttributeCtor
        {
            get
            {
                return Globals.TypeRefs.defaultMemberAttributeCtor;
            }
        }

        internal static MethodInfo deleteMemberMethod
        {
            get
            {
                return Globals.TypeRefs.deleteMemberMethod;
            }
        }

        internal static MethodInfo deleteMethod
        {
            get
            {
                return Globals.TypeRefs.deleteMethod;
            }
        }

        internal static MethodInfo doubleToBooleanMethod
        {
            get
            {
                return Globals.TypeRefs.doubleToBooleanMethod;
            }
        }

        internal static MethodInfo doubleToDecimalMethod
        {
            get
            {
                return Globals.TypeRefs.doubleToDecimalMethod;
            }
        }

        internal static MethodInfo doubleToInt64
        {
            get
            {
                return Globals.TypeRefs.doubleToInt64;
            }
        }

        internal static MethodInfo doubleToStringMethod
        {
            get
            {
                return Globals.TypeRefs.doubleToStringMethod;
            }
        }

        internal static FieldInfo engineField
        {
            get
            {
                return Globals.TypeRefs.engineField;
            }
        }

        internal static ConstructorInfo equalityConstructor
        {
            get
            {
                return Globals.TypeRefs.equalityConstructor;
            }
        }

        internal static MethodInfo equalsMethod
        {
            get
            {
                return Globals.TypeRefs.equalsMethod;
            }
        }

        internal static MethodInfo evaluateBitwiseBinaryMethod
        {
            get
            {
                return Globals.TypeRefs.evaluateBitwiseBinaryMethod;
            }
        }

        internal static MethodInfo evaluateEqualityMethod
        {
            get
            {
                return Globals.TypeRefs.evaluateEqualityMethod;
            }
        }

        internal static MethodInfo evaluateNumericBinaryMethod
        {
            get
            {
                return Globals.TypeRefs.evaluateNumericBinaryMethod;
            }
        }

        internal static MethodInfo evaluatePlusMethod
        {
            get
            {
                return Globals.TypeRefs.evaluatePlusMethod;
            }
        }

        internal static MethodInfo evaluatePostOrPrefixOperatorMethod
        {
            get
            {
                return Globals.TypeRefs.evaluatePostOrPrefixOperatorMethod;
            }
        }

        internal static MethodInfo evaluateRelationalMethod
        {
            get
            {
                return Globals.TypeRefs.evaluateRelationalMethod;
            }
        }

        internal static MethodInfo evaluateUnaryMethod
        {
            get
            {
                return Globals.TypeRefs.evaluateUnaryMethod;
            }
        }

        internal static MethodInfo fastConstructArrayLiteralMethod
        {
            get
            {
                return Globals.TypeRefs.fastConstructArrayLiteralMethod;
            }
        }

        internal static MethodInfo getCurrentMethod
        {
            get
            {
                return Globals.TypeRefs.getCurrentMethod;
            }
        }

        internal static MethodInfo getDefaultThisObjectMethod
        {
            get
            {
                return Globals.TypeRefs.getDefaultThisObjectMethod;
            }
        }

        internal static MethodInfo getEngineMethod
        {
            get
            {
                return Globals.TypeRefs.getEngineMethod;
            }
        }

        internal static MethodInfo getEnumeratorMethod
        {
            get
            {
                return Globals.TypeRefs.getEnumeratorMethod;
            }
        }

        internal static MethodInfo getFieldMethod
        {
            get
            {
                return Globals.TypeRefs.getFieldMethod;
            }
        }

        internal static MethodInfo getFieldValueMethod
        {
            get
            {
                return Globals.TypeRefs.getFieldValueMethod;
            }
        }

        internal static MethodInfo getGlobalScopeMethod
        {
            get
            {
                return Globals.TypeRefs.getGlobalScopeMethod;
            }
        }

        internal static MethodInfo getLenientGlobalObjectMethod
        {
            get
            {
                return Globals.TypeRefs.getLenientGlobalObjectMethod;
            }
        }

        internal static MethodInfo getMemberValueMethod
        {
            get
            {
                return Globals.TypeRefs.getMemberValueMethod;
            }
        }

        internal static MethodInfo getMethodMethod
        {
            get
            {
                return Globals.TypeRefs.getMethodMethod;
            }
        }

        internal static MethodInfo getNamespaceMethod
        {
            get
            {
                return Globals.TypeRefs.getNamespaceMethod;
            }
        }

        internal static MethodInfo getNonMissingValueMethod
        {
            get
            {
                return Globals.TypeRefs.getNonMissingValueMethod;
            }
        }

        internal static MethodInfo getOriginalArrayConstructorMethod
        {
            get
            {
                return Globals.TypeRefs.getOriginalArrayConstructorMethod;
            }
        }

        internal static MethodInfo getOriginalObjectConstructorMethod
        {
            get
            {
                return Globals.TypeRefs.getOriginalObjectConstructorMethod;
            }
        }

        internal static MethodInfo getOriginalRegExpConstructorMethod
        {
            get
            {
                return Globals.TypeRefs.getOriginalRegExpConstructorMethod;
            }
        }

        internal static MethodInfo getParentMethod
        {
            get
            {
                return Globals.TypeRefs.getParentMethod;
            }
        }

        internal static MethodInfo getTypeFromHandleMethod
        {
            get
            {
                return Globals.TypeRefs.getTypeFromHandleMethod;
            }
        }

        internal static MethodInfo getTypeMethod
        {
            get
            {
                return Globals.TypeRefs.getTypeMethod;
            }
        }

        internal static MethodInfo getValue2Method
        {
            get
            {
                return Globals.TypeRefs.getValue2Method;
            }
        }

        internal static ConstructorInfo globalScopeConstructor
        {
            get
            {
                return Globals.TypeRefs.globalScopeConstructor;
            }
        }

        internal static ConstructorInfo hashtableCtor
        {
            get
            {
                return Globals.TypeRefs.hashtableCtor;
            }
        }

        internal static MethodInfo hashTableGetEnumerator
        {
            get
            {
                return Globals.TypeRefs.hashTableGetEnumerator;
            }
        }

        internal static MethodInfo hashtableGetItem
        {
            get
            {
                return Globals.TypeRefs.hashtableGetItem;
            }
        }

        internal static MethodInfo hashtableRemove
        {
            get
            {
                return Globals.TypeRefs.hashtableRemove;
            }
        }

        internal static MethodInfo hashtableSetItem
        {
            get
            {
                return Globals.TypeRefs.hashtableSetItem;
            }
        }

        internal static MethodInfo int32ToDecimalMethod
        {
            get
            {
                return Globals.TypeRefs.int32ToDecimalMethod;
            }
        }

        internal static MethodInfo int32ToStringMethod
        {
            get
            {
                return Globals.TypeRefs.int32ToStringMethod;
            }
        }

        internal static MethodInfo int64ToDecimalMethod
        {
            get
            {
                return Globals.TypeRefs.int64ToDecimalMethod;
            }
        }

        internal static MethodInfo int64ToStringMethod
        {
            get
            {
                return Globals.TypeRefs.int64ToStringMethod;
            }
        }

        internal static MethodInfo isMissingMethod
        {
            get
            {
                return Globals.TypeRefs.isMissingMethod;
            }
        }

        internal static MethodInfo jScriptCompareMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptCompareMethod;
            }
        }

        internal static MethodInfo jScriptEqualsMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptEqualsMethod;
            }
        }

        internal static MethodInfo jScriptEvaluateMethod1
        {
            get
            {
                return Globals.TypeRefs.jScriptEvaluateMethod1;
            }
        }

        internal static MethodInfo jScriptEvaluateMethod2
        {
            get
            {
                return Globals.TypeRefs.jScriptEvaluateMethod2;
            }
        }

        internal static MethodInfo jScriptExceptionValueMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptExceptionValueMethod;
            }
        }

        internal static MethodInfo jScriptFunctionDeclarationMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptFunctionDeclarationMethod;
            }
        }

        internal static MethodInfo jScriptFunctionExpressionMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptFunctionExpressionMethod;
            }
        }

        internal static MethodInfo jScriptGetEnumeratorMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptGetEnumeratorMethod;
            }
        }

        internal static MethodInfo jScriptImportMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptImportMethod;
            }
        }

        internal static MethodInfo jScriptInMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptInMethod;
            }
        }

        internal static MethodInfo jScriptInstanceofMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptInstanceofMethod;
            }
        }

        internal static MethodInfo jScriptPackageMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptPackageMethod;
            }
        }

        internal static MethodInfo jScriptStrictEqualsMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptStrictEqualsMethod;
            }
        }

        internal static MethodInfo jScriptThrowMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptThrowMethod;
            }
        }

        internal static MethodInfo jScriptTypeofMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptTypeofMethod;
            }
        }

        internal static MethodInfo jScriptWithMethod
        {
            get
            {
                return Globals.TypeRefs.jScriptWithMethod;
            }
        }

        internal static ConstructorInfo jsFunctionAttributeConstructor
        {
            get
            {
                return Globals.TypeRefs.jsFunctionAttributeConstructor;
            }
        }

        internal static ConstructorInfo jsLocalFieldConstructor
        {
            get
            {
                return Globals.TypeRefs.jsLocalFieldConstructor;
            }
        }

        internal static ConstructorInfo lateBindingConstructor
        {
            get
            {
                return Globals.TypeRefs.lateBindingConstructor;
            }
        }

        internal static ConstructorInfo lateBindingConstructor2
        {
            get
            {
                return Globals.TypeRefs.lateBindingConstructor2;
            }
        }

        internal static FieldInfo localVarsField
        {
            get
            {
                return Globals.TypeRefs.localVarsField;
            }
        }

        internal static FieldInfo missingField
        {
            get
            {
                return Globals.TypeRefs.missingField;
            }
        }

        internal static MethodInfo moveNextMethod
        {
            get
            {
                return Globals.TypeRefs.moveNextMethod;
            }
        }

        internal static ConstructorInfo numericBinaryConstructor
        {
            get
            {
                return Globals.TypeRefs.numericBinaryConstructor;
            }
        }

        internal static MethodInfo numericbinaryDoOpMethod
        {
            get
            {
                return Globals.TypeRefs.numericbinaryDoOpMethod;
            }
        }

        internal static ConstructorInfo numericUnaryConstructor
        {
            get
            {
                return Globals.TypeRefs.numericUnaryConstructor;
            }
        }

        internal static FieldInfo objectField
        {
            get
            {
                return Globals.TypeRefs.objectField;
            }
        }

        internal static ConstructorInfo plusConstructor
        {
            get
            {
                return Globals.TypeRefs.plusConstructor;
            }
        }

        internal static MethodInfo plusDoOpMethod
        {
            get
            {
                return Globals.TypeRefs.plusDoOpMethod;
            }
        }

        internal static MethodInfo popScriptObjectMethod
        {
            get
            {
                return Globals.TypeRefs.popScriptObjectMethod;
            }
        }

        internal static ConstructorInfo postOrPrefixConstructor
        {
            get
            {
                return Globals.TypeRefs.postOrPrefixConstructor;
            }
        }

        internal static MethodInfo pushScriptObjectMethod
        {
            get
            {
                return Globals.TypeRefs.pushScriptObjectMethod;
            }
        }

        internal static MethodInfo pushStackFrameForMethod
        {
            get
            {
                return Globals.TypeRefs.pushStackFrameForMethod;
            }
        }

        internal static MethodInfo pushStackFrameForStaticMethod
        {
            get
            {
                return Globals.TypeRefs.pushStackFrameForStaticMethod;
            }
        }

        internal static ConstructorInfo referenceAttributeConstructor
        {
            get
            {
                return Globals.TypeRefs.referenceAttributeConstructor;
            }
        }

        internal static MethodInfo regExpConstructMethod
        {
            get
            {
                return Globals.TypeRefs.regExpConstructMethod;
            }
        }

        internal static ConstructorInfo relationalConstructor
        {
            get
            {
                return Globals.TypeRefs.relationalConstructor;
            }
        }

        internal static ConstructorInfo returnOutOfFinallyConstructor
        {
            get
            {
                return Globals.TypeRefs.returnOutOfFinallyConstructor;
            }
        }

        internal static ConstructorInfo scriptExceptionConstructor
        {
            get
            {
                return Globals.TypeRefs.scriptExceptionConstructor;
            }
        }

        internal static MethodInfo scriptObjectStackTopMethod
        {
            get
            {
                return Globals.TypeRefs.scriptObjectStackTopMethod;
            }
        }

        internal static MethodInfo setEngineMethod
        {
            get
            {
                return Globals.TypeRefs.setEngineMethod;
            }
        }

        internal static MethodInfo setFieldValueMethod
        {
            get
            {
                return Globals.TypeRefs.setFieldValueMethod;
            }
        }

        internal static MethodInfo setIndexedPropertyValueStaticMethod
        {
            get
            {
                return Globals.TypeRefs.setIndexedPropertyValueStaticMethod;
            }
        }

        internal static MethodInfo setMemberValue2Method
        {
            get
            {
                return Globals.TypeRefs.setMemberValue2Method;
            }
        }

        internal static MethodInfo setValueMethod
        {
            get
            {
                return Globals.TypeRefs.setValueMethod;
            }
        }

        internal static MethodInfo stringConcat2Method
        {
            get
            {
                return Globals.TypeRefs.stringConcat2Method;
            }
        }

        internal static MethodInfo stringConcat3Method
        {
            get
            {
                return Globals.TypeRefs.stringConcat3Method;
            }
        }

        internal static MethodInfo stringConcat4Method
        {
            get
            {
                return Globals.TypeRefs.stringConcat4Method;
            }
        }

        internal static MethodInfo stringConcatArrMethod
        {
            get
            {
                return Globals.TypeRefs.stringConcatArrMethod;
            }
        }

        internal static MethodInfo stringEqualsMethod
        {
            get
            {
                return Globals.TypeRefs.stringEqualsMethod;
            }
        }

        internal static MethodInfo stringLengthMethod
        {
            get
            {
                return Globals.TypeRefs.stringLengthMethod;
            }
        }

        internal static FieldInfo systemReflectionMissingField
        {
            get
            {
                return Globals.TypeRefs.systemReflectionMissingField;
            }
        }

        internal static MethodInfo throwTypeMismatch
        {
            get
            {
                return Globals.TypeRefs.throwTypeMismatch;
            }
        }

        internal static MethodInfo toBooleanMethod
        {
            get
            {
                return Globals.TypeRefs.toBooleanMethod;
            }
        }

        internal static MethodInfo toForInObjectMethod
        {
            get
            {
                return Globals.TypeRefs.toForInObjectMethod;
            }
        }

        internal static MethodInfo toInt32Method
        {
            get
            {
                return Globals.TypeRefs.toInt32Method;
            }
        }

        internal static MethodInfo toNativeArrayMethod
        {
            get
            {
                return Globals.TypeRefs.toNativeArrayMethod;
            }
        }

        internal static MethodInfo toNumberMethod
        {
            get
            {
                return Globals.TypeRefs.toNumberMethod;
            }
        }

        internal static MethodInfo toObject2Method
        {
            get
            {
                return Globals.TypeRefs.toObject2Method;
            }
        }

        internal static MethodInfo toObjectMethod
        {
            get
            {
                return Globals.TypeRefs.toObjectMethod;
            }
        }

        internal static MethodInfo toStringMethod
        {
            get
            {
                return Globals.TypeRefs.toStringMethod;
            }
        }

        internal static MethodInfo uint32ToDecimalMethod
        {
            get
            {
                return Globals.TypeRefs.uint32ToDecimalMethod;
            }
        }

        internal static MethodInfo uint32ToStringMethod
        {
            get
            {
                return Globals.TypeRefs.uint32ToStringMethod;
            }
        }

        internal static MethodInfo uint64ToDecimalMethod
        {
            get
            {
                return Globals.TypeRefs.uint64ToDecimalMethod;
            }
        }

        internal static MethodInfo uint64ToStringMethod
        {
            get
            {
                return Globals.TypeRefs.uint64ToStringMethod;
            }
        }

        internal static MethodInfo uncheckedDecimalToInt64Method
        {
            get
            {
                return Globals.TypeRefs.uncheckedDecimalToInt64Method;
            }
        }

        internal static FieldInfo undefinedField
        {
            get
            {
                return Globals.TypeRefs.undefinedField;
            }
        }

        internal static ConstructorInfo vsaEngineConstructor
        {
            get
            {
                return Globals.TypeRefs.vsaEngineConstructor;
            }
        }

        internal static MethodInfo writeLineMethod
        {
            get
            {
                return Globals.TypeRefs.writeLineMethod;
            }
        }

        internal static MethodInfo writeMethod
        {
            get
            {
                return Globals.TypeRefs.writeMethod;
            }
        }
    }
}


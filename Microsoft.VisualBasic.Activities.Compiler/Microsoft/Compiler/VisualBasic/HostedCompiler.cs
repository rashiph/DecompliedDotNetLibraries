namespace Microsoft.Compiler.VisualBasic
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;

    internal sealed class HostedCompiler : IDisposable
    {
        private unsafe CompilerBridge* m_pCompilerBridge;

        private unsafe void !HostedCompiler()
        {
            CompilerBridge* pCompilerBridge = this.m_pCompilerBridge;
            if (pCompilerBridge != null)
            {
                CompilerBridge* bridgePtr = pCompilerBridge;
                **(*((int*) bridgePtr))(bridgePtr, 1);
            }
            this.m_pCompilerBridge = null;
        }

        public unsafe HostedCompiler(IList<Assembly> referenceAssemblies)
        {
            CompilerBridge* modopt(IsConst) modopt(IsConst) bridgePtr2;
            if (referenceAssemblies == null)
            {
                referenceAssemblies = new List<Assembly>();
            }
            CompilerBridge* bridgePtr = @new(0x198);
            try
            {
                if (bridgePtr != null)
                {
                    gcroot<System::Collections::Generic::IList<System::Reflection::Assembly ^> ^> local;
                    gcroot<System::Collections::Generic::IList<System::Reflection::Assembly ^> ^>* localPtr = &local;
                    *((int*) &local) = ((IntPtr) GCHandle.Alloc(referenceAssemblies)).ToPointer();
                    bridgePtr2 = Microsoft.Compiler.VisualBasic.CompilerBridge.{ctor}(bridgePtr, (gcroot<System::Collections::Generic::IList<System::Reflection::Assembly ^> ^> modreq(IsCopyConstructed)*) &local);
                }
                else
                {
                    bridgePtr2 = 0;
                }
            }
            fault
            {
                delete((void*) bridgePtr);
            }
            this.m_pCompilerBridge = bridgePtr2;
        }

        private unsafe void ~HostedCompiler()
        {
            CompilerBridge* pCompilerBridge = this.m_pCompilerBridge;
            if (pCompilerBridge != null)
            {
                CompilerBridge* bridgePtr = pCompilerBridge;
                **(*((int*) bridgePtr))(bridgePtr, 1);
            }
            this.m_pCompilerBridge = null;
        }

        public unsafe void CheckInvalid()
        {
            if (this.m_pCompilerBridge == null)
            {
                throw new InvalidOperationException();
            }
        }

        public CompilerResults CompileExpression(string expression, CompilerContext context)
        {
            return this.CompileExpression(expression, context, null);
        }

        public unsafe CompilerResults CompileExpression(string expression, CompilerContext context, Type targetType)
        {
            if (this.m_pCompilerBridge == null)
            {
                throw new InvalidOperationException();
            }
            if ((targetType != null) && typeof(void).Equals(targetType))
            {
                throw new ArgumentException("", "targetType");
            }
            CompilerResults results = new CompilerResults();
            if (!string.IsNullOrEmpty(expression))
            {
                CompilerBridge* pCompilerBridge;
                gcroot<System::String ^> local;
                gcroot<Microsoft::Compiler::VisualBasic::CompilerContext ^> local2;
                gcroot<System::Type ^> local3;
                gcroot<Microsoft::Compiler::VisualBasic::CompilerResults ^> local4;
                if (context == null)
                {
                    context = CompilerContext.Empty;
                }
                gcroot<Microsoft::Compiler::VisualBasic::CompilerResults ^>* localPtr4 = &local4;
                *((int*) &local4) = ((IntPtr) GCHandle.Alloc(results)).ToPointer();
                try
                {
                    gcroot<System::Type ^>* localPtr3 = &local3;
                    *((int*) &local3) = ((IntPtr) GCHandle.Alloc(targetType)).ToPointer();
                    try
                    {
                        gcroot<Microsoft::Compiler::VisualBasic::CompilerContext ^>* localPtr2 = &local2;
                        *((int*) &local2) = ((IntPtr) GCHandle.Alloc(context)).ToPointer();
                        try
                        {
                            gcroot<System::String ^>* localPtr = &local;
                            *((int*) &local) = ((IntPtr) GCHandle.Alloc(expression)).ToPointer();
                            try
                            {
                                pCompilerBridge = this.m_pCompilerBridge;
                            }
                            fault
                            {
                                ___CxxCallUnwindDtor(gcroot<System::String ^>.{dtor}, (void*) localPtr);
                            }
                        }
                        fault
                        {
                            ___CxxCallUnwindDtor(gcroot<Microsoft::Compiler::VisualBasic::CompilerContext ^>.{dtor}, (void*) localPtr2);
                        }
                    }
                    fault
                    {
                        ___CxxCallUnwindDtor(gcroot<System::Type ^>.{dtor}, (void*) localPtr3);
                    }
                }
                fault
                {
                    ___CxxCallUnwindDtor(gcroot<Microsoft::Compiler::VisualBasic::CompilerResults ^>.{dtor}, (void*) localPtr4);
                }
                Microsoft.Compiler.VisualBasic.CompilerBridge.CompileExpression(pCompilerBridge, (gcroot<System::String ^> modreq(IsCopyConstructed)*) &local, (gcroot<Microsoft::Compiler::VisualBasic::CompilerContext ^> modreq(IsCopyConstructed)*) &local2, (gcroot<System::Type ^> modreq(IsCopyConstructed)*) &local3, (gcroot<Microsoft::Compiler::VisualBasic::CompilerResults ^> modreq(IsCopyConstructed)*) &local4);
            }
            return results;
        }

        public sealed override void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [HandleProcessCorruptedStateExceptions]
        protected void Dispose([MarshalAs(UnmanagedType.U1)] bool flag1)
        {
            if (flag1)
            {
                this.~HostedCompiler();
            }
            else
            {
                try
                {
                    this.!HostedCompiler();
                }
                finally
                {
                    base.Finalize();
                }
            }
        }

        protected sealed override void Finalize()
        {
            this.Dispose(false);
        }
    }
}


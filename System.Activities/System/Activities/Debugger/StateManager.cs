namespace System.Activities.Debugger
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.SymbolStore;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;

    [DebuggerNonUserCode]
    public sealed class StateManager : IDisposable
    {
        private static readonly LocalsItemDescription[] debugInfoDescriptions = new LocalsItemDescription[] { new LocalsItemDescription("debugInfo", typeof(DebugInfo)) };
        private bool debugStartedAtRoot;
        private ModuleBuilder dynamicModule;
        private int indexLastBaked;
        private Dictionary<System.Activities.Debugger.State, MethodInfo> islands;
        private Dictionary<System.Activities.Debugger.State, MethodInfo> islandsWithPriming;
        private static MethodInfo islandWorkerMethodInfo = threadWorkerControllerType.GetMethod("IslandWorker", BindingFlags.Public | BindingFlags.Static);
        private const string Md5Identifier = "406ea660-64cf-4c82-b6f0-42d48172a799";
        internal const string MethodWithPrimingPrefix = "_";
        private Properties properties;
        private Dictionary<string, ISymbolDocumentWriter> sourceDocuments;
        private List<System.Activities.Debugger.State> states;
        private List<LogicalThread> threads;
        private static Type threadWorkerControllerType = typeof(ThreadWorkerController);
        private static readonly Guid WorkflowLanguageGuid = new Guid("1F1149BB-9732-4EB8-9ED4-FA738768919C");

        internal StateManager() : this(new Properties(), true)
        {
        }

        internal StateManager(Properties properties, bool debugStartedAtRoot)
        {
            this.properties = properties;
            this.threads = new List<LogicalThread>();
            this.states = new List<System.Activities.Debugger.State>();
            this.islands = new Dictionary<System.Activities.Debugger.State, MethodInfo>();
            this.debugStartedAtRoot = debugStartedAtRoot;
            if (!this.debugStartedAtRoot)
            {
                this.islandsWithPriming = new Dictionary<System.Activities.Debugger.State, MethodInfo>();
            }
            this.sourceDocuments = new Dictionary<string, ISymbolDocumentWriter>();
            this.InitDynamicModule(this.properties.ModuleNamePrefix);
        }

        internal void Bake()
        {
            this.Bake(this.properties.TypeNamePrefix);
        }

        internal void Bake(string typeName)
        {
            for (int i = 1; this.dynamicModule.GetType(typeName) != null; i++)
            {
                typeName = this.properties.TypeNamePrefix + "_" + i.ToString(CultureInfo.InvariantCulture);
            }
            TypeBuilder typeBuilder = this.dynamicModule.DefineType(typeName, TypeAttributes.Public);
            for (int j = this.indexLastBaked; j < this.states.Count; j++)
            {
                MethodBuilder builder2 = this.CreateIsland(typeBuilder, this.states[j], false);
                if (!this.DebugStartedAtRoot)
                {
                    this.CreateIsland(typeBuilder, this.states[j], true);
                }
                this.states[j].CacheMethodInfo(typeBuilder, builder2.Name);
            }
            typeBuilder.CreateType();
            this.indexLastBaked = this.states.Count;
        }

        private MethodBuilder CreateIsland(TypeBuilder typeBuilder, System.Activities.Debugger.State state, bool withPrimingTest)
        {
            MethodBuilder builder = this.CreateMethodBuilder(typeBuilder, threadWorkerControllerType, state, withPrimingTest);
            ILGenerator iLGenerator = builder.GetILGenerator();
            SourceLocation location = state.Location;
            ISymbolDocumentWriter sourceDocument = this.GetSourceDocument(location.FileName);
            Label label = iLGenerator.DefineLabel();
            if (withPrimingTest)
            {
                iLGenerator.MarkSequencePoint(sourceDocument, 0xfeefee, 1, 0xfeefee, 100);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Brtrue, label);
            }
            iLGenerator.MarkSequencePoint(sourceDocument, location.StartLine, location.StartColumn, location.EndLine, location.EndColumn);
            iLGenerator.Emit(OpCodes.Nop);
            iLGenerator.MarkLabel(label);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.EmitCall(OpCodes.Call, islandWorkerMethodInfo, null);
            iLGenerator.Emit(OpCodes.Ret);
            return builder;
        }

        internal int CreateLogicalThread(string threadName)
        {
            int threadId = -1;
            for (int i = 1; i < this.threads.Count; i++)
            {
                if (this.threads[i] == null)
                {
                    this.threads[i] = new LogicalThread(i, threadName, this);
                    threadId = i;
                    break;
                }
            }
            if (threadId < 0)
            {
                threadId = this.threads.Count;
                this.threads.Add(new LogicalThread(threadId, threadName, this));
            }
            return threadId;
        }

        internal MethodBuilder CreateMethodBuilder(TypeBuilder typeBuilder, Type typeIslandArguments, System.Activities.Debugger.State state, bool withPriming)
        {
            string name = (state.Name != null) ? state.Name : ("Line_" + state.Location.StartLine);
            if (withPriming)
            {
                name = "_" + name;
            }
            IEnumerable<LocalsItemDescription> earlyLocals = state.EarlyLocals;
            int numberOfEarlyLocals = state.NumberOfEarlyLocals;
            Type[] parameterTypes = new Type[2 + numberOfEarlyLocals];
            parameterTypes[0] = typeof(bool);
            parameterTypes[1] = typeIslandArguments;
            if (numberOfEarlyLocals > 0)
            {
                int index = 2;
                foreach (LocalsItemDescription description in earlyLocals)
                {
                    parameterTypes[index] = description.Type;
                    index++;
                }
            }
            Type returnType = typeof(void);
            MethodBuilder builder = typeBuilder.DefineMethod(name, MethodAttributes.Static | MethodAttributes.Public, returnType, parameterTypes);
            builder.DefineParameter(1, ParameterAttributes.None, "isPriming");
            builder.DefineParameter(2, ParameterAttributes.None, "typeIslandArguments");
            if (numberOfEarlyLocals > 0)
            {
                int position = 3;
                foreach (LocalsItemDescription description2 in earlyLocals)
                {
                    builder.DefineParameter(position, ParameterAttributes.None, description2.Name);
                    position++;
                }
            }
            return builder;
        }

        internal System.Activities.Debugger.State DefineState(SourceLocation location)
        {
            return this.DefineState(location, string.Empty, null, 0);
        }

        internal System.Activities.Debugger.State DefineState(SourceLocation location, string name)
        {
            return this.DefineState(location, name, null, 0);
        }

        internal System.Activities.Debugger.State DefineState(SourceLocation location, string name, LocalsItemDescription[] earlyLocals, int numberOfEarlyLocals)
        {
            System.Activities.Debugger.State item = new System.Activities.Debugger.State(location, name, earlyLocals, numberOfEarlyLocals);
            this.states.Add(item);
            return item;
        }

        internal System.Activities.Debugger.State DefineStateWithDebugInfo(SourceLocation location, string name)
        {
            return this.DefineState(location, name, debugInfoDescriptions, debugInfoDescriptions.Length);
        }

        public void Dispose()
        {
            foreach (LogicalThread thread in this.threads)
            {
                if (thread != null)
                {
                    thread.Exit();
                }
            }
            this.threads.Clear();
        }

        internal void EnterState(int threadIndex, VirtualStackFrame stackFrame)
        {
            int indexLastBaked = this.indexLastBaked;
            int count = this.states.Count;
            this.threads[threadIndex].EnterState(stackFrame);
        }

        internal void EnterState(int threadIndex, System.Activities.Debugger.State state, IDictionary<string, object> locals)
        {
            this.EnterState(threadIndex, new VirtualStackFrame(state, locals));
        }

        public void Exit(int threadIndex)
        {
            this.threads[threadIndex].Exit();
            this.threads[threadIndex] = null;
        }

        private MethodInfo GetIsland(System.Activities.Debugger.State state)
        {
            MethodInfo methodInfo = null;
            if (this.IsPriming)
            {
                if (!this.islandsWithPriming.TryGetValue(state, out methodInfo))
                {
                    methodInfo = state.GetMethodInfo(true);
                    this.islandsWithPriming[state] = methodInfo;
                }
                return methodInfo;
            }
            if (!this.islands.TryGetValue(state, out methodInfo))
            {
                methodInfo = state.GetMethodInfo(false);
                this.islands[state] = methodInfo;
            }
            return methodInfo;
        }

        internal ISymbolDocumentWriter GetSourceDocument(string fileName)
        {
            ISymbolDocumentWriter writer;
            if (!this.sourceDocuments.TryGetValue(fileName, out writer))
            {
                writer = this.dynamicModule.DefineDocument(fileName, WorkflowLanguageGuid, SymLanguageVendor.Microsoft, SymDocumentType.Text);
                this.sourceDocuments.Add(fileName, writer);
                MD5 md = new MD5CryptoServiceProvider();
                byte[] checkSum = null;
                using (StreamReader reader = new StreamReader(fileName))
                {
                    checkSum = md.ComputeHash(reader.BaseStream);
                }
                if (checkSum != null)
                {
                    writer.SetCheckSum(new Guid("406ea660-64cf-4c82-b6f0-42d48172a799"), checkSum);
                }
            }
            return writer;
        }

        private void InitDynamicModule(string asmName)
        {
            AssemblyName name = new AssemblyName {
                Name = asmName
            };
            AssemblyBuilder builder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave, (string) null);
            Type type = typeof(DebuggableAttribute);
            CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(type.GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) }), new object[] { DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.Default });
            builder.SetCustomAttribute(customBuilder);
            this.dynamicModule = builder.DefineDynamicModule(asmName, true);
        }

        internal void InvokeWorker(object islandArguments, VirtualStackFrame stackFrame)
        {
            System.Activities.Debugger.State state = stackFrame.State;
            MethodInfo island = this.GetIsland(state);
            IDictionary<string, object> locals = stackFrame.Locals;
            int numberOfEarlyLocals = state.NumberOfEarlyLocals;
            object[] parameters = new object[2 + numberOfEarlyLocals];
            parameters[0] = this.IsPriming;
            parameters[1] = islandArguments;
            if (numberOfEarlyLocals > 0)
            {
                int index = 2;
                foreach (LocalsItemDescription description in state.EarlyLocals)
                {
                    object obj2;
                    string name = description.Name;
                    if (!locals.TryGetValue(name, out obj2))
                    {
                        obj2 = Activator.CreateInstance(description.Type);
                    }
                    parameters[index] = obj2;
                    index++;
                }
            }
            island.Invoke(null, parameters);
        }

        internal void LeaveState(int threadIndex, System.Activities.Debugger.State state)
        {
            this.threads[threadIndex].LeaveState(state);
        }

        internal bool DebugStartedAtRoot
        {
            get
            {
                return this.debugStartedAtRoot;
            }
        }

        internal bool IsPriming { get; set; }

        internal Properties ManagerProperties
        {
            get
            {
                return this.properties;
            }
        }

        [DebuggerNonUserCode]
        private class LogicalThread
        {
            private Stack<VirtualStackFrame> callStack;
            private ThreadWorkerController controller;
            private int threadId;

            public LogicalThread(int threadId, string threadName, StateManager stateManager)
            {
                this.threadId = threadId;
                this.callStack = new Stack<VirtualStackFrame>();
                this.controller = new ThreadWorkerController();
                this.controller.Initialize(threadName + "." + threadId.ToString(CultureInfo.InvariantCulture), stateManager);
            }

            public void EnterState(VirtualStackFrame stackFrame)
            {
                if ((stackFrame != null) && (stackFrame.State != null))
                {
                    this.callStack.Push(stackFrame);
                    this.controller.EnterState(stackFrame);
                }
                else
                {
                    this.callStack.Push(null);
                }
            }

            public void Exit()
            {
                this.UnwindCallStack();
                this.controller.Exit();
            }

            public void LeaveState(System.Activities.Debugger.State state)
            {
                if ((this.callStack.Count > 0) && (this.callStack.Pop() != null))
                {
                    this.controller.LeaveState();
                }
            }

            private void UnwindCallStack()
            {
                while (this.callStack.Count > 0)
                {
                    this.LeaveState(this.callStack.Peek().State);
                }
            }
        }

        [DebuggerNonUserCode]
        internal class Properties
        {
            public Properties() : this("Locals", "Script", "States", "WorkflowDebuggerThread", true)
            {
            }

            public Properties(string defaultLocalsName, string moduleNamePrefix, string typeNamePrefix, string auxiliaryThreadName, bool breakOnStartup)
            {
                this.DefaultLocalsName = defaultLocalsName;
                this.ModuleNamePrefix = moduleNamePrefix;
                this.TypeNamePrefix = typeNamePrefix;
                this.AuxiliaryThreadName = auxiliaryThreadName;
                this.BreakOnStartup = breakOnStartup;
            }

            public string AuxiliaryThreadName { get; set; }

            public bool BreakOnStartup { get; set; }

            public string DefaultLocalsName { get; set; }

            public string ModuleNamePrefix { get; set; }

            public string TypeNamePrefix { get; set; }
        }
    }
}


namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Security;

    internal sealed class BinaryMethodCall
    {
        private object[] args;
        private Type[] argTypes;
        private bool bArgsPrimitive = true;
        private object[] callA;
        private object callContext;
        private Type[] instArgs;
        private MessageEnum messageEnum;
        private string methodName;
        private object methodSignature;
        private object properties;
        private string scallContext;
        private string typeName;
        private string uri;

        internal void Dump()
        {
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ContextInline))
                {
                    string callContext = this.callContext as string;
                }
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ArgsInline))
                {
                    for (int i = 0; i < this.args.Length; i++)
                    {
                    }
                }
            }
        }

        [SecurityCritical]
        internal void Read(__BinaryParser input)
        {
            this.messageEnum = (MessageEnum) input.ReadInt32();
            this.methodName = (string) IOUtil.ReadWithCode(input);
            this.typeName = (string) IOUtil.ReadWithCode(input);
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ContextInline))
            {
                this.scallContext = (string) IOUtil.ReadWithCode(input);
                LogicalCallContext context = new LogicalCallContext {
                    RemotingData = { LogicalCallID = this.scallContext }
                };
                this.callContext = context;
            }
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ArgsInline))
            {
                this.args = IOUtil.ReadArgs(input);
            }
        }

        [SecurityCritical]
        internal IMethodCallMessage ReadArray(object[] callA, object handlerObject)
        {
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ArgsIsArray))
            {
                this.args = callA;
            }
            else
            {
                int num = 0;
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ArgsInArray))
                {
                    if (callA.Length < num)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    }
                    this.args = (object[]) callA[num++];
                }
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.GenericMethod))
                {
                    if (callA.Length < num)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    }
                    this.instArgs = (Type[]) callA[num++];
                }
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.MethodSignatureInArray))
                {
                    if (callA.Length < num)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    }
                    this.methodSignature = callA[num++];
                }
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ContextInArray))
                {
                    if (callA.Length < num)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    }
                    this.callContext = callA[num++];
                }
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.PropertyInArray))
                {
                    if (callA.Length < num)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    }
                    this.properties = callA[num++];
                }
            }
            return new MethodCall(handlerObject, new BinaryMethodCallMessage(this.uri, this.methodName, this.typeName, this.instArgs, this.args, this.methodSignature, (LogicalCallContext) this.callContext, (object[]) this.properties));
        }

        internal void Write(__BinaryWriter sout)
        {
            sout.WriteByte(0x15);
            sout.WriteInt32((int) this.messageEnum);
            IOUtil.WriteStringWithCode(this.methodName, sout);
            IOUtil.WriteStringWithCode(this.typeName, sout);
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ContextInline))
            {
                IOUtil.WriteStringWithCode((string) this.callContext, sout);
            }
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ArgsInline))
            {
                sout.WriteInt32(this.args.Length);
                for (int i = 0; i < this.args.Length; i++)
                {
                    IOUtil.WriteWithCode(this.argTypes[i], this.args[i], sout);
                }
            }
        }

        internal object[] WriteArray(string uri, string methodName, string typeName, Type[] instArgs, object[] args, object methodSignature, object callContext, object[] properties)
        {
            this.uri = uri;
            this.methodName = methodName;
            this.typeName = typeName;
            this.instArgs = instArgs;
            this.args = args;
            this.methodSignature = methodSignature;
            this.callContext = callContext;
            this.properties = properties;
            int num = 0;
            if ((args == null) || (args.Length == 0))
            {
                this.messageEnum = MessageEnum.NoArgs;
            }
            else
            {
                this.argTypes = new Type[args.Length];
                this.bArgsPrimitive = true;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] != null)
                    {
                        this.argTypes[i] = args[i].GetType();
                        if ((!this.argTypes[i].IsPrimitive && !object.ReferenceEquals(this.argTypes[i], Converter.typeofString)) || (args[i] is ISerializable))
                        {
                            this.bArgsPrimitive = false;
                            break;
                        }
                    }
                }
                if (this.bArgsPrimitive)
                {
                    this.messageEnum = MessageEnum.ArgsInline;
                }
                else
                {
                    num++;
                    this.messageEnum = MessageEnum.ArgsInArray;
                }
            }
            if (instArgs != null)
            {
                num++;
                this.messageEnum |= MessageEnum.GenericMethod;
            }
            if (methodSignature != null)
            {
                num++;
                this.messageEnum |= MessageEnum.MethodSignatureInArray;
            }
            if (callContext == null)
            {
                this.messageEnum |= MessageEnum.NoContext;
            }
            else if (callContext is string)
            {
                this.messageEnum |= MessageEnum.ContextInline;
            }
            else
            {
                num++;
                this.messageEnum |= MessageEnum.ContextInArray;
            }
            if (properties != null)
            {
                num++;
                this.messageEnum |= MessageEnum.PropertyInArray;
            }
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ArgsInArray) && (num == 1))
            {
                this.messageEnum ^= MessageEnum.ArgsInArray;
                this.messageEnum |= MessageEnum.ArgsIsArray;
                return args;
            }
            if (num <= 0)
            {
                return null;
            }
            int index = 0;
            this.callA = new object[num];
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ArgsInArray))
            {
                this.callA[index++] = args;
            }
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.GenericMethod))
            {
                this.callA[index++] = instArgs;
            }
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.MethodSignatureInArray))
            {
                this.callA[index++] = methodSignature;
            }
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ContextInArray))
            {
                this.callA[index++] = callContext;
            }
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.PropertyInArray))
            {
                this.callA[index] = properties;
            }
            return this.callA;
        }
    }
}


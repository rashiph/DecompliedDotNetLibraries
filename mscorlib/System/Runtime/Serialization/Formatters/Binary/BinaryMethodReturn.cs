namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Security;

    internal sealed class BinaryMethodReturn : IStreamable
    {
        private object[] args;
        private Type[] argTypes;
        private bool bArgsPrimitive = true;
        private object[] callA;
        private object callContext;
        private Exception exception;
        private static object instanceOfVoid = FormatterServices.GetUninitializedObject(Converter.typeofSystemVoid);
        private MessageEnum messageEnum;
        private object properties;
        private Type returnType;
        private object returnValue;
        private string scallContext;

        internal BinaryMethodReturn()
        {
        }

        public void Dump()
        {
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                IOUtil.FlagTest(this.messageEnum, MessageEnum.ReturnValueInline);
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
        public void Read(__BinaryParser input)
        {
            this.messageEnum = (MessageEnum) input.ReadInt32();
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.NoReturnValue))
            {
                this.returnValue = null;
            }
            else if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ReturnValueVoid))
            {
                this.returnValue = instanceOfVoid;
            }
            else if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ReturnValueInline))
            {
                this.returnValue = IOUtil.ReadWithCode(input);
            }
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
        internal IMethodReturnMessage ReadArray(object[] returnA, IMethodCallMessage methodCallMessage, object handlerObject)
        {
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ArgsIsArray))
            {
                this.args = returnA;
            }
            else
            {
                int num = 0;
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ArgsInArray))
                {
                    if (returnA.Length < num)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    }
                    this.args = (object[]) returnA[num++];
                }
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ReturnValueInArray))
                {
                    if (returnA.Length < num)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    }
                    this.returnValue = returnA[num++];
                }
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ExceptionInArray))
                {
                    if (returnA.Length < num)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    }
                    this.exception = (Exception) returnA[num++];
                }
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ContextInArray))
                {
                    if (returnA.Length < num)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    }
                    this.callContext = returnA[num++];
                }
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.PropertyInArray))
                {
                    if (returnA.Length < num)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    }
                    this.properties = returnA[num++];
                }
            }
            return new MethodResponse(methodCallMessage, handlerObject, new BinaryMethodReturnMessage(this.returnValue, this.args, this.exception, (LogicalCallContext) this.callContext, (object[]) this.properties));
        }

        public void Write(__BinaryWriter sout)
        {
            sout.WriteByte(0x16);
            sout.WriteInt32((int) this.messageEnum);
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ReturnValueInline))
            {
                IOUtil.WriteWithCode(this.returnType, this.returnValue, sout);
            }
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

        internal object[] WriteArray(object returnValue, object[] args, Exception exception, object callContext, object[] properties)
        {
            this.returnValue = returnValue;
            this.args = args;
            this.exception = exception;
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
                        if (!this.argTypes[i].IsPrimitive && !object.ReferenceEquals(this.argTypes[i], Converter.typeofString))
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
            if (returnValue == null)
            {
                this.messageEnum |= MessageEnum.NoReturnValue;
            }
            else if (returnValue.GetType() == typeof(void))
            {
                this.messageEnum |= MessageEnum.ReturnValueVoid;
            }
            else
            {
                this.returnType = returnValue.GetType();
                if (this.returnType.IsPrimitive || object.ReferenceEquals(this.returnType, Converter.typeofString))
                {
                    this.messageEnum |= MessageEnum.ReturnValueInline;
                }
                else
                {
                    num++;
                    this.messageEnum |= MessageEnum.ReturnValueInArray;
                }
            }
            if (exception != null)
            {
                num++;
                this.messageEnum |= MessageEnum.ExceptionInArray;
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
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ReturnValueInArray))
            {
                this.callA[index++] = returnValue;
            }
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ExceptionInArray))
            {
                this.callA[index++] = exception;
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


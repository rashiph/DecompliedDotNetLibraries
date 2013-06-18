namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Security;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class VB6InputFile : VB6File
    {
        public VB6InputFile(string FileName, OpenShare share) : base(FileName, OpenAccess.Read, share, -1)
        {
        }

        internal override bool CanInput()
        {
            return true;
        }

        internal override bool EOF()
        {
            return base.m_eof;
        }

        public override OpenMode GetMode()
        {
            return OpenMode.Input;
        }

        internal override void Input(ref bool Value)
        {
            Value = BooleanType.FromObject(this.ParseInputString(ref this.InputStr()));
        }

        internal override void Input(ref byte Value)
        {
            Value = ByteType.FromObject(this.InputNum(VariantType.Byte));
        }

        internal override void Input(ref char Value)
        {
            string str = this.InputStr();
            if (str.Length > 0)
            {
                Value = str[0];
            }
            else
            {
                Value = '\0';
            }
        }

        internal override void Input(ref DateTime Value)
        {
            Value = DateType.FromObject(this.ParseInputString(ref this.InputStr()));
        }

        internal override void Input(ref decimal Value)
        {
            Value = DecimalType.FromObject(this.InputNum(VariantType.Decimal), Utils.GetInvariantCultureInfo().NumberFormat);
        }

        internal override void Input(ref double Value)
        {
            Value = DoubleType.FromObject(this.InputNum(VariantType.Double), Utils.GetInvariantCultureInfo().NumberFormat);
        }

        internal override void Input(ref short Value)
        {
            Value = ShortType.FromObject(this.InputNum(VariantType.Short));
        }

        internal override void Input(ref int Value)
        {
            Value = IntegerType.FromObject(this.InputNum(VariantType.Integer));
        }

        internal override void Input(ref long Value)
        {
            Value = LongType.FromObject(this.InputNum(VariantType.Long));
        }

        [SecurityCritical]
        internal override void Input(ref object obj)
        {
            int num = this.SkipWhiteSpaceEOF();
            switch (num)
            {
                case 0x22:
                    num = base.m_sr.Read();
                    base.m_position += 1L;
                    obj = this.ReadInField(1);
                    this.SkipTrailingWhiteSpace();
                    break;

                case 0x23:
                    obj = this.ParseInputString(ref this.InputStr());
                    break;

                default:
                {
                    string str = this.ReadInField(3);
                    obj = Conversion.ParseInputField(str, VariantType.Empty);
                    this.SkipTrailingWhiteSpace();
                    break;
                }
            }
        }

        internal override void Input(ref float Value)
        {
            Value = SingleType.FromObject(this.InputNum(VariantType.Single), Utils.GetInvariantCultureInfo().NumberFormat);
        }

        internal override void Input(ref string Value)
        {
            Value = this.InputStr();
        }

        internal override long LOC()
        {
            return ((base.m_position + 0x7fL) / 0x80L);
        }

        internal override void OpenFile()
        {
            try
            {
                base.m_file = new FileStream(base.m_sFullPath, FileMode.Open, (FileAccess) base.m_access, (FileShare) base.m_share);
            }
            catch (FileNotFoundException exception)
            {
                throw ExceptionUtils.VbMakeException(exception, 0x35);
            }
            catch (SecurityException)
            {
                throw ExceptionUtils.VbMakeException(0x35);
            }
            catch (DirectoryNotFoundException exception3)
            {
                throw ExceptionUtils.VbMakeException(exception3, 0x4c);
            }
            catch (IOException exception4)
            {
                throw ExceptionUtils.VbMakeException(exception4, 0x4b);
            }
            catch (StackOverflowException exception5)
            {
                throw exception5;
            }
            catch (OutOfMemoryException exception6)
            {
                throw exception6;
            }
            catch (ThreadAbortException exception7)
            {
                throw exception7;
            }
            catch (Exception exception8)
            {
                throw ExceptionUtils.VbMakeException(exception8, 0x4c);
            }
            base.m_Encoding = Utils.GetFileIOEncoding();
            base.m_sr = new StreamReader(base.m_file, base.m_Encoding, false, 0x80);
            base.m_eof = base.m_file.Length == 0L;
        }

        internal object ParseInputString(ref string sInput)
        {
            object obj2 = sInput;
            if ((sInput[0] == '#') && (sInput.Length != 1))
            {
                sInput = sInput.Substring(1, sInput.Length - 2);
                if (sInput == "NULL")
                {
                    return DBNull.Value;
                }
                if (sInput == "TRUE")
                {
                    return true;
                }
                if (sInput == "FALSE")
                {
                    return false;
                }
                if (Strings.Left(sInput, 6) == "ERROR ")
                {
                    int num;
                    if (sInput.Length > 6)
                    {
                        num = IntegerType.FromString(Strings.Mid(sInput, 7));
                    }
                    return num;
                }
                try
                {
                    obj2 = DateTime.Parse(Utils.ToHalfwidthNumbers(sInput, Utils.GetCultureInfo()));
                }
                catch (StackOverflowException exception)
                {
                    throw exception;
                }
                catch (OutOfMemoryException exception2)
                {
                    throw exception2;
                }
                catch (ThreadAbortException exception3)
                {
                    throw exception3;
                }
                catch (Exception)
                {
                }
            }
            return obj2;
        }

        public string ReadLine()
        {
            string s = base.m_sr.ReadLine();
            base.m_position += base.m_Encoding.GetByteCount(s) + 2;
            return null;
        }
    }
}


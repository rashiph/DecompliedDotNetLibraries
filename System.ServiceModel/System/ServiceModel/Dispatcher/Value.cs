namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Value
    {
        private bool boolVal;
        private double dblVal;
        private StackFrame frame;
        private NodeSequence sequence;
        private string strVal;
        private ValueDataType type;
        internal bool Boolean
        {
            get
            {
                return this.boolVal;
            }
            set
            {
                this.type = ValueDataType.Boolean;
                this.boolVal = value;
            }
        }
        internal double Double
        {
            get
            {
                return this.dblVal;
            }
            set
            {
                this.type = ValueDataType.Double;
                this.dblVal = value;
            }
        }
        internal StackFrame Frame
        {
            get
            {
                return this.frame;
            }
        }
        internal int FrameEndPtr
        {
            set
            {
                this.frame.EndPtr = value;
            }
        }
        internal int NodeCount
        {
            get
            {
                return this.sequence.Count;
            }
        }
        internal NodeSequence Sequence
        {
            get
            {
                return this.sequence;
            }
            set
            {
                this.type = ValueDataType.Sequence;
                this.sequence = value;
            }
        }
        internal string String
        {
            get
            {
                return this.strVal;
            }
            set
            {
                this.type = ValueDataType.String;
                this.strVal = value;
            }
        }
        internal ValueDataType Type
        {
            get
            {
                return this.type;
            }
        }
        internal void Add(double val)
        {
            this.dblVal += val;
        }

        internal void Clear(ProcessingContext context)
        {
            if (ValueDataType.Sequence == this.type)
            {
                this.ReleaseSequence(context);
            }
            this.type = ValueDataType.None;
        }

        internal bool CompareTo(ref Value val, RelationOperator op)
        {
            switch (this.type)
            {
                case ValueDataType.Boolean:
                    switch (val.type)
                    {
                        case ValueDataType.Boolean:
                            return QueryValueModel.Compare(this.boolVal, val.boolVal, op);

                        case ValueDataType.Double:
                            return QueryValueModel.Compare(this.boolVal, val.dblVal, op);

                        case ValueDataType.Sequence:
                            return QueryValueModel.Compare(this.boolVal, val.sequence, op);

                        case ValueDataType.String:
                            return QueryValueModel.Compare(this.boolVal, val.strVal, op);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));

                case ValueDataType.Double:
                    switch (val.type)
                    {
                        case ValueDataType.Boolean:
                            return QueryValueModel.Compare(this.dblVal, val.boolVal, op);

                        case ValueDataType.Double:
                            return QueryValueModel.Compare(this.dblVal, val.dblVal, op);

                        case ValueDataType.Sequence:
                            return QueryValueModel.Compare(this.dblVal, val.sequence, op);

                        case ValueDataType.String:
                            return QueryValueModel.Compare(this.dblVal, val.strVal, op);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));

                case ValueDataType.Sequence:
                    switch (val.type)
                    {
                        case ValueDataType.Boolean:
                            return QueryValueModel.Compare(this.sequence, val.boolVal, op);

                        case ValueDataType.Double:
                            return QueryValueModel.Compare(this.sequence, val.dblVal, op);

                        case ValueDataType.Sequence:
                            return QueryValueModel.Compare(this.sequence, val.sequence, op);

                        case ValueDataType.String:
                            return QueryValueModel.Compare(this.sequence, val.strVal, op);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));

                case ValueDataType.String:
                    switch (val.type)
                    {
                        case ValueDataType.Boolean:
                            return QueryValueModel.Compare(this.strVal, val.boolVal, op);

                        case ValueDataType.Double:
                            return QueryValueModel.Compare(this.strVal, val.dblVal, op);

                        case ValueDataType.Sequence:
                            return QueryValueModel.Compare(this.strVal, val.sequence, op);

                        case ValueDataType.String:
                            return QueryValueModel.Compare(this.strVal, val.strVal, op);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
        }

        internal bool CompareTo(double val, RelationOperator op)
        {
            switch (this.type)
            {
                case ValueDataType.Boolean:
                    return QueryValueModel.Compare(this.boolVal, val, op);

                case ValueDataType.Double:
                    return QueryValueModel.Compare(this.dblVal, val, op);

                case ValueDataType.Sequence:
                    return QueryValueModel.Compare(this.sequence, val, op);

                case ValueDataType.String:
                    return QueryValueModel.Compare(this.strVal, val, op);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
        }

        internal void ConvertTo(ProcessingContext context, ValueDataType newType)
        {
            if (newType != this.type)
            {
                switch (newType)
                {
                    case ValueDataType.Boolean:
                        this.boolVal = this.ToBoolean();
                        break;

                    case ValueDataType.Double:
                        this.dblVal = this.ToDouble();
                        break;

                    case ValueDataType.String:
                        this.strVal = this.ToString();
                        break;
                }
                if (ValueDataType.Sequence == this.type)
                {
                    this.ReleaseSequence(context);
                }
                this.type = newType;
            }
        }

        internal bool Equals(string val)
        {
            switch (this.type)
            {
                case ValueDataType.Boolean:
                    return QueryValueModel.Equals(this.boolVal, val);

                case ValueDataType.Double:
                    return QueryValueModel.Equals(this.dblVal, val);

                case ValueDataType.Sequence:
                    return QueryValueModel.Equals(this.sequence, val);

                case ValueDataType.String:
                    return QueryValueModel.Equals(this.strVal, val);
            }
            return false;
        }

        internal bool Equals(double val)
        {
            switch (this.type)
            {
                case ValueDataType.Boolean:
                    return QueryValueModel.Equals(this.boolVal, val);

                case ValueDataType.Double:
                    return QueryValueModel.Equals(this.dblVal, val);

                case ValueDataType.Sequence:
                    return QueryValueModel.Equals(this.sequence, val);

                case ValueDataType.String:
                    return QueryValueModel.Equals(val, this.strVal);
            }
            return false;
        }

        internal bool GetBoolean()
        {
            if (ValueDataType.Boolean != this.type)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
            }
            return this.boolVal;
        }

        internal double GetDouble()
        {
            if (ValueDataType.Double != this.type)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
            }
            return this.dblVal;
        }

        internal NodeSequence GetSequence()
        {
            if (ValueDataType.Sequence != this.type)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
            }
            return this.sequence;
        }

        internal string GetString()
        {
            if (ValueDataType.String != this.type)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
            }
            return this.strVal;
        }

        internal bool IsType(ValueDataType type)
        {
            return (type == this.type);
        }

        internal void Multiply(double val)
        {
            this.dblVal *= val;
        }

        internal void Negate()
        {
            this.dblVal = -this.dblVal;
        }

        internal void Not()
        {
            this.boolVal = !this.boolVal;
        }

        internal void ReleaseSequence(ProcessingContext context)
        {
            context.ReleaseSequence(this.sequence);
            this.sequence = null;
        }

        internal void StartFrame(int start)
        {
            this.type = ValueDataType.StackFrame;
            this.frame.basePtr = start + 1;
            this.frame.endPtr = start;
        }

        internal bool ToBoolean()
        {
            switch (this.type)
            {
                case ValueDataType.Boolean:
                    return this.boolVal;

                case ValueDataType.Double:
                    return QueryValueModel.Boolean(this.dblVal);

                case ValueDataType.Sequence:
                    return QueryValueModel.Boolean(this.sequence);

                case ValueDataType.String:
                    return QueryValueModel.Boolean(this.strVal);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
        }

        internal double ToDouble()
        {
            switch (this.type)
            {
                case ValueDataType.Boolean:
                    return QueryValueModel.Double(this.boolVal);

                case ValueDataType.Double:
                    return this.dblVal;

                case ValueDataType.Sequence:
                    return QueryValueModel.Double(this.sequence);

                case ValueDataType.String:
                    return QueryValueModel.Double(this.strVal);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
        }

        public override string ToString()
        {
            switch (this.type)
            {
                case ValueDataType.Boolean:
                    return QueryValueModel.String(this.boolVal);

                case ValueDataType.Double:
                    return QueryValueModel.String(this.dblVal);

                case ValueDataType.Sequence:
                    return QueryValueModel.String(this.sequence);

                case ValueDataType.String:
                    return this.strVal;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
        }

        internal void Update(ProcessingContext context, bool val)
        {
            if (ValueDataType.Sequence == this.type)
            {
                context.ReleaseSequence(this.sequence);
            }
            this.Boolean = val;
        }

        internal void Update(ProcessingContext context, double val)
        {
            if (ValueDataType.Sequence == this.type)
            {
                context.ReleaseSequence(this.sequence);
            }
            this.Double = val;
        }

        internal void Update(ProcessingContext context, string val)
        {
            if (ValueDataType.Sequence == this.type)
            {
                context.ReleaseSequence(this.sequence);
            }
            this.String = val;
        }

        internal void Update(ProcessingContext context, NodeSequence val)
        {
            if (ValueDataType.Sequence == this.type)
            {
                context.ReleaseSequence(this.sequence);
            }
            this.Sequence = val;
        }
    }
}


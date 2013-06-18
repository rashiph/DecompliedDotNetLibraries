namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class RuleEvaluationIncompatibleTypesException : RuleException, ISerializable
    {
        private Type m_leftType;
        private CodeBinaryOperatorType m_op;
        private Type m_rightType;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleEvaluationIncompatibleTypesException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleEvaluationIncompatibleTypesException(string message) : base(message)
        {
        }

        protected RuleEvaluationIncompatibleTypesException(SerializationInfo serializeInfo, StreamingContext context) : base(serializeInfo, context)
        {
            if (serializeInfo == null)
            {
                throw new ArgumentNullException("serializeInfo");
            }
            string typeName = serializeInfo.GetString("left");
            if (typeName != "null")
            {
                this.m_leftType = Type.GetType(typeName);
            }
            this.m_op = (CodeBinaryOperatorType) serializeInfo.GetValue("op", typeof(CodeBinaryOperatorType));
            typeName = serializeInfo.GetString("right");
            if (typeName != "null")
            {
                this.m_rightType = Type.GetType(typeName);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleEvaluationIncompatibleTypesException(string message, Exception ex) : base(message, ex)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleEvaluationIncompatibleTypesException(string message, Type left, CodeBinaryOperatorType op, Type right) : base(message)
        {
            this.m_leftType = left;
            this.m_op = op;
            this.m_rightType = right;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleEvaluationIncompatibleTypesException(string message, Type left, CodeBinaryOperatorType op, Type right, Exception ex) : base(message, ex)
        {
            this.m_leftType = left;
            this.m_op = op;
            this.m_rightType = right;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("left", (this.m_leftType != null) ? this.m_leftType.AssemblyQualifiedName : "null");
            info.AddValue("op", this.m_op);
            info.AddValue("right", (this.m_rightType != null) ? this.m_rightType.AssemblyQualifiedName : "null");
        }

        public Type Left
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_leftType;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_leftType = value;
            }
        }

        public CodeBinaryOperatorType Operator
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_op;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_op = value;
            }
        }

        public Type Right
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_rightType;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_rightType = value;
            }
        }
    }
}


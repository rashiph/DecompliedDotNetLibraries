namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    internal class DirectionalAction : IComparable<DirectionalAction>
    {
        private string action;
        private MessageDirection direction;
        private bool isNullAction;

        internal DirectionalAction(MessageDirection direction, string action)
        {
            if (!MessageDirectionHelper.IsDefined(direction))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("direction"));
            }
            this.direction = direction;
            if (action == null)
            {
                this.action = "*";
                this.isNullAction = true;
            }
            else
            {
                this.action = action;
                this.isNullAction = false;
            }
        }

        public int CompareTo(DirectionalAction other)
        {
            if (other == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("other");
            }
            if ((this.direction == MessageDirection.Input) && (other.direction == MessageDirection.Output))
            {
                return -1;
            }
            if ((this.direction == MessageDirection.Output) && (other.direction == MessageDirection.Input))
            {
                return 1;
            }
            return this.action.CompareTo(other.action);
        }

        public override bool Equals(object other)
        {
            DirectionalAction action = other as DirectionalAction;
            if (action == null)
            {
                return false;
            }
            return this.Equals(action);
        }

        public bool Equals(DirectionalAction other)
        {
            if (other == null)
            {
                return false;
            }
            return ((this.direction == other.direction) && (this.action == other.action));
        }

        public override int GetHashCode()
        {
            return this.action.GetHashCode();
        }

        public string Action
        {
            get
            {
                if (!this.isNullAction)
                {
                    return this.action;
                }
                return null;
            }
        }

        public MessageDirection Direction
        {
            get
            {
                return this.direction;
            }
        }
    }
}


namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class OrElse : Activity<bool>
    {
        public OrElse()
        {
            Func<Activity> func = null;
            if (func == null)
            {
                func = delegate {
                    if ((this.Left != null) && (this.Right != null))
                    {
                        If @if = new If {
                            Condition = this.Left
                        };
                        Assign<bool> assign = new Assign<bool> {
                            To = new OutArgument<bool>(context => this.Result.Get(context)),
                            Value = 1
                        };
                        @if.Then = assign;
                        Assign<bool> assign2 = new Assign<bool> {
                            To = new OutArgument<bool>(context => this.Result.Get(context)),
                            Value = new InArgument<bool>(this.Right)
                        };
                        @if.Else = assign2;
                        return @if;
                    }
                    return null;
                };
            }
            this.Implementation = func;
        }

        protected override void CacheMetadata(ActivityMetadata metadata)
        {
            metadata.AddImportedChild(this.Left);
            metadata.AddImportedChild(this.Right);
            if (this.Left == null)
            {
                metadata.AddValidationError(System.Activities.SR.BinaryExpressionActivityRequiresArgument("Left", "OrElse", base.DisplayName));
            }
            if (this.Right == null)
            {
                metadata.AddValidationError(System.Activities.SR.BinaryExpressionActivityRequiresArgument("Right", "OrElse", base.DisplayName));
            }
        }

        [DefaultValue((string) null)]
        public Activity<bool> Left { get; set; }

        [DefaultValue((string) null)]
        public Activity<bool> Right { get; set; }
    }
}


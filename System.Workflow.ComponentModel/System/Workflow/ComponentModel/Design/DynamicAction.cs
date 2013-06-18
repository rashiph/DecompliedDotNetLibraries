namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    internal class DynamicAction : IDisposable
    {
        private ActionTypes actionType = ActionTypes.Standard;
        private Size borderSize = new Size(2, 2);
        private ItemList<ActionButton> buttons;
        private Size buttonSize = Sizes[2];
        private ButtonSizes buttonSizeType = ButtonSizes.Medium;
        private static float DefaultTransparency = 0f;
        private DesignerContentAlignment dockAlignment = DesignerContentAlignment.TopLeft;
        private Size dockMargin = Sizes[2];
        private Size margin = Margins[2];
        private static Size[] Margins = new Size[] { new Size(1, 1), new Size(1, 1), new Size(2, 2), new Size(2, 2), new Size(3, 3) };
        private float minimumTransparency = DefaultTransparency;
        private static Size[] Sizes = new Size[] { new Size(20, 20), new Size(0x18, 0x18), new Size(0x1c, 0x1c), new Size(0x20, 0x20), new Size(0x24, 0x24) };
        private float transparency = DefaultTransparency;

        internal DynamicAction()
        {
            this.buttons = new ItemList<ActionButton>(this);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            foreach (ActionButton button in this.buttons)
            {
                ((IDisposable) button).Dispose();
            }
            this.buttons.Clear();
        }

        internal void Draw(Graphics graphics)
        {
            if ((this.transparency != 0f) && (this.buttons.Count != 0))
            {
                ActivityDesignerPaint.Draw3DButton(graphics, null, this.Bounds, this.transparency - 0.1f, ButtonState.Normal);
                for (int i = 0; i < this.buttons.Count; i++)
                {
                    Rectangle buttonBounds = this.GetButtonBounds(i);
                    ActionButton button = this.buttons[i];
                    if (button.StateImages.Length == 1)
                    {
                        Image image = button.StateImages[0];
                        if ((button.State == ActionButton.States.Normal) || (button.State == ActionButton.States.Disabled))
                        {
                            buttonBounds.Inflate(-2, -2);
                            ActivityDesignerPaint.DrawImage(graphics, image, buttonBounds, new Rectangle(Point.Empty, image.Size), DesignerContentAlignment.Fill, this.transparency, button.State == ActionButton.States.Disabled);
                        }
                        else
                        {
                            ButtonState buttonState = (button.State == ActionButton.States.Highlight) ? ButtonState.Normal : ButtonState.Pushed;
                            ActivityDesignerPaint.Draw3DButton(graphics, image, buttonBounds, this.transparency, buttonState);
                        }
                    }
                    else
                    {
                        Image image2 = this.buttons[i].StateImages[(int) this.buttons[i].State];
                        buttonBounds.Inflate(-2, -2);
                        ActivityDesignerPaint.DrawImage(graphics, image2, buttonBounds, new Rectangle(Point.Empty, image2.Size), DesignerContentAlignment.Fill, this.transparency, false);
                    }
                }
            }
        }

        ~DynamicAction()
        {
            this.Dispose(false);
        }

        internal Rectangle GetButtonBounds(int buttonIndex)
        {
            if ((buttonIndex < 0) || (buttonIndex >= this.buttons.Count))
            {
                throw new ArgumentOutOfRangeException("buttonIndex");
            }
            Rectangle empty = Rectangle.Empty;
            empty.X = (this.borderSize.Width + (buttonIndex * this.buttonSize.Width)) + ((buttonIndex + 1) * this.margin.Width);
            empty.Y = this.borderSize.Height + this.margin.Height;
            empty.Size = this.buttonSize;
            return empty;
        }

        internal ActionTypes ActionType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.actionType;
            }
        }

        internal Rectangle Bounds
        {
            get
            {
                Size empty = Size.Empty;
                int num = Math.Max(1, this.buttons.Count);
                empty.Width = ((2 * this.borderSize.Width) + (num * this.buttonSize.Width)) + ((num + 1) * this.margin.Width);
                empty.Height = ((2 * this.borderSize.Height) + this.buttonSize.Height) + (2 * this.margin.Height);
                return new Rectangle(Point.Empty, empty);
            }
        }

        internal IList<ActionButton> Buttons
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.buttons;
            }
        }

        internal ButtonSizes ButtonSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.buttonSizeType;
            }
            set
            {
                if (this.buttonSizeType != value)
                {
                    this.buttonSizeType = value;
                    this.buttonSize = Sizes[(int) this.buttonSizeType];
                    this.margin = Margins[(int) this.buttonSizeType];
                }
            }
        }

        internal DesignerContentAlignment DockAlignment
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dockAlignment;
            }
            set
            {
                if (this.dockAlignment != value)
                {
                    this.dockAlignment = value;
                }
            }
        }

        internal Size DockMargin
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dockMargin;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.dockMargin = value;
            }
        }

        internal float Transparency
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transparency;
            }
            set
            {
                if (this.transparency != value)
                {
                    this.transparency = Math.Max(DefaultTransparency, value);
                }
            }
        }

        internal enum ActionTypes
        {
            Standard = 1,
            TwoState = 2
        }

        internal enum ButtonSizes
        {
            Small,
            SmallMedium,
            Medium,
            MediumLarge,
            Large
        }
    }
}


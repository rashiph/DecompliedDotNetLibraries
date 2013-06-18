namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal sealed class DynamicActionMessageFilter : WorkflowDesignerMessageFilter
    {
        private List<DynamicAction> actions = new List<DynamicAction>();
        private int draggedActionIndex = -1;
        private int draggedButtonIndex = -1;
        private bool infoTipSet;

        internal DynamicActionMessageFilter()
        {
        }

        internal bool ActionExists(DynamicAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            return this.actions.Contains(action);
        }

        internal void AddAction(DynamicAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (!this.actions.Contains(action))
            {
                if (this.IsButtonDragged)
                {
                    this.SetDraggedButton(-1, -1);
                }
                this.actions.Add(action);
                this.RefreshAction(action);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                IServiceContainer service = base.GetService(typeof(IServiceContainer)) as IServiceContainer;
                if (service != null)
                {
                    service.RemoveService(typeof(DynamicActionMessageFilter));
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private Rectangle GetActionBounds(int actionIndex)
        {
            Rectangle destination = new Rectangle(Point.Empty, base.ParentView.ViewPortSize);
            DynamicAction action = this.actions[actionIndex];
            destination.Inflate(-action.DockMargin.Width, -action.DockMargin.Height);
            return new Rectangle(ActivityDesignerPaint.GetRectangleFromAlignment(action.DockAlignment, destination, action.Bounds.Size).Location, action.Bounds.Size);
        }

        private Rectangle GetButtonBounds(int actionIndex, int buttonIndex)
        {
            Rectangle actionBounds = this.GetActionBounds(actionIndex);
            Rectangle buttonBounds = this.actions[actionIndex].GetButtonBounds(buttonIndex);
            buttonBounds.Offset(actionBounds.Location);
            return buttonBounds;
        }

        protected override void Initialize(WorkflowView parentView)
        {
            base.Initialize(parentView);
            IServiceContainer service = base.GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (service != null)
            {
                service.RemoveService(typeof(DynamicActionMessageFilter));
                service.AddService(typeof(DynamicActionMessageFilter), this);
            }
        }

        protected override bool OnMouseCaptureChanged()
        {
            if (this.IsButtonDragged)
            {
                this.SetDraggedButton(-1, -1);
            }
            return false;
        }

        protected override bool OnMouseDoubleClick(MouseEventArgs eventArgs)
        {
            for (int i = this.actions.Count - 1; i >= 0; i--)
            {
                DynamicAction local1 = this.actions[i];
                if (this.GetActionBounds(i).Contains(new Point(eventArgs.X, eventArgs.Y)))
                {
                    return true;
                }
            }
            return false;
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            Point point = new Point(eventArgs.X, eventArgs.Y);
            this.Refresh();
            this.UpdateTransparency(point);
            bool flag = false;
            if ((eventArgs.Button & MouseButtons.Left) > MouseButtons.None)
            {
                for (int i = this.actions.Count - 1; i >= 0; i--)
                {
                    DynamicAction action = this.actions[i];
                    if (this.GetActionBounds(i).Contains(point))
                    {
                        for (int j = 0; j < action.Buttons.Count; j++)
                        {
                            if (this.GetButtonBounds(i, j).Contains(point) && (action.Buttons[j].State == ActionButton.States.Disabled))
                            {
                                return true;
                            }
                        }
                        for (int k = 0; k < action.Buttons.Count; k++)
                        {
                            ActionButton button = action.Buttons[k];
                            if (button.State != ActionButton.States.Disabled)
                            {
                                if (this.GetButtonBounds(i, k).Contains(point))
                                {
                                    button.State = ActionButton.States.Pressed;
                                    if (action.ActionType != DynamicAction.ActionTypes.TwoState)
                                    {
                                        this.SetDraggedButton(i, k);
                                    }
                                }
                                else if (action.ActionType == DynamicAction.ActionTypes.TwoState)
                                {
                                    button.State = ActionButton.States.Normal;
                                }
                            }
                        }
                        flag = true;
                    }
                }
            }
            return flag;
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            this.UpdateTransparency(new Point(eventArgs.X, eventArgs.Y));
            this.Refresh();
            return false;
        }

        protected override bool OnMouseLeave()
        {
            base.ParentView.ShowInfoTip(string.Empty);
            this.UpdateTransparency(Point.Empty);
            this.Refresh();
            return false;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            Point point = new Point(eventArgs.X, eventArgs.Y);
            this.Refresh();
            this.UpdateTransparency(point);
            string text = string.Empty;
            bool isButtonDragged = this.IsButtonDragged;
            if (!this.IsButtonDragged)
            {
                for (int i = this.actions.Count - 1; i >= 0; i--)
                {
                    DynamicAction action = this.actions[i];
                    Rectangle actionBounds = this.GetActionBounds(i);
                    for (int j = 0; j < action.Buttons.Count; j++)
                    {
                        ActionButton button = action.Buttons[j];
                        if (actionBounds.Contains(point))
                        {
                            bool flag2 = this.GetButtonBounds(i, j).Contains(point);
                            if (flag2 && (text.Length == 0))
                            {
                                text = button.Description;
                            }
                            if ((button.State != ActionButton.States.Disabled) && (button.State != ActionButton.States.Pressed))
                            {
                                if (flag2)
                                {
                                    button.State = ActionButton.States.Highlight;
                                }
                                else
                                {
                                    button.State = ActionButton.States.Normal;
                                }
                            }
                            isButtonDragged = true;
                        }
                        else if (button.State == ActionButton.States.Highlight)
                        {
                            button.State = ActionButton.States.Normal;
                        }
                    }
                }
            }
            WorkflowView parentView = base.ParentView;
            if (text.Length > 0)
            {
                this.infoTipSet = true;
                parentView.ShowInfoTip(text);
                return isButtonDragged;
            }
            if (this.infoTipSet)
            {
                parentView.ShowInfoTip(string.Empty);
                this.infoTipSet = false;
            }
            return isButtonDragged;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            Point point = new Point(eventArgs.X, eventArgs.Y);
            this.Refresh();
            this.UpdateTransparency(point);
            bool flag = false;
            if ((eventArgs.Button & MouseButtons.Left) > MouseButtons.None)
            {
                for (int i = this.actions.Count - 1; i >= 0; i--)
                {
                    DynamicAction action = this.actions[i];
                    if (this.GetActionBounds(i).Contains(point))
                    {
                        for (int j = 0; j < action.Buttons.Count; j++)
                        {
                            ActionButton button = action.Buttons[j];
                            if (button.State != ActionButton.States.Disabled)
                            {
                                if (this.GetButtonBounds(i, j).Contains(point) && (action.ActionType != DynamicAction.ActionTypes.TwoState))
                                {
                                    button.State = ActionButton.States.Highlight;
                                }
                                else if (button.State == ActionButton.States.Highlight)
                                {
                                    button.State = ActionButton.States.Normal;
                                }
                            }
                        }
                        flag = true;
                    }
                }
            }
            if (this.IsButtonDragged)
            {
                this.SetDraggedButton(-1, -1);
            }
            return flag;
        }

        protected override bool OnPaintWorkflowAdornments(PaintEventArgs e, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            for (int i = 0; i < this.actions.Count; i++)
            {
                GraphicsContainer container = e.Graphics.BeginContainer();
                Point location = this.GetActionBounds(i).Location;
                e.Graphics.TranslateTransform((float) location.X, (float) location.Y);
                this.actions[i].Draw(e.Graphics);
                e.Graphics.EndContainer(container);
            }
            return false;
        }

        private void Refresh()
        {
            WorkflowView parentView = base.ParentView;
            for (int i = 0; i < this.actions.Count; i++)
            {
                parentView.InvalidateClientRectangle(this.GetActionBounds(i));
            }
        }

        internal void RefreshAction(DynamicAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            int index = this.actions.IndexOf(action);
            if (index >= 0)
            {
                base.ParentView.InvalidateClientRectangle(this.GetActionBounds(index));
            }
        }

        internal void RemoveAction(DynamicAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (this.actions.Contains(action))
            {
                if (this.IsButtonDragged)
                {
                    this.SetDraggedButton(-1, -1);
                }
                this.RefreshAction(action);
                this.actions.Remove(action);
            }
        }

        private void SetDraggedButton(int actionIndex, int buttonIndex)
        {
            if ((this.draggedActionIndex != actionIndex) || (this.draggedButtonIndex != buttonIndex))
            {
                WorkflowView parentView = base.ParentView;
                if ((this.draggedActionIndex >= 0) && (this.draggedButtonIndex >= 0))
                {
                    if (this.draggedActionIndex < this.actions.Count)
                    {
                        this.actions[this.draggedActionIndex].Buttons[this.draggedButtonIndex].State = ActionButton.States.Highlight;
                    }
                    this.draggedActionIndex = -1;
                    this.draggedButtonIndex = -1;
                    parentView.Capture = false;
                    this.UpdateTransparency(parentView.PointToClient(Control.MousePosition));
                }
                this.draggedActionIndex = actionIndex;
                this.draggedButtonIndex = buttonIndex;
                if ((this.draggedActionIndex >= 0) && (this.draggedButtonIndex >= 0))
                {
                    parentView.Capture = true;
                }
            }
        }

        private void UpdateTransparency(Point point)
        {
            for (int i = 0; i < this.actions.Count; i++)
            {
                float num2 = 0f;
                if (!point.IsEmpty)
                {
                    Rectangle actionBounds = this.GetActionBounds(i);
                    if (actionBounds.Contains(point) || (this.draggedActionIndex == i))
                    {
                        num2 = 1f;
                    }
                    else
                    {
                        Rectangle viewPortRectangle = base.ParentView.ViewPortRectangle;
                        double num3 = DesignerGeometryHelper.DistanceFromPointToRectangle(point, actionBounds);
                        if ((num3 > (viewPortRectangle.Width / 3)) || (num3 > (viewPortRectangle.Height / 3)))
                        {
                            num2 = 0.3f;
                        }
                        else
                        {
                            num2 = 1f;
                        }
                    }
                }
                this.actions[i].Transparency = num2;
            }
        }

        private bool IsButtonDragged
        {
            get
            {
                return ((this.draggedActionIndex >= 0) && (this.draggedButtonIndex >= 0));
            }
        }
    }
}


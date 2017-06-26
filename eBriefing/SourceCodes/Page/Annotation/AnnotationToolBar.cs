/*
Copyright (C) 2017 MetroStar Systems

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

The full license text can be found is the included LICENSE file.

You can freely use any of this software which you make publicly 
available at no charge.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

using System;
using Foundation;
using UIKit;
using CoreGraphics;

namespace eBriefingMobile
{
    public class AnnotationToolBar : UIView
    {
        private bool highlighterMode = false;
        private UIImageView grabberView;
        private UIImageView backgroundView;
        private UIButton penButton;
        private ColorButton colorButton;
        private UIButton undoButton;
        private UIButton redoButton;
        private UIButton doneButton;
        private UIButton cancelButton;
        private PenPickerBar penPicker;
        private ColorPickerBar colorPicker;
        private CGPoint location;
        private CGPoint startLocation;
        private UIView parentView;

        public delegate void AnnotationToolBarDelegate0 ();

        public delegate void AnnotationToolBarDelegate1 (CGRect frame);

        public event AnnotationToolBarDelegate0 PenSelectedEvent;
        public event AnnotationToolBarDelegate0 HighlighterSelectedEvent;
        public event AnnotationToolBarDelegate0 ColorSelectedEvent;
        public event AnnotationToolBarDelegate0 UndoEvent;
        public event AnnotationToolBarDelegate0 RedoEvent;
        public event AnnotationToolBarDelegate0 DoneEvent;
        public event AnnotationToolBarDelegate0 CancelEvent;
        public event AnnotationToolBarDelegate1 UpdateFrameEvent;

        #region AnnotationToolBar

        public AnnotationToolBar(CGRect rectangle, UIView parentView) : base(rectangle)
        {
            this.BackgroundColor = UIColor.Clear;
            this.parentView = parentView;

            // Set the position of the frame
            startLocation = rectangle.Location;

            // grabberView
            grabberView = new UIImageView(UIImage.FromBundle("Assets/Buttons/grabber_unpressed.png"));
            grabberView.HighlightedImage = UIImage.FromBundle("Assets/Buttons/grabber_pressed.png");
            grabberView.Frame = new CGRect(0, 0, rectangle.Width, 32);
            this.AddSubview(grabberView);

            // backgroundView
            UIImage backgroundImage = UIImage.FromBundle("Assets/Backgrounds/toolBar.png").CreateResizableImage(new UIEdgeInsets(0, 0, 200, 0));
            backgroundView = new UIImageView(new CGRect(0, grabberView.Frame.Bottom, rectangle.Width, rectangle.Height - grabberView.Frame.Height));
            backgroundView.Image = backgroundImage;
            backgroundView.BackgroundColor = UIColor.Clear;
            this.AddSubview(backgroundView);

            // penButton
            AddPenButton();

            // colorButton
            AddColorButton();

            // undoButton
            AddUndoButton();

            // redoButton
            AddRedoButton();

            // doneButton
            AddDoneButton();

            // cancelButton
            AddCancelButton();
        }
        // This event occurs when you just touch the object
        public override void TouchesBegan(NSSet touches, UIEvent e)
        {
            location = this.Frame.Location;

            if (e.TouchesForView(this) != null)
            {
                var touch = (UITouch)e.TouchesForView(this).AnyObject;
                var bounds = Bounds;

                startLocation = touch.LocationInView(this);
                this.Frame = new CGRect(location, bounds.Size);

                // Make draggable only if user presses inside grabberView frame
                if (grabberView.Frame.Contains(startLocation))
                {
                    grabberView.Highlighted = true;
                }
            }
        }
        // This event occurs when you drag it around
        public override void TouchesMoved(NSSet touches, UIEvent e)
        {
            if (e.TouchesForView(this) != null)
            {
                var touch = (UITouch)e.TouchesForView(this).AnyObject;
                var bounds = Bounds;

                // Make draggable only if user presses inside grabberView frame
                if (grabberView.Frame.Contains(startLocation))
                {
                    // Always refer to the StartLocation of the object that you've been dragging.
                    CGPoint tempLocation = new CGPoint(location.X, location.Y);
                    tempLocation.X += touch.LocationInView(this).X - startLocation.X;
                    tempLocation.Y += touch.LocationInView(this).Y - startLocation.Y;

                    // Make sure the toolBar stays inside parent frame
                    CGRect tempFrame = new CGRect(tempLocation, bounds.Size);
                    CGRect leftRect = new CGRect(-1, 0, 1, parentView.Frame.Height);
                    CGRect rightRect = new CGRect(parentView.Frame.Width, 0, 1, parentView.Frame.Height);
                    CGRect topRect = new CGRect(0, -1, parentView.Frame.Width, 1);
                    CGRect bottomRect = new CGRect(0, parentView.Frame.Bottom, parentView.Frame.Width, 44);
                    if (!tempFrame.IntersectsWith(leftRect) && !tempFrame.IntersectsWith(rightRect) && !tempFrame.IntersectsWith(topRect) && !tempFrame.IntersectsWith(bottomRect))
                    {
                        location = tempLocation;

                        this.Frame = tempFrame;

                        UpdateFrame(new CGRect(location.X, location.Y, this.Frame.Width, this.Frame.Height));
                    }
                }
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent e)
        {
            grabberView.Highlighted = false;
        }

        public void SelectPen()
        {
            HandlePenSelectedEvent();
        }

        public UIColor GetAnnotationColor()
        {
            UIColor color;

            if (highlighterMode)
            {
                if (String.IsNullOrEmpty(Settings.DefaultHighlighterColor) || Settings.DefaultHighlighterColor.Equals(StringRef.Yellow))
                {
                    color = eBriefingAppearance.Color(240, 232, 66);
                }
                else if (Settings.DefaultHighlighterColor.Equals(StringRef.Cyan))
                {
                    color = eBriefingAppearance.Color(31, 162, 251);
                }
                else if (Settings.DefaultHighlighterColor.Equals(StringRef.LightGreen))
                {
                    color = eBriefingAppearance.Color(0, 228, 52);
                }
                else
                {
                    color = eBriefingAppearance.Color(255, 33, 169);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(Settings.DefaultPenColor) || Settings.DefaultPenColor.Equals(StringRef.Black))
                {
                    color = UIColor.Black;
                }
                else if (Settings.DefaultPenColor.Equals(StringRef.Red))
                {
                    color = eBriefingAppearance.Color(255, 1, 22);
                }
                else if (Settings.DefaultPenColor.Equals(StringRef.Blue))
                {
                    color = eBriefingAppearance.Color(33, 105, 222);
                }
                else
                {
                    color = eBriefingAppearance.Color(0, 167, 86);
                }
            }

            return color;
        }

        private void UpdateFrame(CGRect rectangle)
        {
            if (UpdateFrameEvent != null)
            {
                UpdateFrameEvent(rectangle);
            }
        }

        private void HidePickers(String name)
        {
            if (name != StringRef.Pen)
            {
                HidePenPickerBar(false);
            }

            if (name != StringRef.Color)
            {
                HideColorPickerBar(false);
            }
        }

        #endregion

        #region PenButton

        private void AddPenButton()
        {
            penButton = UIButton.FromType(UIButtonType.Custom);
            penButton.Frame = new CGRect(this.Frame.Width / 2 - 36, grabberView.Frame.Bottom + 14, 72, 72);
            penButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/toolBar_unpressed.png"), UIControlState.Normal);
            penButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/toolBar_pressed.png"), UIControlState.Selected);
            penButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/toolBar_pressed.png"), UIControlState.Selected | UIControlState.Highlighted);
            penButton.TouchUpInside += HandlePenButtonTouchUpInside;
            this.AddSubview(penButton);

            UpdatePenIcon();
        }

        private void ShowPenPickerBar()
        {
            HidePickers(StringRef.Pen);

            penButton.Selected = true;

            if (penPicker != null)
            {
                penPicker.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f);
                UIView.Animate(0.2d, 0d, UIViewAnimationOptions.CurveEaseOut, delegate
                {
                    penPicker.Transform = CGAffineTransform.MakeIdentity();
                }, delegate
                {
                    UpdateFrame(new CGRect(this.Frame.X, this.Frame.Y, penPicker.Frame.Right, this.Frame.Height));
                });
            }
        }

        private void HidePenPickerBar(bool updateFrame)
        {
            penButton.Selected = false;

            if (penPicker != null)
            {
                penPicker.Transform = CGAffineTransform.MakeIdentity();
                UIView.Animate(0.2d, 0d, UIViewAnimationOptions.CurveEaseOut, delegate
                {
                    penPicker.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f);
                }, delegate
                {
                    if (updateFrame)
                    {
                        UpdateFrame(new CGRect(this.Frame.X, this.Frame.Y, backgroundView.Frame.Right, this.Frame.Height));
                    }

                    penPicker.RemoveFromSuperview();
                    penPicker.Dispose();
                    penPicker = null;
                });
            }
        }

        private void UpdatePenIcon()
        {
            if (highlighterMode)
            {
                penButton.SetImage(UIImage.FromBundle("Assets/Icons/highlighter.png"), UIControlState.Normal);
                penButton.SetImage(UIImage.FromBundle("Assets/Icons/highlighter.png"), UIControlState.Highlighted);
            }
            else
            {
                penButton.SetImage(UIImage.FromBundle("Assets/Icons/pen.png"), UIControlState.Normal);
                penButton.SetImage(UIImage.FromBundle("Assets/Icons/pen.png"), UIControlState.Highlighted);
            }
        }

        void HandlePenButtonTouchUpInside(object sender, EventArgs e)
        {
            if (penPicker == null)
            {
                penPicker = new PenPickerBar(new CGRect(backgroundView.Frame.Right, penButton.Frame.Y - ((85f - penButton.Frame.Height) / 2f), 180, 85));
                penPicker.PenSelectedEvent += HandlePenSelectedEvent;
                penPicker.HighlighterSelectedEvent += HandleHighlighterSelectedEvent;
                this.AddSubview(penPicker);

                ShowPenPickerBar();
            }
            else
            {
                HidePenPickerBar(true);
            }
        }

        void HandlePenSelectedEvent()
        {
            highlighterMode = false;

            HidePenPickerBar(true);

            UpdateColorIcon();
            UpdatePenIcon();

            if (PenSelectedEvent != null)
            {
                PenSelectedEvent();
            }
        }

        void HandleHighlighterSelectedEvent()
        {
            highlighterMode = true;

            HidePenPickerBar(true);

            UpdateColorIcon();
            UpdatePenIcon();

            if (HighlighterSelectedEvent != null)
            {
                HighlighterSelectedEvent();
            }
        }

        #endregion

        #region ColorButton

        private void AddColorButton()
        {
            colorButton = new ColorButton(new CGRect(this.Frame.Width / 2 - 36, penButton.Frame.Bottom + 8, 72, 72), highlighterMode);
            colorButton.TouchUpInside += HandleColorButtonTouchUpInside;
            this.AddSubview(colorButton);
        }

        private void ShowColorPickerBar()
        {
            HidePickers(StringRef.Color);

            colorButton.Selected = true;

            if (colorPicker != null)
            {
                colorPicker.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f);
                UIView.Animate(0.2d, 0d, UIViewAnimationOptions.CurveEaseOut, delegate
                {
                    colorPicker.Transform = CGAffineTransform.MakeIdentity();
                }, delegate
                {
                    UpdateFrame(new CGRect(this.Frame.X, this.Frame.Y, colorPicker.Frame.Right, this.Frame.Height));
                });
            }
        }

        private void HideColorPickerBar(bool updateFrame)
        {
            colorButton.Selected = false;

            if (colorPicker != null)
            {
                colorPicker.Transform = CGAffineTransform.MakeIdentity();
                UIView.Animate(0.2d, 0d, UIViewAnimationOptions.CurveEaseOut, delegate
                {
                    colorPicker.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f);
                }, delegate
                {
                    if (updateFrame)
                    {
                        UpdateFrame(new CGRect(this.Frame.X, this.Frame.Y, backgroundView.Frame.Right, this.Frame.Height));
                    }

                    colorPicker.RemoveFromSuperview();
                    colorPicker.Dispose();
                    colorPicker = null;
                });
            }
        }

        private void UpdateColorIcon()
        {
            if (colorButton != null)
            {
                colorButton.UpdateColor(highlighterMode);
            }
        }

        void HandleColorButtonTouchUpInside(object sender, EventArgs e)
        {
            if (colorPicker == null)
            {
                colorPicker = new ColorPickerBar(new CGRect(backgroundView.Frame.Right, colorButton.Frame.Y - ((83f - colorButton.Frame.Height) / 2f), 180, 83), highlighterMode);
                colorPicker.ColorSelectedEvent += HandleColorSelectedEvent;
                this.AddSubview(colorPicker);

                ShowColorPickerBar();
            }
            else
            {
                HideColorPickerBar(true);
            }
        }

        void HandleColorSelectedEvent(UIColor color)
        {
            UpdateColorIcon();

            HideColorPickerBar(true);

            if (ColorSelectedEvent != null)
            {
                ColorSelectedEvent();
            }
        }

        #endregion

        #region UndoButton

        private void AddUndoButton()
        {
            undoButton = UIButton.FromType(UIButtonType.Custom);
            undoButton.Frame = new CGRect(10, colorButton.Frame.Bottom + 14, 41, 41);
            undoButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/undo_unpressed.png"), UIControlState.Normal);
            undoButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/undo_pressed.png"), UIControlState.Highlighted);
            undoButton.TouchUpInside += HandleUndoButtonTouchUpInside;
            this.AddSubview(undoButton);

            EnableUndoButton(true);
        }

        public void EnableUndoButton(bool enable)
        {
            undoButton.Enabled = enable;
        }

        void HandleUndoButtonTouchUpInside(object sender, EventArgs e)
        {
            if (UndoEvent != null)
            {
                UndoEvent();
            }
        }

        #endregion

        #region RedoButton

        private void AddRedoButton()
        {
            redoButton = UIButton.FromType(UIButtonType.Custom);
            redoButton.Frame = new CGRect(this.Frame.Width - 51, undoButton.Frame.Y, 41, 41);
            redoButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/redo_unpressed.png"), UIControlState.Normal);
            redoButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/redo_pressed.png"), UIControlState.Highlighted);
            redoButton.TouchUpInside += HandleRedoButtonTouchUpInside;
            this.AddSubview(redoButton);

            EnableRedoButton(true);
        }

        public void EnableRedoButton(bool enable)
        {
            redoButton.Enabled = enable;
        }

        void HandleRedoButtonTouchUpInside(object sender, EventArgs e)
        {
            if (RedoEvent != null)
            {
                RedoEvent();
            }
        }

        #endregion

        #region DoneButton

        private void AddDoneButton()
        {
            doneButton = UIButton.FromType(UIButtonType.Custom);
            doneButton.Frame = new CGRect(this.Frame.Width / 2 - (81f / 2f), backgroundView.Frame.Bottom - 72, 81, 24);
            doneButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/boxed.png"), UIControlState.Normal);
            doneButton.SetTitle("DONE", UIControlState.Normal);
            doneButton.Font = eBriefingAppearance.ThemeRegularFont(14);
            doneButton.TouchUpInside += HandleDoneButtonTouchUpInside;
            this.AddSubview(doneButton);
        }

        void HandleDoneButtonTouchUpInside(object sender, EventArgs e)
        {
            if (DoneEvent != null)
            {
                DoneEvent();
            }

            HidePickers(StringRef.Pen);
            HidePickers(StringRef.Color);

            UpdateFrame(new CGRect(this.Frame.X, this.Frame.Y, backgroundView.Frame.Right, this.Frame.Height));
        }

        #endregion

        #region CancelButton

        private void AddCancelButton()
        {
            cancelButton = UIButton.FromType(UIButtonType.Custom);
            cancelButton.Frame = new CGRect(this.Frame.Width / 2 - (81f / 2f), doneButton.Frame.Bottom + 8, 81, 24);
            cancelButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/boxed.png"), UIControlState.Normal);
            cancelButton.SetTitle("CANCEL", UIControlState.Normal);
            cancelButton.Font = eBriefingAppearance.ThemeRegularFont(14);
            cancelButton.TouchUpInside += HandleCancelButtonTouchUpInside;
            this.AddSubview(cancelButton);
        }

        void HandleCancelButtonTouchUpInside(object sender, EventArgs e)
        {
            UIAlertView alert = new UIAlertView(StringRef.confirmation, "Cancel your changes?", null, StringRef.no, StringRef.yes);
            alert.Dismissed += (object sender1, UIButtonEventArgs e1) =>
            {
                if (e1.ButtonIndex == 1)
                {
                    Cancelled();
                }
            };
            alert.Show();
        }

        void Cancelled()
        {
            if (CancelEvent != null)
            {
                CancelEvent();
            }

            HidePickers(StringRef.Pen);
            HidePickers(StringRef.Color);

            UpdateFrame(new CGRect(this.Frame.X, this.Frame.Y, backgroundView.Frame.Right, this.Frame.Height));
        }

        #endregion
    }
}


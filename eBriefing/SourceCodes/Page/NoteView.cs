using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace eBriefingMobile
{
    public class NoteView : UIView
    {
        private UIInterfaceOrientation orientation;
        private UIImageView pullTab;
        private UIImageView barView;
        private CustomTextView textView;
        private UIScrollView scrollView;
        private UILabel placeholder;
        private float locationX;
        private float startX;
        private float leftLimitX;
        private float rightLimitX;
        private KeyboardAutoScroll autoScroll;
        private String pageID;
        public static float ARROW_WIDTH = 32;
        public static float ARROW_HEIGHT = 75;

        public Book NoteBook { get; set; }

        public delegate void TouchFinishedDelegate();

        public event TouchFinishedDelegate TouchFinishedEvent;

        public NoteView(RectangleF rectangle, UIInterfaceOrientation orientation) : base(rectangle)
        {
            this.BackgroundColor = UIColor.Clear;
            this.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            this.orientation = orientation;

            // pullTab
            pullTab = new UIImageView(UIImage.FromBundle("Assets/Buttons/pull_tab.png"));
            pullTab.Frame = new RectangleF(0, rectangle.Height / 2 - 25, ARROW_WIDTH, ARROW_HEIGHT);
            this.AddSubview(pullTab);

            // barView
            barView = new UIImageView();
            barView.Image = UIImage.FromBundle("Assets/Backgrounds/vertical_bar.png").CreateResizableImage(new UIEdgeInsets(10, 0, 10, 0));
            barView.Frame = new RectangleF(pullTab.Frame.Right, 0, barView.Image.Size.Width, rectangle.Height);
            this.AddSubview(barView);

            // scrollView
            scrollView = new UIScrollView();
            scrollView.Frame = new RectangleF(barView.Frame.Right, 0, rectangle.Width - barView.Frame.Right, rectangle.Height);
            scrollView.ContentSize = new SizeF(scrollView.Frame.Width, scrollView.Frame.Height);
            scrollView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            scrollView.BackgroundColor = eBriefingAppearance.Color(44, 44, 44);
            this.AddSubview(scrollView);

            // textView
            textView = new CustomTextView();
            textView.Frame = new RectangleF(0, 0, scrollView.Frame.Width, scrollView.Frame.Height);
            textView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            textView.BackgroundColor = eBriefingAppearance.Color(44, 44, 44);
            textView.TextColor = UIColor.White;
            textView.Font = UIFont.SystemFontOfSize(17f);
            textView.SpellCheckingType = UITextSpellCheckingType.No;
            textView.AutocorrectionType = UITextAutocorrectionType.Yes;

            scrollView.AddSubview(textView);

            // placeholder
            placeholder = new UILabel();
            placeholder.Frame = new RectangleF(barView.Frame.Right + 10, textView.Center.Y, textView.Frame.Width - 20, 42);
            placeholder.Font = UIFont.BoldSystemFontOfSize(17f);
            placeholder.TextAlignment = UITextAlignment.Center;
            placeholder.BackgroundColor = UIColor.Clear;
            placeholder.TextColor = UIColor.White;
            placeholder.Lines = 0;
            placeholder.LineBreakMode = UILineBreakMode.WordWrap;
            placeholder.Text = "Tap to enter notes for the current page";
            placeholder.SizeToFit();
            placeholder.Alpha = 0f;
            this.AddSubview(placeholder);

            RegisterTextViewEvents();

            // Register tableView Keyboard Notification for auto scroll
            autoScroll = new KeyboardAutoScroll();
            autoScroll.RegisterForKeyboardNotifications(textView, KeyboardAutoScroll.ScrollType.TEXTVIEW);

            leftLimitX = rectangle.X - rectangle.Width;
            rightLimitX = rectangle.X - ARROW_WIDTH;
        }

        /// <summary>
        /// This event occurs when you just touch the object
        /// </summary>
        /// <param name="touches">Touches.</param>
        /// <param name="e">E.</param>
        public override void TouchesBegan(NSSet touches, UIEvent e)
        {
            // Hide keyboard on touch
            HideKeyboard();

            locationX = this.Frame.Location.X;
            
            var touch = (UITouch)e.TouchesForView(this).AnyObject;
            var bounds = Bounds;
            
            startX = touch.LocationInView(this).X;
            this.Frame = new RectangleF(new PointF(locationX, this.Frame.Y), bounds.Size);
        }

        /// <summary>
        /// This event occurs when you drag it around
        /// </summary>
        /// <param name="touches">Touches.</param>
        /// <param name="e">E.</param>
        public override void TouchesMoved(NSSet touches, UIEvent e)
        {
            var touch = (UITouch)e.TouchesForView(this).AnyObject;
            var bounds = Bounds;
            
            // Always refer to the StartLocation of the object that you've been dragging.
            locationX += touch.LocationInView(this).X - startX;

            if (locationX >= leftLimitX && locationX <= rightLimitX)
            {
                this.Frame = new RectangleF(new PointF(locationX, this.Frame.Y), bounds.Size);

                // UITextView does not draw text until it is shown
                // Set textView frame when dragging rather than from initialization
                textView.Frame = textView.Frame;
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent e)
        {
            startX = locationX;

            if (TouchFinishedEvent != null)
            {
                TouchFinishedEvent();
            }
        }

        public void UnregisterKeyboardNotifications()
        {
            autoScroll.UnregisterKeyboardNotifications();
        }

        public void HideKeyboard()
        {
            if (textView.IsFirstResponder)
            {
                textView.ResignFirstResponder();
            }
        }

        public void UpdateNote(String pageID)
        {
            // Save previous note
            SaveNote();

            // Show new note
            this.pageID = pageID;

            Note note = BooksOnDeviceAccessor.GetNote(NoteBook.ID, pageID);
            if (note == null)
            {
                textView.Text = String.Empty;

                UpdateOpacity(0.7f, 0.7f);
            }
            else
            {
                textView.Text = note.Text;

                UpdateOpacity(0f, 0.7f);
            }
        }

        private void SaveNote()
        {
            HideKeyboard();

            if (!String.IsNullOrEmpty(NoteBook.ID) && !String.IsNullOrEmpty(pageID))
            {
                NoteUpdater.SaveNote(NoteBook, pageID, textView.Text);
            }
        }

        private void UpdateOpacity(float placeAlpha, float scrollAlpha)
        {
            UIView.Animate(0.3d, delegate
            {
                placeholder.Alpha = placeAlpha;
                textView.Alpha = scrollAlpha;
            });
        }

        private void RegisterTextViewEvents()
        {
            textView.ShouldBeginEditing = delegate
            {
                // To prevent text showing up behind keyboard, resize the textview
                float keyboardHeight = 264 - 44f;
                if (orientation == UIInterfaceOrientation.LandscapeLeft || orientation == UIInterfaceOrientation.LandscapeRight)
                {
                    keyboardHeight = 352 - 44f;
                }

                textView.Frame = new RectangleF(textView.Frame.X, textView.Frame.Y, textView.Frame.Width, this.Frame.Height - keyboardHeight);

                UpdateOpacity(0f, 1f);

                return true;
            };

            textView.ShouldEndEditing = delegate
            {
                // Resize back to the original size
                textView.Frame = new RectangleF(textView.Frame.X, textView.Frame.Y, textView.Frame.Width, this.Frame.Height);

                if (String.IsNullOrEmpty(textView.Text))
                {
                    UpdateOpacity(0.7f, 0.7f);
                }
                else
                {
                    UpdateOpacity(0f, 0.7f);
                }

                SaveNote();
                
                return true;
            };
        }
    }
}


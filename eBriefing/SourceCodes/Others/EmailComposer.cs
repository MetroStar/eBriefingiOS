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
using MssFramework;
using MessageUI;
using UIKit;
using Foundation;

namespace eBriefingMobile
{
	public class EmailComposer
	{
		public MFMailComposeViewController ComposeController { get; set; }

		public String Recipient { get; set; }

		public String Subject { get; set; }

		public String Body { get; set; }

		public String[] Attachments { get; set; }

		public bool IsHtml { get; set; }

		public delegate void EmailSentDelegate();

		public event EmailSentDelegate EmailSentEvent;

		public EmailComposer()
		{
			if (MFMailComposeViewController.CanSendMail)
			{
				ComposeController = new MFMailComposeViewController ();
				ComposeController.MailComposeDelegate = new CustomMailComposeDelegate ();
			}
		}

		public void PresentViewController(UIViewController parent)
		{
			if (MFMailComposeViewController.CanSendMail)
			{
				// Recipients
				if (!String.IsNullOrEmpty(Recipient))
				{
					String[] recipients = Recipient.Split(new char[]
					{
						' ',
						';',
						',',
						'|'
					}, StringSplitOptions.RemoveEmptyEntries);
					ComposeController.SetToRecipients(recipients);
				}

				// Subject
				if (!String.IsNullOrEmpty(Subject))
				{
					ComposeController.SetSubject(Subject);
				}

				// Body
				if (!String.IsNullOrEmpty(Body))
				{
					ComposeController.SetMessageBody(Body, IsHtml);
				}

				// Attachment
				if (Attachments != null)
				{
					ComposeController.AddAttachmentData(NSData.FromFile(Attachments[0]), Attachments[1], Attachments[2]);
				}

				ComposeController.NavigationBar.TintColor = eBriefingAppearance.BlueColor;

				parent.PresentViewController(ComposeController, true, delegate
				{
					UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.Default, false);
				});
			}
		}

		void HandleEmailSentEvent()
		{
			if (EmailSentEvent != null)
			{
				EmailSentEvent();
			}
		}

		private class CustomMailComposeDelegate : MFMailComposeViewControllerDelegate
		{
			public override void Finished(MFMailComposeViewController controller, MFMailComposeResult result, NSError error)
			{
				switch (result)
				{
				case MFMailComposeResult.Failed:
					MessageBox alert = new MessageBox();
					alert.ShowAlert("Email Failed", error.Description, "Ok");
					break;
				default:
					break;
				}

				this.InvokeOnMainThread(delegate
				{
					controller.DismissViewController(true, delegate
					{
						if (controller != null)
						{
							// Dispose of the view object.
							controller.Dispose();
							controller = null;
						}
					});
				});

			}
		}
	}
}


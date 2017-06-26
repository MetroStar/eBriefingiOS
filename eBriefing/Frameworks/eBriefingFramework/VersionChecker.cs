using System;
using System.IO;
using Foundation;
using UIKit;

namespace eBriefingMobile
{
	public class VersionChecker
	{
		public static bool IsIOS8OrBetter
		{
			get
			{
				String version = UIDevice.CurrentDevice.SystemVersion;
				String[] versionElements = version.Split('.');

				if (versionElements.Length > 0)
				{
					int versionInt = 0;
					int minorVersion = 0;
					if (int.TryParse(versionElements[0], out versionInt))
					{
						if (int.TryParse(versionElements[1], out minorVersion))
						{
							if (versionInt >= 8)
								return true;
						}
					}

					return false;
				}

				return false;
			}
		}

		public static bool IsIOS7OrBetter
		{
			get
			{
				String version = UIDevice.CurrentDevice.SystemVersion;
				String[] versionElements = version.Split('.');

				if (versionElements.Length > 0)
				{
					int versionInt = 0;
					int minorVersion = 0;
					if (int.TryParse(versionElements[0], out versionInt))
					{
						if (int.TryParse(versionElements[1], out minorVersion))
						{
							if (versionInt >= 7)
								return true;
						}
					}

					return false;
				}

				return false;
			}
		}

		public static bool IsTall
		{
			get
			{
				return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone && UIScreen.MainScreen.Bounds.Height * UIScreen.MainScreen.Scale >= 1136;
			}
		}

		public static UIImage FromBundle16x9(String path)
		{
			// Adopt the -568h@2x naming convention
			if (IsTall)
			{
				var imagePath = Path.GetDirectoryName(path);
				var imageFile = Path.GetFileNameWithoutExtension(path);
				var imageExt = Path.GetExtension(path);
				imageFile = imageFile + "-568h@2x" + imageExt;
				return UIImage.FromFile(Path.Combine(imagePath, imageFile));
			}
			else
			{
				return UIImage.FromBundle(path);
			}
		}
	}
}


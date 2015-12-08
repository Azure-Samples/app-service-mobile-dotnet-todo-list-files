using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace MobileAppsFilesSample.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
        UIWindow window;

        public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init ();

			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

            // IMPORTANT: uncomment this code to enable sync on Xamarin.iOS
            // For more information, see: http://go.microsoft.com/fwlink/?LinkId=620342
            SQLitePCL.CurrentPlatform.Init();

            LoadApplication(App.InitAsync().Result);

            window = new UIWindow(UIScreen.MainScreen.Bounds);

            App.UIContext = window.RootViewController;

            return base.FinishedLaunching (app, options);
		}

    }
}


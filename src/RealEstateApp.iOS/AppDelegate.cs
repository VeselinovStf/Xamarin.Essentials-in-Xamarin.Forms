﻿using Foundation;
using UIKit;
using Xamarin.Essentials;

namespace RealEstateApp.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            global::Xamarin.Essentials.Platform = 

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}

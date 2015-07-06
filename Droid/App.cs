using System;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Xamarin.Forms;

namespace MobileAppsFilesSample
{
	public class App : Application
	{
		public App ()
		{
			// The root page of your application
            MainPage = new NavigationPage(new TodoList());
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}

        public static Android.Content.Context UIContext { get; set; }
	}
}


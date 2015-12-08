using System.Threading.Tasks;
using Xamarin.Forms;

namespace MobileAppsFilesSample
{
    public class App : Application
    {
        private App () { }

        public static async Task<App> InitAsync()
        {
            var app = new App();

            var todoListPage = await TodoList.CreateAsync();
            app.MainPage = new NavigationPage(todoListPage);

            return app;
        }

        public static object UIContext { get; set; }

        protected override void OnStart () { }

        protected override void OnSleep () { }

        protected override void OnResume () { }
    }
}


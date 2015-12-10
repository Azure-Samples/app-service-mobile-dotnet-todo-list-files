using System.Threading.Tasks;
using MobileAppsFilesSample.ViewModels;
using Xamarin.Forms;

namespace MobileAppsFilesSample
{
    public class App : Application
    {
        public App()
        {            
            this.MainPage = new NavigationPage(new TodoList(new TodoListViewModel()));
        }

        public static object UIContext { get; set; }

        protected override void OnStart () { }

        protected override void OnSleep () { }

        protected override void OnResume () { }
    }
}


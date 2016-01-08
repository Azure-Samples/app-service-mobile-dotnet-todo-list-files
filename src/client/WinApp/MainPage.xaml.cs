namespace WinApp
{
    public sealed partial class MainPage 
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new MobileAppsFilesSample.App());
        }
    }
}

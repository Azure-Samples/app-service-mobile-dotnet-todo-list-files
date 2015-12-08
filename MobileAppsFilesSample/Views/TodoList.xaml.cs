using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;

namespace MobileAppsFilesSample
{
    public partial class TodoList : ContentPage
    {
        TodoItemManager manager;

        public TodoList()
        {
            InitializeComponent();

            manager = new TodoItemManager();

            // OnPlatform<T> doesn't currently support the "Windows" target platform, so we have this check here.
            if (Device.OS == TargetPlatform.Windows || Device.OS == TargetPlatform.WinPhone)
            {
                syncButton.IsVisible = true;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (todoList.ItemsSource == null)
            {
                await SyncItemsAsync(true);
                await LoadItems();
            }
        }

        private async Task LoadItems()
        {
            IEnumerable<TodoItem> items = await manager.GetTodoItemsAsync();
            var viewModels = new List<TodoItemViewModel>();

            foreach (var i in items) {
                viewModels.Add(await TodoItemViewModel.CreateAsync(i, this.manager));
            }

            todoList.ItemsSource = viewModels;
        }

        // Data methods
        private async Task AddItem(TodoItem item)
        {
            await manager.SaveTaskAsync(item);
            await LoadItems();
        }

        async Task DeleteItem(TodoItem item)
        {
            await manager.DeleteTaskAsync(item);
            await LoadItems();
        }

        public async void OnAdd(object sender, EventArgs e)
        {
            var todo = new TodoItem { Name = newItemName.Text };
            await AddItem(todo);
            newItemName.Text = "";
            newItemName.Unfocus();
        }

        // Event handlers
        public async void OnSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var todo = e.SelectedItem as TodoItemViewModel;

            if (todo != null)
            {
                var detailsView = new TodoItemDetailsView();
                detailsView.BindingContext = todo;

                await Navigation.PushAsync(detailsView);
            }

            //if (Device.OS != TargetPlatform.iOS && todo != null)
            //{
            //    // Not iOS - the swipe-to-delete is discoverable there
            //    if (Device.OS == TargetPlatform.Android)
            //    {
            //        await DisplayAlert(todo.Name, "Press-and-hold to delete task " + todo.Name, "Got it!");
            //    }
            //    else
            //    {
            //        // Windows, not all platforms support the Context Actions yet
            //        if (await DisplayAlert("Delete?", "Do you wish to delete " + todo.Name + "?", "Delete", "Cancel"))
            //        {
            //            await DeleteItem(todo);
            //        }
            //    }
            //}
            // prevents background getting highlighted
            todoList.SelectedItem = null;
        }

        // http://developer.xamarin.com/guides/cross-platform/xamarin-forms/working-with/listview/#context
        public async void OnDelete(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var todoViewModel = mi.CommandParameter as TodoItemViewModel;
            await DeleteItem(todoViewModel.GetItem());
        }

        // http://developer.xamarin.com/guides/cross-platform/xamarin-forms/working-with/listview/#pulltorefresh
        public async void OnRefresh(object sender, EventArgs e)
        {
            var list = (ListView)sender;
            var success = false;
            try
            {
                await SyncItemsAsync(false);
                success = true;
            }
            catch (Exception)
            {
                // requires C# 6
                //await DisplayAlert ("Refresh Error", "Couldn't refresh data ("+ex.Message+")", "OK");
            }
            list.EndRefresh();
            if (!success)
                await DisplayAlert("Refresh Error", "Couldn't refresh data", "OK");
        }

        public async void OnSyncItems(object sender, EventArgs e)
        {
            await SyncItemsAsync(true);
        }

        private async Task SyncItemsAsync(bool showActivityIndicator)
        {
            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator))
            {
                await manager.SyncAsync();
                await LoadItems();
            }
        }

        private class ActivityIndicatorScope : IDisposable
        {
            private bool showIndicator;
            private ActivityIndicator indicator;
            private Task indicatorDelay;

            public ActivityIndicatorScope(ActivityIndicator indicator, bool showIndicator)
            {
                this.indicator = indicator;
                this.showIndicator = showIndicator;

                if (showIndicator)
                {
                    indicatorDelay = Task.Delay(2000);
                    SetIndicatorActivity(true);
                }
                else
                {
                    indicatorDelay = Task.FromResult(0);
                }
            }

            private void SetIndicatorActivity(bool isActive)
            {
                this.indicator.IsVisible = isActive;
                this.indicator.IsRunning = isActive;
            }

            public void Dispose()
            {
                if (showIndicator)
                {
                    indicatorDelay.ContinueWith(t => SetIndicatorActivity(false), TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
    }
}


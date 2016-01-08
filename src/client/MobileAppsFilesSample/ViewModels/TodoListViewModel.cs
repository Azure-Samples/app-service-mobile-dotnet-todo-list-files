using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Xamarin.Forms;

namespace MobileAppsFilesSample.ViewModels
{
    public class TodoListViewModel : ViewModel
    {
        private TodoItemManager manager;
        private string newItemText = "";

        private ObservableCollection<TodoItemViewModel> todoItems;
        private long pendingChanges;
        private bool isStatusBarVisible;

        public ObservableCollection<TodoItemViewModel> TodoItems
        {
            get
            {
                return todoItems;
            }
            set
            {
                todoItems = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddItemCommand { get; set; }
        public ICommand DeleteItemCommand { get; set; }

        public string NewItemText
        {
            get
            {
                return newItemText;
            }

            set
            {
                newItemText = value;
                OnPropertyChanged();
            }
        }

        public long PendingChanges
        {
            get
            {
                return pendingChanges;
            }

            set
            {
                pendingChanges = value;
                OnPropertyChanged();
            }
        }

        public bool IsStatusBarVisible
        {
            get
            {
                return isStatusBarVisible;
            }

            set
            {
                isStatusBarVisible = value;
                OnPropertyChanged();
            }
        }

        public TodoListViewModel()
        {
            InitCommands();

            TodoItemManager.CreateAsync().ContinueWith(x => {
                this.manager = x.Result;
                this.manager.MobileServiceClient.EventManager.Subscribe<StoreOperationCompletedEvent>(StoreOperationEventHandler);

                Device.BeginInvokeOnMainThread(async () => { await SyncItemsAsync(); });
            });
        }

        private async void StoreOperationEventHandler(StoreOperationCompletedEvent mobileServiceEvent)
        {
            await Task.Delay(500);
            PendingChanges = manager.MobileServiceClient.SyncContext.PendingOperations;
            IsStatusBarVisible = PendingChanges > 0;
        }

        private void InitCommands()
        {
            this.AddItemCommand = new DelegateCommand(AddItem);
            this.DeleteItemCommand = new DelegateCommand(DeleteItem);
        }

        private async Task LoadItems()
        {
            IEnumerable<TodoItem> items = await manager.GetTodoItemsAsync();
            TodoItems = new ObservableCollection<TodoItemViewModel>();

            foreach (var i in items) {
                TodoItems.Add(await TodoItemViewModel.CreateAsync(i, this.manager));
                Debug.WriteLine("Created view model for: " + i.Name);
            }
        }

        public async Task SyncItemsAsync()
        {
            await manager.SyncAsync();
            await LoadItems();
        }

        private async void AddItem(object data)
        {
            var newItem = new TodoItem();
            newItem.Name = NewItemText;

            await manager.SaveTaskAsync(newItem);
            await LoadItems();

            NewItemText = "";
        }

        async void DeleteItem(object data)
        {
            var viewModel = data as TodoItemViewModel;

            await manager.DeleteTaskAsync(viewModel.GetItem());
            await LoadItems();
        }

        public async Task NavigateToDetailsView(TodoItemViewModel todo, INavigation navigation)
        {
            await todo.LoadImagesAsync(); // reload images as they download asynchronously
            var detailsView = new TodoItemDetailsView();
            detailsView.BindingContext = todo;

            await navigation.PushAsync(detailsView);
        }
    }
}

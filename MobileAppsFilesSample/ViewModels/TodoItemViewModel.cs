using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.WindowsAzure.MobileServices.Files;
using Xamarin.Forms;

namespace MobileAppsFilesSample
{
    public class TodoItemViewModel : ViewModel
    {
        private TodoItem todoItem;
        private ICollection<TodoItemImageViewModel> images;
        private TodoItemManager itemManager;

        private TodoItemViewModel()
        {
        }

        public static async Task<TodoItemViewModel> CreateAsync(TodoItem todoItem, TodoItemManager itemManager)
        {
            if (todoItem == null) {
                throw new ArgumentNullException("todoItem");
            }

            if (itemManager == null) {
                throw new ArgumentNullException("itemManager");
            }

            TodoItemViewModel result = new TodoItemViewModel();

            result.todoItem = todoItem;
            result.itemManager = itemManager;

            await result.InitializeImagesAsync(todoItem);
            result.InitializeCommands();

            return result;
        }

        private void InitializeCommands()
        {
            AddImageCommand = new DelegateCommand(AddImage);
        }

        private async Task InitializeImagesAsync(TodoItem todoItem)
        {
            IEnumerable<MobileServiceFile> files = await this.itemManager.GetImageFilesAsync(todoItem);
            this.Images = new ObservableCollection<TodoItemImageViewModel>();

            foreach (var f in files) {
                var viewModel = await TodoItemImageViewModel.CreateAsync(f, this.todoItem, DeleteImage);
                this.Images.Add(viewModel);
            }
        }

        private async void DeleteImage(TodoItemImageViewModel imageViewModel)
        {
            await this.itemManager.DeleteImage(this.todoItem, imageViewModel.File);

            this.images.Remove(imageViewModel);
        }

        public ICommand AddImageCommand { get; set; }

        public string Name
        {
            get
            {
                return this.todoItem.Name;
            }
            set
            {
                if (string.Compare(this.todoItem.Name, value) != 0)
                {
                    this.todoItem.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICollection<TodoItemImageViewModel> Images
        {
            get { return images; }
            set { images = value; }
        }

        private async void AddImage(object obj)
        {
            IPlatform mediaProvider = DependencyService.Get<IPlatform>();
            string sourceImagePath = await mediaProvider.TakePhotoAsync(App.UIContext);

            //var mediaPicker = new MediaPicker(App.UIContext);
            //var photo = await mediaPicker.TakePhotoAsync(new StoreCameraMediaOptions());

            if (sourceImagePath != null)
            {
                MobileServiceFile file = await this.itemManager.AddImage(this.todoItem, sourceImagePath);

                var image = await TodoItemImageViewModel.CreateAsync(file, this.todoItem, DeleteImage);
                this.images.Add(image);
            }
        }

        internal TodoItem GetItem()
        {
            return this.todoItem;
        }
    }
}

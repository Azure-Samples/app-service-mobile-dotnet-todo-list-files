using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.WindowsAzure.MobileServices.Files;
using Xamarin.Media;
using MobileAppsFilesSample.Droid;

namespace MobileAppsFilesSample
{
    public class TodoItemViewModel : ViewModel
    {
        private TodoItem todoItem;
        private ICollection<TodoItemImageViewModel> images;
        private TodoItemManager itemManager;

        public TodoItemViewModel(TodoItem todoItem, TodoItemManager itemManager)
        {
            if (todoItem == null)
            {
                throw new ArgumentNullException("todoItem");
            }

            if (itemManager == null)
            {
                throw new ArgumentNullException("itemManager");
            }

            this.todoItem = todoItem;
            this.itemManager = itemManager;

            InitializeImages(todoItem);
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            AddImageCommand = new DelegateCommand(AddImage);
        }

        private async void InitializeImages(TodoItem todoItem)
        {
            IEnumerable<MobileServiceFile> files = await this.itemManager.GetImageFiles(todoItem);

            this.Images = new ObservableCollection<TodoItemImageViewModel>(files.Select(f => new TodoItemImageViewModel(f, this.todoItem, DeleteImage)));
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

        public string Notes
        {
            get
            {
                return this.todoItem.Notes;
            }
        }

        public ICollection<TodoItemImageViewModel> Images
        {
            get { return images; }
            set { images = value; }
        }

        private async void AddImage(object obj)
        {
            var mediaPicker = new MediaPicker(App.UIContext);
            var photo = await mediaPicker.TakePhotoAsync(new StoreCameraMediaOptions());

            MobileServiceFile file = await this.itemManager.AddImage(this.todoItem, photo.Path);

            var image = new TodoItemImageViewModel(file, this.todoItem, DeleteImage);

            this.images.Add(image);
        }

        internal TodoItem GetItem()
        {
            return this.todoItem;
        }
    }
}

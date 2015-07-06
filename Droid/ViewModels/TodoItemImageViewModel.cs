using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.WindowsAzure.MobileServices.Files;
using MobileAppsFilesSample.Droid;

namespace MobileAppsFilesSample
{
    public class TodoItemImageViewModel : ViewModel
    {
        private string name;
        private string uri;
        private Action<TodoItemImageViewModel> deleteHandler;

        public TodoItemImageViewModel(MobileServiceFile file, TodoItem todoItem, Action<TodoItemImageViewModel> deleteHandler)
        {
            this.deleteHandler = deleteHandler;
            this.uri = FileHelper.GetLocalFilePath(todoItem.Id, file.Name);
            this.name = file.Name;

            this.File = file;

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            DeleteCommand = new DelegateCommand(o => deleteHandler(this));
        }

        public ICommand DeleteCommand { get; set; }

        public string Uri
        {
            get { return uri; }
            set
            {
                if (string.Compare(uri, value) != 0)
                {
                    uri = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (string.Compare(name, value) != 0)
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }

        public MobileServiceFile File { get; private set; }
    }
}

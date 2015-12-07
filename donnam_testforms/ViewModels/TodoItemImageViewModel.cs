using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.WindowsAzure.MobileServices.Files;

namespace donnam_testforms
{
    public class TodoItemImageViewModel : donnam_testforms.ViewModel
    {
        private string name;
        private string uri;
        private Action<TodoItemImageViewModel> deleteHandler;

        private TodoItemImageViewModel() { }

        public static async Task<TodoItemImageViewModel> CreateAsync(MobileServiceFile file, TodoItem todoItem, Action<TodoItemImageViewModel> deleteHandler)
        {
            var result = new TodoItemImageViewModel();

            result.deleteHandler = deleteHandler;
            result.name = file.Name;
            result.File = file;
            result.uri = await FileHelper.GetLocalFilePathAsync(todoItem.Id, file.Name);

            result.InitializeCommands();

            return result;
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

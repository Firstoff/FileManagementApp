using FileManagementApp.Models;
using FileManagementApp.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

//<a href="https://www.flaticon.com/ru/free-icons/" title=" иконки"> иконки от Flat Icons - Flaticon</a>

namespace FileManagementApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<FileModel> _files;
        public ObservableCollection<FileModel> Files
        {
            get { return _files; }
            set
            {
                _files = value;
                OnPropertyChanged(nameof(Files));
            }
        }

        public FileModel SelectedFile { get; set; }
        public string SearchTerm { get; set; }
        public ICommand SearchCommand { get; }
        public ICommand CloseFileCommand { get; }

        public MainViewModel()
        {
            SearchCommand = new RelayCommand(SearchFiles);
            CloseFileCommand = new RelayCommand(CloseFile);

            //Files = new ObservableCollection<FileModel>();

            // Инициализация файлов
            Files = NetApi32.EnumerateOpenFiles("localhost", ""); // так работает

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SearchFiles(object parameter)
        {
            // Проверяем, что SearchTerm не пустой или не null
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                // Можно добавить логику для обработки случая с пустым SearchTerm, если нужно
                Files = NetApi32.EnumerateOpenFiles("localhost", "");
                return; // Завершаем выполнение метода
            }

            Files.Clear();

            // Получаем найденные файлы, соответствующие поисковому запросу
            var foundFiles = NetApi32.EnumerateOpenFiles("localhost", SearchTerm); // Замените на ваш сервер

            // Добавление найденных файлов в коллекцию
            foreach (var file in foundFiles)
            {
                Files.Add(file);
            }
        }

        private void CloseFile(object parameter)
        {
            if (SelectedFile != null)
            {
                // Показываем диалоговое окно с вопросом
                var result = MessageBox.Show("Вы действительно хотите закрыть этот файл?", "Подтверждение",
                                               MessageBoxButton.YesNo, MessageBoxImage.Question);

                // Проверяем, выбрал ли пользователь "Да"
                if (result == MessageBoxResult.Yes)
                {
                    NetApi32.CloseFileOnServer(SelectedFile.Id, "localhost");
                    Files.Remove(SelectedFile);
                }
            }
        }



        private void CloseFile1(object parameter)
        {
            if (SelectedFile != null)
            {
                NetApi32.CloseFileOnServer(SelectedFile.Id, "localhost");
                Files.Remove(SelectedFile);
            }
        }

    }
}

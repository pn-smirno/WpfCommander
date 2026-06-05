using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace WpfCommander.Models
{
    public class FileSystemItem : INotifyPropertyChanged
    {
        private string _name;
        private string _type;
        private string _size;
        private DateTime _modifiedDate;
        private bool _isDirectory;
        private string _fullPath;

        public event PropertyChangedEventHandler? PropertyChanged;

        public FileSystemItem(string fullPath)
        {
            _fullPath = fullPath;

            try
            {
                if (Directory.Exists(fullPath))
                {
                    _isDirectory = true;
                    var dirInfo = new DirectoryInfo(fullPath);
                    _name = dirInfo.Name;
                    _modifiedDate = dirInfo.LastWriteTime;
                    _type = "Папка";
                    _size = "";
                }
                else if (File.Exists(fullPath))
                {
                    _isDirectory = false;
                    var fileInfo = new FileInfo(fullPath);
                    _name = fileInfo.Name;
                    _modifiedDate = fileInfo.LastWriteTime;
                    _type = string.IsNullOrEmpty(fileInfo.Extension) ? "Файл" : fileInfo.Extension;
                    _size = FormatSize(fileInfo.Length);
                }
                else
                {
                    _name = Path.GetFileName(fullPath);
                    _type = "?";
                    _modifiedDate = DateTime.Now;
                    _size = "";
                }
            }
            catch (UnauthorizedAccessException)
            {
                _name = Path.GetFileName(fullPath);
                _type = "Нет доступа";
                _modifiedDate = DateTime.Now;
                _size = "";
                _isDirectory = false;
            }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        public string Size
        {
            get => _size;
            set { _size = value; OnPropertyChanged(); }
        }

        public DateTime ModifiedDate
        {
            get => _modifiedDate;
            set { _modifiedDate = value; OnPropertyChanged(); }
        }

        public bool IsDirectory
        {
            get => _isDirectory;
            set { _isDirectory = value; OnPropertyChanged(); }
        }

        public string FullPath
        {
            get => _fullPath;
            set { _fullPath = value; OnPropertyChanged(); }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string FormatSize(long bytes)
        {
            string[] sizes = { "Б", "КБ", "МБ", "ГБ", "ТБ" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}

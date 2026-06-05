using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using WpfCommander.Models;
using WpfCommander.Services;

namespace WpfCommander.ViewModels
{
    public class PanelViewModel : INotifyPropertyChanged
    {
        private readonly FileManagerService _fileService;
        private string _currentPath;
        private ObservableCollection<FileSystemItem> _items;
        private FileSystemItem _selectedItem;
        private List<FileSystemItem> _selectedItems;
        private bool _isActive;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PanelViewModel(FileManagerService fileService, string startPath)
        {
            _fileService = fileService;
            _currentPath = startPath;
            _items = new ObservableCollection<FileSystemItem>();
            _selectedItems = new List<FileSystemItem>();
            LoadDirectory(_currentPath);
        }

        public string CurrentPath
        {
            get => _currentPath;
            set
            {
                if (_currentPath != value)
                {
                    _currentPath = value;
                    OnPropertyChanged();
                    LoadDirectory(_currentPath);
                }
            }
        }

        public ObservableCollection<FileSystemItem> Items
        {
            get => _items;
            set { _items = value; OnPropertyChanged(); }
        }

        public FileSystemItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanOperate));
            }
        }

        public List<FileSystemItem> SelectedItems
        {
            get => _selectedItems;
            set
            {
                _selectedItems = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanOperate));
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ActiveBorderBrush));
            }
        }

        public string ActiveBorderBrush => IsActive ? "Blue" : "Gray";
        public bool CanOperate => SelectedItems != null && SelectedItems.Count > 0;

        public void LoadDirectory(string path)
        {
            try
            {
                var items = _fileService.GetDirectoryContents(path);
                Items.Clear();
                foreach (var item in items)
                    Items.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        public void NavigateInto(FileSystemItem item)
        {
            if (item != null && item.IsDirectory)
            {
                if (item.Name == "..")
                {
                    var parent = Directory.GetParent(CurrentPath)?.FullName;
                    if (parent != null)
                        CurrentPath = parent;
                }
                else
                {
                    CurrentPath = item.FullPath;
                }
            }
        }

        public void Refresh() => LoadDirectory(CurrentPath);

        public List<string> GetSelectedPaths()
        {
            var paths = new List<string>();
            foreach (var item in SelectedItems)
            {
                if (item.Name != "..")
                    paths.Add(item.FullPath);
            }
            return paths;
        }

        public void SetSelectedItems(System.Collections.IList selectedItems)
        {
            SelectedItems = selectedItems?.Cast<FileSystemItem>().ToList() ?? new List<FileSystemItem>();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


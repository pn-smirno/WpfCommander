using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfCommander.Services;

namespace WpfCommander.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly FileManagerService _fileService;
        private PanelViewModel _leftPanel;
        private PanelViewModel _rightPanel;
        private PanelViewModel _activePanel;
        private List<string> _drives;

        public ICommand CopyCommand { get; }
        public ICommand MoveCommand { get; }
        public ICommand CreateFolderCommand { get; }
        public ICommand DeleteCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _fileService = new FileManagerService();
            _drives = _fileService.GetDrives();

            string leftDrive = _drives.Count > 0 ? _drives[0] : "C:\\";
            string rightDrive = _drives.Count > 1 ? _drives[1] : _drives[0];

            _leftPanel = new PanelViewModel(_fileService, leftDrive);
            _rightPanel = new PanelViewModel(_fileService, rightDrive);

            _activePanel = _leftPanel;
            _leftPanel.IsActive = true;

            CopyCommand = new RelayCommand(ExecuteCopy, CanExecuteFileOperation);
            MoveCommand = new RelayCommand(ExecuteMove, CanExecuteFileOperation);
            CreateFolderCommand = new RelayCommand(ExecuteCreateFolder);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteFileOperation);
        }

        public List<string> Drives
        {
            get => _drives;
            set { _drives = value; OnPropertyChanged(); }
        }

        public PanelViewModel LeftPanel
        {
            get => _leftPanel;
            set { _leftPanel = value; OnPropertyChanged(); }
        }

        public PanelViewModel RightPanel
        {
            get => _rightPanel;
            set { _rightPanel = value; OnPropertyChanged(); }
        }

        public PanelViewModel ActivePanel
        {
            get => _activePanel;
            set
            {
                _activePanel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PassivePanel));
            }
        }

        public PanelViewModel PassivePanel => ActivePanel == LeftPanel ? RightPanel : LeftPanel;

        private bool CanExecuteFileOperation() => ActivePanel != null && ActivePanel.CanOperate;

        private void ExecuteCopy()
        {
            ExecuteFileOperation(_fileService.Copy, "Копирование");
        }

        private void ExecuteMove()
        {
            ExecuteFileOperation(_fileService.Move, "Перемещение");
        }

        private void ExecuteFileOperation(Func<string, string, bool, bool> operation, string operationName)
        {
            var sourcePaths = ActivePanel.GetSelectedPaths();
            string targetPath = PassivePanel.CurrentPath;

            if (sourcePaths.Count == 0)
            {
                MessageBox.Show("Ничего не выбрано", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int success = 0, fail = 0;

            foreach (string sourcePath in sourcePaths)
            {
                bool isDir = _fileService.IsDirectory(sourcePath);
                string destPath = Path.Combine(targetPath, _fileService.GetItemName(sourcePath));

                if (operation(sourcePath, destPath, isDir))
                    success++;
                else
                    fail++;
            }

            LeftPanel.Refresh();
            RightPanel.Refresh();

            MessageBox.Show($"{operationName} завершено.\nУспешно: {success}, Ошибок: {fail}");
        }

        private void ExecuteCreateFolder()
        {
            string folderName = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите имя новой папки:",
                "Создание папки",
                "Новая папка");

            if (!string.IsNullOrWhiteSpace(folderName))
            {
                _fileService.CreateDirectory(ActivePanel.CurrentPath, folderName);
                ActivePanel.Refresh();
            }
        }

        private void ExecuteDelete()
        {
            var items = ActivePanel.GetSelectedPaths();
            if (items.Count == 0) return;

            var result = MessageBox.Show($"Удалить {items.Count} элемент(ов)?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int success = 0, fail = 0;
                foreach (string path in items)
                {
                    if (_fileService.Delete(path, _fileService.IsDirectory(path)))
                        success++;
                    else
                        fail++;
                }
                ActivePanel.Refresh();
                MessageBox.Show($"Удаление завершено.\nУспешно: {success}, Ошибок: {fail}");
            }
        }

        public void SetActivePanel(PanelViewModel panel)
        {
            if (ActivePanel == panel) return;

            LeftPanel.IsActive = false;
            RightPanel.IsActive = false;
            ActivePanel = panel;
            ActivePanel.IsActive = true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
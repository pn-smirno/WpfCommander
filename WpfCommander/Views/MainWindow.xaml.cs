using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using WpfCommander.ViewModels;

namespace WpfCommander.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning;
            PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        private void LeftPanel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as System.Windows.Controls.ListView;
            if (listView?.SelectedItem is Models.FileSystemItem item)
                _viewModel.LeftPanel.NavigateInto(item);
        }

        private void RightPanel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as System.Windows.Controls.ListView;
            if (listView?.SelectedItem is Models.FileSystemItem item)
                _viewModel.RightPanel.NavigateInto(item);
        }

        private void LeftPanel_GotFocus(object sender, RoutedEventArgs e)
        {
            _viewModel.SetActivePanel(_viewModel.LeftPanel);
        }

        private void RightPanel_GotFocus(object sender, RoutedEventArgs e)
        {
            _viewModel.SetActivePanel(_viewModel.RightPanel);
        }

        // ДОБАВЬТЕ ЭТОТ МЕТОД
        private void LeftPanel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var listView = sender as System.Windows.Controls.ListView;
            if (listView != null && _viewModel != null)
            {
                _viewModel.LeftPanel.SetSelectedItems(listView.SelectedItems);
                Debug.WriteLine($"Left panel selected: {_viewModel.LeftPanel.SelectedItems.Count} items");
            }
        }

        // ДОБАВЬТЕ ЭТОТ МЕТОД
        private void RightPanel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var listView = sender as System.Windows.Controls.ListView;
            if (listView != null && _viewModel != null)
            {
                _viewModel.RightPanel.SetSelectedItems(listView.SelectedItems);
                Debug.WriteLine($"Right panel selected: {_viewModel.RightPanel.SelectedItems.Count} items");
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.F5:
                    if (_viewModel.CopyCommand.CanExecute(null))
                        _viewModel.CopyCommand.Execute(null);
                    break;
                case Key.F6:
                    if (_viewModel.MoveCommand.CanExecute(null))
                        _viewModel.MoveCommand.Execute(null);
                    break;
                case Key.F7:
                    _viewModel.CreateFolderCommand.Execute(null);
                    break;
                case Key.F8:
                    if (_viewModel.DeleteCommand.CanExecute(null))
                        _viewModel.DeleteCommand.Execute(null);
                    break;
            }
        }
    }
}

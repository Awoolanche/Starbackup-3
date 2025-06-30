using Avalonia.Controls;
using Starbackup_3.ViewModels;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Starbackup_3.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.DataContextChanged += MainWindow_DataContextChanged;
        this.Opened += Window_Opened;
    }

    private async void MainWindow_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            if (viewModel.LogEntries is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= LogEntries_CollectionChanged;
            }

            if (viewModel.LogEntries is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += LogEntries_CollectionChanged;
            }

            await viewModel.InitializeAsync(this);
        }
    }

    private async void Window_Opened(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await Task.Delay(100);

            if (viewModel.IsLogVisible)
            {
                var scrollView = this.FindControl<ScrollViewer>("LogScrollViewer");
                scrollView?.ScrollToEnd();
            }
        }
    }

    private void LogEntries_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Reset)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var scrollView = this.FindControl<ScrollViewer>("LogScrollViewer");
                scrollView?.ScrollToEnd();
            });
        }
    }
}
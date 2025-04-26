using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Bachelor.ViewModels;

namespace Bachelor.Views;

public partial class OutputView : UserControl
{
    private ScrollViewer _logScrollViewer;
    private object _previousDataContext;
    public OutputView()
    {
        InitializeComponent();
        this.Loaded += (s, e) => {
            _logScrollViewer = this.FindControl<ScrollViewer>("LogScrollViewer");
            
            if (DataContext is OutputViewModel viewModel)
            {
                ((INotifyCollectionChanged)viewModel.LogEntries).CollectionChanged += OnLogEntriesChanged;
            }
        };
        
        this.DataContextChanged += (s, e) => {
            // Unsubscribe from old viewmodel
            if (_previousDataContext is OutputViewModel oldVM)
                ((INotifyCollectionChanged)oldVM.LogEntries).CollectionChanged -= OnLogEntriesChanged;

            // Store current DataContext for next time
            _previousDataContext = DataContext;
            
            // Subscribe to new viewmodel
            if (DataContext is OutputViewModel newVM)
                ((INotifyCollectionChanged)newVM.LogEntries).CollectionChanged += OnLogEntriesChanged;
        };
    }
    
    private void OnLogEntriesChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            _logScrollViewer?.ScrollToEnd();
        }
    }
    
}
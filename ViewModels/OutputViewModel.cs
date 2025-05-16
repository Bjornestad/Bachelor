using System;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Threading;
using ReactiveUI;

namespace Bachelor.ViewModels;

public class OutputViewModel : ViewModelBase
{
    private readonly IDispatcher _dispatcher;
    private string _logText = string.Empty;
    private readonly StringBuilder _logBuilder = new StringBuilder();
    private readonly ObservableCollection<string> _logEntries = new ObservableCollection<string>();
    private int _maxLogEntries = 400;
    
    public OutputViewModel(IDispatcher? dispatcher = null)
    {
        _dispatcher = dispatcher ?? Dispatcher.UIThread;
    }

    public string LogText
    {
        get => _logText;
        private set => this.RaiseAndSetIfChanged(ref _logText, value);
    }

    public ObservableCollection<string> LogEntries => _logEntries;

    public void Log(string message)
    {
        AddLogEntry(message);
    }

    public void LogFormat(string format, params object[] args)
    {
        AddLogEntry(string.Format(format, args));
    }

    public void LogKeyEvent(string action, string keyName, string movementName)
    {
        AddLogEntry($"{action}: {keyName} | Movement: {movementName}");
    }

    private void AddLogEntry(string message)
    {
        _dispatcher.Post(() =>
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string entry = $"{message}";

            _logBuilder.AppendLine(entry);
            LogText = _logBuilder.ToString();
            _logEntries.Add(entry);

            // Limit log size
            if (_logEntries.Count > _maxLogEntries)
            {
                _logEntries.RemoveAt(0);
                RebuildLogText();
            }
        });
    }

    private void RebuildLogText()
    {
        _logBuilder.Clear();
        foreach (string entry in _logEntries)
        {
            _logBuilder.AppendLine(entry);
        }
        LogText = _logBuilder.ToString();
    }

    public void Clear()
    {
        _dispatcher.Post(() =>
        {
            _logBuilder.Clear();
            _logEntries.Clear();
            LogText = string.Empty;
        });
    }
}
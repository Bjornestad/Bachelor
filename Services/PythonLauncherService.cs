using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Bachelor.Services;

public class PythonLauncherService
{
    private Process? _pythonProcess;
    
    public void StartPythonScript()
    {
        string pythonPath = "python";
        if (OperatingSystem.IsMacOS())
        {
            pythonPath = "python3";
        }
        string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backend", "face.py");
        
        _pythonProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = scriptPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            },
            EnableRaisingEvents = true
        };
        
        _pythonProcess.OutputDataReceived += (sender, args) => Console.WriteLine($"Python: {args.Data}");
        _pythonProcess.ErrorDataReceived += (sender, args) => Console.WriteLine($"Python Error: {args.Data}");
        
        _pythonProcess.Start();
        _pythonProcess.BeginOutputReadLine();
        _pythonProcess.BeginErrorReadLine();
        
        Console.WriteLine("Python script started");
    }
    
    public void StopPythonScript()
    {
        if (_pythonProcess != null && !_pythonProcess.HasExited)
        {
            _pythonProcess.Kill();
            _pythonProcess.Dispose();
            _pythonProcess = null;
            Console.WriteLine("Python script stopped");
        }
    }
}
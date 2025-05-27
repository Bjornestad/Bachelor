using System;
using System.Diagnostics;
using System.IO;

namespace Bachelor.Services;

public class PythonLauncherService
{
    private Process? _pythonProcess;

    public void StartPythonScript()
    {
        string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backend", "face.exe");
    
        if (OperatingSystem.IsWindows() && File.Exists(exePath))
        {
            _pythonProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            Console.WriteLine("Starting face tracking application (EXE)");
        }
        else
        {
            string pythonPath = OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() ? "python3" : "python";
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

            Console.WriteLine($"Starting face tracking application (Python script) - {(OperatingSystem.IsWindows() ? "EXE not found" : "Non-Windows OS")}");
        }

        _pythonProcess.OutputDataReceived += (sender, args) => Console.WriteLine($"App: {args.Data}");
        _pythonProcess.ErrorDataReceived += (sender, args) => Console.WriteLine($"App Error: {args.Data}");

        _pythonProcess.Start();
        _pythonProcess.BeginOutputReadLine();
        _pythonProcess.BeginErrorReadLine();
    }

    public void StopPythonScript()
    {
        if (_pythonProcess != null && !_pythonProcess.HasExited)
        {
            _pythonProcess.Kill();
            _pythonProcess.Dispose();
            _pythonProcess = null;
            Console.WriteLine("Face tracking application stopped");
        }
    }
}
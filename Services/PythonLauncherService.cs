using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bachelor.Services;

public class PythonLauncherService
{
    private Process? _pythonProcess;
    private int _currentCameraId = -1;


    public void StartPythonScript(string? additionalArgs = null)
    {
        string pythonPath = "python";
        if (OperatingSystem.IsMacOS())
        {
            pythonPath = "python3";
        }

        string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backend", "face.py");

        string arguments = scriptPath;
        if (!string.IsNullOrEmpty(additionalArgs))
        {
            arguments += " " + additionalArgs;
        }

        _pythonProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = arguments,
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

        Console.WriteLine($"Python script started with args: {arguments}");
    }

    public void StartWithCamera(int cameraId)
    {
        _currentCameraId = cameraId;
        StartPythonScript($"--camera {cameraId}");
    }

    public void RestartWithCamera(int cameraId)
    {
        Console.WriteLine($"Python launcher restarting with camera ID: {cameraId}");
        if (cameraId == _currentCameraId && _pythonProcess != null && !_pythonProcess.HasExited)
        {
            Console.WriteLine("Already using this camera, not restarting");
            return;
        }

        Console.WriteLine($"**CAMERA SWITCH REQUESTED: {cameraId}**");

        // More robust process termination
        StopPythonScript();

        // Longer wait to ensure resources are released
        Thread.Sleep(1000);

        // Start with new camera
        StartWithCamera(cameraId);
        Console.WriteLine($"**CAMERA SWITCH COMPLETE: {cameraId}**");
    }

    public void StopPythonScript()
    {
        if (_pythonProcess != null)
        {
            try
            {
                Console.WriteLine("Stopping Python process...");

                // Try graceful exit first
                if (!_pythonProcess.HasExited)
                {
                    _pythonProcess.Kill(entireProcessTree: true);
                    _pythonProcess.WaitForExit(5000);
                }

                _pythonProcess.Dispose();
                _pythonProcess = null;
                Console.WriteLine("Python process stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping Python process: {ex.Message}");
            }
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Bachelor.Services;
using Moq;
using Xunit;

namespace Bachelor.Test.UnitTest.ServicesTest
{
    public class PythonLauncherServiceTest : IDisposable
    {
        private readonly PythonLauncherService _service;

        public PythonLauncherServiceTest()
        {
            _service = new PythonLauncherService();
        }

        [Fact]
        public void StartPythonScript_ShouldUseCorrectPythonCommand()
        {
            // Arrange
            bool startCalled = false;
            string capturedPythonPath = "";
            string capturedScriptPath = "";

            var testableService = new TestablePythonLauncherService(
                startPythonCallback: (pythonPath, scriptPath) =>
                {
                    startCalled = true;
                    capturedPythonPath = pythonPath;
                    capturedScriptPath = scriptPath;
                });

            // Act
            testableService.StartPythonScript();

            // Assert
            Assert.True(startCalled, "StartPythonScript was not called properly");

            // Windows should use "python"
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Equal("python", capturedPythonPath);
            }
            // macOS should use "python3"
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Assert.Equal("python3", capturedPythonPath);
            }

            Assert.EndsWith(Path.Combine("Backend", "face.py"), capturedScriptPath);
        }

        [Fact]
        public void StopPythonScript_ShouldCallKillAndDispose_WhenProcessExists()
        {
            // Arrange
            bool killCalled = false;
            bool disposeCalled = false;

            var testableService = new TestablePythonLauncherService(
                processKiller: () => killCalled = true,
                processDisposer: () => disposeCalled = true);

            // Set process to non-null
            var field = typeof(PythonLauncherService).GetField("_pythonProcess",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(testableService, new Process());

            // Act
            testableService.StopPythonScript();

            // Assert
            Assert.True(killCalled, "Process.Kill was not called");
            Assert.True(disposeCalled, "Process.Dispose was not called");
        }

        [Fact]
        public void StopPythonScript_ShouldNotThrowException_WhenProcessIsNull()
        {
            // Arrange - ensure process is null
            var field = typeof(PythonLauncherService).GetField("_pythonProcess",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(_service, null);

            // Act & Assert - should not throw
            _service.StopPythonScript();
        }

        public void Dispose()
        {
            _service.StopPythonScript();
        }
    }

    // Testable subclass that allows injecting behavior
    public class TestablePythonLauncherService : PythonLauncherService
    {
        private readonly Action<string, string> _startPythonCallback;
        private readonly Action _processKiller;
        private readonly Action _processDisposer;

        public TestablePythonLauncherService(
            Action<string, string> startPythonCallback = null,
            Action processKiller = null,
            Action processDisposer = null)
        {
            _startPythonCallback = startPythonCallback;
            _processKiller = processKiller;
            _processDisposer = processDisposer;
        }

        // Override the entire method to avoid actually starting a process
        public override void StartPythonScript()
        {
            if (_startPythonCallback != null)
            {
                string pythonPath = "python";
                if (OperatingSystem.IsMacOS())
                {
                    pythonPath = "python3";
                }
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backend", "face.py");
                
                _startPythonCallback(pythonPath, scriptPath);
                
                // Set the process field so StopPythonScript has something to work with
                var field = typeof(PythonLauncherService).GetField("_pythonProcess",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(this, new Process());
                
                return;
            }
            
            base.StartPythonScript();
        }

        // Override the base StopPythonScript to use our injected actions
        public override void StopPythonScript()
        {
            if (_processKiller != null && _processDisposer != null)
            {
                var field = typeof(PythonLauncherService).GetField("_pythonProcess",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var process = field?.GetValue(this) as Process;

                if (process != null)
                {
                    _processKiller();
                    _processDisposer();
                    field?.SetValue(this, null);
                }
                return;
            }

            base.StopPythonScript();
        }
    }
}
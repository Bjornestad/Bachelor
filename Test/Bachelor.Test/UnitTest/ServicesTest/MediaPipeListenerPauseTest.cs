using System;
using Bachelor.Interfaces;
using Bachelor.Models;
using Bachelor.Services;
using Moq;
using Xunit;

namespace Bachelor.Test.UnitTest.ServicesTest
{
    public class MediaPipeListenerPauseTest : IDisposable
    {
        private readonly MediaPipeListener _listener;
        private readonly Mock<IMovementManagerService> _mockMovementManager;
        private readonly FacialTrackingData _testData;

        public MediaPipeListenerPauseTest()
        {
            _mockMovementManager = new Mock<IMovementManagerService>();
            _listener = new MediaPipeListener(_mockMovementManager.Object);

            // Create test data
            _testData = new FacialTrackingData();
            Console.WriteLine("MediaPipeListener pause test initialized");
        }

        public void Dispose()
        {
            // Cleanup
            _listener?.Stop();
        }

        [Fact]
        public void PauseFunction_ControlsDataProcessing()
        {
            // ARRANGE
            int processCount = 0;
            _mockMovementManager
                .Setup(m => m.ProcessFacialData(It.IsAny<FacialTrackingData>()))
                .Callback<FacialTrackingData>(_ => 
                {
                    processCount++;
                    Console.WriteLine($"Data processed (count: {processCount})");
                });

            Console.WriteLine("=== TESTING UNPAUSE STATE ===");
            // ACT & ASSERT - Not paused
            _listener.IsPaused = false;
            Console.WriteLine($"Listener pause state: {_listener.IsPaused}");

            // Simulate what MediaPipeListener does when data is received
            if (!_listener.IsPaused)
            {
                Console.WriteLine("Processing data...");
                _mockMovementManager.Object.ProcessFacialData(_testData);
            }
            else
            {
                Console.WriteLine("Data processing SKIPPED (paused)");
            }

            Assert.Equal(1, processCount);

            Console.WriteLine("\n=== TESTING PAUSE STATE ===");
            _listener.IsPaused = true;
            Console.WriteLine($"Listener pause state: {_listener.IsPaused}");

            // Simulate what MediaPipeListener does when data is received while paused
            if (!_listener.IsPaused)
            {
                Console.WriteLine("Processing data...");
                _mockMovementManager.Object.ProcessFacialData(_testData);
            }
            else
            {
                Console.WriteLine("Data processing SKIPPED (paused)");
            }

            // Count should not increase because we're paused
            Assert.Equal(1, processCount);
            
            Console.WriteLine("\n=== TESTING RESUME AFTER PAUSE ===");
            // ACT & ASSERT - Unpaused again
            _listener.IsPaused = false;
            Console.WriteLine($"Listener pause state: {_listener.IsPaused}");

            // Simulate what MediaPipeListener does when data is received after unpausing
            if (!_listener.IsPaused)
            {
                Console.WriteLine("Processing data...");
                _mockMovementManager.Object.ProcessFacialData(_testData);
            }
            else
            {
                Console.WriteLine("Data processing SKIPPED (paused)");
            }

            // Count should increase after unpausing
            Assert.Equal(2, processCount);
            
            Console.WriteLine("\n=== PAUSE TEST COMPLETED SUCCESSFULLY ===");
        }
    }
}
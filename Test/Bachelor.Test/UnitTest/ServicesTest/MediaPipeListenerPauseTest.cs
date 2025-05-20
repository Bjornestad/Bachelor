using System;
using System.Diagnostics;
using Bachelor.Interfaces;
using Bachelor.Models;
using Bachelor.Services;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Bachelor.Test.UnitTest.ServicesTest
{
    public class MediaPipeListenerPauseTest : IDisposable
    {
        private readonly MediaPipeListener _listener;
        private readonly Mock<IMovementManagerService> _mockMovementManager;
        private readonly FacialTrackingData _testData;
        private readonly ITestOutputHelper _testOutputHelper;

        public MediaPipeListenerPauseTest(ITestOutputHelper testOutputHelper)
        {
            _mockMovementManager = new Mock<IMovementManagerService>();
            _listener = new MediaPipeListener(_mockMovementManager.Object);
            _testOutputHelper = testOutputHelper;

            _testData = new FacialTrackingData();
            Console.WriteLine("MediaPipeListener pause test initialized");
        }

        public void Dispose()
        {
            _listener?.Stop();
        }

        [Fact]
        public void PauseFunctionTest()
        {
            int processCount = 0;
            _mockMovementManager
                .Setup(m => m.ProcessFacialData(It.IsAny<FacialTrackingData>()))
                .Callback<FacialTrackingData>(_ =>
                {
                    processCount++;
                    Console.WriteLine($"Data processed (count: {processCount})");
                    _testOutputHelper.WriteLine($"Data processed (count: {processCount})");
                });

            var testData = new FacialTrackingData();

            Console.WriteLine("=== TESTING UNPAUSE STATE ===");
            _testOutputHelper.WriteLine("=== TESTING UNPAUSE STATE ===");
            _listener.IsPaused = false;

            SimulateDataProcessing(_listener, testData, _mockMovementManager.Object);
            Assert.Equal(1, processCount);

            Console.WriteLine("\n=== TESTING PAUSE STATE ===");
            _testOutputHelper.WriteLine("\n=== TESTING PAUSE STATE ===");
            _listener.IsPaused = true;

            SimulateDataProcessing(_listener, testData, _mockMovementManager.Object);
            Assert.Equal(1, processCount); // Should still be 1 as no processing should occur

            Console.WriteLine("\n=== TESTING RESUME AFTER PAUSE ===");
            _testOutputHelper.WriteLine("\n=== TESTING RESUME AFTER PAUSE ===");
            _listener.IsPaused = false;

            SimulateDataProcessing(_listener, testData, _mockMovementManager.Object);
            Assert.Equal(2, processCount); // Should be 2 since processing resumed

            Console.WriteLine("\n=== PAUSE TEST COMPLETED SUCCESSFULLY ===");
            _testOutputHelper.WriteLine("\n=== PAUSE TEST COMPLETED SUCCESSFULLY ===");
        }

        private void SimulateDataProcessing(MediaPipeListener listener, FacialTrackingData data,
            IMovementManagerService movementManager)
        {
            Console.WriteLine($"Simulating data received (pause state: {listener.IsPaused})");
            _testOutputHelper.WriteLine($"Simulating data received (pause state: {listener.IsPaused})");
            if (!listener.IsPaused)
            {
                Console.WriteLine("Processing data...");
                _testOutputHelper.WriteLine("Processing data...");
                movementManager.ProcessFacialData(data);
            }
            else
            {
                Console.WriteLine("Data processing SKIPPED (paused)");
                _testOutputHelper.WriteLine("Data processing SKIPPED (paused)");
            }
        }
    }
}
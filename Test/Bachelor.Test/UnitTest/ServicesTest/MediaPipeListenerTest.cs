using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Bachelor.Interfaces;
using Bachelor.Models;
using Bachelor.Services;
using Moq;
using Xunit;

namespace Bachelor.Test.UnitTest.ServicesTest
{
    public class MediaPipeListenerRealTimeTest
    {
        [Fact]
        public async Task RealTimeTracking_ProcessesDataWithMinimalLatency()
        {
            var mockMovementManager = new Mock<IMovementManagerService>();
            var processed = new TaskCompletionSource<bool>();

            // Track when data is processed
            mockMovementManager
                .Setup(m => m.ProcessFacialData(It.IsAny<FacialTrackingData>()))
                .Callback<FacialTrackingData>(data => processed.SetResult(true));

            var listener = new MediaPipeListener(mockMovementManager.Object);
            listener.Start();

            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync("127.0.0.1", 5005);
                using var stream = client.GetStream();

                // Create a mock facial tracking data JSON directly
                var jsonObject = new
                {
                    timestamp = DateTime.Now.Ticks,
                    headRotation = 0.5,
                    headPitch = 0.2
                };
                var jsonData = JsonSerializer.Serialize(jsonObject);
                
                var dataPacket = $"DATA:{jsonData}\n";
                var buffer = Encoding.UTF8.GetBytes(dataPacket);

                // Send test data and measure latency
                var stopwatch = Stopwatch.StartNew();
                await stream.WriteAsync(buffer, 0, buffer.Length);

                var processed_result = await Task.WhenAny(processed.Task, Task.Delay(2000));                stopwatch.Stop();

                Assert.True(processed_result == processed.Task, "Data wasn't processed within timeout period");
                Assert.True(stopwatch.ElapsedMilliseconds < 100,
                    $"Processing latency ({stopwatch.ElapsedMilliseconds}ms) exceeds real-time threshold of 100ms");
                Console.WriteLine($"Processing latency: {stopwatch.ElapsedMilliseconds}ms");

                mockMovementManager.Verify(m => m.ProcessFacialData(It.IsAny<FacialTrackingData>()), Times.Once());
            }
            finally
            {
                // Cleanup
                listener.Stop();
            }
        }
    }
}
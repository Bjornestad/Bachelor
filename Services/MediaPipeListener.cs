using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Bachelor.Models;

namespace Bachelor.Services;

public class MediaPipeListener
{
    private bool debug = false;
    private readonly MovementManagerService _movementManager;
    private TcpListener _server;
    private bool _isRunning;

    // Event for video frame updates
    public event EventHandler<Bitmap> VideoFrameReceived;

    public MediaPipeListener(MovementManagerService movementManager)
    {
        _movementManager = movementManager;
    }

    public void Start()
    {
        _server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5005);
        _server.Start();
        _isRunning = true;

        Task.Run(() => ListenForData());
        Console.WriteLine("MediaPipeListener started and listening on 127.0.0.1:5005");
    }

    private async Task ListenForData()
    {
        DateTime lastPrintTime = DateTime.MinValue;
        TimeSpan printInterval = TimeSpan.FromMilliseconds(500);

        while (_isRunning)
        {
            TcpClient client = null;
            try
            {
                client = await _server.AcceptTcpClientAsync();
                Console.WriteLine("Client connected!");
                NetworkStream stream = client.GetStream();

                byte[] headerBuffer = new byte[1024]; // Buffer for reading headers
                int headerLength;

                while (client.Connected && _isRunning)
                {
                    try
                    {
                        // Read headers until we get a newline
                        headerLength = 0;
                        bool foundNewline = false;

                        while (!foundNewline && headerLength < headerBuffer.Length)
                        {
                            int b = stream.ReadByte();
                            if (b == -1) // Connection closed
                            {
                                break;
                            }

                            headerBuffer[headerLength++] = (byte)b;

                            // Check if we've found a newline
                            if (b == '\n')
                            {
                                foundNewline = true;
                                break;
                            }
                        }

                        if (!foundNewline)
                        {
                            break;
                        }

                        // Convert header to string and remove newline
                        string header = Encoding.UTF8.GetString(headerBuffer, 0, headerLength).TrimEnd();

                        if (header.StartsWith("DATA:"))
                        {
                            // Process facial tracking data
                            string jsonData = header.Substring(5);
                            var data = JsonSerializer.Deserialize<FacialTrackingData>(jsonData);

                            if (debug && DateTime.Now - lastPrintTime > printInterval)
                            {
                                Console.WriteLine($"{data.MouthBotY - data.MouthTopY:F3} : Mouth openness");
                                Console.WriteLine($"{data.MouthLX - data.MouthRX:F3} : Mouth width");
                                Console.WriteLine($"{data.rEyebrowY - data.rEyesocketY:F3} : EyebrowR height");
                                Console.WriteLine($"{data.lEyebrowY - data.lEyesocketY:F3} : EyebrowL height");
                                Console.WriteLine($"{data.Roll:F3} : Head tilt");
                                Console.WriteLine($"{data.HeadRotation:F3} : Head rotation");
                                Console.WriteLine($"{data.HeadPitch:F3} : Head pitch");
                                lastPrintTime = DateTime.Now;
                            }
                            
                            _movementManager.ProcessFacialData(data);
                        }
                        else if (header.StartsWith("IMAGE:"))
                        {
                            // Process image frame
                            if (int.TryParse(header.Substring(6), out int size))
                            {
                                byte[] imageBuffer = new byte[size];

                                // Read entire image
                                int bytesRead = 0;
                                int totalBytesRead = 0;

                                while (totalBytesRead < size)
                                {
                                    bytesRead = await stream.ReadAsync(imageBuffer, totalBytesRead, size - totalBytesRead);
                                    if (bytesRead == 0)
                                    {
                                        break;
                                    }
                                    totalBytesRead += bytesRead;
                                }

                                if (totalBytesRead == size)
                                {
                                    await ProcessImageFrameAsync(imageBuffer);
                                }
                            }
                        }
                        else
                        {
                            // Try legacy format - plain JSON
                            try
                            {
                                var data = JsonSerializer.Deserialize<FacialTrackingData>(header);
                                _movementManager.ProcessFacialData(data);
                            }
                            catch (JsonException)
                            {
                                // Not valid JSON, skipping
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"IO error: {ex.Message}");
                        break; // Exit the inner loop on IO errors
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing data: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
            }
            finally
            {
                client?.Close();
                await Task.Delay(1000); // Wait before accepting new connections
            }
        }
    }

    private async Task ProcessImageFrameAsync(byte[] imageData)
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                try
                {
                    using var ms = new MemoryStream(imageData);
                    var bitmap = new Bitmap(ms);
                    VideoFrameReceived?.Invoke(this, bitmap);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating bitmap: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing image: {ex.Message}");
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _server?.Stop();
        Console.WriteLine("MediaPipeListener stopped");
    }
}
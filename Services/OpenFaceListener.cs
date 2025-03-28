using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Bachelor.Models;

namespace Bachelor.Services;

public class OpenFaceListener
{
    private bool debug = true;
    private readonly MovementManagerService _movementManager;
    private TcpListener _server;
    private bool _isRunning;

    public OpenFaceListener(MovementManagerService movementManager)
    {
        _movementManager = movementManager;
    }

    public void Start()
    {
        _server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5005);
        _server.Start();
        _isRunning = true;

        Task.Run(() => ListenForData());
    }

    private async Task ListenForData()
    {
        DateTime lastPrintTime = DateTime.MinValue;
        TimeSpan printInterval = TimeSpan.FromMilliseconds(500); // Print every 500ms
    
        while (_isRunning)
        {
            try
            {
                TcpClient client = await _server.AcceptTcpClientAsync();
                Console.WriteLine("Client connected!");
                NetworkStream stream = client.GetStream();

                using StreamReader reader = new StreamReader(stream);

                while (client.Connected && _isRunning)
                {
                    string jsonLine = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(jsonLine))
                        continue;
                
                    try
                    {
                        var data = JsonSerializer.Deserialize<FacialTrackingData>(jsonLine);
                    
                        // Only print once every 500ms
                        if(debug){
                            if (DateTime.Now - lastPrintTime > printInterval)
                            {
                                Console.WriteLine($"Received data: {jsonLine}");
                                Console.WriteLine($"Parsed data: X={data.X:F3}, Y={data.Y:F3}, Z={data.Z:F3}, Roll={data.Roll:F3}, " +
                                                  $"LeftEyebrow={data.LeftEyebrowHeight:F3}, RightEyebrow={data.RightEyebrowHeight:F3}, " +
                                                  $"MouthWidth={data.MouthWidth:F3}, MouthHeight={data.MouthHeight:F3}");
                                lastPrintTime = DateTime.Now;
                            }}
                    
                        _movementManager.ProcessFacialData(data);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON parsing error: {ex.Message}");
                    }
                }

                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _server?.Stop();
    }
}
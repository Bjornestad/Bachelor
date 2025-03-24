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
        while (_isRunning)
        {
            try
            {
                TcpClient client = await _server.AcceptTcpClientAsync();
                NetworkStream stream = client.GetStream();

                // Create a memory stream to collect all data
                using MemoryStream memoryStream = new MemoryStream();
                byte[] buffer = new byte[4096]; // Larger buffer
                int bytesRead;
            
                // Read until there's no more data
                do
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        memoryStream.Write(buffer, 0, bytesRead);
                    }
                } while (stream.DataAvailable);

                // Process the complete data
                string json = Encoding.UTF8.GetString(memoryStream.ToArray());
                var data = JsonSerializer.Deserialize<FacialTrackingData>(json);
                _movementManager.ProcessFacialData(data);
                

                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing data: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _server?.Stop();
    }
}
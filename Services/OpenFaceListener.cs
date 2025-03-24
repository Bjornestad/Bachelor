using System;
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

                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var data = JsonSerializer.Deserialize<FacialTrackingData>(json);
                    _movementManager.ProcessFacialData(data);
                }

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
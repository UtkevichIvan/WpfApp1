using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApp1;

public class ReaderService : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;


    private readonly ILogger<ReaderService> _logger;
    private MainViewModel _model;

    public ReaderService(ILogger<ReaderService> logger, IHostApplicationLifetime lifetime, MainWindow window)
    {

        _logger = logger;
        _lifetime = lifetime;
        _model = (MainViewModel)window.DataContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await WaitForAppStartup(_lifetime, stoppingToken))
        {
            return;
        }

        //SQL
        MySqlConnection conn = new MySqlConnection("SERVER=" + "10.135.253.1" + "; port=3306; Database=" + "skudDB" + "; UID=" + "ivan" + "; PASSWORD=" + "Ii2150908" + ";");
        //Камера
        using var httpClient = new HttpClient();

        // Конвертер
        using var tcpClient = new TcpClient();

        //УЫЗ
        IPAddress localAddr = IPAddress.Parse("192.168.0.103");
        IPEndPoint ipPoint = new IPEndPoint(localAddr, 8888);
        using Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        tcpListener.Bind(ipPoint);
        tcpListener.Listen();
        using var tcpC = await tcpListener.AcceptAsync();

        try
        {
            await tcpClient.ConnectAsync(IPAddress.Parse("10.135.0.172"), 1001);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue
                ("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"admin:Exolaunch1")));
        }
        catch (SocketException ex)
        {
        }
        catch (Exception ex)
        {
        }

        var tcpStream = tcpClient.GetStream();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var responseData = new byte[16];
                await tcpStream.ReadAsync(responseData);
                string line = Encoding.ASCII.GetString(responseData);
                if (line != "")
                {
                    var cameraResponse = await httpClient.GetAsync
                        ("http://admin:Exolaunch1@10.135.0.40/ISAPI/Streaming/channels/101/picture?snapShotImageType=JPEG");
                    var photo = await cameraResponse.Content.ReadAsStreamAsync();

                    var lengthCardId = line.IndexOf("\r");
                    var cardId = line.Substring(0, lengthCardId);
                    var sqlExpression = $"SELECT * FROM Students WHERE CardId = '{cardId}'";
                    conn.Open();
                    MySqlCommand command = new MySqlCommand(sqlExpression, conn);
                    MySqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        var student = new Student()
                        {
                            Id = reader.GetInt32(0),
                            CardId = reader.GetString(1),
                            Age = reader.GetInt32(2),
                            Name = reader.GetString(3),
                            SecondName = reader.GetString(4),
                            Patronomic = reader.GetString(5),
                            Course = reader.GetInt32(6),
                            Group = reader.GetInt32(7),
                        };

                        _model.AddStudent(student, photo);
                        conn.Close();
                        byte[] data = Encoding.UTF8.GetBytes("Y");
                        await tcpC.SendAsync(data);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
    private static async Task<bool> WaitForAppStartup(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
    {
        var startedSource = new TaskCompletionSource();
        await using var reg1 = lifetime.ApplicationStarted.Register(() => startedSource.SetResult());

        var cancelledSource = new TaskCompletionSource();
        await using var reg2 = stoppingToken.Register(() => cancelledSource.SetResult());

        var completedTask = await Task.WhenAny(startedSource.Task, cancelledSource.Task).ConfigureAwait(false);

        return completedTask == startedSource.Task;
    }
}
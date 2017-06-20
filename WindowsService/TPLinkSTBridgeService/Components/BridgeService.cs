using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;

namespace TPLinkSTBridgeService
{
	public class BridgeService
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public bool Start()
		{
			_logger.Trace("Started service");
			ThreadPool.QueueUserWorkItem(DoWork, _cancellationTokenSource.Token);
			return true;
		}

		public bool Stop()
		{
			_logger.Trace("Stopping service");
			_cancellationTokenSource.Cancel();

			return true;
		}

		private static void DoWork(object obj)
		{
			var token = (CancellationToken)obj;

			var listener = new HttpListener();

			IPAddress[] ipAddresses = Dns.GetHostEntry("localhost").AddressList;

			foreach (var ipAddress in ipAddresses.Where(x => x.AddressFamily == AddressFamily.InterNetwork))
			{
				var s = $"http://{ipAddress}:8082/";
				listener.Prefixes.Add(s);
				_logger.Trace($"Added prefix: {s}");
			}

			listener.Start();

			_logger.Trace("Listening...");

			while (!token.IsCancellationRequested)
			{
				ProcessIncomingRequests(listener);
			}

			_logger.Trace("Exiting DoWork");
		}

		private static void ProcessIncomingRequests(HttpListener listener)
		{
			var context = listener.GetContext();
			_logger.Trace("Got request from {0}", context.Request.RemoteEndPoint?.Address);

			var request = context.Request;
			var response = context.Response;
			var command = request.Headers["command"];

			_logger.Trace("Processing command: {0}", command);

			response.StatusCode = 200;

			switch (command)
			{
				case "restartPC":
					{
						_logger.Trace("Restarting PC");
						response.AddHeader("cmd-response", "restartPC");
						response.Close();

						Process.Start("shutdown", "/r /t 005");

						break;
					}

				case "pollServer":
					{
						_logger.Trace("Server poll response sent to SmartThings");
						response.AddHeader("cmd-response", "ok");
						response.Close();
						break;
					}

				case "deviceCommand":
					{
						ProcessDeviceCommand(request, response);
						response.Close();
						break;
					}

				default:
					{
						_logger.Warn("Invalid Command received from SmartThings: {0}", command);
						response.AddHeader("cmd-response", "TcpTimeout");
						response.Close();
						break;
					}
			}
		}

		private static void ProcessDeviceCommand(HttpListenerRequest request, HttpListenerResponse response)
		{
			var command = request.Headers["tplink-command"];
			var deviceIP = request.Headers["tplink-iot-ip"];
			_logger.Trace($"Sending to IP address: {deviceIP} Command: {command}");

			var commandSender = new CommandSender(deviceIP, command);
			commandSender.Send();
		}
	}
}
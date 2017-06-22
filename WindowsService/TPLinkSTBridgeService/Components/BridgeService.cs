using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;

namespace TPLinkSTBridgeService
{
	/// <summary>
	/// The BridgeService is a bridge between SmartThings and TP-Link devices on the network
	/// </summary>
	public class BridgeService
	{
		#region Fields

		/// <summary>
		/// The logger
		/// </summary>
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// The cancellation token source
		/// </summary>
		readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		#endregion

		#region Public Methods

		/// <summary>
		/// Starts the bridge service
		/// </summary>
		public bool Start()
		{
			_logger.Trace("Started service");
			ThreadPool.QueueUserWorkItem(DoWork, _cancellationTokenSource.Token);
			return true;
		}

		/// <summary>
		/// Stops the bridge service
		/// </summary>
		public bool Stop()
		{
			_logger.Trace("Stopping service");
			_cancellationTokenSource.Cancel();

			return true;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Starts a new listener and processes incoming requests until cancelled
		/// </summary>
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

		/// <summary>
		/// Processes the incoming requests.
		/// </summary>
		/// <param name="listener">The listener.</param>
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
					_logger.Trace("Processing device command");

					var deviceCommand = request.Headers["tplink-command"];
					var deviceHost = request.Headers["tplink-iot-ip"];

					try
					{
						var deviceResponse = ProcessDeviceCommand(deviceCommand, deviceHost);

						response.AddHeader("cmd-response", deviceResponse);
						response.Close();
					}
					catch (Exception e)
					{
						_logger.Error(e, "Error processing device command");
						response.StatusCode = 500;
						response.Close(Encoding.ASCII.GetBytes(e.Message), false);
					}

					break;
				}

				case "bridgeCommand":
				{
					_logger.Trace("Processing bridge command");

					var bridgeCommand = request.Headers["bridge-command"];

					try
					{
						var bridgeResponse = ProcessBridgeCommand(bridgeCommand);

						response.ContentType = "application/json";
						response.Close(Encoding.ASCII.GetBytes(bridgeResponse), false);
					}
					catch (Exception e)
					{
							_logger.Error(e, "Error processing bridge command");
							response.StatusCode = 500;
							response.Close(Encoding.ASCII.GetBytes(e.Message), false);
						}

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

		/// <summary>
		/// Processes the bridge command.
		/// </summary>
		/// <param name="command">The command.</param>
		/// <returns>System.String.</returns>
		private static string ProcessBridgeCommand(string command)
		{
			_logger.Trace($"ProcessBridgeCommand: {command}");

			var commandSender = new BridgeCommandSender();
			return commandSender.ExecuteCommand(command);
		}

		/// <summary>
		/// Processes the device command.
		/// </summary>
		/// <param name="command">The command.</param>
		/// <param name="host">The host.</param>
		/// <returns>System.String.</returns>
		private static string ProcessDeviceCommand(string command, string host)
		{
			_logger.Trace($"Sending to IP address: {host} Command: {command}");

			var commandSender = new DeviceCommandSender(host, command);
			return commandSender.Send();
		}

		#endregion
	}
}
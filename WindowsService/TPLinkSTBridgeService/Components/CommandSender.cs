using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using NLog;

namespace TPLinkSTBridgeService
{
	internal class CommandSender
	{
		private readonly string _host;
		private readonly string _command;

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public CommandSender(string host, string command)
		{
			_host = host;
			_command = command;

			_logger.Trace("Initialized CommandSender - host: {0}, command: {1}", host, command);
		}

		public void Send()
		{
			_logger.Trace("Sending command");

			var commandBytes = EncryptCommand(_command);
			_logger.Trace("Encrypted command");

			using (var client = new TcpClient())
			{
				var result = client.BeginConnect(_host, 9999, null, null);

				var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(4));

				if (!success)
				{
					throw new ConnectionFailureException();
				}

				client.EndConnect(result);

				if (client.Connected)
				{
					_logger.Trace("Connected successfully.  Sending command...");

					client.Client.Send(commandBytes, commandBytes.Length, SocketFlags.None);
					_logger.Trace("Command sent");
				}
			}
		}

		private byte[] EncryptCommand(string input)
		{
			var ints = new List<byte>();

			var key = 171;

			foreach (var inputChar in input)
			{
				byte x = Convert.ToByte(inputChar ^ key);
				ints.Add(x);

				key = x;
			}

			ints.InsertRange(0, BitConverter.GetBytes(input.Length).Reverse());

			return ints.Select(Convert.ToByte).ToArray();
		}

		public class ConnectionFailureException : Exception
		{
		}
	}
}

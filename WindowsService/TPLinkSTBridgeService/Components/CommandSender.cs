﻿using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using NLog;

namespace TPLinkSTBridgeService
{
	internal class CommandSender
	{
		#region Fields

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly CommandEncryptor _commandEncryptor = new CommandEncryptor();

		private readonly string _host;

		private readonly string _command;

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandSender"/> class.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <param name="command">The command.</param>
		public CommandSender(string host, string command)
		{
			_host = host;
			_command = command;

			_logger.Trace("Initialized CommandSender - host: {0}, command: {1}", host, command);
		}

		/// <summary>
		/// Sends the command to the host and gets the response
		/// </summary>
		public string Send()
		{
			_logger.Trace("Sending command");

			var commandBytes = _commandEncryptor.Encrypt(_command);
			_logger.Trace("Encrypted command");

			using (var client = new TcpClient())
			{
				client.Connect(_host, 9999);

				if (client.Connected)
				{
					_logger.Trace("Connected successfully.  Sending command...");

					client.Client.Send(commandBytes, commandBytes.Length, SocketFlags.None);
					_logger.Trace("Command sent");

					using (NetworkStream networkStream = client.GetStream())
					{
						if (networkStream.CanRead)
						{
							var readBuffer = new byte[client.ReceiveBufferSize];

							using (var writer = new MemoryStream())
							{
								while (!networkStream.DataAvailable)
								{
									// TODO: Refactor this
									// https://stackoverflow.com/questions/1159264/best-way-to-wait-for-tcpclient-data-to-become-available

									_logger.Trace("No response. Waiting 500ms");
									Thread.Sleep(500);
								}

								while (networkStream.DataAvailable)
								{
									int numberOfBytesRead = networkStream.Read(readBuffer, 0, readBuffer.Length);

									if (numberOfBytesRead <= 0)
									{
										break;
									}

									writer.Write(readBuffer, 0, numberOfBytesRead);
								}

								// Ignore the first 4 bytes of the response data
								// as this contains empty data which we don't need
								// and corrupts the decryption process
								var bytes = writer.ToArray().Skip(4).ToArray();

								// Decrypt the data
								var decrypted = _commandEncryptor.Decrypt(bytes);

								_logger.Trace("Got response: {0}", decrypted);

								return decrypted;
							}
						}
					}
				}
			}

			return null;
		}
	}
}

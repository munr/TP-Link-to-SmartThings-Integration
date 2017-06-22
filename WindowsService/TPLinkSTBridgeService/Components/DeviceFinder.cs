using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json.Linq;
using NLog;

namespace TPLinkSTBridgeService
{
	internal class DeviceFinder
	{
		#region Fields

		private const int Port = 8095;

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private static readonly DeviceCommandEncryptor _encryptor = new DeviceCommandEncryptor(true);

		#endregion

		public List<FoundDeviceInfo> Find()
		{
			var receiver = new Receiver();
			receiver.Start();

			// Wait a couple of seconds for devices to be found
			_logger.Trace("Waiting for responses");
			Thread.Sleep(TimeSpan.FromSeconds(2));

			receiver.Stop();
			_logger.Trace("Stopped listening");

			// Return found devices
			return receiver.FoundDevices;
		}

		private class Receiver
		{
			#region Fields

			private IPEndPoint _ipEndpoint;

			private readonly UdpClient _udpClient = new UdpClient();

			#endregion

			#region Properties

			public List<FoundDeviceInfo> FoundDevices { get; } = new List<FoundDeviceInfo>();

			#endregion

			#region Constructor

			public Receiver()
			{
				var ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
				_ipEndpoint = new IPEndPoint(ipAddress, Port);
				_logger.Trace("Initialized receiver with ip: {0}, port: {1}", ipAddress, Port);
			}

			#endregion

			#region Public Methods

			public void Start()
			{
				SendCommand();
				StartReceiving();
			}

			public void Stop()
			{
				_udpClient.Close();
				_udpClient.Dispose();
			}

			#endregion

			#region Private Methods

			private void SendCommand()
			{
				var broadcastMessage = "{\"system\":{\"get_sysinfo\":{}}}";
				var bytes = _encryptor.Encrypt(broadcastMessage);

				_udpClient.Client.Bind(_ipEndpoint);

				var ip = new IPEndPoint(IPAddress.Broadcast, 9999);
				_udpClient.Send(bytes, bytes.Length, ip);
			}

			private void StartReceiving()
			{
				_udpClient.BeginReceive(Receive, new object());
			}

			private void Receive(IAsyncResult ar)
			{
				try
				{
					var bytes = _udpClient.EndReceive(ar, ref _ipEndpoint);

					var json = _encryptor.Decrypt(bytes);

					var di = GetDeviceInfoFromResponse(json);

					FoundDevices.Add(di);
					_logger.Trace("Found device: {0}", di.Alias);

					StartReceiving();
				}
				catch (Exception e)
				{
					//_logger.Trace("Error receiving: {0}", e.Message);
				}
			}

			private static FoundDeviceInfo GetDeviceInfoFromResponse(string json)
			{
				var jObject = JObject.Parse(json);
				var deviceInfo = jObject["system"]["get_sysinfo"];

				var di = new FoundDeviceInfo
				         {
					         Alias = deviceInfo["alias"].ToString(),
					         SoftwareVersion = deviceInfo["sw_ver"].ToString(),
					         HardwareVersion = deviceInfo["hw_ver"].ToString(),
					         Model = deviceInfo["model"].ToString(),
					         MacAddress = deviceInfo["mac"].ToString(),
					         DeviceId = deviceInfo["deviceId"].ToString(),
					         DeviceType = deviceInfo["mic_type"].ToString(),
					         DeviceName = deviceInfo["dev_name"].ToString(),
					         HardwareId = deviceInfo["hwId"].ToString(),
					         FirmwareId = deviceInfo["fwId"].ToString(),
					         OemId = deviceInfo["oemId"].ToString(),
					         IsSwitchedOn = deviceInfo["relay_state"].ToString() == "1"
				         };

				return di;
			}

			#endregion
		}
	}
}

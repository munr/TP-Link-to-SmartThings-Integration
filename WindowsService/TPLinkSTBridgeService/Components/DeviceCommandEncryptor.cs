using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPLinkSTBridgeService
{
	/// <summary>
	/// The CommandEncryptor is used to encrypt commands so that they can be understood by
	/// TP-Link devices, and for decrypting the command response returned from TP-Link devices
	/// </summary>
	internal class DeviceCommandEncryptor
	{
		#region Fields

		/// <summary>
		/// The first key used to encrypt and decrypt data
		/// </summary>
		private const int FirstKey = 171;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is encryption for UDP.
		/// </summary>
		/// <value><c>true</c> if this instance is encryption for UDP; otherwise, <c>false</c>.</value>
		private bool IsEncryptionForUDP { get; set; } = false;

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="DeviceCommandEncryptor"/> class.
		/// </summary>
		/// <param name="isEncryptionForUdp">Specifies whether we are using this for a UDP connection</param>
		public DeviceCommandEncryptor(bool isEncryptionForUdp)
		{
			IsEncryptionForUDP = isEncryptionForUdp;
		}

		/// <summary>
		/// Encrypts the specified input.
		/// </summary>
		public byte[] Encrypt(string input)
		{
			var bytes = new List<byte>();

			var key = FirstKey;

			foreach (var inputChar in input)
			{
				byte b = Convert.ToByte(inputChar ^ key);
				bytes.Add(b);

				key = b;
			}

			if (!IsEncryptionForUDP)
			{
				bytes.InsertRange(0, BitConverter.GetBytes(input.Length).Reverse());
			}

			return bytes.Select(Convert.ToByte).ToArray();
		}

		/// <summary>
		/// Decrypts the specified input.
		/// </summary>
		public string Decrypt(byte[] bytes)
		{
			var sb = new StringBuilder();

			var key = FirstKey;

			if (!IsEncryptionForUDP)
			{
				bytes = bytes.Skip(4).ToArray();
			}

			foreach (var b in bytes)
			{
				var nextKey = b;
				var nb = Convert.ToByte(b ^ key);
				key = nextKey;

				var c = Convert.ToChar(nb);
				sb.Append(c);
			}

			return sb.ToString();
		}
	}
}

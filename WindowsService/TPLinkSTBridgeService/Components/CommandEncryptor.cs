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
	internal class CommandEncryptor
	{
		/// <summary>
		/// The first key used to encrypt and decrypt data
		/// </summary>
		private const int FirstKey = 171;

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

			bytes.InsertRange(0, BitConverter.GetBytes(input.Length).Reverse());

			return bytes.Select(Convert.ToByte).ToArray();
		}

		/// <summary>
		/// Decrypts the specified input.
		/// </summary>
		public string Decrypt(byte[] bytes)
		{
			var sb = new StringBuilder();

			var key = FirstKey;

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

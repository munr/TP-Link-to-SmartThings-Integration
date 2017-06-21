using NLog;

namespace TPLinkSTBridgeService
{
	/// <summary>
	/// Sends commands to the bridge
	/// </summary>
	public class BridgeCommandSender
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Sends the specified command and gets the response
		/// </summary>
		public string ExecuteCommand(string command)
		{
			_logger.Trace("Executing Bridge Command: {0}", command);

			switch (command)
			{
				case "getDeviceList":
				{
					return "Not Implemented";
					break;
				}
				default:
				{
					return $"Unknown command: {command}";
				}
			}
		}
	}
}

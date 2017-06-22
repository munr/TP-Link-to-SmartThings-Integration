namespace TPLinkSTBridgeService
{
	public class FoundDeviceInfo
	{
		public string Alias { get; set; }
		public bool IsSwitchedOn { get; set; }

		public string DeviceName { get; set; }
		public string DeviceType { get; set; }
		public string DeviceId { get; set; }

		public string SoftwareVersion { get; set; }
		public string HardwareVersion { get; set; }

		public string Model { get; set; }
		public string MacAddress { get; set; }

		public string HardwareId { get; set; }
		public string FirmwareId { get; set; }
		public string OemId { get; set; }
	}
}

using Topshelf;

namespace TPLinkSTBridgeService
{
	class Program
	{
		static void Main()
		{
			HostFactory.Run(x =>
			{
				x.Service<BridgeService>(s =>
				{
					s.ConstructUsing(name => new BridgeService());
					s.WhenStarted(service => service.Start());
					s.WhenStopped(service => service.Stop());
				});

				x.UseNLog();

				x.StartAutomatically();
				x.RunAsLocalSystem();

				// For the services applet
				x.SetDescription("Allows integration of TP-Link devices with SmartThings");
				x.SetDisplayName("TP-Link to SmartThings Bridge Service");

				// Used for net start/stop
				x.SetServiceName("tplinkstbridge");
			});
		}
	}
}

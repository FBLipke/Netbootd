namespace Netboot.Common.Network.HTTP
{
	public class NetbootHttpContext
	{

		public HttpRequest Request { get; private set; }

		public HttpResponse Response { get; private set; }

		public NetbootHttpContext(HttpRequest request)
		{
			Request = request;
		}
	}
}

namespace AngleSharp.Xml.Tests.Mocks
{
    using AngleSharp.Io;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Serves fixed resources for the mock: and http: protocols, so tests load external
    /// declarations through the resource loader without leaving the machine: registered as the
    /// only requester, no request reaches the network, and the http: addresses use the reserved
    /// .test domain. Unknown addresses get a 404.
    /// </summary>
    sealed class MockRequester : BaseRequester
    {
        public const String Protocol = "mock";

        private readonly Dictionary<String, String> _resources = new Dictionary<String, String>(StringComparer.Ordinal);

        public List<String> RequestedAddresses { get; } = new List<String>();

        public MockRequester Serve(String address, String content)
        {
            _resources[address] = content;
            return this;
        }

        public override Boolean SupportsProtocol(String protocol) =>
            String.Equals(protocol, Protocol, StringComparison.OrdinalIgnoreCase) ||
            String.Equals(protocol, "http", StringComparison.OrdinalIgnoreCase);

        protected override Task<IResponse> PerformRequestAsync(Request request, CancellationToken cancel)
        {
            var address = request.Address.Href;
            RequestedAddresses.Add(address);
            var found = _resources.TryGetValue(address, out var content);
            IResponse response = new DefaultResponse
            {
                Address = request.Address,
                StatusCode = found ? HttpStatusCode.OK : HttpStatusCode.NotFound,
                Content = new MemoryStream(Encoding.UTF8.GetBytes(found ? content : String.Empty)),
            };
            return Task.FromResult(response);
        }

        /// <summary>
        /// A configuration with XML, resource loading enabled and this requester as the only one,
        /// so no request can reach the network.
        /// </summary>
        public IConfiguration CreateConfiguration(Boolean isResourceLoadingEnabled = true) =>
            Configuration.Default
                .WithXml()
                .With(this)
                .WithDefaultLoader(new LoaderOptions { IsResourceLoadingEnabled = isResourceLoadingEnabled });
    }
}

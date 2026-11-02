namespace AngleSharp.Xml.Benchmarks
{
    using AngleSharp.Io;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Serves file: URLs from the local disk, as AngleSharp.Io's file requester does, so the
    /// External rows can load their DTD through the resource loader.
    /// </summary>
    sealed class LocalFileRequester : BaseRequester
    {
        public override Boolean SupportsProtocol(String protocol) =>
            String.Equals(protocol, "file", StringComparison.OrdinalIgnoreCase);

        protected override Task<IResponse> PerformRequestAsync(Request request, CancellationToken cancel)
        {
            var path = new Uri(request.Address.Href).LocalPath;
            IResponse response = new DefaultResponse
            {
                Address = request.Address,
                StatusCode = HttpStatusCode.OK,
                Content = new MemoryStream(File.ReadAllBytes(path)),
            };
            return Task.FromResult(response);
        }
    }
}

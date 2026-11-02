namespace AngleSharp.Xml.Benchmarks
{
    using AngleSharp.Io;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Serves the External rows' DTD from the local disk, so builds that fetch the external subset
    /// through the resource loader can load it. The DOCTYPE names the plain Windows path older
    /// builds read directly; as a URL, its drive letter is the protocol (C:\... is c:).
    /// </summary>
    sealed class LocalFileRequester : BaseRequester
    {
        public override Boolean SupportsProtocol(String protocol) =>
            protocol.Length == 1 && Char.IsLetter(protocol[0]);

        protected override Task<IResponse> PerformRequestAsync(Request request, CancellationToken cancel)
        {
            IResponse response = new DefaultResponse
            {
                Address = request.Address,
                StatusCode = HttpStatusCode.OK,
                Content = new MemoryStream(File.ReadAllBytes(request.Address.Href)),
            };
            return Task.FromResult(response);
        }
    }
}

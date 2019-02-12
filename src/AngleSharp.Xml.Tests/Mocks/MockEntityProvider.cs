namespace AngleSharp.Xml.Tests.Mocks
{
    using AngleSharp.Dom;
    using System;

    sealed class MockEntityProvider : IEntityProvider
    {
        private readonly Func<String, String> _resolver;

        public MockEntityProvider(Func<String, String> resolver)
        {
            _resolver = resolver;
        }

        public String GetSymbol(String name)
        {
            return _resolver.Invoke(name);
        }
    }
}

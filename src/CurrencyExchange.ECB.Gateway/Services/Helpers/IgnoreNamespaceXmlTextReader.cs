using System.Xml;

namespace CurrencyExchange.ECB.Gateway.Services.Helpers
{
    public class IgnoreNamespaceXmlTextReader : XmlTextReader
    {
        public IgnoreNamespaceXmlTextReader(TextReader reader) : base(reader)
        {
        }

        public override string NamespaceURI => string.Empty;
    }
}

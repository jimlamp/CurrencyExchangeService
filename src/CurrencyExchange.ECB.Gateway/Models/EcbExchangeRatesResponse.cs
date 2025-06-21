using System.Xml.Serialization;

namespace CurrencyExchange.ECB.Gateway.Models
{
    
    public class EcbExchangeRatesResponse
    {
        public bool HasError { get; set; }
     
        public Envelope Envelope { get; set; }
    }

    [XmlRoot(ElementName = "Envelope")]
    public class Envelope
    {
        [XmlElement("Cube")]
        public EcbCubeRoot CubeRoot { get; set; }
    }

    public class EcbCubeRoot
    {
        [XmlElement("Cube")]
        public List<EcbTimeCube> TimeCubes { get; set; }
    }

    public class EcbTimeCube
    {
        [XmlAttribute("time")]
        public string Time { get; set; }

        [XmlElement("Cube")]
        public List<EcbCurrencyRate> Rates { get; set; }
    }

    public class EcbCurrencyRate
    {
        [XmlAttribute("currency")]
        public string Currency { get; set; }

        [XmlAttribute("rate")]
        public decimal Rate { get; set; }
    }
}

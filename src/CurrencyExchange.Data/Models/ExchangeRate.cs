using System.ComponentModel.DataAnnotations;

namespace CurrencyExchange.Data.Models
{
    public class ExchangeRate
    {
        [Key]
        public int Id { get; set; }

        public string Currency { get; set; }

        public decimal Rate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}

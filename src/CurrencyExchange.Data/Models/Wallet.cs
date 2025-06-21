using System.ComponentModel.DataAnnotations;

namespace CurrencyExchange.Data.Models
{
    public class Wallet
    {
        [Key]
        public int Id { get; set; }

        public decimal Balance { get; set; }

        public string Currency { get; set; }
    }
}

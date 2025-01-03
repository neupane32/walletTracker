using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace practice.Models
{
    public class User
    {
        public required string Username { get; set; }
        public required string Password { get; set; } 
         public required string Email { get; set; }

        public int Credit { get; set; } = 0;

        public int Debit { get; set; } = 0;

        public int Debt { get; set; } = 0;

        public List<Transaction> Transactions { get; set; } = new List <Transaction>();

    }
}

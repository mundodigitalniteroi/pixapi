using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Models.PayloadModels
{
    public class Merchant
    {
        public Merchant(string _name, string _city) { this.Name = _name; this.City = _city; }

        public string Name { get; set; }
        public string City { get; set; }
    }
}

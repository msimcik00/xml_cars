using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xml_cars.Models
{
    class Car
    {
        public required string ModelName { get; set; }
        public DateTime SaleDate { get; set; }
        public double Price { get; set; }
        public double Tax { get; set; }
    }
}
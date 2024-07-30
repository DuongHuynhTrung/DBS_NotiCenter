using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class DriverOnline : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Radius { get; set; }
        public List<Guid> ListDrivers { get; set; } = new List<Guid>();
    }
}

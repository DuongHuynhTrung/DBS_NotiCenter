using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models;

public class LocationModel
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class DriverOnlineModel
{
    public Guid CustomerId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Radius { get; set; }
    public List<Guid> ListDrivers { get; set; } = new List<Guid>();
}

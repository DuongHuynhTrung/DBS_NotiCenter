using AutoMapper;
using Data.DataAccess;
using Data.Entities;
using Data.Models;
using MongoDB.Driver;
using Services.SignalR;
using System.Device.Location;

namespace Services.Core
{
    public interface ILocationService
    {
        Task TrackingDriverLocation(KafkaModel model);
        Task GetDriverOnline(KafkaModel model);
        Task DriverStatusOnline(KafkaModel model);
        Task DriverStatusOffline(KafkaModel model);
        Task DriverStatusBusy(KafkaModel model);
        Task DriverStatusFree(KafkaModel model);
    }

    public class LocationService : ILocationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILocationHub _locationHub;
        private readonly IFireBaseNotificationService _fireBaseNotificationService;
        public LocationService(AppDbContext dbContext, IMapper mapper, ILocationHub locationHub, IFireBaseNotificationService fireBaseNotificationService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _locationHub = locationHub;
            _fireBaseNotificationService = fireBaseNotificationService;
        }

        public async Task TrackingDriverLocation(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    await _locationHub.TrackingDriverLocation(_mapper.Map<LocationModel>(kafkaModel.Payload), item.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task GetDriverOnline(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var driverOnlines = _mapper.Map<DriverOnline>(kafkaModel.Payload);
                    var checkExist = _dbContext.DriverOnlines.Find(_ => _.CustomerId == driverOnlines.CustomerId).FirstOrDefault();
                    if (checkExist != null)
                    {
                        checkExist.ListDrivers = driverOnlines.ListDrivers;
                        _dbContext.DriverOnlines.ReplaceOne(_ => _.Id == checkExist.Id, checkExist);
                        await _locationHub.GetDriverOnlines(_mapper.Map<DriverOnlineModel>(driverOnlines), item.ToString());
                    }
                    else
                    {
                        await _dbContext.DriverOnlines.InsertOneAsync(driverOnlines);
                        await _locationHub.GetDriverOnlines(_mapper.Map<DriverOnlineModel>(driverOnlines), item.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task DriverStatusOnline(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var driverLocation = _mapper.Map<LocationModel>(kafkaModel.Payload);
                    var driverOnlines = _dbContext.DriverOnlines.Find(_ => !_.IsDeleted).ToList();
                    foreach (var driverOnline in driverOnlines)
                    {
                        double distance = CalculateDistance(driverOnline.Latitude, driverOnline.Longitude, driverLocation.Latitude, driverLocation.Longitude);

                        if (distance <= 3000) // khoảng cách tính bằng mét
                        {
                            driverOnline.ListDrivers.Add(item);
                            _dbContext.DriverOnlines.ReplaceOne(_ => _.Id == driverOnline.Id, driverOnline);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task DriverStatusOffline(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var driverLocation = _mapper.Map<LocationModel>(kafkaModel.Payload);
                    var driverOnlines = _dbContext.DriverOnlines.Find(_ => !_.IsDeleted).ToList();
                    foreach (var driverOnline in driverOnlines)
                    {
                        if (driverOnline.ListDrivers.Contains(item))
                        {
                            driverOnline.ListDrivers.Remove(item);
                            _dbContext.DriverOnlines.ReplaceOne(_ => _.Id == driverOnline.Id, driverOnline);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task DriverStatusBusy(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var driverLocation = _mapper.Map<LocationModel>(kafkaModel.Payload);
                    var driverOnlines = _dbContext.DriverOnlines.Find(_ => !_.IsDeleted).ToList();
                    foreach (var driverOnline in driverOnlines)
                    {
                        if (driverOnline.ListDrivers.Contains(item))
                        {
                            driverOnline.ListDrivers.Remove(item);
                            _dbContext.DriverOnlines.ReplaceOne(_ => _.Id == driverOnline.Id, driverOnline);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task DriverStatusFree(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var driverLocation = _mapper.Map<LocationModel>(kafkaModel.Payload);
                    var driverOnlines = _dbContext.DriverOnlines.Find(_ => !_.IsDeleted).ToList();
                    foreach (var driverOnline in driverOnlines)
                    {
                        double distance = CalculateDistance(driverOnline.Latitude, driverOnline.Longitude, driverLocation.Latitude, driverLocation.Longitude);

                        if (distance <= 3000) // khoảng cách tính bằng mét
                        {
                            driverOnline.ListDrivers.Add(item);
                            _dbContext.DriverOnlines.ReplaceOne(_ => _.Id == driverOnline.Id, driverOnline);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        private double CalculateDistance(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            var coordinate1 = new GeoCoordinate(latitude1, longitude1);
            var coordinate2 = new GeoCoordinate(latitude2, longitude2);
            return coordinate1.GetDistanceTo(coordinate2); // khoảng cách tính bằng mét
        }
    }
}

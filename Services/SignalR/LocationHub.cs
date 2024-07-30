using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Services.ClaimExtensions;

namespace Services.SignalR
{

    public interface ILocationHub
    {
        Task TrackingDriverLocation(LocationModel model, string userId);
        Task GetDriverOnlines(DriverOnlineModel model, string userId);
    }
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class LocationHub : Hub, ILocationHub
    {
        public static ConcurrentDictionary<string, List<string>> ConnectedUsers = new ConcurrentDictionary<string, List<string>>();
        public IHubContext<LocationHub> Current { get; set; }

        public LocationHub(IHubContext<LocationHub> current)
        {
            Current = current;
        }

        public async Task TrackingDriverLocation(LocationModel model, string userId)
        {
            try
            {
                List<string> ReceiverConnectionids;
                ConnectedUsers.TryGetValue(userId, out ReceiverConnectionids);
                await Current.Clients.Clients(ReceiverConnectionids).SendAsync("TrackingDriverLocation", model);
            }
            catch (Exception) { }
        }

        public async Task GetDriverOnlines(DriverOnlineModel model, string userId)
        {
            try
            {
                List<string> ReceiverConnectionids;
                ConnectedUsers.TryGetValue(userId, out ReceiverConnectionids);
                await Current.Clients.Clients(ReceiverConnectionids).SendAsync("DriverOnlines", model);
            }
            catch (Exception) { }
        }
    }
}

using System.Xml.Linq;
using Data.Entities;
using MongoDB.Driver;

namespace Data.DataAccess
{
    public class AppDbContext
    {
        private readonly IMongoDatabase _db;
        private IMongoClient _mongoClient;
        private ReadPreference readPreference;

        public AppDbContext(IMongoClient client, string databaseName)
        {
            _db = client.GetDatabase(databaseName);
            _mongoClient = client;
            readPreference = new ReadPreference(ReadPreferenceMode.Primary);
        }

        public IMongoCollection<Notification> Notifications => _db.GetCollection<Notification>("notifications");
        public IMongoCollection<User> Users => _db.GetCollection<User>("users");
        public IMongoCollection<DriverOnline> DriverOnlines => _db.GetCollection<DriverOnline>("driveronlines");
        public IMongoCollection<Message> Messages => _db.GetCollection<Message>("messages");
        public IMongoCollection<Conversation> Conversations => _db.GetCollection<Conversation>("conversations");

        public IClientSessionHandle StartSession()
        {
            return _mongoClient.StartSession();
        }

        public void CreateCollectionsIfNotExists()
        {
            var collectionNames = _db.ListCollectionNames().ToList();
            if (!collectionNames.Any(name => name == "notifications"))
            {
                _db.CreateCollection("notifications");
            }

            if (!collectionNames.Any(name => name == "users"))
            {
                _db.CreateCollection("users");
            }

            if (!collectionNames.Any(name => name == "driveronlines"))
            {
                _db.CreateCollection("driveronlines");
            }

            if (!collectionNames.Any(name => name == "messages"))
            {
                _db.CreateCollection("messages");
            }
            if (!collectionNames.Any(name => name == "conversations"))
            {
                _db.CreateCollection("conversations");
            }
        }
    }
}


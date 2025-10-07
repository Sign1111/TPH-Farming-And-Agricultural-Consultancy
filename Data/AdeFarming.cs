using Ade_Farming.Models;  
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Ade_Farming.Data
{
    public class AdeFarming
    {
         public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDB:ConnectionString"];
            var databaseName = configuration["MongoDB:DatabaseName"];

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        // Example: Users collection
    }
    }
}

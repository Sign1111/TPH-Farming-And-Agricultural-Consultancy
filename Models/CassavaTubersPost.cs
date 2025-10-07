using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;

namespace Ade_Farming.Models
{
    [CollectionName("CassavaTubersPost")]

    public class CassavaTubersPost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; } // Stored in DB
        public string SellerId { get; set; }
        public string SellerName { get; set; }
        public string SellerEmail { get; set; } // Use registered email
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System;

namespace Ade_Farming.Models
{
    [CollectionName("CassavaStemPosts")]
    public class CassavaStemPost
    {
        [BsonId]  // Marks as MongoDB primary key
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }   // Use string so Mongo can auto-generate

        public string Title { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }  // Stored in DB
        public string SellerId { get; set; }
        public string SellerName { get; set; }
        public string SellerEmail { get; set; }  

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

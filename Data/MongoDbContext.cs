﻿using KDomBackend.Helpers;
using KDomBackend.Models.MongoEntities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace KDomBackend.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        
        public IMongoCollection<KDom> KDoms => _database.GetCollection<KDom>("kdoms");
        public IMongoCollection<Post> Posts => _database.GetCollection<Post>("posts");
        public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("comments");
        public IMongoCollection<UserProfile> UserProfiles => _database.GetCollection<UserProfile>("user_profiles");
        public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("notifications");
        public IMongoCollection<Activity> Activities => _database.GetCollection<Activity>("activity");

        
        public IMongoCollection<KDomEdit> KDomEdits => _database.GetCollection<KDomEdit>("kdom_edits");
        public IMongoCollection<PostEdit> PostEdits => _database.GetCollection<PostEdit>("post_edits");
        public IMongoCollection<CommentEdit> CommentEdits => _database.GetCollection<CommentEdit>("comment_edits");
    }
}

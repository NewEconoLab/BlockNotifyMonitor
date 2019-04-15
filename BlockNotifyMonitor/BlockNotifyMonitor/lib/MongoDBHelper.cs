using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace BlockNotifyMonitor
{
    class MongoDBHelper
    {
        public JArray GetData(string mongodbConnStr, string mongodbDatabase, string coll, string findStr, string fieldStr = "{_id:0}", string sortStr = "{}", int skip = 0, int limit = 0)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = null;
            if (limit == 0)
            {
                query = collection.Find(BsonDocument.Parse(findStr)).Project(BsonDocument.Parse(fieldStr)).ToList();
            }
            else
            {
                query = collection.Find(BsonDocument.Parse(findStr)).Project(BsonDocument.Parse(fieldStr)).Sort(sortStr).Skip(skip).Limit(limit).ToList();
            }
            client = null;

            if (query.Count > 0)
            {
                return JArray.Parse(query.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict }));
            }
            else { return new JArray(); }
        }
    }

}

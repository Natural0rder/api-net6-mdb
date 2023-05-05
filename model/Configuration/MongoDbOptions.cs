namespace model.Configuration
{
    public class MongoDbOptions
    {
        public const string MongoDb = "MongoDb";

        public string Database { get; set; } = String.Empty;
        public string ArticlesCollectionName { get; set; } = String.Empty;

        public string PlacesCollectionName { get; set; } = String.Empty;
    }
}
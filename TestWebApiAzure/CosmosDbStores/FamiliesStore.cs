using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using TestWebApiAzure.Controllers;

namespace TestWebApiAzure.CosmosDbStores
{
    public class FamiliesStore
    {
        private const string EndpointUrl = "https://testwebapidocdb.documents.azure.com:443/";
        private const string PrimaryKey = "axtbeQJTKErHbzLuXj12jWshBJAV5MnUKlqHS4uTJsSBYe7LEsH1wAWi2A7Xm4xY08BJbaKX9ehMXgr8j92hWw==";
        private readonly DocumentClient _client;
        private readonly Uri _familiesLink;

        public FamiliesStore()
        {
            _client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
            _familiesLink = UriFactory.CreateDocumentCollectionUri("familiesdb", "families");
        }

        public IReadOnlyCollection<Family> GetFamilies()
        {
            return _client.CreateDocumentQuery<Family>(_familiesLink)                
                .ToArray();
        }

        public Family GetFamily(string id)
        {
            return _client
                .CreateDocumentQuery<Family>(_familiesLink)
                .Where(f => f.Id == id)
                .AsEnumerable()
                .FirstOrDefault();
        }

        public async Task InsertFamily(List<Family> families)
        {
            foreach (var family in families)
            {
                await _client.CreateDocumentAsync(_familiesLink, family);
            }
        }
    }
}
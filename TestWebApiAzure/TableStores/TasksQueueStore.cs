using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace TestWebApiAzure.TableStores
{
    public class TasksQueueStore
    {
        private readonly CloudQueueClient _client;
        private readonly Uri _baseUri = new Uri("https://testwebapistore.queue.core.windows.net/");

        private CloudQueue TasksQueue => _client.GetQueueReference("tasks");

        public TasksQueueStore(string storageAccountKey)
        {
            _client = new CloudQueueClient(_baseUri, 
                new StorageCredentials("testwebapistore", storageAccountKey));
        }

        public async Task QueueMessage(object msg)
        {
            await TasksQueue.CreateIfNotExistsAsync();

            var message = new CloudQueueMessage(JsonConvert.SerializeObject(msg));
            await TasksQueue.AddMessageAsync(message);
        }

        public async Task<object> PeekMessage()
        {
            var result = await TasksQueue.PeekMessageAsync();
            return JsonConvert.DeserializeObject(result.AsString);
        }

        public async Task<object> GetMessage()
        {
            var result = await TasksQueue.GetMessageAsync();
            await TasksQueue.DeleteMessageAsync(result);
            return JsonConvert.DeserializeObject(result.AsString);
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using TestWebApiAzure.Models;

namespace TestWebApiAzure.TableStores
{
    public class EmployeeStore
    {
        private readonly CloudTableClient _client;
        private readonly Uri _baseUri = new Uri("https://testwebapistore.table.core.windows.net/");
    
        public EmployeeStore()
        {
            _client = new CloudTableClient(_baseUri, new StorageCredentials("testwebapistore",
                "pHuPKdWxMfzmWvHA9XcooKvzrtUaLDswgZkGTLqdn15lJnf6sukra+NVPd45Cvl0GkwykWmL16qc5NGawlzbUQ=="));
        }

        public async Task<Employee> GetEmployee(string id)
        {
            var table = _client.GetTableReference("employees");
            var operation = TableOperation.Retrieve<Employee>("AA", id);
            var ds = await table.ExecuteAsync(operation);
            return ds.Result as Employee;
        }
    }
}
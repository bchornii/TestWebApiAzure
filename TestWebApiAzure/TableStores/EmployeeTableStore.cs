using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using TestWebApiAzure.Models;

namespace TestWebApiAzure.TableStores
{
    public class EmployeeTableStore
    {
        private readonly CloudTableClient _client;
        private readonly Uri _baseUri = new Uri("https://testwebapistore.table.core.windows.net/");

        private CloudTable EmployeeTable => _client.GetTableReference("employees");

        public EmployeeTableStore(string storageAccountKey)
        {
            _client = new CloudTableClient(_baseUri, 
                new StorageCredentials("testwebapistore", storageAccountKey));
        }

        public async Task<IReadOnlyCollection<Employee>> GetEmployee(string partitionKey)
        {
            var filter = TableQuery.GenerateFilterCondition(
                "PartitionKey", QueryComparisons.Equal, partitionKey);
            var query = new TableQuery<Employee>().Where(filter);
            var employees = await EmployeeTable.ExecuteQueryAsync(query);
            return employees;
        }

        public async Task<Employee> GetEmployee(string partitionKey, string rowId)
        {
            var operation = TableOperation.Retrieve<Employee>(partitionKey, rowId);
            var ds = await EmployeeTable.ExecuteAsync(operation);
            return ds.Result as Employee;
        }

        public async Task CreateEmployee(Employee employee)
        {
            await EmployeeTable.CreateIfNotExistsAsync();
            var tableOperation = TableOperation.Insert(employee);
            await EmployeeTable.ExecuteAsync(tableOperation);
        }

        public async Task CreateEmployee(Employee[] employees)
        {
            var batch = new TableBatchOperation();
            foreach (var employee in employees)
            {
                batch.Insert(employee);
            }
            await EmployeeTable.ExecuteBatchAsync(batch);
        }

        public async Task UpdateEmployee(Employee employee)
        {            
            var operation = TableOperation.Replace(employee);
            await EmployeeTable.ExecuteAsync(operation);
        }

        public async Task DeleteEmployee(Employee employee)
        {
            var operation = TableOperation.Delete(employee);
            await EmployeeTable.ExecuteAsync(operation);
        }
    }
}
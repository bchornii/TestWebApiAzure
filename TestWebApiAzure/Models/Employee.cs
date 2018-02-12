using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace TestWebApiAzure.Models
{
    public class Employee : TableEntity
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public Employee()
        {
            PartitionKey = "US";
            RowKey = Guid.NewGuid().ToString();
        }
    }
}
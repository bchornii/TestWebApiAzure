using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TestWebApiAzure.BlobStores;
using TestWebApiAzure.CosmosDbStores;
using TestWebApiAzure.Models;
using TestWebApiAzure.TableStores;

namespace TestWebApiAzure.Controllers
{    
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly FamiliesStore _familiesStore;
        private readonly ImageBlobStore _imageStore;
        private readonly EmployeeTableStore _employeeStore;
        private readonly TasksQueueStore _queueStore;

        public ValuesController(IOptions<AppSettings> appOptions)
        {
            _appSettings = appOptions.Value;      
            _familiesStore = new FamiliesStore();
            _imageStore = new ImageBlobStore();
            _employeeStore = new EmployeeTableStore();
            _queueStore = new TasksQueueStore();
        }

        // Get data from Azure SQL Db

        #region Azure Sql Db

        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            using (var connection = new SqlConnection(_appSettings.ConnectionStrings?.TestWebApiDb))
            {
                var result = await connection.QueryAsync<string>("SELECT name FROM section_type");
                return result;
            }
        }

        #endregion

        #region Azure CosmosDB
        
        [HttpGet("families")]
        public IActionResult GetFamilies()
        {
            var f = _familiesStore.GetFamilies();
            return Ok(f);
        }
        
        [HttpGet("families/{id}")]
        public IActionResult GetFamily(string id)
        {
            var f = _familiesStore.GetFamily(id);
            return Ok(f);
        }

        [HttpPost]
        public async Task Post([FromBody]string value)
        {
            // Create a Family object.
            var mother = new Parent { familyName = "Wakefield", givenName = "Robin" };
            var father = new Parent { familyName = "Miller", givenName = "Ben" };
            var child = new Child { familyName = "Merriam", givenName = "Jesse", gender = "female", grade = 1 };
            var pet = new Pet { givenName = "Fluffy" };
            var address = new Address { state = "NY", county = "Manhattan", city = "NY" };
            var family = new Family { parents = new[] { mother, father }, children = new[] { child }, isRegistered = false };

            await _familiesStore.InsertFamily(new List<Family> { family });
        }

        #endregion        
        
        // Deployment slots test
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return _appSettings.AppName + " v2";
        }

        // Application Ins test
        [HttpGet("error")]
        public IActionResult GetError()
        {
            throw new NotImplementedException("This feature has not been implemented");
        }       

        #region Azure Blob Storage

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var id = await _imageStore.SaveImage(file.OpenReadStream());
            return RedirectToAction("GetUploadedImageUrl", new { imageId = id });
        }

        // Get file link from Azure Blob storage
        [HttpGet("imageurl")]
        public string GetUploadedImageUrl(string imageId)
        {
            var uri = _imageStore.UriFor(imageId);
            return uri.ToString();
        }

        // Get list of blobs Uri
        [HttpGet("images/all")]
        public async Task<IActionResult> GetAllImages()
        {
            var result = await _imageStore.GetBlobsUri();
            return Ok(result);
        }

        #endregion

        #region Azure Storage Table

        [HttpGet("employee/{partitionKey}")]
        public async Task<IActionResult> GetEmployee(
            string partitionKey)
        {
            var result = await _employeeStore
                .GetEmployee(partitionKey);
            return Ok(result);
        }

        [HttpGet("employee/{partitionKey}/{rowId}")]
        public async Task<IActionResult> GetEmployee(
            string partitionKey, string rowId)
        {
            var result = await _employeeStore
                .GetEmployee(partitionKey, rowId);
            return Ok(result);
        }

        [HttpPost("employee")]
        public async Task<IActionResult> CreateEmployee(
            [FromBody] Employee[] employees)
        {
            await _employeeStore.CreateEmployee(employees);
            return Ok(employees);
        }

        [HttpPut("employee/{partitionKey}/{id}")]
        public async Task<IActionResult> UpdateEmployee(
            [FromBody] Employee employee)
        {
            await _employeeStore.UpdateEmployee(employee);
            return NoContent();
        }

        [HttpDelete("employee/{partitionKey}/{id}")]
        public async Task<IActionResult> DeleteEmployee(
            [FromBody] Employee employee)
        {
            await _employeeStore.DeleteEmployee(employee);
            return NoContent();
        }

        #endregion

        #region Azure Queue
        [HttpPost("task")]
        public async Task<IActionResult> CreateMessage(
            [FromBody] object msg)
        {
            await _queueStore.QueueMessage(msg);
            return Ok();
        }

        [HttpGet("task/peek")]
        public async Task<IActionResult> PeekMessage()
        {
            var result = await _queueStore.PeekMessage();
            return Ok(result);
        }

        [HttpGet("task")]
        public async Task<IActionResult> GetMessage()
        {
            var result = await _queueStore.GetMessage();
            return Ok(result);
        }
        #endregion
    }
}

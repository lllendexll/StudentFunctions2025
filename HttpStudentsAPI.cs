using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StudentFunctions.Models.School;

namespace School.Function
{
    public class HttpStudentsAPI
    {

        private readonly SchoolContext _context;

        public HttpStudentsAPI(ILoggerFactory loggerFactory, SchoolContext context)
        {
            _logger = loggerFactory.CreateLogger<HttpStudentsAPI>();
            _context = context;
        }


        private readonly ILogger<HttpStudentsAPI> _logger;

        public HttpStudentsAPI(ILogger<HttpStudentsAPI> logger)
        {
            _logger = logger;
        }

        [Function("HttpStudentsAPI")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }

        [Function("GetStudents")]
        public HttpResponseData GetStudents(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "students")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP GET/posts trigger function processed a request in GetStudents().");

            var students = _context.Students.ToArray();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            response.WriteStringAsync(JsonConvert.SerializeObject(students));

            return response;
        }

        [Function("GetStudentById")]
        public HttpResponseData GetStudentById
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "students/{id}")] HttpRequestData req,
            int id
        )
        {
            _logger.LogInformation("Processing GET request for student ID: " + id);

            var student = _context.Students.FindAsync(id).Result;
            if (student == null)
            {
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteStringAsync("{\"message\": \"Student not found\"}");
                return response;
            }

            var response2 = req.CreateResponse(HttpStatusCode.OK);
            response2.Headers.Add("Content-Type", "application/json");
            response2.WriteStringAsync(JsonConvert.SerializeObject(student));
            return response2;
        }


        [Function("CreateStudent")]
        public HttpResponseData CreateStudent
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "students")] HttpRequestData req
        )
        {
            _logger.LogInformation("Processing POST request to create a student.");

            var student = JsonConvert.DeserializeObject<Student>(req.ReadAsStringAsync().Result);
            _context.Students.Add(student);
            _context.SaveChanges();

            var response = req.CreateResponse(HttpStatusCode.Created);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteStringAsync(JsonConvert.SerializeObject(student));
            return response;
        }


        [Function("UpdateStudent")]
        public HttpResponseData UpdateStudent
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "students/{id}")] HttpRequestData req,
            int id
        )
        {
            _logger.LogInformation("Processing PUT request to update student ID: " + id);

            var student = _context.Students.FindAsync(id).Result;
            if (student == null)
            {
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteStringAsync("{\"message\": \"Student not found\"}");
                return response;
            }

            var updatedStudent = JsonConvert.DeserializeObject<Student>(req.ReadAsStringAsync().Result);
            student.FirstName = updatedStudent.FirstName;
            student.LastName = updatedStudent.LastName;
            student.School = updatedStudent.School;
            
            _context.SaveChanges();

            var response2 = req.CreateResponse(HttpStatusCode.OK);
            response2.Headers.Add("Content-Type", "application/json");
            response2.WriteStringAsync(JsonConvert.SerializeObject(student));
            return response2;
        }


        [Function("DeleteStudent")]
        public HttpResponseData DeleteStudent
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "students/{id}")] HttpRequestData req,
            int id
        )
        {
            _logger.LogInformation("Processing DELETE request for student ID: " + id);

            var student = _context.Students.FindAsync(id).Result;
            if (student == null)
            {
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteStringAsync("{\"message\": \"Student not found\"}");
                return response;
            }

            _context.Students.Remove(student);
            _context.SaveChanges();

            var response2 = req.CreateResponse(HttpStatusCode.OK);
            response2.Headers.Add("Content-Type", "application/json");
            response2.WriteStringAsync(JsonConvert.SerializeObject(student));
            return response2;
        }




    }
}

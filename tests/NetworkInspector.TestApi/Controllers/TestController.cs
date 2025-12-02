using Microsoft.AspNetCore.Mvc;

namespace NetworkInspector.TestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TestController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { Message = "Pong", Time = DateTime.UtcNow });
        }

        [HttpPost("echo")]
        public IActionResult Echo([FromBody] object data)
        {
            return Ok(data);
        }

        [HttpGet("http-call")]
        public async Task<IActionResult> MakeHttpCall()
        {
            // Call a public API using HttpClientFactory for proper Activity tracking
            try 
            {
                var httpClient = _httpClientFactory.CreateClient("my-client");
                var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/todos/1");
                var content = await response.Content.ReadAsStringAsync();
                return Ok(new { Status = response.StatusCode, Content = content });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
        
        [HttpGet("http-call1")]
        public async Task<IActionResult> MakeHttpCall1()
        {
            // Call a public API using HttpClientFactory for proper Activity tracking
            try 
            {
                var httpClient = _httpClientFactory.CreateClient("my-client");
                var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/todos/1");
                var response1 = await httpClient.GetAsync("https://jsonplaceholsdsdder.typicode.com/todos/1");
                var content = await response.Content.ReadAsStringAsync();
                return Ok(new { Status = response.StatusCode, Content = content });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
        
        [HttpGet("error")]
        public IActionResult Error()
        {
            return StatusCode(500, new { Error = "Something went wrong" });
        }
    }
}

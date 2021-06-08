using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Skraebul_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WordsController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching", "Afrika paprika"
        };

        private readonly ILogger<WordsController> _logger;

        public WordsController(ILogger<WordsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            string[] words = new string[3];

            for (int i = 0; i < words.Length; i++)
            {
                words[i] = Summaries[new Random().Next(0, Summaries.Length)];
            }
            return Ok(words);
        }
    }
}

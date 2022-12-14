using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQWordToPdf.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWordToPdf.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult WordToPdfPage()
        {
            return View();
        }

        [HttpPost]
        public IActionResult WordToPdfPage(WordToPdf wordToPdf)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_configuration["ConnectionStrings:RabbitMQ"])
            };
            using (var conn = factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare("convert-exchange", ExchangeType.Direct, true, false, null);
                    channel.QueueDeclare(queue: "file", true, false, false, null);
                    channel.QueueBind("file", "convert-exchange", "WordToPdf");
                    byte[] byteWord = null;
                    using (var stream = new MemoryStream())
                    {
                        wordToPdf.WordFile.CopyTo(stream);
                        byteWord = stream.ToArray();
                    }

                    var message = new MessageWordToPdf
                    {
                        Email = wordToPdf.Email,
                        FileName = Path.GetFileNameWithoutExtension(wordToPdf.WordFile.FileName),
                        WordByte = byteWord
                    };

                    string jsonMessage = JsonConvert.SerializeObject(message);
                    byte[] byteMessage = Encoding.UTF8.GetBytes(jsonMessage);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish("convert-exchange", "WordToPdf", basicProperties: properties,body:byteMessage);

                    ViewBag.result = "Dosyanız Pdf'e dönüştürülmek üzere işleme alındı. Tamamlandığında size email gönderilecektir";
                }
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

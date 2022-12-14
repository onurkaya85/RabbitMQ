using RabbitMQ.Client;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.Publisher
{
    public enum LogNames
    {
        Critical = 1,
        Error = 2,
        Warning = 3,
        Info = 4
    }

    class Program
    {
        static void Main(string[] args)
        {
            #region Without_Exchange
            //var factory = new ConnectionFactory();
            //factory.Uri = new Uri("amqps://uvhluous:YNx_NQnUqTG6-aVL5KeQIyIMFWWPyWMA@cow.rmq2.cloudamqp.com/uvhluous");
            //
            //using (var connection = factory.CreateConnection())
            //{
            //    var channel = connection.CreateModel();
            //
            //    //durable false olursa queue lar memoryde durur. rabbitMq restart olursa kuyruk silinir.
            //    //exclusive true olursa bu kuyruğua sadece burda oluşturulan channel üzerinden ulaşılabilir.
            //    //autoDelete true olursa ortada subscriber olmazsa ya da subscribe  lar down olursa kuyruk silinir.
            //    channel.QueueDeclare("hello-queue", true, false, false);
            //
            //    Enumerable.Range(1, 20).ToList().ForEach(x =>
            //    {
            //        string message = $"Message {x}";
            //        var messageBody = Encoding.UTF8.GetBytes(message);
            //        channel.BasicPublish(string.Empty, "hello-queue", null, messageBody);
            //        Console.WriteLine($"Mesaj gönderilmiştir:{ message}");
            //       
            //    });
            //    Console.ReadLine();
            //
            //}
            #endregion

            #region With_FanoutExchange

            //MainWithFanoutExchange();
            #endregion

            #region With_DirectExchange

            MainWithDirectExchange();
            #endregion

            #region With_TopicExchange

            //MainWithTopicExchange();
            #endregion

            #region With_HeaderExchange

            //MainWithHeaderExchange();
            #endregion
        }

        static void MainWithFanoutExchange()
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(".....uvhluous:YNx_NQnUqTG6-aVL5........QIyIMFWWPyWMA@cow.rmq2.cloudamqp.com/uvhluous");

            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();

                channel.ExchangeDeclare("logs-fanout", durable: true, type: ExchangeType.Fanout);

                Enumerable.Range(1, 20).ToList().ForEach(x =>
                {
                    string message = $"Log {x}";
                    var messageBody = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish("logs-fanout","", null, messageBody);
                    Console.WriteLine($"Mesaj gönderilmiştir:{ message}");

                });
                Console.ReadLine();

            }
        }

        static void MainWithDirectExchange()
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(".....uvhluous:YNx_NQnUqTG6-aVL5........QIyIMFWWPyWMA@cow.rmq2.cloudamqp.com/uvhluous");

            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();

                channel.ExchangeDeclare("logs-direct", durable: true, type: ExchangeType.Direct);

                Enum.GetNames(typeof(LogNames)).ToList().ForEach(c =>
                {
                    var routeKey = $"route-{c}";
                    var queueName = $"direct-queue-{c}";
                    channel.QueueDeclare(queueName, true, false, false);
                    channel.QueueBind(queueName, "logs-direct",routeKey, null);
                });


                Enumerable.Range(1, 20).ToList().ForEach(x =>
                {
                    LogNames log = (LogNames)new Random().Next(1, 5);

                    string message = $"log-type: {log}";
                    var messageBody = Encoding.UTF8.GetBytes(message);

                    var routeKey = $"route-{log}";

                    channel.BasicPublish("logs-direct", routeKey, null, messageBody);
                    Console.WriteLine($"Log gönderilmiştir:{ message}");

                }); 
                Console.ReadLine();

            }
        }

        static void MainWithTopicExchange()
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(".....uvhluous:YNx_NQnUqTG6-aVL5........QIyIMFWWPyWMA@cow.rmq2.cloudamqp.com/uvhluous");

            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();

                channel.ExchangeDeclare("logs-topic", durable: true, type: ExchangeType.Topic);

                Random rnd = new Random();
                Enumerable.Range(1, 20).ToList().ForEach(x =>
                {
                    LogNames log1 = (LogNames)rnd.Next(1, 5);
                    LogNames log2 = (LogNames)rnd.Next(1, 5);
                    LogNames log3 = (LogNames)rnd.Next(1, 5);

                    var routeKey = $"{log1}.{log2}.{log3}";
                    string message = $"log-type: {log1}-{log2}-{log3}";
                    var messageBody = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish("logs-topic", routeKey, null, messageBody);
                    Console.WriteLine($"Log gönderilmiştir:{ message}");

                });
                Console.ReadLine();

            }

        }

        static void MainWithHeaderExchange()
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(".....uvhluous:YNx_NQnUqTG6-aVL5........QIyIMFWWPyWMA@cow.rmq2.cloudamqp.com/uvhluous");

            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();

                channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers);

                var headers = new Dictionary<string, object>();
                headers.Add("format", "pdf");
                headers.Add("shape", "a4");

                var properties = channel.CreateBasicProperties();
                properties.Headers = headers;

                //Tüm exchangelerde properties oluşturup Persistent property sini true yaparsak mesaj da kalıcı hae gelir.Silinmez

                var product = new Product
                {
                    Id = 1,
                    Name = "Kalem",
                    Price = 20,
                    Stock = 5
                };

                var productJson = JsonSerializer.Serialize(product);
                channel.BasicPublish("header-exchange", string.Empty, properties,Encoding.UTF8.GetBytes(productJson));
                Console.WriteLine("Mesaj gönderilmiştir");
                Console.ReadLine();
            }
        }
    }
}

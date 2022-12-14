using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace RabbitMQ.Subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Without_Exchange
            //var factory = new ConnectionFactory();
            //factory.Uri = new Uri("amqps://uvhluous:YNx_NQnUqTG6-aVL5KeQIyIMFWWPyWMA@cow.rmq2.cloudamqp.com/uvhluous");
            //
            //using var connection = factory.CreateConnection();
            //
            //    var channel = connection.CreateModel();
            //
            ////1 değeri kaç mesajın gideceğeni gösterir.
            ////global değeri false olursa her subscriber a 1 tane gönderir. true olursa mesaj sayını subbscriber sayısına böler. 6 tane ise 2 subscriber varsa 3'er 3'er göndrir. false olursa hepsine 6 gönderir.
            //channel.BasicQos(0, 1, false);
            //
            ////Bu satırı silmezsek publisher bu kuyruğu oluşturamadıysa burası oluşturur. Bi zararı yoktur. 
            ////channel.QueueDeclare("hello-queue", true, false, false);
            //var consumer = new EventingBasicConsumer(channel);
            //
            ////autoAck true olursa rabbitmq mesajı, subscriber mesajı aldığında doğru da işlese yanlış da işlese siler.false olursa subscriber işldikten sonra haber verir.
            //channel.BasicConsume("hello-queue", false, consumer);
            //
            //consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            //{
            //    var message = Encoding.UTF8.GetString(e.Body.ToArray());
            //    Thread.Sleep(1500);
            //    Console.WriteLine("Message:" + message);
            //
            //        //RabbitMq ya ilgili mesajın işlendiğini haber veriyoruz.
            //        channel.BasicAck(e.DeliveryTag, false);
            //};
            //
            //Console.ReadLine();
            #endregion

            #region With_FanoutExchange

            // MainWithFanoutExchange();
            #endregion

            #region With_DirectExchange

            //MainWithDirectExchange();
            #endregion

            #region With_TopicExchange

            //MainWithTopicExchange();
            #endregion

            #region With_HeaderExchange

            MainWithHeaderExchange();
            #endregion
        }

        static void MainWithFanoutExchange()
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(".....uvhluous:YNx_NQnUqTG6-aVL5........QIyIMFWWPyWMA@cow.rmq2.cloudamqp.com/uvhluous");

            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            var randomQueueName = channel.QueueDeclare().QueueName;

            //QueueBind ile yapıyoruz ki sunscriber işini bitirdikten sonra kuyruk silinsin. QueueDeclare ile yaparsak silinmez.
            channel.QueueBind(randomQueueName, "logs-fanout", "", null);

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);

            channel.BasicConsume(randomQueueName, false, consumer);

            Console.WriteLine("Logları dinliyorum");

            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Thread.Sleep(1500);
                Console.WriteLine("Message:" + message);

                //RabbitMq ya ilgili mesajın işlendiğini haber veriyoruz.
                channel.BasicAck(e.DeliveryTag, false);
            };

            Console.ReadLine();
        }

        static void MainWithDirectExchange()
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(".....uvhluous:YNx_NQnUqTG6-aVL5........QIyIMFWWPyWMA@cow.rmq2.cloudamqp.com/uvhluous");

            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            var queueName = "direct-queue-Critical";
            channel.BasicConsume(queueName, false, consumer);

            Console.WriteLine("Logları dinliyorum");

            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Thread.Sleep(1500);
                Console.WriteLine("Message:" + message);
                //bin-debug-net5 in içine atar dosyayı
                //File.AppendAllText("log-critical.txt", message+ "\n");
                //RabbitMq ya ilgili mesajın işlendiğini haber veriyoruz.
                channel.BasicAck(e.DeliveryTag, false);
            };

            Console.ReadLine();
        }

        static void MainWithTopicExchange()
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(".....uvhluous:YNx_NQnUqTG6-aVL5........QIyIMFWWPyWMA@cow.rmq2.cloudamqp.com/uvhluous");

            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            var queueName = channel.QueueDeclare().QueueName;
            var routeKey = "*.Error.*";
            channel.QueueBind(queueName, "logs-topic", routeKey);
            channel.BasicConsume(queueName, false, consumer);

            Console.WriteLine("Logları dinliyorum");

            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Thread.Sleep(1500);
                Console.WriteLine("Message:" + message);
                //bin-debug-net5 in içine atar dosyayı
                //File.AppendAllText("log-critical.txt", message+ "\n");
                //RabbitMq ya ilgili mesajın işlendiğini haber veriyoruz.
                channel.BasicAck(e.DeliveryTag, false);
            };

            Console.ReadLine();
        }

        static void MainWithHeaderExchange()
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(".....uvhluous:YNx_NQnUqTG6-aVL5........QIyIMFWWPyWMA@cow.rmq2.cloudamqp.com/uvhluous");

            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            var queueName = channel.QueueDeclare().QueueName;


            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("format", "pdf");
            headers.Add("shape", "a4");
            headers.Add("x-match", "all");

            
            channel.QueueBind(queueName, "header-exchange",string.Empty, headers);
            channel.BasicConsume(queueName, false, consumer);

            Console.WriteLine("Logları dinliyorum");

            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                var product = JsonSerializer.Deserialize<Product>(message);

                Thread.Sleep(1500);
                Console.WriteLine($"Gelen Mesaj: {product.Id}-{product.Name}-{product.Price}-{product.Stock}");

                channel.BasicAck(e.DeliveryTag, false);
            };

            Console.ReadLine();
        }
    }
}

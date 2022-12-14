using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Spire.Doc;
using System;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace RabbitMqWordToPdf.Consumer
{
    class Program
    {
        public static bool SendEmail(string email,MemoryStream ms,string fileName)
        {
            try
            {
                ms.Position = 0;
                System.Net.Mime.ContentType ct = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Application.Pdf);
                Attachment att = new Attachment(ms, ct);
                att.ContentDisposition.FileName = $"{fileName}.pdf";
                MailMessage message = new MailMessage();
                SmtpClient smtpClient = new SmtpClient();
                message.From = new MailAddress("test132test@test.com");
                message.To.Add(email);
                message.Subject = "Pdf Dosyası";
                message.Body = "Pdf dosyanız oluşturulmuştur ve ektedir";
                message.IsBodyHtml = true;
                message.Attachments.Add(att);
                smtpClient.Host = "";
                smtpClient.Port = 587;
                smtpClient.Credentials = new System.Net.NetworkCredential("", "");
                smtpClient.Send(message);

                ms.Close();
                ms.Dispose();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error");
                return false;
            }
        }

        static void Main(string[] args)
        {
            bool result = false;
            var factory = new ConnectionFactory
            {
                Uri = new Uri(".....uvhluous:YNx_NQnUqTG6-aVL5........QIyIMFWWPyWMA@cow.rmq2.cloudamqp.com/uvhluous")
            };
            using (var conn = factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare("convert-exchange", ExchangeType.Direct, true, false, null);
                    channel.QueueBind(queue: "file", exchange: "convert-exchange",routingKey: "WordToPdf");
                    channel.BasicQos(0, 1, false);
                    var consumer = new EventingBasicConsumer(channel);
                    channel.BasicConsume("file", false, consumer);

                    consumer.Received += (model,ea) => 
                    {
                        try
                        {
                            Console.WriteLine("Kuyruktan bir mesaj alındı ve işleniyor");
                            Document document = new Document();
                            string deserializeValue = Encoding.UTF8.GetString(ea.Body.ToArray());
                            var message = JsonSerializer.Deserialize<MessageWordToPdf>(deserializeValue);
                            document.LoadFromStream(new MemoryStream(message.WordByte), FileFormat.Docm2013);
                            using (var ms = new MemoryStream())
                            {
                                document.SaveToStream(ms, FileFormat.PDF);
                                result = SendEmail(message.Email, ms, message.FileName);
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("Hata:" + ex.Message);
                        }

                        if(result)
                        {
                            Console.WriteLine("Kuyruktan mesaj işlendi.");
                            channel.BasicAck(ea.DeliveryTag, false);
                        }
                    };

                    Console.WriteLine("Çıkmak için tıklayınız");
                    Console.ReadLine();
                }
            }

        }
    }
}

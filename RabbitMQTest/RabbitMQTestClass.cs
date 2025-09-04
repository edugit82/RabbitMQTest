using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RabbitMQTest
{
    public class RabbitMQTestClass
    {
        IConnection? conn;
        IChannel? channel;

        private async Task OpenConnection()
        {
            try
            {
                ConnectionFactory factory = new ConnectionFactory();
                factory.HostName = "68.211.177.16"; // Or the IP address of your RabbitMQ server
                factory.UserName = "edco82_rabbitmq";     // Or your configured username
                factory.Password = "sU6#pf2@";     // Or your configured password
                factory.VirtualHost = "/";      // Or your desired virtual host

                // this name will be shared by all connections instantiated by
                // this factory
                factory.ClientProvidedName = "Test RabbitMQ";

                this.conn = await factory.CreateConnectionAsync();
                this.channel = await conn.CreateChannelAsync();

                await this.channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
            }
        }
        private async Task CloseConnection()
        {
            try
            {
                if (this.channel is null)
                    return;

                if (this.conn is null)
                    return;

                await this.channel.CloseAsync();
                await this.channel.DisposeAsync();

                await this.conn.CloseAsync();
                await this.conn.DisposeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
            }
        }
        private async Task Sender(int pos)
        {
            // Your message sending logic here

            if (this.channel is null)
                return;

            string message = $"Hello World! {pos:00}";
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello", body: body);
            Debug.WriteLine($" [x] Sent {message}");

            Debug.WriteLine(" Press [enter] to exit.");
        }
        private async Task Receiver()
        {
            if (this.channel is null)
                return;

            Debug.WriteLine(" [*] Waiting for messages.");

            var consumer = new AsyncEventingBasicConsumer(channel);
            int i = 0; ;
            consumer.ReceivedAsync += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Debug.WriteLine($" [x] Received {message}");
                Debug.WriteLine($" Pos: {i:00}");
                i++;
                return Task.CompletedTask;
            };

            await channel.BasicConsumeAsync("hello", autoAck: true, consumer: consumer);

            Debug.WriteLine(" Press [enter] to exit.");

        }

        [Fact]
        public async Task AOpenConnection()
        {
            await OpenConnection();
        }
        [Fact]
        public async Task ESender()
        {
            await Sender(0);
        }
        [Fact]
        public async Task EReceiver()
        {
            await Receiver();
        }
        [Fact]
        public async Task ZCloseConnection()
        {
            await CloseConnection();
        }

        [Fact]
        public async Task FullTest()
        {
            int intervalo = 10000;

            await OpenConnection();
            await Task.Delay(intervalo); // Wait for a while to receive messages
            await Sender(0);
            await Task.Delay(intervalo); // Wait for a while to receive messages
            await Receiver();
            await Task.Delay(intervalo); // Wait for a while to receive messages
            await CloseConnection();
        }
        
        [Fact]
        public async Task Enviar() 
        {   
            for (var i = 0; i < 10; i++)
            {
                await OpenConnection();
                await Sender(i);
                await Task.Delay(1000);
                await CloseConnection();
            }            
        }

        [Fact]
        public async Task Receber()
        {
            await OpenConnection();
            await Receiver();
            await CloseConnection();
        }
    }
}

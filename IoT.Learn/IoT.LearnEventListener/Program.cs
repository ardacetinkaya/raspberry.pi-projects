using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.LearnEventListener
{
    class Program
    {
        static void Main(string[] args)
        {

            EventListener listener = new EventListener();
            listener.Start();

        }
    }


    public class EventListener
    {

        private readonly string _connectionString = "";
        private readonly string _endPoint = "messages/events";
        private EventHubClient _eventHubClient;
        private string[] _partions;
        public EventListener()
        {
            _eventHubClient = EventHubClient.CreateFromConnectionString(_connectionString, _endPoint);

            _partions = _eventHubClient.GetRuntimeInformation().PartitionIds;

        }

        public void Start()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            System.Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...Please wait.");
            };

            var tasks = new List<Task>();
            foreach (string partition in _partions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cts.Token));
            }
            Task.WaitAll(tasks.ToArray());
        }

        private async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            var eventHubReceiver = _eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);
            while (true)
            {
                if (ct.IsCancellationRequested) break;
                EventData eventData = await eventHubReceiver.ReceiveAsync();
                if (eventData == null)
                {
                    Console.WriteLine("No data");
                    continue;
                }

                string data = Encoding.UTF8.GetString(eventData.GetBytes());
                Console.WriteLine($"Message is received. Data {data}");
            }

        }
    }
}

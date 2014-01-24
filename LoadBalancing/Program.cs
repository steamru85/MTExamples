using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;

namespace LoadBalancing
{
    class Program
    {
        static void Main(string[] args)
        {

            int firstServerFilesCount = 0;
            IServiceBus firstServer = ServiceBusFactory.New(sbc =>
            {
                sbc.UseRabbitMq();
                //указываем количество паралельных потоков получающих сообщения с сервера очередей
                sbc.SetConcurrentConsumerLimit(3);
                sbc.Subscribe(subs => subs.Handler<VideoFile>(msg =>
                    {
                        firstServerFilesCount++; 
                        Thread.Sleep(msg.TimeToConvert);
                        Console.WriteLine("Сервер 1. {0}Файл {1} обработан за {2} мс. Потоков: {3} из 3. ThreadId - {4}", Environment.NewLine, msg.Num, msg.TimeToConvert, firstServerFilesCount, Thread.CurrentThread.ManagedThreadId); 
                        firstServerFilesCount--;
                    }));
                //prefetch=3. Сообщаем серверу очередей что мы готовы разбирать до трех сообщений одновременно 
                sbc.ReceiveFrom("rabbitmq://localhost/filesToConvert?prefetch=3");
            });


            int secondServerFilesCount = 0;
            IServiceBus secondServer = ServiceBusFactory.New(sbc =>
            {
                sbc.UseRabbitMq();
                //указываем количество паралельных потоков получающих сообщения с сервера очередей
                sbc.SetConcurrentConsumerLimit(5);
                sbc.UseControlBus();
                sbc.Subscribe(subs => subs.Handler<VideoFile>(msg =>
                {
                    secondServerFilesCount++;
                    Thread.Sleep(msg.TimeToConvert);
                    Console.WriteLine("Сервер 2. {0}Файл {1} обработан за {2} мс. Потоков: {3} из 5. ThreadId - {4}", Environment.NewLine, msg.Num, msg.TimeToConvert, secondServerFilesCount, Thread.CurrentThread.ManagedThreadId);
                    secondServerFilesCount--;
                }));
                //prefetch=3. Сообщаем серверу очередей что мы готовы разбирать до пяти сообщений одновременно 
                sbc.ReceiveFrom("rabbitmq://localhost/filesToConvert?prefetch=5");
            });


            IServiceBus publisher = ServiceBusFactory.New(sbc =>
            {
                sbc.UseRabbitMq();
                sbc.ReceiveFrom("rabbitmq://localhost/publisher");
            });

            Random rnd = new Random();
            for (int i = 1; i <= 100; i++)
            {
                publisher.Publish(new VideoFile() {Num = i, TimeToConvert = rnd.Next(100, 5000)});
            }

        }
    }
}

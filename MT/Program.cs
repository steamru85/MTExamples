using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace MT
{
    class Program
    {
        static void Main(string[] args) 
        {   
            //подписчик
            IServiceBus subscriber = ServiceBusFactory.New(sbc =>
            {
                //указываем что в качестве транспорта мы будем использовать rabbitMq
                sbc.UseRabbitMq();
                //указываем очередь из которой мы будем получать сообщения на которые мы подписались
                sbc.ReceiveFrom("rabbitmq://localhost/subscriber");
                //подписываемся на сообщение KeyWasPressed. При поступлении соответствующего сообщения выводим его на экран
                sbc.Subscribe(subs => subs.Handler<KeyWasPressed>(msg => Console.WriteLine("{0}{1}{2}{3}",Environment.NewLine,"Key  '", msg.PressedKey, "' was pressed")));
            });

            //издатель
            IServiceBus publisher = ServiceBusFactory.New(sbc =>
            {
                //указываем что в качестве транспорта мы будем использовать rabbitMq
                sbc.UseRabbitMq();
                //указываем очередь из которой мы будем получать сообщения
                sbc.ReceiveFrom("rabbitmq://localhost/publisher");
            });

            //еще один подписчик
            IServiceBus anotherSubscriber = ServiceBusFactory.New(sbc =>
            {
                //указываем что в качестве транспорта мы будем использовать rabbitMq
                sbc.UseRabbitMq();
                //указываем очередь из которой мы будем получать сообщения на которые мы подписались
                sbc.ReceiveFrom("rabbitmq://localhost/anothersubscriber");
                //подписываемся на сообщение KeyWasPressed. При поступлении соответствующего сообщения выводим его на экран
                sbc.Subscribe(subs => subs.Handler<KeyWasPressed>(msg => Console.WriteLine("{0}{1}{2}{3}", Environment.NewLine, "Key with code  ", (int)msg.PressedKey, " was pressed")));
            });

         
            Console.WriteLine("Press any key...");

            while (true)
            {
                publisher.Publish(new KeyWasPressed() { PressedKey = Console.ReadKey().Key });
            }
        }
    }
}

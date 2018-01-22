using System;
using System.Threading.Tasks;
using PB.ITOps.Messaging.PatLite;
using PB.ITOps.Messaging.PatSender;

namespace TestSubscriber
{
    public class EventToPublish
    {
        
    }

    public class PatLiteTestHandler : IHandleEvent<MyEvent1>, IHandleEvent<MyEvent2>, IHandleEvent<MyEvent1AllNew>
    {
        private readonly IMessagePublisher _messagePublisher;

        public PatLiteTestHandler(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task HandleAsync(MyEvent1 message)
        {
            Console.WriteLine("HandleAsync:MyEvent1");
            await _messagePublisher.PublishEvent(new EventToPublish());
        }

        public Task HandleAsync(MyEvent2 message)
        {
            Console.WriteLine("HandleAsync:MyEvent2");
            return Task.CompletedTask;
        }

        public Task HandleAsync(MyDerivedEvent2 message)
        {
            throw new NotImplementedException();
        }

        public Task HandleAsync(MyEvent1AllNew message)
        {
            throw new NotImplementedException();
        }
    }
}
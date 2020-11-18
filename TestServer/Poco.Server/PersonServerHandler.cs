using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using Poco.Comon;
using System;
using System.Diagnostics;

namespace Poco.Server
{
    public class PersonServerHandler : SimpleChannelInboundHandler<Person>
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<PersonServerHandler>();

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            // Detect when client disconnects
            ctx.Channel.CloseCompletion.ContinueWith((x) => Console.WriteLine("Channel Closed") );
        }

        // The Channel is closed hence the connection is closed
        public override void ChannelInactive(IChannelHandlerContext ctx) => Console.WriteLine("Client disconnected");

        protected override void ChannelRead0(IChannelHandlerContext ctx, Person person)
        {
            
            Console.WriteLine("Received message: " + person);
            person.Name = person.Name.ToUpper();
            ctx.WriteAndFlushAsync(person);
        }

        public override bool IsSharable => true;
    }
}
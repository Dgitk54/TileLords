using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Reactive.Linq;
using MessagePack;
using System.IO;
using DataModel.Common.Messages;

namespace GameServer
{

    class Program
    {
        static MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        static async Task Main(string[] args)
        {
            var listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 8080));

            Console.WriteLine("Listening on port 8080");
            listenSocket.Listen(120);

            while (true)
            {
                var socket = await listenSocket.AcceptAsync();
                _ = ProcessLinesAsync(socket);
            }
        }
        static byte[] NewLineDelimiter = new byte[] { (byte)'\n' };
        static byte[] ProtocolEnd = new byte[] { (byte)'\n', (byte)'\n', (byte)'\n' };

        private static async Task ProcessLinesAsync(Socket socket)
        {
            Console.WriteLine($"[{socket.RemoteEndPoint}]: connected");

            // Create a PipeReader over the network stream
            var stream = new NetworkStream(socket);
            var reader = PipeReader.Create(stream);
            var writer = PipeWriter.Create(stream);

            var inboundTraffic = new Subject<byte[]>();

            var disposable = ReactiveSocketWrapper.AttachGatewayNoTask(inboundTraffic).Subscribe(v => writer.WriteAsync(v));

            /*var disposable = ReactiveSocketWrapper.AttachGateWay(inboundTraffic)
                .Select(v => Observable.Defer(() => Observable.Start(() =>
                {
                    var data = MessagePackSerializer.Serialize(v, lz4Options);
                    var result = data.Concat(NewLineDelimiter).ToArray();
                    return new ReadOnlyMemory<byte>(result);
                }).Catch<ReadOnlyMemory<byte>, Exception>(v => { return Observable.Empty<ReadOnlyMemory<byte>>(); })))
                .SelectMany(v => v)
                .Subscribe(v => 
                {
                    writer.WriteAsync(v);
                });
            */


            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;

                while (TryParseMessageSequenceReader(ref buffer, out ReadOnlySequence<byte> line))
                {
                    // Process the line.
                    ProcessLine(line, inboundTraffic);
                }

                // Tell the PipeReader how much of the buffer has been consumed.
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming.
                if (result.IsCompleted)
                {
                    break;
                }
            }
            disposable.Dispose();
            // Mark the PipeReader as complete.
            await reader.CompleteAsync();

            Console.WriteLine($"[{socket.RemoteEndPoint}]: disconnected");
        }

        private static bool TryParseMessageSequenceReader(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> msg)
        {
            ReadOnlySpan<byte> readOnly = NewLineDelimiter;

            SequenceReader<byte> reader = new SequenceReader<byte>(buffer);
            bool advanceResult = true;
            while (advanceResult)
            {
                advanceResult = reader.TryAdvanceToAny(readOnly, true);
                if (IsProtocolEnd(ref reader))
                {
                    reader.Rewind(1);
                    msg = buffer.Slice(0, reader.Position);
                    buffer = buffer.Slice(buffer.GetPosition(4, reader.Position));
                    return true;
                }
            }
            msg = default;
            return false;
        }

        public static bool IsProtocolEnd(ref SequenceReader<byte> reader)
        {
            ReadOnlySpan<byte> readonlyProtocolEnd = ProtocolEnd;
            return reader.IsNext(readonlyProtocolEnd, false);
        }

        private static void ProcessLine(in ReadOnlySequence<byte> buffer, IObserver<byte[]> observableWrapper)
        {
            foreach (var segment in buffer)
            {
                observableWrapper.OnNext(segment.Span.ToArray());
            }
            Console.WriteLine();
        }

    }

}

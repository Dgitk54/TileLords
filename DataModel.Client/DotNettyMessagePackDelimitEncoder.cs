﻿using DataModel.Common.Messages;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DataModel.Client
{
    public class DotNettyMessagePackDelimitEncoder : MessageToByteEncoder<object>
    {
        MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        public byte[] newLineDelimiter = new byte[] { (byte)'\n' };
        protected override void Encode(IChannelHandlerContext context, object message, IByteBuffer output)
        {
            if (message is IMessage)
            {
                var msg = message as IMessage;
                var data = MessagePackSerializer.Serialize(msg, lz4Options);
                var withDelimiter = data.Concat(newLineDelimiter).Concat(newLineDelimiter).Concat(newLineDelimiter).Concat(newLineDelimiter).ToArray();
                output.WriteBytes(withDelimiter);
            }
            else
            {
                throw new Exception("Encoding non messagepack objects");
            }
        }
    }
}

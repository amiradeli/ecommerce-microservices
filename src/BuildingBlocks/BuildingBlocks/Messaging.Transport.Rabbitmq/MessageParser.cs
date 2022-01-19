using System.Text;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Serialization;
using BuildingBlocks.Core.Objects;
using BuildingBlocks.Domain.Events.External;
using Microsoft.Net.Http.Headers;
using RabbitMQ.Client;

namespace BuildingBlocks.Messaging.Transport.Rabbitmq;

public class MessageParser : IMessageParser
{
    private readonly IMessageSerializer _serializer;
    private readonly ITypeResolver _typeResolver;

    public MessageParser(IMessageSerializer serializer, ITypeResolver typeResolver)
    {
        _serializer = serializer;
        _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
    }

    public IIntegrationEvent Resolve(IBasicProperties basicProperties, byte[] body)
    {
        if (basicProperties is null)
            throw new ArgumentNullException(nameof(basicProperties));
        if (body is null)
            throw new ArgumentNullException(nameof(body));

        if (basicProperties.Headers is null)
            throw new ArgumentNullException(nameof(basicProperties), "message headers are missing");

        if (!basicProperties.Headers.TryGetValue(HeaderNames.MessageType, out var tmp) || tmp is not byte[] messageTypeBytes)
            throw new ArgumentException("invalid message type");

        var messageTypeName = Encoding.UTF8.GetString(messageTypeBytes);

        var dataType = _typeResolver.Resolve(messageTypeName);
        if (dataType is null)
            throw new ArgumentException("unable to detect message type from headers");

        var decodedObj = _serializer.Deserialize(System.Text.Encoding.UTF8.GetString(body), dataType);
        if (decodedObj is not IIntegrationEvent message)
            throw new ArgumentException($"message has the wrong type: '{messageTypeName}'");
        return message;
    }
}
﻿namespace BuildingBlocks.Messaging.Message
{
    public interface IMessageProcessor
    {
        Task ProcessAsync<TMessage>(TMessage message, IMessageContext messageContext = null, CancellationToken
            cancellationToken = default) where TMessage : IMessage;
    }
}
﻿namespace BuildingBlocks.Messaging.Message;

public interface IMessageDispatcher
{
    public Task DispatchMessageAsync<TMessage>(
        TMessage message,
        IMessageContext messageContext = null,
        CancellationToken cancellationToken = default)
        where TMessage : IMessage;
}
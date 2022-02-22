﻿using System.Text;
using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Core.Events;
using EventStore.Client;
using Newtonsoft.Json;

namespace BuildingBlocks.Persistence.EventStoreDB.Serialization;

public static class EventStoreDbSerializer
{
    public static T Deserialize<T>(this ResolvedEvent resolvedEvent) => (T)Deserialize(resolvedEvent);

    public static object Deserialize(this ResolvedEvent resolvedEvent)
    {
        // get type
        var eventType = EventTypeMapper.ToType(resolvedEvent.Event.EventType);

        // deserialize event
        return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span), eventType!)!;
    }

    public static EventData ToJsonEventData(this IEvent @event) =>
        new(
            Uuid.NewUuid(),
            EventTypeMapper.ToName(@event.GetType()),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { }))
        );
}

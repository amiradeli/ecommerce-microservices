﻿using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Serialization;
using BuildingBlocks.Abstractions.Scheduler;
using BuildingBlocks.CQRS.Command;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Serialization;
using BuildingBlocks.Scheduling.Internal.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Scheduling.Internal.MessagesScheduler;

public class InternalScheduler : ICommandScheduler, IMessageScheduler, IScheduler
{
    private readonly InternalMessageSchedulerOptions _options;
    private readonly ILogger<InternalScheduler> _logger;
    private readonly IInternalSchedulerService _schedulerService;

    public InternalScheduler(
        IOptions<InternalMessageSchedulerOptions> options,
        ILogger<InternalScheduler> logger,
        IInternalSchedulerService schedulerService,
        IMessageSerializer messageSerializer
    )
    {
        _options = options.Value;
        _logger = logger;
        _schedulerService = schedulerService;
    }

    public Task ScheduleAsync(IInternalCommand command, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(command, nameof(command));
        return _schedulerService.SaveAsync(command, cancellationToken);
    }

    public Task ScheduleAsync(IInternalCommand[] commands, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(commands, nameof(commands));
        return _schedulerService.SaveAsync(commands, cancellationToken);
    }

    public Task ScheduleAsync(IInternalCommand command, DateTimeOffset scheduleAt, string? description = null)
    {
        throw new NotImplementedException();
    }

    public Task ScheduleAsync(IInternalCommand[] command, DateTimeOffset scheduleAt, string? description = null)
    {
        throw new NotImplementedException();
    }

    public Task ScheduleRecurringAsync(
        IInternalCommand command,
        string name,
        string cronExpression,
        string? description = null)
    {
        throw new NotImplementedException();
    }

    public Task ScheduleAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(message, nameof(message));
        return _schedulerService.SaveAsync(message, cancellationToken);
    }

    public Task ScheduleAsync(IMessage[] messages, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(messages, nameof(messages));
        return _schedulerService.SaveAsync(messages, cancellationToken);
    }

    public Task ScheduleAsync(
        ScheduleSerializedObject scheduleSerializedObject,
        DateTimeOffset scheduleAt,
        string? description = null)
    {
        throw new NotImplementedException();
    }

    public Task ScheduleAsync(
        ScheduleSerializedObject scheduleSerializedObject,
        TimeSpan delay,
        string? description = null)
    {
        throw new NotImplementedException();
    }

    public Task ScheduleRecurringAsync(
        ScheduleSerializedObject scheduleSerializedObject,
        string name,
        string cronExpression,
        string? description = null)
    {
        throw new NotImplementedException();
    }
}

﻿using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Persistence.Dapper;

public static class Extensions
{
    public static void AddCustomDapper(IServiceCollection services)
    {
        services.AddTransient(typeof(IDapperRepository<>), typeof(DapperRepository<>));
    }
}

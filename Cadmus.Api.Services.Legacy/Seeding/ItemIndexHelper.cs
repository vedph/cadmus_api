﻿using Cadmus.Index.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Cadmus.Api.Services.Seeding;

/// <summary>
/// Item index helper.
/// </summary>
public static class ItemIndexHelper
{
    /// <summary>
    /// Gets the index factory.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>Factory or null.</returns>
    public static async Task<ItemIndexFactory?> GetIndexFactoryAsync(
        IConfiguration configuration, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        string? profileSource = configuration["Seed:ProfileSource"];
        if (string.IsNullOrEmpty(profileSource)) return null;

        ResourceLoaderService loaderService = new(serviceProvider);

        string profile;
        using (StreamReader reader = new(
            (await loaderService.LoadAsync(profileSource))!, Encoding.UTF8))
        {
            profile = reader.ReadToEnd();
        }

        IItemIndexFactoryProvider factoryProvider =
            serviceProvider.GetService<IItemIndexFactoryProvider>()!;
        return factoryProvider.GetFactory(profile);
    }
}

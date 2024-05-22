﻿using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.TelegramClient.Commands.Handler;

namespace PasswordManager.TelegramClient.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAllFormRegistrations(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes();
        var formRegistrationTypes = types.Where(t => typeof(IFormRegistration).IsAssignableFrom(t) && !t.IsAbstract);
        foreach (var type in formRegistrationTypes)
        {
            services.AddTransient(typeof(IFormRegistration), type);
        }
        return services;
    }
}
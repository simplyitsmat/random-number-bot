﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordRandomNumber.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordRandomNumber
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(string[] args)
        {
            var builder = new ConfigurationBuilder() // Create a new instance of the config builder
                .SetBasePath(AppContext.BaseDirectory) // Specify the default location for the config file
                .AddYamlFile("config.yml"); // Add this (yaml encoded) file to the configuration
            Configuration = builder.Build(); // Build the configuration
        }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            // Create a new instance of a service collection
            var services = new ServiceCollection();
            ConfigureServices(services);

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Start the logging service, and the command handler service
            provider.GetRequiredService<LoggingService>();
            provider.GetRequiredService<CommandHandler>();

            // Start the startup service
            await provider.GetRequiredService<StartupService>().StartAsync();

            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                // Add discord to the collection
                LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                MessageCacheSize = 1000 // Cache 1,000 messages per channel
            }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    // Add the command service to the collection
                    LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                    DefaultRunMode = RunMode.Async, // Force all commands to run async by default
                }))
                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()
                .AddSingleton<LoggingService>()
                .AddSingleton<Random>()
                .AddSingleton(Configuration);
        }
    }
}
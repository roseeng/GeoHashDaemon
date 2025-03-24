using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.Versioning;
using System.ServiceProcess;
using System.Text;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GeoHashDaemon
{
    public class MyServiceLifetime : WindowsServiceLifetime
    {
        private ILogger _logger;
        private static bool _configured = false;

        public MyServiceLifetime(IHostEnvironment environment, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory, IOptions<HostOptions> optionsAccessor)
        : base(environment, applicationLifetime, loggerFactory, optionsAccessor)
        {
            // Enable whichever events you want to be notified of
            CanPauseAndContinue = true;
            CanShutdown = true;
            CanHandleSessionChangeEvent = true;
            CanHandlePowerEvent = true;

            _logger = loggerFactory.CreateLogger("MyServiceLifetime");

            _configured = true;
        }

        public static bool Configured
        {
            get { return _configured;  }
        }

        protected override void OnStart(String[] args)
        {
            base.OnStart(args);
            // Custom start behaviour

            _logger.LogWarning("OnStart");
        }

        protected override void OnStop()
        {
            base.OnStop();
            // Custom stop behaviour

            _logger.LogWarning("OnStop");
        }

        public static Action<bool> PauseHandler;

        protected override void OnPause()
        {
            base.OnPause();
            // Service continue handler
            PauseHandler?.Invoke(true);

            _logger.LogWarning("OnPause");
        }

        protected override void OnContinue()
        {
            base.OnContinue();
            // Service pause handler
            PauseHandler?.Invoke(false);

            _logger.LogWarning("OnContinue");
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
            // System shutdown handler

            _logger.LogWarning("OnShutdown");
        }

        public static Func<PowerBroadcastStatus, bool, bool> PowerHandler;

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            _logger.LogWarning($"OnPowerEvent: {powerStatus}");
            // Custom power event behaviour

            bool result = base.OnPowerEvent(powerStatus);

            if (PowerHandler != null)
                result = PowerHandler.Invoke(powerStatus, result);

            return result;
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
            // Session change handler

            _logger.LogWarning("OnSessionChange");
        }

        /// <summary>
        /// sc control {service name} {command}
        /// </summary>
        /// <param name="command">An integer value between 128 and 255 inclusive</param>
        protected override void OnCustomCommand(Int32 command)
        {
            base.OnCustomCommand(command);
            // Custom command handler

            _logger.LogWarning($"OnCustomCommand: {command}");
        }
    }
}

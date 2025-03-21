using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.ServiceProcess;

namespace GeoHashDaemon
{
    class GeoHashNotifier : BackgroundService
    {
        private readonly ILogger<GeoHashNotifier> _logger;
        private readonly IConfiguration _config;

        public GeoHashNotifier(ILogger<GeoHashNotifier> logger, IHostApplicationLifetime lifetime, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            lifetime?.ApplicationStarted.Register(OnStarted);
            lifetime?.ApplicationStopping.Register(OnStopping);
            lifetime?.ApplicationStopped.Register(OnStopped);

            logger.LogInformation("This is an information");
            logger.LogWarning($"MyServiceLifetime configured is: {MyServiceLifetime.Configured}");
            if (MyServiceLifetime.Configured)
            {
                MyServiceLifetime.PauseHandler += OnPauseAndContinue;
            }
        }

        private int lastCheckHour = 25; // Keep track of last time we checked, to detect new days and to log every hour
        private bool hasRunToday = false;
        private const int RUN_AT = 16; // DJIA is released 9:30 eastern time => 15:30 CET
        private const int checkday = 1; // 0 =  today, 1 = tomorrow (RUN_AT must be 16 CET or later)

        private void DoWork()
        {
            int latitude = _config.GetValue<int>("latitude");
            int longitude = _config.GetValue<int>("longitude");
            DateTime targetDate = DateTime.Now.AddDays(checkday); // Tomorrow

            List<int> globalLimits = _config.GetSection("globalLimits").Get<List<int>>();
            List<Graticule> localGraticules = _config.GetSection("localGraticules").Get<List<Graticule>>();

            // Globalhash:
            var globalhash = GeoHash.GetGlobalHash(targetDate);
            int glat = GeoHash.Graticule(globalhash[0]);
            int glon = GeoHash.Graticule(globalhash[1]);
            if (glat >= globalLimits[0] && glat <= globalLimits[2] &&
                glon >= globalLimits[1] && glon <= globalLimits[3])
            {
                _logger.LogWarning("Sending a globalhash alert");
                PushoverImpl.SendAlert("Globalhash ALERT", (checkday == 0 ? "Today's" : "Tomorrow's") + $" globalhash is https:" + $"//maps.google.com/maps?q=@{globalhash[0].toGoogle()},{globalhash[1].toGoogle()}");
            }
            else
            {
                // Not between our limits, but we still log the distance
                var castle_pos = new double[] { 59.3262866, 18.0713312 };
                var d = GeoHash.CalcDistance(castle_pos, globalhash);
                var mil = Convert.ToInt32(d / 10000);
                _logger.LogWarning($"Distance from the royal castle to the globalhash is {mil} swedish mil");
                PushoverImpl.SendNotification("Globalhash info", $"Distance from the royal castle to the globalhash is {mil} swedish mil");
            }

            // Localhash:
            var fractions = GeoHash.GetFractions(targetDate, latitude, longitude);
            int centicule = GeoHash.Centicule(fractions);

            foreach (var grat in localGraticules)
            {
                foreach (var centi in grat.centicules)
                {
                    if (centi == centicule)
                    {
                        _logger.LogWarning("Sending a geohash alert");
                        var coords = GeoHash.Fractions2Coord(fractions, grat.lat, grat.lon);
                        PushoverImpl.SendAlert("Geohash alert", $"Tomorrow's geohash is https:" + $"//maps.google.com/maps?q=@{coords[0].toGoogle()},{coords[1].toGoogle()}");
                    }
                }
            }
        }

        private void RunOnceAt()
        {
            if (DateTime.Now.Hour < lastCheckHour) // Current hour _less_ than last time -> must be a new day
            {
                _logger.LogWarning("GeoHashDaemon: A new day!");
                hasRunToday = false;
            }

            if (DateTime.Now.Hour != lastCheckHour) // Not the same hour as last time -> must be a new hour
                _logger.LogWarning($"GeoHashDaemon still alive, hasRunToday={hasRunToday}, Now.Hour={DateTime.Now.Hour}, lastCheckHour={lastCheckHour}, RUN_AT={RUN_AT}");


            lastCheckHour = DateTime.Now.Hour;

            if (!hasRunToday && DateTime.Now.Hour >= RUN_AT)
            {
                hasRunToday = true;

                if (WindowsServiceHelpers.IsWindowsService())
                    _logger.LogWarning("GeoHashNotifier running as a Windows Service at: {time}", DateTime.Now);
                else
                    _logger.LogWarning("GeoHashNotifier running from the command line at: {time}", DateTime.Now);

                DoWork();
            }
            else
            {
#if VERBOSE
                _logger.LogWarning("GeoHashDaemon: Not now, dear.");
#endif
            }
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var mutex = new Mutex(false, "GeoHashNotifier"))
                {
                    if (mutex.WaitOne(1000))
                    {
                        try
                        {
                            RunOnceAt();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, ex.Message);
                        }

                        await Task.Delay(60 * 1000, stoppingToken);
                    }
                    else
                    {
                        _logger.LogWarning("GeoHashNotifier was already running, exiting.");
                    }
                }
            }
            _logger.LogWarning("Cancellation requested - leaving loop");
        }

        private void OnStarted()
        {
            _logger.LogWarning("GeoHashNotifier 1.3 started.");
        }

        private void OnStopping()
        {
            _logger.LogWarning("GeoHashNotifier stopping....");
        }

        private void OnStopped()
        {
            _logger.LogWarning("GeoHashNotifier stopped.");
        }

        private void OnPauseAndContinue(bool pause)
        {
            _logger.LogWarning($"GeoHashNotifier paused: {pause}");
        }

        private bool OnPowerEvent(PowerBroadcastStatus powerStatus, bool result)
        {
            _logger.LogWarning($"GeoHashNotifier Power event: {powerStatus}, base class returned {result}");

            return result;
        }
    }

    public class Graticule
    {
        public int lat { get; set; }
        public int lon { get; set; }
        public int[] centicules { get; set; }
    }
}

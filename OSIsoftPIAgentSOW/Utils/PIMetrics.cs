using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OSIsoft.AF;
using OSIsoft.AF.Diagnostics;
using OSIsoft.AF.PI;

namespace OSIsoftPIAgentSOW.Utils
{
    class PIMetrics
    {
        
        public class MetricsTicker
        {
            public PISystem AssetServer { get; set; }
            public PIServer DataArchive { get; set; }
            public MetricsSnapshot StartSnapshot { get; private set; }
            public MetricsSnapshot EndSnapshot { get; private set; }

            public bool IsStarted
            {
                get
                {
                    return StartSnapshot != null;
                }
            }

            public bool IsStopped
            {
                get
                {
                    return EndSnapshot != null;
                }
            }

            public bool InProgress
            {
                get
                {
                    return IsStarted && !IsStopped;
                }
            }

            public MetricsTicker(PISystem assetServer)
                : this(assetServer, null) { }

            public MetricsTicker(PIServer dataArchive)
                : this(null, dataArchive) { }

            public MetricsTicker(PISystem assetServer, PIServer dataArchive)
            {
                AssetServer = assetServer;
                DataArchive = dataArchive;
            }

            public static MetricsTicker StartNew(PISystem assetServer)
            {
                return StartNew(assetServer, null);
            }
            public static MetricsTicker StartNew(PIServer dataArchive)
            {
                return StartNew(null, dataArchive);
            }
            public static MetricsTicker StartNew(PISystem assetServer, PIServer dataArchive)
            {
                var x = new MetricsTicker(assetServer, dataArchive);
                x.Start();
                return x;
            }

            public void Start()
            {
                if (IsStarted)
                    return;
                StartSnapshot = MetricsSnapshot.GetSnapshot(AssetServer, DataArchive, starting: true);
            }

            public void Stop()
            {
                if (!IsStarted || IsStopped)
                    return;
                EndSnapshot = MetricsSnapshot.GetSnapshot(AssetServer, DataArchive, starting: false);
            }

            public void Reset()
            {
                EndSnapshot = null;
                StartSnapshot = null;
            }

            public void Restart()
            {
                Reset();
                Start();
            }

            public void Resume()
            {
                EndSnapshot = null;
            }

            public string DisplayDelta(bool round = true, bool showServerRpcMetrics = true, bool showClientRpcMetrics = true,
                bool ignoreClienteRpcMetricsCalls = false)
            {
                if (StartSnapshot == null)
                    return "MetricsTicker has not been started.";

                var tempStop = !IsStopped;
                if (tempStop)
                    Stop();

                var output = EndSnapshot.DisplayDelta(StartSnapshot, round, showServerRpcMetrics, showClientRpcMetrics,
                    ignoreClienteRpcMetricsCalls);

                if (tempStop)
                    Resume();

                return output;
            }
        }

        public class MetricsSnapshot
        {
            public PISystem AssetServer { get; set; }
            public PIServer DataArchive { get; set; }
            public long TotalMemory { get; private set; }
            public long WorkingMemory { get; private set; }
            public long PeakWorkingMemory { get; private set; }
            public long NetworkBytesSent { get; private set; }
            public long NetworkBytesRcvd { get; private set; }
            public IList<AFRpcMetric> ClientRpcMetrics { get; private set; }
            public IList<AFRpcMetric> ServerRpcMetrics { get; private set; }
            public IList<AFRpcMetric> DataArchiveRpcMetrics { get; private set; }
            public DateTime Timetamp { get; private set; }

            private string InstanceName { get; set; }
            private const string NetworkingCategoryName = ".NET CLR Networking 4.0.0.0";
            private static int HasGarbageBeenCollected = 0;

            private MetricsSnapshot(PISystem assetServer)
                : this(assetServer, null) { }

            private MetricsSnapshot(PIServer dataArchive)
                : this(null, dataArchive) { }

            private MetricsSnapshot(PISystem assetServer, PIServer dataArchive)
            {
                AssetServer = assetServer;
                DataArchive = dataArchive;
                InstanceName = FindInstanceName();
            }

            private static void CollectGarbage()
            {
                GC.Collect(2, GCCollectionMode.Forced, blocking: true);
                Interlocked.Increment(ref HasGarbageBeenCollected);
            }

            private static long GetTotalMemory(bool forceGC)
            {
                var force = false;
                if (HasGarbageBeenCollected == 0 || forceGC)
                {
                    CollectGarbage();
                    force = true;
                }
                return GC.GetTotalMemory(forceFullCollection: force);
            }

            private long GetWorkingMemory()
            {
                var ram = new PerformanceCounter("Process", "Working Set", Process.GetCurrentProcess().ProcessName);
                return ram.NextSample().RawValue;
            }

            private long GetPeakWorkingMemory()
            {
                var ram = new PerformanceCounter("Process", "Working Set Peak", Process.GetCurrentProcess().ProcessName);
                return ram.NextSample().RawValue;
            }

            private AFRpcMetric[] GetClientRpcMetrics()
            {
                return AssetServer == null ? null : AssetServer.GetClientRpcMetrics();
            }

            private AFRpcMetric[] GetServerRpcMetrics()
            {
                return AssetServer == null ? null : AssetServer.GetRpcMetrics();
            }

            private AFRpcMetric[] GetDataArchiveClientRpcMetrics()
            {
                return DataArchive == null ? null : DataArchive.GetClientRpcMetrics();
            }

            public static MetricsSnapshot GetSnapshot(PISystem assetServer, bool starting)
            {
                return GetSnapshot(assetServer, null, starting);
            }
            public static MetricsSnapshot GetSnapshot(PIServer dataArchive, bool starting)
            {
                return GetSnapshot(null, dataArchive, starting);
            }
            public static MetricsSnapshot GetSnapshot(PISystem assetServer, PIServer dataArchive, bool starting)
            {
                var x = new MetricsSnapshot(assetServer, dataArchive);
                if (starting)
                {
                    x.GetStartSnapshot();
                }
                else
                {
                    x.GetEndSnapshot();
                }
                return x;
            }

            private void GetStartSnapshot()
            {
                ServerRpcMetrics = GetServerRpcMetrics();
                ClientRpcMetrics = GetClientRpcMetrics();
                DataArchiveRpcMetrics = GetDataArchiveClientRpcMetrics();
                NetworkBytesSent = GetPerformanceCounter(NetworkingCategoryName, "Bytes Sent");
                NetworkBytesRcvd = GetPerformanceCounter(NetworkingCategoryName, "Bytes Received");
                TotalMemory = GetTotalMemory(forceGC: true);
                WorkingMemory = GetWorkingMemory();
                PeakWorkingMemory = GetPeakWorkingMemory();
                Timetamp = DateTime.UtcNow;
            }

            private void GetEndSnapshot()
            {
                Timetamp = DateTime.UtcNow;
                WorkingMemory = GetWorkingMemory();
                PeakWorkingMemory = GetPeakWorkingMemory();
                TotalMemory = GetTotalMemory(forceGC: false);
                NetworkBytesSent = GetPerformanceCounter(NetworkingCategoryName, "Bytes Sent");
                NetworkBytesRcvd = GetPerformanceCounter(NetworkingCategoryName, "Bytes Received");
                ClientRpcMetrics = GetClientRpcMetrics();
                ServerRpcMetrics = GetServerRpcMetrics();
                DataArchiveRpcMetrics = GetDataArchiveClientRpcMetrics();
            }

            private static string FindInstanceName()
            {
                PerformanceCounterCategory category = PerformanceCounterCategory.GetCategories().Where(c => c.CategoryName.Contains(NetworkingCategoryName)).First();
                var instances = category.GetInstanceNames();
                string applicationName = Process.GetCurrentProcess().ProcessName.ToLowerInvariant();
                //return instances.Where(n => n.ToLowerInvariant().Contains(applicationName)).First();
                return instances[0];
            }

            private static string GetInstanceName()
            {
                // Used Reflector to find the correct formatting:
                string assemblyName = GetAssemblyName();
                if ((assemblyName == null) || (assemblyName.Length == 0))
                {
                    assemblyName = AppDomain.CurrentDomain.FriendlyName;
                }
                StringBuilder builder = new StringBuilder(assemblyName);
                for (int i = 0; i < builder.Length; i++)
                {
                    switch (builder[i])
                    {
                        case '/':
                        case '\\':
                        case '#':
                            builder[i] = '_';
                            break;
                        case '(':
                            builder[i] = '[';
                            break;

                        case ')':
                            builder[i] = ']';
                            break;
                    }
                }

                return string.Format(CultureInfo.CurrentCulture,
                         "{0}_p{1}",
                         builder.ToString().ToLower(), //<== dont miss ToLower()
                         Process.GetCurrentProcess().Id);
            }

            private static string GetAssemblyName()
            {
                string str = null;
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    AssemblyName name = entryAssembly.GetName();
                    if (name != null)
                    {
                        str = name.Name;
                    }
                }
                return str;
            }

            private long GetPerformanceCounter(string categoryName, string counterName)
            {
                var counter = new PerformanceCounter();
                counter.CategoryName = categoryName;
                counter.CounterName = counterName;
                counter.InstanceName = InstanceName;
                counter.ReadOnly = true;
                return counter.NextSample().RawValue;
            }

            public string DisplayDelta(MetricsSnapshot other, bool round = true, bool showServerRpcMetrics = true,
                bool showClientRpcMetrics = true, bool ignoreClienteRpcMetricsCalls = false)
            {
                return other.Timetamp > Timetamp
                    ? DisplayDelta(other, this, round, showServerRpcMetrics, showClientRpcMetrics, ignoreClienteRpcMetricsCalls)
                    : DisplayDelta(this, other, round, showServerRpcMetrics, showClientRpcMetrics, ignoreClienteRpcMetricsCalls);
            }

            private static string DisplayDelta(MetricsSnapshot newer, MetricsSnapshot older, bool round,
                bool showServerRpcMetrics, bool showClientRpcMetrics, bool ignoreClienteRpcMetricsCalls)
            {
                var builder = new StringBuilder();
                IList<AFRpcMetric> afClientDiff = new List<AFRpcMetric>();
                IList<AFRpcMetric> afServerDiff = new List<AFRpcMetric>();
                IList<AFRpcMetric> piClientDiff = new List<AFRpcMetric>();

                if (showClientRpcMetrics && newer.DataArchiveRpcMetrics != null)
                {
                    piClientDiff = AFRpcMetric.SubtractList(newer.DataArchiveRpcMetrics, older.DataArchiveRpcMetrics);

                    //var log1 = newer.DataArchiveRpcMetrics.Select(x => new { x.Name, x.Count, x.Milliseconds, x.MillisecondsPerCall }).ToList();
                    //var log2 = older.DataArchiveRpcMetrics.Select(x => new { x.Name, x.Count, x.Milliseconds, x.MillisecondsPerCall }).ToList();
                    //var log3 = piClientDiff.Select(x => new { x.Name, x.Count, x.Milliseconds, x.MillisecondsPerCall }).ToList();

                    builder.AppendLine(DisplayRpcDelta(piClientDiff, "PI Client"));
                    builder.AppendLine();
                }

                if (showClientRpcMetrics && newer.ClientRpcMetrics != null)
                {
                    afClientDiff = AFRpcMetric.SubtractList(newer.ClientRpcMetrics, older.ClientRpcMetrics);

                    //var log1 = newer.ClientRpcMetrics.Select(x => new { x.Name, x.Count, x.Milliseconds, x.MillisecondsPerCall }).ToList();
                    //var log2 = older.ClientRpcMetrics.Select(x => new { x.Name, x.Count, x.Milliseconds, x.MillisecondsPerCall }).ToList();
                    //var log3 = afClientDiff.Select(x => new { x.Name, x.Count, x.Milliseconds, x.MillisecondsPerCall }).ToList();

                    if (ignoreClienteRpcMetricsCalls)
                    {
                        afClientDiff = afClientDiff.Where(x => !x.Name.Equals("GetRpcMetrics")).ToList();
                    }

                    builder.AppendLine(DisplayRpcDelta(afClientDiff, "AF Client"));
                    builder.AppendLine();
                }

                if (showServerRpcMetrics && newer.ServerRpcMetrics != null)
                {
                    afServerDiff = AFRpcMetric.SubtractList(newer.ServerRpcMetrics, older.ServerRpcMetrics);

                    //var log1 = newer.ServerRpcMetrics.Select(x => new { x.Name, x.Count, x.Milliseconds, x.MillisecondsPerCall }).ToList();
                    //var log2 = older.ServerRpcMetrics.Select(x => new { x.Name, x.Count, x.Milliseconds, x.MillisecondsPerCall }).ToList();
                    //var log3 = afServerDiff.Select(x => new { x.Name, x.Count, x.Milliseconds, x.MillisecondsPerCall }).ToList();

                    var clientList = afClientDiff.Select(x => x.Name).ToList();

                    //var log4 = afServerDiff.Where(x => !clientList.Contains(x.Name)).Select(x => x.Name).ToList();
                    //var log5 = afServerDiff.Where(x => clientList.Contains(x.Name)).Select(x => x.Name).ToList();

                    IList<AFRpcMetric> applicationList = afServerDiff.Where(x => clientList.Contains(x.Name)).ToList();

                    builder.AppendLine(DisplayRpcDelta(applicationList, "AF Server (App)"));
                    builder.AppendLine();

                    IList<AFRpcMetric> internalList = afServerDiff.Where(x => !clientList.Contains(x.Name)).ToList();

                    builder.AppendLine(DisplayRpcDelta(internalList, "AF Server (Int)"));
                    builder.AppendLine();
                }

                //A couple of notes regardless of the round condition:
                //   1. Working and Peak Working Memory should not be a delta.
                //   2. The very last thing we write (elapsed time) should use simple Append and not AppendLine so as not to include
                //      a trailing Environment.NewLine sequence.  Leave it to the calling Console.WriteLine(x) to issue the last NewLine.
                if (round)
                {
                    builder.AppendLine(String.Format("Total GC Memory: {0}", BytesToSmartString(newer.TotalMemory - older.TotalMemory)));
                    builder.AppendLine(String.Format("Working Memory : {0}", BytesToSmartString(newer.WorkingMemory)));
                    builder.AppendLine(String.Format("Peak Wrk Memory: {0}", BytesToSmartString(newer.PeakWorkingMemory)));
                    builder.AppendLine(String.Format("Network Sent   : {0}", BytesToSmartString(newer.NetworkBytesSent - older.NetworkBytesSent)));
                    builder.AppendLine(String.Format("Network Receivd: {0}", BytesToSmartString(newer.NetworkBytesRcvd - older.NetworkBytesRcvd)));
                    var elapsed = (newer.Timetamp - older.Timetamp).ToString(@"mm\:ss\.f");
                    builder.Append(String.Format("Elapsed Time   : {0}", elapsed));
                }
                else
                {
                    builder.AppendLine(String.Format("Total GC Memory: {0} bytes", (newer.TotalMemory - older.TotalMemory)));
                    builder.AppendLine(String.Format("Working Memory : {0} bytes", newer.WorkingMemory));
                    builder.AppendLine(String.Format("Peak Wrk Memory: {0} bytes", newer.PeakWorkingMemory));
                    builder.AppendLine(String.Format("Network Sent   : {0} bytes", (newer.NetworkBytesSent - older.NetworkBytesSent)));
                    builder.AppendLine(String.Format("Network Receivd: {0} bytes", (newer.NetworkBytesRcvd - older.NetworkBytesRcvd)));
                    builder.Append(String.Format("Elapsed Time   : {0}", (newer.Timetamp - older.Timetamp)));
                }

                return builder.ToString();
            }

            private static string DisplayRpcDelta(IList<AFRpcMetric> deltaMetrics, string heading)
            {
                if (null == deltaMetrics || deltaMetrics.Count == 0)
                    return "";

                var builder = new StringBuilder();

                //Col widths: 25, 8, 12, 12
                // Luckily "PI Client", "AF Client" and "AF Server" are all 9 chars
                var title = heading + " RPC Metrics";
                builder.AppendLine(String.Format("{0,-24}    Count     Duration(ms)  PerCall(ms)", title));
                builder.AppendLine("-------------------------  --------  ------------  ------------");
                long count = 0;
                double ms = 0;
                double perCall = 0;
                foreach (AFRpcMetric item in deltaMetrics.OrderBy(x => x.Name))
                {
                    builder.AppendLine(String.Format("{0,-25} {1,8} {2,12} {3,12}", item.Name, item.Count, item.Milliseconds, item.MillisecondsPerCall));
                    count += item.Count;
                    ms += item.Milliseconds;
                    perCall += (item.Count * item.MillisecondsPerCall);
                }
                builder.AppendLine("-------------------------  --------  ------------  ------------");

                // The very last thing we write should use simple Append and not AppendLine so as not to include
                // a trailing Environment.NewLine sequence.  Leave it to hte calling Console.WriteLine(x) to
                // issue the last NewLine.

                builder.Append(String.Format("Subtotal                   {0,8} {1,12} {2,12}", count, ms, perCall));

                return builder.ToString();
            }

            private const double Kilobyte = 1024;
            private const double Megabyte = Kilobyte * Kilobyte;
            private const double Gigabyte = Kilobyte * Megabyte;
            private static double ToMegabytes(long bytes)
            {
                return bytes / Megabyte;
            }

            public static string BytesToSmartString(long bytes)
            {
                if (bytes < 1024)
                    return String.Format("{0} bytes", bytes);
                double value = bytes;
                var units = new string[] { "KB", "MB", "GB" };
                for (var i = 0; i < units.Length; i++)
                {
                    value /= 1024;
                    if (value < 1024)
                        return String.Format("{0} {1}", value, units[i]);
                }
                return String.Format("{0} TB", value);
            }
        }
    }
}

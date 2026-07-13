using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace StayFlow.Infrastructure.Observability;

/// <summary>Central, low-cardinality metrics emitted by StayFlow infrastructure.</summary>
public sealed class StayFlowMetrics : IDisposable
{
    public const string MeterName = "StayFlow";

    private readonly Meter _meter = new(MeterName);
    private readonly Counter<long> _outboxPublishes;
    private readonly Histogram<double> _jobDuration;
    private readonly Counter<long> _jobOutcomes;
    private readonly Counter<long> _notifications;
    private readonly Counter<long> _businessEvents;
    private long _outboxPending;
    private long _outboxOldestAgeBits;

    public StayFlowMetrics()
    {
        _outboxPublishes = _meter.CreateCounter<long>("stayflow.outbox.publish", unit: "{message}");
        _jobDuration = _meter.CreateHistogram<double>("stayflow.job.duration", unit: "s");
        _jobOutcomes = _meter.CreateCounter<long>("stayflow.job.outcome", unit: "{run}");
        _notifications = _meter.CreateCounter<long>("stayflow.notification", unit: "{notification}");
        _businessEvents = _meter.CreateCounter<long>("stayflow.business.event", unit: "{event}");
        _meter.CreateObservableGauge("stayflow.outbox.pending", () => Interlocked.Read(ref _outboxPending), "{message}");
        _meter.CreateObservableGauge(
            "stayflow.outbox.oldest_age",
            () => BitConverter.Int64BitsToDouble(Interlocked.Read(ref _outboxOldestAgeBits)),
            "s");
    }

    public void ObserveOutbox(long pending, TimeSpan oldestAge)
    {
        Interlocked.Exchange(ref _outboxPending, pending);
        Interlocked.Exchange(ref _outboxOldestAgeBits, BitConverter.DoubleToInt64Bits(Math.Max(0, oldestAge.TotalSeconds)));
    }

    public void RecordOutboxPublish(bool succeeded) =>
        _outboxPublishes.Add(1, new KeyValuePair<string, object?>("outcome", succeeded ? "success" : "failure"));

    public JobMeasurement MeasureJob(string job) => new(this, job);

    public void RecordNotification(string channel, bool succeeded) => _notifications.Add(1,
        new KeyValuePair<string, object?>("channel", channel),
        new KeyValuePair<string, object?>("outcome", succeeded ? "success" : "failure"));

    public void RecordBusinessEvent(string eventName) =>
        _businessEvents.Add(1, new KeyValuePair<string, object?>("event", eventName));

    public void Dispose() => _meter.Dispose();

    public sealed class JobMeasurement : IDisposable
    {
        private readonly StayFlowMetrics _metrics;
        private readonly string _job;
        private readonly long _started = Stopwatch.GetTimestamp();
        private bool _succeeded;

        internal JobMeasurement(StayFlowMetrics metrics, string job)
        {
            _metrics = metrics;
            _job = job;
        }

        public void Succeed() => _succeeded = true;

        public void Dispose()
        {
            var tags = new TagList { { "job", _job } };
            _metrics._jobDuration.Record(Stopwatch.GetElapsedTime(_started).TotalSeconds, tags);
            tags.Add("outcome", _succeeded ? "success" : "failure");
            _metrics._jobOutcomes.Add(1, tags);
        }
    }
}

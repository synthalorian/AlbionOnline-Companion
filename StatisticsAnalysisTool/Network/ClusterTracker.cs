using Serilog;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network;

/// <summary>
/// Tracks the current zone/cluster the player is in.
/// Detects zone changes for auto-reset of tracking stats.
/// </summary>
public class ClusterTracker
{
    private string _currentCluster = string.Empty;
    private string _currentClusterName = string.Empty;
    private DateTime _enteredAt = DateTime.UtcNow;
    private readonly List<ClusterVisit> _history = new();

    public static ClusterTracker Instance { get; } = new();

    private ClusterTracker() { }

    public string CurrentCluster => _currentCluster;
    public string CurrentClusterName => _currentClusterName;
    public DateTime EnteredAt => _enteredAt;
    public TimeSpan TimeInZone => DateTime.UtcNow - _enteredAt;
    public IReadOnlyList<ClusterVisit> History => _history;

    public event EventHandler<ClusterChangedEventArgs>? ClusterChanged;

    /// <summary>
    /// Update the current cluster. Called when zone change is detected.
    /// </summary>
    public void SetCluster(string clusterId, string clusterName = "")
    {
        if (_currentCluster == clusterId) return;

        var previousCluster = _currentCluster;
        var previousDuration = TimeInZone;

        // Record the visit
        if (!string.IsNullOrEmpty(_currentCluster))
        {
            _history.Add(new ClusterVisit
            {
                ClusterId = _currentCluster,
                ClusterName = _currentClusterName,
                EnteredAt = _enteredAt,
                LeftAt = DateTime.UtcNow,
                Duration = previousDuration
            });
        }

        _currentCluster = clusterId;
        _currentClusterName = clusterName;
        _enteredAt = DateTime.UtcNow;

        Log.Information("Zone changed: {Old} → {New} (spent {Duration} in {Old})",
            previousCluster, clusterId, previousDuration, previousCluster);

        ClusterChanged?.Invoke(this, new ClusterChangedEventArgs
        {
            PreviousCluster = previousCluster,
            NewCluster = clusterId,
            NewClusterName = clusterName,
            TimeInPrevious = previousDuration
        });
    }

    /// <summary>
    /// Get the display name for the current zone.
    /// </summary>
    public string GetDisplayName()
    {
        if (!string.IsNullOrEmpty(_currentClusterName))
            return _currentClusterName;
        if (!string.IsNullOrEmpty(_currentCluster))
            return _currentCluster;
        return "Unknown";
    }

    /// <summary>
    /// Clear history (on session reset).
    /// </summary>
    public void ClearHistory()
    {
        _history.Clear();
    }
}

public class ClusterVisit
{
    public string ClusterId { get; set; } = string.Empty;
    public string ClusterName { get; set; } = string.Empty;
    public DateTime EnteredAt { get; set; }
    public DateTime LeftAt { get; set; }
    public TimeSpan Duration { get; set; }
}

public class ClusterChangedEventArgs : EventArgs
{
    public string PreviousCluster { get; set; } = string.Empty;
    public string NewCluster { get; set; } = string.Empty;
    public string NewClusterName { get; set; } = string.Empty;
    public TimeSpan TimeInPrevious { get; set; }
}

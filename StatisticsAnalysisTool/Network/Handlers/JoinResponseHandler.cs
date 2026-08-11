using Serilog;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Handles JoinResponse — the key packet that fires when you enter a zone.
/// Contains username, UserObjectId, map, silver, gold, respec, guild, alliance.
/// This is how we know WHO the local player is.
/// </summary>
public class JoinResponseHandler : ResponsePacketHandler<JoinResponse>
{
    private readonly DashboardViewModel _dashboard;
    private readonly EntityTracker _entityTracker;
    private readonly ClusterTracker _clusterTracker;
    private readonly DashboardEventHandler _dashboardHandler;

    public JoinResponseHandler(
        DashboardViewModel dashboard,
        DashboardEventHandler dashboardHandler)
        : base((int)OperationCodes.Join)
    {
        _dashboard = dashboard;
        _dashboardHandler = dashboardHandler;
        _entityTracker = EntityTracker.Instance;
        _clusterTracker = ClusterTracker.Instance;
    }

    protected override Task OnActionAsync(JoinResponse value)
    {
        try
        {
            Log.Information("JoinResponse: {Username} (ID:{Id}) entered {Map} | Silver:{Silver} Guild:{Guild}",
                value.Username, value.UserObjectId, value.MapIndex,
                value.Silver.DoubleValue, value.GuildName);

            // Set local player in EntityTracker
            _entityTracker.SetLocalPlayer(value.UserObjectId, value.Username);

            // Resolve cluster index ("3003") to display name ("Caerleon")
            var zoneName = Common.ClusterDatabase.Instance.GetName(value.MapIndex);

            // Update cluster/zone
            _clusterTracker.SetCluster(value.MapIndex, zoneName);

            // Update dashboard
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _dashboard.PlayerName = value.Username;
                _dashboard.GuildName = value.GuildName;
                _dashboard.CurrentZone = zoneName;
                _dashboard.TotalSilver = FormatNumber(value.Silver.DoubleValue);

                if (value.ReSpecPoints.InternalValue > 0)
                {
                    _dashboard.TotalReSpecPoints = FormatNumber(value.ReSpecPoints.DoubleValue);
                }
            });

            // Set silver baseline in dashboard handler
            _dashboardHandler.SetSilverBaseline(value.Silver.DoubleValue);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "JoinResponseHandler error");
        }

        return Task.CompletedTask;
    }

    private static string FormatNumber(double value)
    {
        return value switch
        {
            >= 1_000_000_000 => $"{value / 1_000_000_000:F2}B",
            >= 1_000_000 => $"{value / 1_000_000:F1}M",
            >= 1_000 => $"{value / 1_000:F1}K",
            _ => $"{value:F0}"
        };
    }
}

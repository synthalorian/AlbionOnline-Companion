using Serilog;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Handles dungeon-related events and updates the DungeonTrackerViewModel.
/// </summary>
public class DungeonTrackerEventHandler : 
    EventPacketHandler<NewRandomDungeonExitEvent>,
    IPacketHandler
{
    private readonly DungeonTrackerViewModel _viewModel;
    private DateTime _dungeonEntryTime = DateTime.MinValue;
    private bool _isInDungeon;

    public DungeonTrackerEventHandler(DungeonTrackerViewModel viewModel) 
        : base((int)EventCodes.NewRandomDungeonExit)
    {
        _viewModel = viewModel;
    }

    protected override Task OnActionAsync(NewRandomDungeonExitEvent value)
    {
        try
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var dungeon = new DungeonEntry
                {
                    Name = $"Dungeon Exit (Level {value.Level})",
                    Tier = $"T{value.Level + 1}",
                    Type = "Random",
                    Fame = 0,
                    Silver = 0,
                    Duration = TimeSpan.Zero,
                    EnteredAt = DateTime.Now,
                    Status = "Active"
                };

                _viewModel.Dungeons.Insert(0, dungeon);
            });
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "DungeonTrackerEventHandler error");
        }

        return Task.CompletedTask;
    }

    public void OnDungeonEntered()
    {
        _isInDungeon = true;
        _dungeonEntryTime = DateTime.UtcNow;
    }

    public void OnDungeonExited()
    {
        if (!_isInDungeon) return;

        _isInDungeon = false;
        var duration = DateTime.UtcNow - _dungeonEntryTime;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (_viewModel.Dungeons.Count > 0)
            {
                _viewModel.Dungeons[0].Duration = duration;
                _viewModel.Dungeons[0].Status = "Completed";
            }
        });
    }

}

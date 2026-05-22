using Microsoft.AspNetCore.SignalR;

namespace API_PortalSantosTech.Hubs;

public class FinalChallengeHub : Hub
{
    public const string HubRoute = "/hubs/final-challenge";

    public override async Task OnConnectedAsync()
    {
        var eventIdRaw = Context.GetHttpContext()?.Request.Query["eventId"].ToString();
        if (int.TryParse(eventIdRaw, out var eventId))
            await Groups.AddToGroupAsync(Context.ConnectionId, BuildGroupName(eventId));

        await base.OnConnectedAsync();
    }

    public Task SubscribeToEvent(int eventId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, BuildGroupName(eventId));
    }

    public Task UnsubscribeFromEvent(int eventId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildGroupName(eventId));
    }

    public static string BuildGroupName(int eventId)
    {
        return $"final-challenge:{eventId}";
    }
}
using Microsoft.AspNetCore.SignalR;

public class DisplayHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
using Microsoft.AspNetCore.SignalR;

public class CoordinateHub : Hub
{
    public async Task SendCoordinate(string coord)
    {
        await Clients.All.SendAsync("ReceiveCoordinate", coord);
    }
}

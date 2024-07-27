using System;
using System.Timers;
using NetTopologySuite.Geometries;
using Microsoft.AspNetCore.SignalR;

public class ProgressReporter
{
    int callCount = 0;
    private System.Timers.Timer timer;
    private List<Coordinate> linePoints;
    private readonly IHubContext<CoordinateHub> _hubContext; // SignalR Hub Context

    // Event to subscribe to for progress updates
    public event EventHandler<ProgressEventArgs> ProgressChanged;

    public ProgressReporter(int interval, List<Coordinate> lPoints,IHubContext<CoordinateHub> hubContext)
    {
        _hubContext = hubContext;
        foreach(Coordinate c in lPoints)
        {
            System.Console.WriteLine($"There are {lPoints.Count()} points. THis is Pont along line {c}.");
        }
        linePoints = lPoints;
        timer = new System.Timers.Timer(interval);
        timer.Elapsed += OnTimerElapsed;
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        callCount ++;
        Coordinate coordUpdate = linePoints[callCount];
        _hubContext.Clients.All.SendAsync("ReceiveCoordinate", coordUpdate.ToString()); // Send coordinate to clients
        // Raise the progress update event
        ProgressChanged?.Invoke(this, new ProgressEventArgs($"Current coordinate {coordUpdate}"));
    }

    public void Start()
    {
        timer.Start();
    }

    public void Stop()
    {
        timer.Stop();
    }
}

public class ProgressEventArgs : EventArgs
{
    public string Message { get; }

    public ProgressEventArgs(string message)
    {
        Message = message;
    }
}

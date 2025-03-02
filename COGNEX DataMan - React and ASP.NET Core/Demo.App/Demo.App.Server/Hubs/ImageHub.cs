using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using Demo.App.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace Demo.App.Server.Hubs;

public interface IImageHub
{
    Task ReceiveImage(string base64Image);
}

internal class ImageHub : Hub<IImageHub>
{
    [SupportedOSPlatform("windows")]
    public ImageHub(Scanner scanner)
    {
        scanner.SendImageAsync = SendImageAsync;
    }

    [SupportedOSPlatform("windows")]
    private async Task SendImageAsync(Image image)
    {
        using var memoryStream = new MemoryStream();
        image.Save(memoryStream, ImageFormat.Png);
        var imageBytes = memoryStream.ToArray();
        var base64Image = Convert.ToBase64String(imageBytes);
        await Clients.All.ReceiveImage(base64Image);
    }
}

using TaskQueueing.ObjectModel.Models;
using Microsoft.EntityFrameworkCore;

namespace TaskQueueing.ObjectModel
{
    public interface IJobContext
    {
        DbSet<ChannelSetting> ChannelSettings { get; set; }
        DbSet<Request> Requests { get; set; }
        DbSet<Response> Responses { get; set; }
    }
}

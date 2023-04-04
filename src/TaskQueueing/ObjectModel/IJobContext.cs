using TaskQueueing.ObjectModel.Models;
using Microsoft.EntityFrameworkCore;

namespace TaskQueueing.ObjectModel
{
    public interface IJobContext
    {
        DbSet<Request> Requests { get; set; }
        DbSet<Response> Responses { get; set; }
        DbSet<Publication> Publications { get; set; }
    }
}

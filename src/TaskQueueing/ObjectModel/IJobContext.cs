using TaskQueueing.ObjectModel.Models;
using Microsoft.EntityFrameworkCore;

namespace TaskQueueing.ObjectModel
{
    public interface IJobContext
    {
        DbSet<Request> Requests { get; set; }

        //DatabaseFacade Database { get; }

        //void SetState( object entity, EntityState state );
        //void SetValues( object entity, object values );
    }
}

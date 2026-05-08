using Microsoft.EntityFrameworkCore;

namespace SCICHRPortal.Repository.Implementations
{
    
    public class TimelogContextRepository
    {
        protected TimekeepingContext TimeLogContext { get; }
        public TimelogContextRepository(TimekeepingContext timeLogContext)
        {
            TimeLogContext = timeLogContext;
        }
    }
}

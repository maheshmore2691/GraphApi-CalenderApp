
namespace Scheduler.WebClient.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Graph;

    public interface IGraphApiClient
    {
        Task<ICalendarEventsCollectionPage> GetCalenderEventAsync(string userName);
        Task<Event> CreateCalenderEventAsync(string emailAddress, DateTime appointmentDateTime);
    }
}

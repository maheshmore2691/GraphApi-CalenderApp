
namespace Scheduler.WebClient.Models
{
    using Microsoft.Graph;

    public class EventViewModel
    {
        public string Subject { get; set; }
        public string EventBody { get; set; }
        public string Organizer { get; set; }
        public string Participants { get; set; }
        public DateTimeTimeZone Start { get; set; }
        public string StartDateTime { get; set; }
        public string StartDate { get; set; }
        public string StartTime { get; set; }
        public DateTimeTimeZone End { get; set; }
        public string EndDateTime { get; set; }
        public string EndDate { get; set; }
        public string EndTime { get; set; }
    }
}

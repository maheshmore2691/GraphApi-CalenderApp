
namespace Scheduler.WebClient.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Graph.Extensions;
    using Scheduler.WebClient.Interfaces;
    using Scheduler.WebClient.Models;

    public class EventsController : Controller
    {
        private readonly IGraphApiClient _graphApiClient;

        public EventsController(IGraphApiClient graphApiClient)
        {
            _graphApiClient = graphApiClient ?? throw new ArgumentNullException(nameof(graphApiClient));
        }

        public IActionResult MeetingEvents()
        {
            return View();
        }
             
        [HttpGet]
        [ActionName("getevents")]
        public async Task<IActionResult> GetEvents(string userName)
        {
            if(string.IsNullOrWhiteSpace(userName))
            {
                return BadRequest();
            }

            var meetingEvents = await _graphApiClient.GetCalenderEventAsync(userName).ConfigureAwait(false);

            if (meetingEvents != null && meetingEvents.Count > 0)
            {
                List<EventViewModel> events = new List<EventViewModel>();
                foreach (var meetingEvent in meetingEvents)
                {
                    var participants = string.Empty;

                    foreach(var participant in meetingEvent.Attendees)
                    {
                        participants += $"{participant.EmailAddress.Address}{Environment.NewLine}";
                    }

                    events.Add(new EventViewModel
                    {
                        Subject = meetingEvent.Subject,
                        EventBody = meetingEvent.BodyPreview,
                        Organizer = meetingEvent.Organizer.EmailAddress.Address,
                        Participants = participants,
                        
                        Start = meetingEvent.Start,
                        StartDateTime = meetingEvent.Start.ToDateTime().ToLocalTime().ToString(),
                        StartDate = meetingEvent.Start.ToDateTime().ToLocalTime().Date.ToShortDateString(),
                        StartTime = meetingEvent.Start.ToDateTime().ToLocalTime().TimeOfDay.ToString(),

                        End = meetingEvent.End,
                        EndDateTime = meetingEvent.End.ToDateTime().ToLocalTime().ToString(),
                        EndDate = meetingEvent.End.ToDateTime().ToLocalTime().Date.ToShortDateString(),
                        EndTime = meetingEvent.End.ToDateTime().ToLocalTime().TimeOfDay.ToString()
                    });
                }

                return Ok(events);
            }
            else
            {
                return NotFound();
            }
        }             

        public IActionResult CreateEvents()
        {
            return View();
        }

        [HttpPost]
        [ActionName("createmeetingevent")]
        public async Task<IActionResult> CreateMeetingEvents(string emailAddress, string appointmentDateTime, List<string> participants)
        {
            if(string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(appointmentDateTime) || !appointmentDateTime.Any())
            {
                return BadRequest();
            }

            var meetingEvent = await _graphApiClient.CreateCalenderEventAsync(emailAddress, 
                Convert.ToDateTime(appointmentDateTime, CultureInfo.CurrentCulture), participants).ConfigureAwait(false);

            if(meetingEvent != null)
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }

        public IActionResult Calendar()
        {
            return View();
        }

        [HttpGet]
        [ActionName("getcalendarevents")]
        public async Task<IActionResult> GetCalendarEvents(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return NotFound();
            }

            var result = await GetEvents(userName).ConfigureAwait(false);

            if (result != null)
            {
                var meetings = (result as OkObjectResult).Value as List<EventViewModel>;
                var calendarEvents = new List<CalendarEvent>();

                foreach (var item in meetings)
                {
                    calendarEvents.Add(new CalendarEvent
                    {
                        title = item.Subject,
                        start = item.Start.ToDateTime().ToLocalTime().ToString("O"),
                        end = item.End.ToDateTime().ToLocalTime().ToString("O")
                    });
                }

                return Ok(calendarEvents);
            }
            else
            {
                return NotFound();
            }
        }
    }
}

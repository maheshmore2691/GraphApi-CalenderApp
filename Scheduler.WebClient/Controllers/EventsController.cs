
namespace Scheduler.WebClient.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
                        EventBody = meetingEvent.BodyPreview,
                        Organizer = meetingEvent.Organizer.EmailAddress.Address,
                        Participants = participants,
                        StartDateTime = meetingEvent.Start.ToDateTime().ToString(),
                        StartDate = meetingEvent.Start.ToDateTime().Date.ToShortDateString(),
                        StartTime = meetingEvent.Start.ToDateTime().TimeOfDay.ToString(),
                        EndDateTime = meetingEvent.End.ToDateTime().ToString(),
                        EndDate = meetingEvent.End.ToDateTime().Date.ToShortDateString(),
                        EndTime = meetingEvent.End.ToDateTime().TimeOfDay.ToString()
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
        public async Task<IActionResult> CreateMeetingEvents(string emailAddress, string appointmentDateTime)
        {
            if(string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(appointmentDateTime))
            {
                return BadRequest();
            }

            var meetingEvent = await _graphApiClient.CreateCalenderEventAsync(emailAddress, 
                Convert.ToDateTime(appointmentDateTime, CultureInfo.CurrentCulture)).ConfigureAwait(false);

            if(meetingEvent != null)
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }

        public IActionResult EmailEvent()
        {
            return View();
        }

        [HttpPost]
        [ActionName("createappointmentconfirmationemailevent")]
        public async Task<IActionResult> CreateAppointmentConfirmationEmailEvent(string emailAddress, string appointmentDateTime)
        {
            emailAddress = "ankit.dhadse@emtecinc.com";
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(appointmentDateTime))
            {
                return BadRequest();
            }

            var meetingEvent = await _graphApiClient.SendAppointmentConfirmationEmail(emailAddress,
                Convert.ToDateTime(appointmentDateTime, CultureInfo.CurrentCulture)).ConfigureAwait(false);

            return meetingEvent ? Ok() : StatusCode(500);
        }
    }
}

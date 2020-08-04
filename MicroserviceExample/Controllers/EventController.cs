using System;
using System.Net;
using System.Threading.Tasks;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Query;
using MicroserviceExample.Models;
using Microsoft.AspNetCore.Mvc;

namespace MicroserviceExample.Controllers
{
    public class EventController : Controller
    {
        private readonly IBucketProvider _bucketProvider;

        public EventController(IBucketProvider bucketProvider)
        {
            _bucketProvider = bucketProvider;
        }

        [HttpGet]
        [Route("/hostname")]
        public IActionResult GetHostName()
        {
            return Ok(Dns.GetHostName());
        }

        [HttpPost]
        [Route("/event/add/{userId}")]
        public async Task<IActionResult> AddEventToUser(string userId, UserEventPost evt)
        {
            var bucket = await _bucketProvider.GetBucketAsync("useractivity");
            var coll = bucket.DefaultCollection();
            
            var activityId = $"activity::{Guid.NewGuid()}";
            await coll.InsertAsync(activityId, new
            {
                userId,
                evt.Description,
                evt.EventType,
                EventDt = DateTime.Now
            });

            return Ok();
        }

        [HttpGet]
        [Route("/activity/get/{userId}")]
        public async Task<IActionResult> GetEventsForUser(string userId)
        {
            // CREATE INDEX ix_userid on useractivity (userId);
            var bucket = await _bucketProvider.GetBucketAsync("useractivity");
            var cluster = bucket.Cluster;
            var events = await cluster.QueryAsync<UserEventGet>(
                @"SELECT description, eventType, eventDt
                            FROM useractivity
                            WHERE userId = $userId
                            ORDER BY eventDt DESC",
                QueryOptions.Create().Parameter("$userId", userId));
            return Ok(events.Rows);
        }
    }
}

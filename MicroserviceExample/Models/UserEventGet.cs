using System;

namespace MicroserviceExample.Models
{
    public class UserEventGet
    {
        public string Description { get; set; }
        public string EventType { get; set; }
        public DateTime EventDt { get; set; }
    }
}
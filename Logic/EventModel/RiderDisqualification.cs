﻿﻿using System;

 namespace maxbl4.Race.Logic.EventModel
{
    public class RiderDisqualification
    {
        public Guid RiderDisqualificationId { get; set; }
        public Guid RiderRegistrationId { get; set; }
        public Guid? ScheduleId { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? EventId { get; set; }
    }
}
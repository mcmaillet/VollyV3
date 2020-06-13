﻿using System;

namespace VollyV3.Data
{
    public class Organization
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactEmail { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string WebsiteLink { get; set; }
        public string MissionStatement { get; set; }
        public string FullDescription { get; set; }
        public virtual Cause Cause { get; set; }
        public virtual Location Location { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public bool Enabled { get; set; }
    }
}

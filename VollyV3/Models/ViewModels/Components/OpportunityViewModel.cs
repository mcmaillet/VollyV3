﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VollyV3.Data;

namespace VollyV3.Models.ViewModels.Components
{
    public class OpportunityViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationLink { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string ImageUrl { get; set; }
        public string ExternalSignUpUrl { get; set; }
        public OpportunityType OpportunityType { get; set; }
        public string ContactEmail { get; set; }
        public List<OccurrenceViewModel> OccurrenceViews { get; set; }

        public static OpportunityViewModel FromOpportunity(Opportunity opportunity)
        {
            return new OpportunityViewModel()
            {
                Id = opportunity.Id,
                Name = opportunity.Name,
                Description = opportunity.Description,
                Address = opportunity.Address,
                OrganizationName = opportunity.Organization.Name,
                OrganizationLink = opportunity.Organization.WebsiteLink,
                Latitude = opportunity.Location?.Latitude,
                Longitude = opportunity.Location?.Longitude,
                ImageUrl = opportunity.ImageUrl,
                ExternalSignUpUrl = opportunity.ExternalSignUpUrl,
                OpportunityType = opportunity.OpportunityType,
                OccurrenceViews = opportunity.Occurrences
                .Where(x =>
                (x.ApplicationDeadline == DateTime.MinValue || x.ApplicationDeadline > DateTime.Now)
                && (x.Openings == 0 || x.Openings > x.Applications.Count))
                .OrderBy(x => x.StartTime)
                .Select(OccurrenceViewModel.FromOccurrence)
                .ToList(),
                ContactEmail = opportunity.ContactEmail
            };
        }
    }
}

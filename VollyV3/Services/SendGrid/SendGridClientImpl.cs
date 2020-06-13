﻿using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VollyV3.Models;
using VollyV3.Services.SendGrid.Results;

namespace VollyV3.Services.SendGrid
{
    public class SendGridClientImpl
    {
        private readonly HttpClient _client;
        private readonly IMemoryCache _memoryCache;

        public SendGridClientImpl(
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache
            )
        {
            _client = httpClientFactory.CreateClient("sendgrid");
            _memoryCache = memoryCache;
        }
        public async Task<Response> SendNewsletterAsync(
           string opportunityHtml)
        {
            var apikey = Environment.GetEnvironmentVariable("sendgrid_api_key");
            var infoFromEmail = Environment.GetEnvironmentVariable("info_from_email");
            var infoFromName = Environment.GetEnvironmentVariable("info_from_name");

            var client = new SendGridClient(apikey);
            var templateData = new TemplateData
            {
                OpportunityHtml = opportunityHtml,
            };
            var msg = new SendGridMessage();
            msg.SetFrom(new EmailAddress(infoFromEmail, infoFromName));
            msg.SetTemplateId(Environment.GetEnvironmentVariable("newsletter_template_id"));
            msg.Personalizations = new List<Personalization>()
            {
                new Personalization()
                {
                    Tos=new List<EmailAddress>(){
                        new EmailAddress("maillet.mark@gmail.com","Mark")
                    },
                    TemplateData=templateData
                },
                new Personalization()
                {
                    Tos=new List<EmailAddress>(){
                        new EmailAddress("maillet.mark.test@gmail.com","Mark Maillet")
                    },
                    TemplateData=templateData
                }
            };
            
            msg.SetAsm(int.Parse(Environment.GetEnvironmentVariable("volly_newsletter_unsubscribe_id")));

            return await client.SendEmailAsync(msg);
        }
        private class TemplateData
        {
            [JsonProperty("opportunity_html")]
            public string OpportunityHtml { get; set; }
        }
        public async Task<HttpResponseMessage> AddUserToNewsletterAsync(VollyV3User user)
        {
            return await AddUsersToSegmentsAsync(
                new List<VollyV3User>() { user },
                new List<string>() { Environment.GetEnvironmentVariable("sendgrid_newsletter_custom_field") });
        }
        public async Task<HttpResponseMessage> AddUserToSegmentAsync(VollyV3User user, string segment)
        {
            return await AddUsersToSegmentsAsync(
                new List<VollyV3User>() { user },
                new List<string>() { segment });
        }
        public async Task<HttpResponseMessage> AddUsersToSegmentsAsync(IEnumerable<VollyV3User> users, List<string> segments)
        {
            var allCustomFields = await GetAllCustomFields();

            var customFields = new Dictionary<string, string>();

            foreach (CustomFields.Result result in allCustomFields.custom_fields)
            {
                if (segments.Contains(result.name))
                {
                    customFields.Add(result.id, "Yes");
                }
            }

            var parameters = new
            {
                contacts = users.Select(user => new
                {
                    email = user.Email,
                    custom_fields = customFields
                }).ToList()
            };

            var json = JsonConvert.SerializeObject(parameters);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, SendGridEndpoints.MarketingContacts)
            {
                Content = new StringContent(json)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await _client.SendAsync(request);
        }

        public async Task<CustomFields> GetAllCustomFields()
        {
            return JsonConvert.DeserializeObject<CustomFields>(
                await SendGridCache.GetAllCustomFieldsJson(_memoryCache, _client)
                );
        }
    }
}

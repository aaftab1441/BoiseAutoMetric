using BA.BoiseAutoMetric.DataModels;
using BA.BoiseAutoMetric.Configuration;
using BA.Common.Models;
using BA.Common.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;

namespace BA.BoiseAutoMetric.Adapters
{
    public class ContactUsAdapter
    {
        private readonly IConfiguration _config;
        private readonly EmailConfigurationModel _emailConfiguration;
        private readonly EmailTemplateModel _emailTemplate;
        private readonly ISmtpService _smtpService;
        private readonly List<EmailAttachmentModel> _attachments;

        public EmailServerConfigModel ServerConfig { get; private set; }
        public EmailMessageModel Message { get; private set; }

        public ContactUsAdapter(IConfiguration config, ISmtpService smtpService)
        {
            _config = config;
            _emailConfiguration = _config.GetSection("EmailConfiguration").Get<EmailConfigurationModel>();
            _emailTemplate = _config.GetSection("EmailTemplates:ContactUs").Get<EmailTemplateModel>();
            _smtpService = smtpService;
            _attachments = new List<EmailAttachmentModel>();
        }

        public async Task<bool> CreateAndSendEmail(ContactUsModel contactUs)
        {

            try
            {
                //Get Template
                string body = GetEmailTemplate();

                //Get Subject from Body
                string messageSubject = GetEmailSubject(body);

                //Replace Tokens
                string messageBody = ReplaceTokens(body, contactUs);

                //MAP ContactUsAdapter to SMTPService
                EmailAddressModel fromAddress = new EmailAddressModel(contactUs.Ndkvufdnkfj, contactUs.Kfujbmrlfjnt);
                List<EmailAddressModel> toAddresses = new List<EmailAddressModel>()
                {
                    new EmailAddressModel(_emailConfiguration.ToName, _emailConfiguration.DefaultToAddress)
                };

                List<EmailAddressModel> ccAddresses = new List<EmailAddressModel>()
                {
                    new EmailAddressModel("Support", "support@shawnwbailey.com")
                };

                Message = new EmailMessageModel(fromAddress, toAddresses, ccAddresses, messageSubject, messageBody, _emailTemplate.TemplateType);
                ServerConfig = new EmailServerConfigModel(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, _emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

               if (contactUs.Attachments != null) {
                   foreach (IFormFile formFile in contactUs.Attachments)
                   {
                        if (formFile.Length > 0)
                        {
                            var cd = ContentDispositionHeaderValue.Parse(formFile.ContentDisposition);
                            string fileName = Path.GetFileName(cd.FileName).Trim('"');

                            Stream stream = formFile.OpenReadStream();
                            _attachments.Add(new EmailAttachmentModel(fileName, stream));
                        }
                   }
               }
               return ( await SendMailAsync());

            }
            catch(Exception e)
            {
                throw new Exception("ContactUsAdapter.Create", e);
            }
        }

        private MemoryStream StreamToMemory(FileStream fileStream)
        {
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.SetLength(fileStream.Length);
            fileStream.Read(memoryStream.GetBuffer(), 0, (int)fileStream.Length);


            memoryStream.Flush();
            fileStream.Close();
            memoryStream.Close();
            return memoryStream;


        }

        private string GetEmailTemplate()
        {
            string body = null;
            FileStream file = new FileStream(string.Format(@"Templates\\{0}", _emailTemplate.TemplateName), FileMode.Open, FileAccess.Read);
            using(StreamReader reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }
            return (body);
        }

        private string GetEmailSubject(string body)
        {
            Match match = Regex.Match(body, @"<title>\s*(.+?)\s*</title>*");
            if(match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return string.Empty;
            }
        }

        private string ReplaceTokens(string body, ContactUsModel contact)
        {
            string _body = body.Replace(@"[Name]", contact.Ndkvufdnkfj)
                .Replace(@"[Email]", contact.Kfujbmrlfjnt)
                .Replace(@"[Phone]", contact.jfielxicng)
                .Replace(@"[Referral]", contact.Jmlvnjrmdx)
                .Replace(@"[Comments]", contact.Idmvpmsdhbv)
                .Replace(@"[Page]", contact.Lkwpvjfrslv);

            return _body;
        }

        private async Task<bool> SendMailAsync()
        {
            try
            {
                return(await _smtpService.SendMailAsync(Message, ServerConfig, _attachments));
            }
            catch(Exception e)
            {
                throw new Exception("ContactUsAdapter failed to send email", e);
            }
        }
    }
}

using System.Threading.Tasks;
using BA.BoiseAutoMetric.DataModels;
using BA.BoiseAutoMetric.Adapters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using BA.Common.Services;
using reCAPTCHA.AspNetCore;
using reCAPTCHA.AspNetCore.Attributes;
using System;
using System.Diagnostics;
using RestSharp;


namespace BA.BoiseAutoMetric.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ISmtpService _smtpService;
        private readonly IRecaptchaService _recaptcha;

        [BindProperty]
        public ContactUsModel contactUsModel { get; set; }
        public CommentCheckModel commentCheckModel = new CommentCheckModel();

        public IndexModel(IConfiguration config, ISmtpService smtpService, IRecaptchaService recaptcha)
        {
            _config = config;
            _smtpService = smtpService;
            _recaptcha = recaptcha;
        }

        public void OnGet() { }
        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    commentCheckModel.blog = _config.GetSection("WebAppUrl").Value;
                    commentCheckModel.comment_type = "contact-form";
                    commentCheckModel.user_ip = HttpContext.Connection.RemoteIpAddress.ToString();
                    commentCheckModel.user_agent = HttpContext.Request.Headers["User-Agent"].ToString();
                    commentCheckModel.comment_author = contactUsModel.Ndkvufdnkfj;
                    commentCheckModel.comment_author_email = contactUsModel.Kfujbmrlfjnt;
                    commentCheckModel.comment_content = contactUsModel.Idmvpmsdhbv;
                    commentCheckModel.is_test = true;
                    string apiUrl = "https://" + _config.GetSection("AkismetApiKey").Value + ".rest.akismet.com/1.1/comment-check";
                    var client = new RestClient(apiUrl);
                    var request = new RestRequest(Method.POST);
                    request.RequestFormat = DataFormat.Json;
                    request.AddObject(commentCheckModel);
                    IRestResponse response = await client.ExecuteAsync(request);
                    if (response.Content == "true")
                    {
                        return RedirectToPage("/ThankYou");
                    }
                    contactUsModel.Lkwpvjfrslv = "Contact Us";
                    RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);
                    if (recaptcha.score > 0.5)
                    {
                        ContactUsAdapter contactUs = new ContactUsAdapter(_config, _smtpService);
                        await contactUs.CreateAndSendEmail(contactUsModel);
                        return RedirectToPage("/ThankYou");
                    }
                    else
                    {
                        ModelState.AddModelError("Recaptcha", "There was an error validating the Recaptcha code.  Please try Again!");
                        return Page();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return Page();
                }


            }
            else
            {
                return Page();
            }
        }
    }
}

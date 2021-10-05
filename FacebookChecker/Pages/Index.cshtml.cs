using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;
using System.Net;
using System.IO;
using System.Text;

namespace FacebookChecker.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public string StatusMsg { get; private set; }
        public string StatusDescription { get; private set; }
        public int cfStatus { get; private set; }
        public int googStatus { get; private set; }
        public int ciscoStatus { get; private set; }
        public int httpStatus { get; private set; }
        public int msgrStatus { get; private set; }


        public void OnGet()
        {
            
            
            var cfDNS = new IPEndPoint(IPAddress.Parse("1.1.1.1"), 53);
            var googDNS = new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53);
            var ciscoDNS = new IPEndPoint(IPAddress.Parse("208.67.220.220"), 53);
            
            var cfClient = new LookupClient(cfDNS);
            var googClient = new LookupClient(googDNS);
            var ciscoClient = new LookupClient(ciscoDNS);

            try
            {
                var tmpcfStatus = cfClient.Query("www.facebook.com", QueryType.A);
                if (tmpcfStatus.HasError.Equals(true))
                {
                    cfStatus = 0;
                }
                else
                {
                    cfStatus = 11;
                }

                var tmpgoogStatus = googClient.Query("www.facebook.com", QueryType.A);
                if (tmpgoogStatus.HasError.Equals(true))
                {
                    googStatus = 0;
                }
                else
                {
                    googStatus = 12;
                }

                var tmpciscoStatus = ciscoClient.Query("www.facebook.com", QueryType.A);
                if (tmpciscoStatus.HasError.Equals(true))
                {
                    ciscoStatus = 0;
                }
                else
                {
                    ciscoStatus = 13;
                }
            }
            catch (Exception) { }


            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.facebook.com/");
                request.UserAgent = "curl/7.54.0";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string line = string.Empty;
                request.Timeout = 1000;

                Stream streamReceive = response.GetResponseStream();
                Encoding encoding = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader streamRead = new System.IO.StreamReader(streamReceive, encoding);

                while (streamRead.EndOfStream != true)
                {
                    line = streamRead.ReadLine();
                    if (line.Contains("<!DOCTYPE html>").Equals(true))
                    {
                        httpStatus = 0;
                    }
                    if (line.Contains("Sorry, something went wrong.").Equals(true))
                    {
                        httpStatus = 0;
                        break;
                    }
                    else
                    {
                        httpStatus = Convert.ToInt32(response.StatusCode);
                    }
                    response.Close();
                }
            }
            catch (Exception) { }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.messenger.com/");
                request.UserAgent = "curl/7.54.0";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string line = string.Empty;
                request.Timeout = 1000;

                Stream streamReceive = response.GetResponseStream();
                Encoding encoding = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader streamRead = new System.IO.StreamReader(streamReceive, encoding);

                while (streamRead.EndOfStream != true)
                {
                    
                    line = streamRead.ReadLine();
                    if (line.Contains("<!DOCTYPE html>").Equals(true))
                    {
                        msgrStatus = 0;
                    }
                    if (line.Contains("Sorry, something went wrong.").Equals(true))
                    {
                        msgrStatus = 0;
                        break;
                    }
                    else
                    {
                        msgrStatus = Convert.ToInt32(response.StatusCode);
                    }
                    response.Close();
                }
            }
            catch (Exception) { }


            int overallStatus = cfStatus + googStatus + ciscoStatus;

            if(overallStatus == 0)
            {
                StatusMsg = "Facebook Appears To Be Down";
                StatusDescription = "We have queried the DNS servers of CloudFlare, Google, and OpenDNS. All three report errors contacting Facebook.";
                return;
            }
            if(overallStatus == 36)
            {
                if (httpStatus != 200 || msgrStatus != 200) {
                    StatusMsg = "Facebook Appears To Be Coming Back";
                    StatusDescription = "While DNS is resolving correctly, we seem to be having trouble getting a response from Facebook itself. Stay Tuned.";
                    return;
                }
                else
                {
                    StatusMsg = "Facebook Appears To Be Up";
                    StatusDescription = "Facebook appears to working! Their DNS and website are responding!";
                    return;
                }
            }
            if(overallStatus > 1 || overallStatus < 36)
            {
                StatusMsg = "Facebook May Be Available Soon";
                StatusDescription = "We have queried the DNS servers of CloudFlare, Google, and OpenDNS. We are getting varied responses, this may be a good sign though. Stay Tuned.";
                return;

            }
            else
            {
                StatusMsg = "Huh.";
                StatusDescription = "Well, something is borked with this website. Try again later.";
                return;
            }

        }
    }
}

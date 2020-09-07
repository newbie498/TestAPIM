using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerTest
{
    public class CookieAuthenticationSetting
    {
        public string LoginPath { get; set; }
        public string LogoutPath { get; set; }
        public string CookieName { get; set; }
        public double ExpireTimeSpan { get; set; }
        public bool SlidingExpiration { get; set; }
        public double CookieExpiration { get; set; }
        public bool SessionStore { get; set; }
        public double WarningMinutes { get; set; } = 5;
    }
}

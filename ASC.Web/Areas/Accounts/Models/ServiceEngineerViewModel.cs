﻿using Microsoft.AspNetCore.Identity;

namespace ASC.Web.Areas.Accounts.Models
{
    public class ServiceEngineerViewModel
    {
        public List<IdentityUser>?ServiceEngineers { get; set; }
        public ServiceEngineerRegistrationViewModelcs Registration { get; set; }
    }
}

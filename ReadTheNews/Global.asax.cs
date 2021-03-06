﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using EFlogger.EntityFramework6;

namespace ReadTheNews
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //EFloggerFor6.Initialize();
            //EFloggerFor6.EnableDecompiling();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}

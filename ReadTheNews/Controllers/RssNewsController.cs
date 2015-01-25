using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReadTheNews.Controllers
{
    public class RssNewsController : Controller
    {

        public ActionResult AddNewFavoriteRssNews(int? newsId, string userId)
        {
            return View();
        }
    }
}
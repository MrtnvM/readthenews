using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReadTheNews.Models;
using System.Threading.Tasks;

namespace ReadTheNews.Controllers
{
    public class HomeController : Controller
    {
        private RssContext db = new RssContext();

        public ActionResult Index()
        {
            ViewBag.RssChannels = db.RssChannels.ToList();
            return View();
        }

        public ActionResult GetNews(int id)
        {
            RssChannel channel;
            RssProcessor processor;
            try {
                processor = RssProcessor.GetRssProcessor(id);
                channel = processor.GetLatestNews();
            }
            catch(Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Error");
            }
            return View(channel);
        }

        [HttpPost]
        public ActionResult GetNews(string url)
        {
            RssProcessor processor = RssProcessor.GetRssProcessor(url);
            if (!processor.IsChannelDownload)
                return RedirectToAction("Error");

            RssChannel channel = processor.GetLatestNews();

            return View(channel);
        }

        public ActionResult Error()
        {
            return View("ErrorMessage");
        }
    }
}
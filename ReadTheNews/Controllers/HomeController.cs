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
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            ViewBag.RssChannels = db.RssChannels.ToList();
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

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
            //channel.RssItems = db.RssItems.Where(item => item.RssChannelId == channel.Id).ToList();
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
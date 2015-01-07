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
            RssChannel channel = db.RssChannels.AsQueryable().First(c => c.Id == id);
            channel.RssItems = channel.RssItems.Take(10).ToList();
            return View(channel);
        }

        [HttpPost]
        public async Task<ActionResult> GetNews(string url)
        {
            RssProcessor processor = await RssProcessor.GetRssProcessorAsync(url);
            if (!processor.IsChannelDownload)
                return Redirect("Error");

            RssChannel channel = await processor.GetRssChannelAsync();

            return View(channel);
        }
    }
}
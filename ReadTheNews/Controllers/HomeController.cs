using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReadTheNews.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace ReadTheNews.Controllers
{
    public class HomeController : Controller
    {
        private RssContext db = new RssContext();
        string userId;

        public ActionResult RssChannels()
        {
            ViewBag.RssChannels = db.RssChannels.ToList();
            return View();
        }

        public ActionResult GetNews(int? id)
        {
            if (id == null)
                return Redirect("RssChannels");

            RssChannel channel;
            RssProcessor processor;
            try {
                processor = RssProcessor.GetRssProcessor(Int32.Parse(id.ToString()));
                this.GetUserId();
                channel = processor.GetLatestNews(userId);
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
            RssProcessor processor;
            try {
                processor = RssProcessor.GetRssProcessor(url);
            } catch (Exception ex) {
                TempData["Error"] = ex.Message;
                return Redirect("Error");
            }
            if (!processor.IsChannelDownload)
                return RedirectToAction("Error");

            this.GetUserId();
            RssChannel channel = processor.GetLatestNews(userId);

            return View(channel);
        }

        public ActionResult Error()
        {
            return View("ErrorMessage");
        }

        private void GetUserId()
        {
            if (String.IsNullOrEmpty(userId))
                userId = User.Identity.GetUserId();
        }
    }
}
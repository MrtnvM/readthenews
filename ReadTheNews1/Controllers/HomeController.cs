using ReadTheNews.Models.RssModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

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

        [HttpPost]
        public async Task<ActionResult> GetNews(string url)
        {
            RssProcessor processor;
            RssChannel channel = new RssChannel();
            try
            {
                processor = await RssProcessor.GetRssProcessorAsync(url);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error");
            }

            channel = await processor.GetRssChannelAsync();

            return PartialView(channel);
        }

        [HttpGet]
        public PartialViewResult GetNews(int id)
        {
            RssChannel channel = db.RssChannels.Find(id);
            return PartialView(channel);
        }

        public ActionResult Error()
        {
            string errorMessage = TempData["ErrorMessage"].ToString();
            return View(errorMessage);
        }
    }
}
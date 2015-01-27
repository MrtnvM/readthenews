using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReadTheNews.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using ReadTheNews.Helpers;

namespace ReadTheNews.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private RssContext db = new RssContext();
        string _userId;

        public ActionResult Index()
        {
            return Redirect("RssChannels");
        }

        [AllowAnonymous]
        public ActionResult RssChannels()
        {
            try
            {
                ViewBag.RssChannels = db.RssChannels.ToList();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToRoute(new { controller = "Home", action = "Error" });
            }
            return View();
        }

        public ActionResult MyFavoriteNews()
        {
            this.GetUserId();

            List<RssItem> favoriteNews;
            using (var dataHelper = new RssDataHelper())
            {
                try
                {
                    favoriteNews = dataHelper.GetFavoriteRssNews(_userId);
                    if (favoriteNews == null)
                        throw new Exception("Не удалось загрузить избрвнные новости. Попробуйте повторить позднее");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                    return Redirect("Error");
                }
                return View(favoriteNews);
            }
        }

        public ActionResult ReadItLater()
        {
            this.GetUserId();

            List<RssItem> readingList;
            using (var dataHelper = new RssDataHelper())
            {
                try
                {
                    readingList = dataHelper.GetReadingList(_userId);
                    if (readingList == null)
                        throw new Exception("Не удалось загрузить список для чтения. Попробуйте повторить позднее");
                }
                catch (Exception ex) 
                {
                    TempData["Error"] = ex.Message;
                    return Redirect("Error");
                }
            }
            return View(readingList);
        }

        public ActionResult Channel(int? id)
        {
            if (id == null)
                return Redirect("RssChannels");

            RssChannel channel;
            RssProcessor processor;
            try {
                processor = RssProcessor.GetRssProcessor(Int32.Parse(id.ToString()));
                this.GetUserId();
                channel = processor.GetLatestNews(_userId);
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
            RssChannel channel = processor.GetLatestNews(_userId);

            return View(channel);
        }

        public ActionResult Error()
        {
            return View("Error");
        }

        public ActionResult AddNewFavoriteRssNews(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Некорректный идентификатор при добавлении новости в избранное";
                return Redirect("Error");
            }
            bool temp;
            using (var dataHelper = new RssDataHelper())
            {
                this.GetUserId();
                int rssNewsId = Int32.Parse(id.ToString());
                temp = dataHelper.AddRssNewsToFavorite(rssNewsId, _userId);
            }
            var result = new { result = temp };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteNewsFromUserNewsList(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Некоректный идентификатор при удалении новости из списка новостей";
                return Redirect("Error");
            }
            bool temp;
            using (var dataHelper = new RssDataHelper())
            {
                this.GetUserId();
                int rssNewsId = Int32.Parse(id.ToString());
                temp = dataHelper.DeleteRssNewsFromUserNewsList(rssNewsId, _userId);
            }
            var result = new { result = temp };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReadLaterThisNews(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Некорректный идентификатор при добавлении новости в список для чтения";
                return Redirect("Error");
            }
            bool temp;
            using (var dataHelper = new RssDataHelper())
            {
                this.GetUserId();
                int rssNewsId = Int32.Parse(id.ToString());
                temp = dataHelper.AddRssNewsToReadingList(rssNewsId, _userId);
            }
            var result = new { result = temp };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private void GetUserId()
        {
            if (String.IsNullOrEmpty(_userId))
                _userId = User.Identity.GetUserId();
        }
    }
}
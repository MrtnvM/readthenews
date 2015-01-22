using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.ServiceModel.Syndication;
using ReadTheNews.Models;

namespace ReadTheNews.Helpers
{
    public static class RssParseHelper
    {
        static Regex imgSrc = new Regex("src\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))");
        static Regex imgTag = new Regex(@"<img.+>");
        static Regex aTag = new Regex(@"<a.+>");
        static RssDataHelper dataHelper = new RssDataHelper();

        public static RssChannel ParseChannel(SyndicationFeed Channel, string rssChannelUrl)
        {
            RssChannel currentChannel = new RssChannel();
            currentChannel.Title = Channel.Title != null ? Channel.Title.Text : "no";
            currentChannel.Language = !String.IsNullOrEmpty(Channel.Language) ? Channel.Language : "no";
            currentChannel.Link = rssChannelUrl;
            currentChannel.Description = Channel.Description != null ? Channel.Description.Text : "no";
            currentChannel.ImageSrc = Channel.ImageUrl != null ? Channel.ImageUrl.ToString() : "no";
            currentChannel.PubDate = Channel.LastUpdatedTime.DateTime != new DateTime() ?
                Channel.LastUpdatedTime.DateTime :
                    Channel.Items.FirstOrDefault() != null ?
                        Channel.Items.FirstOrDefault().PublishDate.DateTime : DateTime.Now;

            return currentChannel;
        }

        public static RssItem ParseItem(SyndicationItem item, RssChannel currentChannel)
        {
            RssItem newRssItem = new RssItem();

            newRssItem.Title = item.Title != null ? item.Title.Text : "no";
            newRssItem.Description = item.Summary != null ? item.Summary.Text : "no";
            aTag.Replace(newRssItem.Description, "");
            newRssItem.Date = item.PublishDate.Date;
            newRssItem.Link = !String.IsNullOrEmpty(item.Id) ? item.Id : "no";
            Match src = imgSrc.Match(newRssItem.Description);
            if (src.Success)
            {
                newRssItem.ImageSrc = src.Groups[1].ToString();
                if (String.IsNullOrEmpty(newRssItem.ImageSrc))
                    newRssItem.ImageSrc = "no";
            }
            newRssItem.Description = imgTag.Replace(newRssItem.Description, "");

            if (currentChannel == null)
                return null;

            newRssItem.RssChannel = currentChannel;

            var categories = new List<RssCategory>(item.Categories.Count);

            foreach (SyndicationCategory category in item.Categories)
            {
                categories.Add(new RssCategory { Name = category.Name });
            }
            categories = categories.Distinct(new RssCategoryEqualityComparer()).ToList();
            foreach (RssCategory category in categories)
            {
                dataHelper.AddRssCategoryInDb(category);
            }
            newRssItem.RssCategories = categories;

            return newRssItem;
        }
    }
}
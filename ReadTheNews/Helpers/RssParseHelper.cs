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
        static Regex tag = new Regex(@"<[^>]*>");
        static Regex aTag = new Regex(@"<a.+>(.+)</a>");
        static Regex image = new Regex(@".+\.(gif|png|jpg|jpeg)$");
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
            if (currentChannel == null)
                return null;

            RssItem newRssItem = new RssItem();

            newRssItem.Title = item.Title != null ? item.Title.Text : "no";

            newRssItem.Description = item.Summary != null ? item.Summary.Text : "no";
            Match aText = aTag.Match(newRssItem.Description);
            if (aText.Success)
            {
                aTag.Replace(newRssItem.Description, aText.Groups[1].ToString());
            }

            newRssItem.Date = item.PublishDate.Date;
            newRssItem.Link = !String.IsNullOrEmpty(item.Id) ? item.Id : item.Links[0].Uri.ToString();

            Match src = imgSrc.Match(newRssItem.Description);
            if (src.Success)
            {
                newRssItem.ImageSrc = src.Groups[1].ToString();                
            }
            newRssItem.Description = tag.Replace(newRssItem.Description, "");
            if (String.IsNullOrEmpty(newRssItem.ImageSrc) && item.Links != null)
            {
                foreach (SyndicationLink link in item.Links)
                {
                    string url = link.Uri.ToString();
                    if (image.IsMatch(url))
                    {
                        newRssItem.ImageSrc = url;
                        break;
                    }
                }
            }
            if (String.IsNullOrEmpty(newRssItem.ImageSrc))
                newRssItem.ImageSrc = currentChannel.ImageSrc;
            else
                newRssItem.ImageSrc = "no";            

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
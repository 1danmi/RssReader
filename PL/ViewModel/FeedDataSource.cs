using Nest;
using BL;
using PL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.Web.Syndication;


namespace PL.ViewModel
{/// <summary>
 /// Encapsulates methods that retrieve and save RSS feed data. 
 /// </summary>
    public static class FeedDataSource
    {
        // public static DAL.EsFeeder _EsClientDAL = new DAL.EsFeeder();
        private static Feed feedData = new Feed();
        //private static Feed favoriteData = new Feed(false);


        /// <summary>
        /// Gets the favorites feed, either from local storage, 
        /// or by initializing a new FeedViewModel instance. 
        /// </summary>
        public static async Task<FeedViewModel> GetFavoritesAsync()
        {
            var favoritesFile = await ApplicationData.Current.LocalFolder
                .TryGetItemAsync("favorites.dat") as StorageFile;
            if (favoritesFile != null)
            {
                var buffer = await FileIO.ReadBufferAsync(favoritesFile);
                return Serializer.Deserialize<FeedViewModel>(buffer.ToArray());
            }
            else
            {
                return new FeedViewModel
                {
                    Name = "Favorites",
                    Description = "Articles that you've starred",
                    Symbol = Symbol.OutlineStar,
                    Link = new Uri("http://localhost"),
                    IsFavoritesFeed = true
                };
            }
        }

        /// <summary>
        /// Gets the initial set of feeds, either from local storage or 
        /// from the app package if there is nothing in local storage.
        /// </summary>
        public static async Task<List<FeedViewModel>> GetFeedsAsync()
        {
            var feeds = new List<FeedViewModel>();
            for (int id = 1; id <= feedData.GetSumFeeds(); id++)
            {
                List<DTO.Feed> typedList = feedData.QueryById(id.ToString());
                feeds.Add(new FeedViewModel() { Name = typedList.First().Name, LinkAsString = typedList.First().Link });
            }
            return feeds;
        }

        /// <summary>
        /// Retrieves feed data from the server and updates the appropriate FeedViewModel properties.
        /// </summary>
        /*private*/
        public static async Task<bool> TryGetFeedAsync(FeedViewModel feedViewModel, CancellationToken? cancellationToken = null)
        {
            try
            {
                var feed = await new SyndicationClient().RetrieveFeedAsync(feedViewModel.Link);

                if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested) return false;

                feedViewModel.LastSyncDateTime = DateTime.Now;
                feedViewModel.Name = String.IsNullOrEmpty(feedViewModel.Name) ? feed.Title.Text : feedViewModel.Name;
                feedViewModel.Description = feed.Subtitle?.Text ?? feed.Title.Text;

                feed.Items.Select(item => new ArticleViewModel
                {
                    Title = item.Title.Text,
                    Summary = item.Summary == null ? string.Empty :
                        item.Summary.Text.RegexRemove("\\&.{0,4}\\;").RegexRemove("<.*?>"),
                    Author = item.Authors.Select(a => a.NodeValue).FirstOrDefault(),
                    Link = item.ItemUri ?? item.Links.Select(l => l.Uri).FirstOrDefault(),
                    PublishedDate = item.PublishedDate
                })
                .ToList().ForEach(article =>
                {
                    var favorites = AppShell.Current.ViewModel.FavoritesFeed;
                    var existingCopy = favorites.Articles.FirstOrDefault(a => a.Equals(article));
                    article = existingCopy ?? article;
                    if (!feedViewModel.Articles.Contains(article)) feedViewModel.Articles.Add(article);
                });
                feedViewModel.IsInError = false;
                feedViewModel.ErrorMessage = null;
                return true;
            }
            catch (Exception)
            {
                if (!cancellationToken.HasValue || !cancellationToken.Value.IsCancellationRequested)
                {
                    feedViewModel.IsInError = true;
                    feedViewModel.ErrorMessage = feedViewModel.Articles.Count == 0 ? BAD_URL_MESSAGE : NO_REFRESH_MESSAGE;
                }
                return false;
            }
        }
        /// <summary>
        /// Retrieves feed data from the server and updates the appropriate FeedViewModel properties.
        /// </summary>
        /*private*/
        public static async Task<bool> TryGetFeedAsync1(FeedViewModel feedViewModel, CancellationToken? cancellationToken = null)
        {
            try
            {
                var feed = await new SyndicationClient().RetrieveFeedAsync(feedViewModel.Link);

                if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested) return false;

                feedViewModel.LastSyncDateTime = DateTime.Now;
                feedViewModel.Name = String.IsNullOrEmpty(feedViewModel.Name) ? feed.Title.Text : feedViewModel.Name;
                feedViewModel.Description = feed.Subtitle?.Text ?? feed.Title.Text;

                feed.Items.Select(item => new ArticleViewModel
                {
                    Title = item.Title.Text,
                    Summary = item.Summary == null ? string.Empty :
                        item.Summary.Text.RegexRemove("\\&.{0,4}\\;").RegexRemove("<.*?>"),
                    Author = item.Authors.Select(a => a.NodeValue).FirstOrDefault(),
                    Link = item.ItemUri ?? item.Links.Select(l => l.Uri).FirstOrDefault(),
                    PublishedDate = item.PublishedDate
                })
                .ToList().ForEach(article =>
                {
                    var favorites = AppShell.Current.ViewModel.FavoritesFeed;
                    var existingCopy = favorites.Articles.FirstOrDefault(a => a.Equals(article));
                    article = existingCopy ?? article;
                    if (!feedViewModel.Articles.Contains(article))
                    {
                        if (Query.nameOfArticle.Equals(""))
                            feedViewModel.Articles.Add(article);

                        else if (article.Title.Contains(Query.nameOfArticle) || article.Summary.Contains(Query.nameOfArticle))
                            feedViewModel.Articles.Add(article);
                    }
                    if (feedViewModel.Articles.Contains(article))
                    {
                        if (Query.nameOfArticle.Equals(""))
                            feedViewModel.Articles.Add(article);

                        else if (!article.Title.Contains(Query.nameOfArticle) && !article.Summary.Contains(Query.nameOfArticle))
                            feedViewModel.Articles.Remove(article);
                    }
                });
                //feedViewModel.Articles = new ObservableCollection<ArticleViewModel>(feedViewModel.Articles.Distinct());
                feedViewModel.IsInError = false;
                feedViewModel.ErrorMessage = null;
                return true;
            }
            catch (Exception)
            {
                if (!cancellationToken.HasValue || !cancellationToken.Value.IsCancellationRequested)
                {
                    feedViewModel.IsInError = true;
                    feedViewModel.ErrorMessage = feedViewModel.Articles.Count == 0 ? BAD_URL_MESSAGE : NO_REFRESH_MESSAGE;
                }
                return false;
            }
        }
        /// <summary>
        /// Attempts to update the feed with new data from the server.
        /// </summary>
        public static async Task RefreshAsync(this FeedViewModel feedViewModel, CancellationToken? cancellationToken = null)
        {
            if (feedViewModel.Link.Host == "localhost" ||
                (feedViewModel.Link.Scheme != "http" && feedViewModel.Link.Scheme != "https")) return;

            feedViewModel.IsLoading = true;

            int numberOfAttempts = 5;
            bool success = false;
            do { success = await TryGetFeedAsync(feedViewModel, cancellationToken); }
            while (!success && numberOfAttempts-- > 0 &&
                (!cancellationToken.HasValue || !cancellationToken.Value.IsCancellationRequested));

            feedViewModel.IsLoading = false;
        }

        /// <summary>
        /// Saves the favorites feed (the first feed of the feeds list) to local storage. 
        /// </summary>
        public static async Task SaveFavoritesAsync(this FeedViewModel favorites)
        {
            //List<FeedViewModel> fav = new List<FeedViewModel>();
            //fav.Add(favorites);
            //favoriteData.Index(fav);
            var file = await ApplicationData.Current.LocalFolder
                .CreateFileAsync("favorites.dat", CreationCollisionOption.ReplaceExisting);
            byte[] array = Serializer.Serialize(favorites);
            await FileIO.WriteBytesAsync(file, array);
        }

        /// <summary>
        /// Saves the feed data (not including the Favorites feed) to local storage. 
        /// </summary>
        public static async Task SaveAsync(this IEnumerable<FeedViewModel> feeds)
        {
            //עושה מחיקה אבל בתכלס צריך לשנות את זה שלא תמיד יבצע מחיקה
            feedData.Delete();
            feedData.Index(feeds);
        }
        private const string BAD_URL_MESSAGE = "Hmm... Are you sure this is an RSS URL?";
        private const string NO_REFRESH_MESSAGE = "Sorry. We can't get more articles right now.";
    }
}

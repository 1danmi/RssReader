using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL.ViewModel
{
    /// <summary>
    /// Represents an article in an RSS feed and user interactions with the article. 
    /// </summary>
    public class ArticleViewModel : BE.ArticleViewModelBase
    {
        
        /// <summary>
        /// Updates the FavoritesFeed when an article is starred or unstarred. 
        /// </summary>
        public void SyncFavoritesFeed() => AppShell.Current.ViewModel.SyncFavoritesFeed(this);

        private bool? _isStarred = false;
    }
}

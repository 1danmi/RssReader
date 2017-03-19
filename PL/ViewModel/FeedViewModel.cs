using PL.Common;
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Windows.UI.Xaml.Controls;

namespace PL.ViewModel
{
    /// <summary>
    /// Represents an RSS feed and user interactions with the feed. 
    /// </summary>
    public class FeedViewModel : BE.FeedViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the FeedViewModel class. 
        /// </summary>
        public FeedViewModel()
        {

        }

      
        [IgnoreDataMember]
        private bool _isInError;

     
        /// <summary>
        /// Determines whether the specified object is equal to the current object. 
        /// </summary>
        public override bool Equals(object obj) =>
            obj is FeedViewModel ? (obj as FeedViewModel).GetHashCode() == GetHashCode() : false;

        /// <summary>
        /// Returns the hash code of the FeedViewModel, which is based on 
        /// a string representation the Link value, using only the host and path.  
        /// </summary>
        public override int GetHashCode() =>
            Link?.GetComponents(UriComponents.Host | UriComponents.Path, UriFormat.Unescaped).GetHashCode() ?? 0;

        private const string NOT_HTTP_MESSAGE = "Sorry. The URL must begin with http:// or https://";
        private const string INVALID_URL_MESSAGE = "Sorry. That is not a valid URL.";

       
    }
}

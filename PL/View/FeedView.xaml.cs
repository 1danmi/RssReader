using BE;
using PL.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PL.View
{
    /// <summary>
    /// Represents the UI for viewing a list of articles from a feed. 
    /// </summary>
    public sealed partial class FeedView : Page
    {
        /// <summary>
        /// Gets the MainViewModel used by the app. 
        /// </summary>
        public MainViewModel ViewModel => AppShell.Current.ViewModel;

        /// <summary>
        /// Initializes a new instance of the FeedView class. 
        /// </summary>
        public FeedView()
        {
            this.InitializeComponent();
            ViewModel.Initialized += (s, e) =>
            {
                // Realize the UI elements marked x:DeferLoadStrategy="Lazy". 
                // Deferred loading ensures that these elements do not appear 
                // in the UI before the feed data is available.
                FindName("NormalFeedView");
                FindName("FeedErrorMessage");
                FindName("FavoritesIsEmptyMessage");
            };
        }

        /// <summary>
        /// Sets the ViewModel.CurrentArticle property to the clicked item. 
        /// </summary>
        /// <remarks>
        /// The ArticlesListView.ItemsSource property must be bound to a property
        /// of type Object, so it is bound to ViewModel.CurrentArticleAsObject.
        /// Making this a two-way binding would require a CurrentArticleAsObject
        /// setter that updates CurrentArticle. However, CurrentArticle must
        /// raise the PropertyChanged event with every setter call (not just ones 
        /// that change its value), which causes an infinite recursion. The easiest
        /// way to prevent this is to use a one-way binding, and update CurrentArticle
        /// in this ItemClick event handler. 
        /// </remarks>
        private void ArticlesListView_ItemClick(object sender, ItemClickEventArgs e) =>
            ViewModel.CurrentArticle = e.ClickedItem as ArticleViewModel;

        /// <summary>
        /// Updates the favorites list when the user stars or unstars an article. 
        /// </summary>
        private void ToggleButton_Toggled(object sender, RoutedEventArgs e) =>
            ViewModel.SyncFavoritesFeed(((ToggleButton)sender).DataContext as ArticleViewModel);

        private async void myInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox objTextBox = myInput as TextBox;
            Query.nameOfArticle = objTextBox.Text;
            if (Query.nameOfArticle == "")
                await FeedDataSource.TryGetFeedAsync(ViewModel.CurrentFeed);
            else
                await FeedDataSource.TryGetFeedAsync1(ViewModel.CurrentFeed);
        }

        private void myInputButton_Click(object sender, RoutedEventArgs e)
        {
            if (myInput.Visibility == Visibility.Collapsed)
            {
                myInput.Visibility = Visibility.Visible;
                ((TextBox)myInput).Text = string.Empty;
                myInput.Focus(FocusState.Programmatic);
            }
            else if (myInput.Visibility == Visibility.Visible)
            {
                myInput.Visibility = Visibility.Collapsed;
            }

        }
    }
}

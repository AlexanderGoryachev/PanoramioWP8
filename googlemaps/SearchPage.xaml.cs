using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Input;
using googlemaps.Common;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace googlemaps
{
    public partial class SearchPage : PhoneApplicationPage
    {
        public SearchPage()
        {
            InitializeComponent();
        }
        
        private async void SearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadingBar.IsIndeterminate = true;
                this.Focus();
                await SearchManager.Search(SearchBox.Text);
                SearchResultsListBox.ItemsSource = SearchManager.SearchResults.results;
                LoadingBar.IsIndeterminate = false;
            }
        }

        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = SearchResultsListBox.SelectedIndex;
            if (selectedIndex > -1)
            { 
                //var lat = SearchManager.SearchResults.results[selectedIndex].geometry.location.lat;
                //var lon = SearchManager.SearchResults.results[selectedIndex].geometry.location.lng;
                //NavigationService.Navigate(new Uri(@"/MainPage.xaml?lat=" + lat + "&lon=" + lon, UriKind.Relative));

                NavigationService.Navigate(new Uri(@"/MainPage.xaml?selectedIndex=" + selectedIndex, UriKind.Relative));
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
    }
}
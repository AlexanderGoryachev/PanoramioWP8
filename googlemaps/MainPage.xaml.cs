//dedicated to maestros

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using System.IO.IsolatedStorage;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using googlemaps.Common;
using System.Globalization;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using System.IO;
using Microsoft.Xna.Framework.Media;

namespace googlemaps
{
    public partial class MainPage : PhoneApplicationPage
    {
        bool isTappedPushpin = false;
        string PhotoOnPanoramioUrl = string.Empty;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            mapItemsControl.ItemsSource = PinManager.PinCollection;
            ApplicationBar = ((ApplicationBar)this.Resources["MainAppBar"]);

            GetAppSettings();
        }

        private async void GetAppSettings()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            SelectMapType();

            if (settings.Contains("ScaleButton"))
            {
                if ((bool)settings["ScaleButton"])
                    ScalePanel.Visibility = Visibility.Visible;
                else
                    ScalePanel.Visibility = Visibility.Collapsed;
                settings.Save();
            }

            // request confirmation of sending geo coordinates
            if (!settings.Contains("LocationConsent"))
            {
                MessageBoxResult result = MessageBox.Show("This app accesses your phone's location. Is that ok?", "Location", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK) 
                    settings["LocationConsent"] = true;
                else 
                    settings["LocationConsent"] = false;
                settings.Save();
            }

            // go to my location if app just launched
            if ((bool)settings["launch"] & (bool)settings["LocationConsent"])
            {
                await GoToMyLocation();
                settings["launch"] = (bool)false;
            }


        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                //var lat = NavigationContext.QueryString["lat"].ToString();
                //var lon = NavigationContext.QueryString["lon"].ToString();
                //var southeast = NavigationContext.QueryString["southeast"].ToString();
                //var northeast = NavigationContext.QueryString["northeast"].ToString();

                var selectedIndex = Int32.Parse(NavigationContext.QueryString["selectedIndex"]);

                var centerLat = SearchManager.SearchResults.results[selectedIndex].geometry.location.lat;
                var centerLon = SearchManager.SearchResults.results[selectedIndex].geometry.location.lng;
                var southwestLon = SearchManager.SearchResults.results[selectedIndex].geometry.bounds.southwest.lng;
                var northeastLon = SearchManager.SearchResults.results[selectedIndex].geometry.bounds.northeast.lng;

                googlemap.Center = new GeoCoordinate(centerLat, centerLon);

                var GLOBE_WIDTH = 256; // a constant in Google's map projection
                var west = southwestLon;
                var east = northeastLon;
                var angle = east - west;
                if (angle < 0) angle += 360;
                var zoom = Math.Round(Math.Log(googlemap.ViewportSize.Width * 360 / angle / GLOBE_WIDTH) / Math.Log(2));

                googlemap.ZoomLevel = zoom + 1;
            }
            catch { }               
           
        }
        
        private void ButtonZoomIn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	googlemap.ZoomLevel++;
        }

        private void ButtonZoomOut_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	googlemap.ZoomLevel--;
        }

        private void SelectMapType()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains("MapType"))
            {
                switch (settings["MapType"].ToString())
                {
                    case "hybrid":
                        hybrid.Visibility = Visibility.Visible;
                        satellite.Visibility = Visibility.Collapsed;
                        street.Visibility = Visibility.Collapsed;
                        physical.Visibility = Visibility.Collapsed;
                        wateroverlay.Visibility = Visibility.Collapsed;
                        break;

                    case "satellite":
                        hybrid.Visibility = Visibility.Collapsed;
                        satellite.Visibility = Visibility.Visible;
                        street.Visibility = Visibility.Collapsed;
                        physical.Visibility = Visibility.Collapsed;
                        wateroverlay.Visibility = Visibility.Collapsed;
                        break;

                    case "street":
                        hybrid.Visibility = Visibility.Collapsed;
                        satellite.Visibility = Visibility.Collapsed;
                        street.Visibility = Visibility.Visible;
                        physical.Visibility = Visibility.Collapsed;
                        wateroverlay.Visibility = Visibility.Collapsed;
                        break;

                    case "physical":
                        hybrid.Visibility = Visibility.Collapsed;
                        satellite.Visibility = Visibility.Collapsed;
                        street.Visibility = Visibility.Collapsed;
                        physical.Visibility = Visibility.Visible;
                        wateroverlay.Visibility = Visibility.Collapsed;
                        break;

                    case "wateroverlay":
                        hybrid.Visibility = Visibility.Collapsed;
                        satellite.Visibility = Visibility.Collapsed;
                        street.Visibility = Visibility.Collapsed;
                        physical.Visibility = Visibility.Collapsed;
                        wateroverlay.Visibility = Visibility.Visible;
                        break;
                }
            }
            else
            {
                hybrid.Visibility = Visibility.Collapsed;
                satellite.Visibility = Visibility.Collapsed;
                street.Visibility = Visibility.Visible;
                physical.Visibility = Visibility.Collapsed;
                wateroverlay.Visibility = Visibility.Collapsed;
                settings["MapType"] = "street";
            }
        }        

        private void SettingsAppBar_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private async void MyLocationAppBar_Click(object sender, EventArgs e)
        {
            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"])
            {
                await GoToMyLocation();
                IsolatedStorageSettings.ApplicationSettings["launch"] = (bool)false;
            }
            else
            {
                MessageBox.Show("Для данного приложения определение местоположения запрещено. \n\nВы можете изменить это в настройках данного приложения");
            }
        }

        private async Task GoToMyLocation()
        {
            LoadingBar.IsIndeterminate = true;

            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                LoadingBar.IsIndeterminate = false;
                return;
            }

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracy = Windows.Devices.Geolocation.PositionAccuracy.High;

            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                var latitude = geoposition.Coordinate.Latitude;
                var longitude = geoposition.Coordinate.Longitude;
                var accuracy =
                googlemap.Center = new GeoCoordinate(latitude, longitude);
                MyLocationPushpin.Location = new GeoCoordinate(latitude, longitude);
                MyLocationPushpin.Visibility = Visibility.Visible;
                googlemap.ZoomLevel = 16;
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    MessageBox.Show("Определение местоположения отключено в настройках телефона");
                }
                //else
                {
                    // something else happened acquring the location
                }
            }

            LoadingBar.IsIndeterminate = false;
        }

        private async void googlemap_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!isTappedPushpin)
            {
                LoadingBar.IsIndeterminate = true;

                PinManager.PinCollection.Clear();

                var centerOfViewport = e.GetPosition(googlemap);
                var viewportPadding = (googlemap.ViewportSize.Width - 180) / 2;
                GeoCoordinate coordinates = googlemap.ViewportPointToLocation(centerOfViewport);
                GeoCoordinate leftBottom = googlemap.ViewportPointToLocation(new Point(centerOfViewport.X - viewportPadding, centerOfViewport.Y + viewportPadding));
                GeoCoordinate rightTop = googlemap.ViewportPointToLocation(new Point(centerOfViewport.X + viewportPadding, centerOfViewport.Y - viewportPadding));

                googlemap.Center = coordinates;

                string miny = leftBottom.Latitude.ToString("0.000000000000000", CultureInfo.InvariantCulture);  //координата x левого нижнего угла
                string minx = leftBottom.Longitude.ToString("0.000000000000000", CultureInfo.InvariantCulture); //координата y левого нижнего угла;
                string maxy = rightTop.Latitude.ToString("0.000000000000000", CultureInfo.InvariantCulture);    //координата x правого верхнего угла
                string maxx = rightTop.Longitude.ToString("0.000000000000000", CultureInfo.InvariantCulture);   //координата y правого верхнего угла;

                var size = "square";
                var order = "full";
                var set = "public";

                var mapfilter = "true";

                var from = 0;
                var to = 20;

                var photosUrl = "http://www.panoramio.com/map/get_panoramas.php?order=" + order + "&set=" + set + "&from=" + from + "&to=" + to + "&minx=" + minx + "&miny=" + miny + "&maxx=" + maxx + "&maxy=" + maxy + "&size=" + size + "&mapfilter=" + mapfilter;

                await PinManager.GetPhotosAround(photosUrl);

                for (int i = 0; i < PinManager.ListOfPhotosAround.photos.Count; i++)
                {
                    PinManager.PinCollection.Add(
                        new PinManager.Pin()
                        {
                            id = i.ToString(),
                            location = new GeoCoordinate(PinManager.ListOfPhotosAround.photos[i].latitude, PinManager.ListOfPhotosAround.photos[i].longitude),
                            iconUri = new Uri(PinManager.ListOfPhotosAround.photos[i].photo_file_url, UriKind.RelativeOrAbsolute)
                        });
                }

                LoadingBar.IsIndeterminate = false;
            }
        }

        private void pushpin_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            isTappedPushpin = true;

            var tappedPushpin = sender as Pushpin;
            var id = Int32.Parse(tappedPushpin.Tag.ToString());
            var uriString = PinManager.ListOfPhotosAround.photos[id].photo_file_url.Replace("square", "medium");
            var imageUrl = new Uri(uriString, UriKind.Absolute);

            PinManager.CurrentId = id;

            ApplicationBar = ((ApplicationBar)this.Resources["PhotoAppBar"]);

            ImageGrid.Visibility = Visibility.Visible;

            FullImage.Source = new BitmapImage() { UriSource = imageUrl };

            TitleImage.Text = PinManager.ListOfPhotosAround.photos[id].photo_title;

            AuthorOfImage.Content = PinManager.ListOfPhotosAround.photos[id].owner_name;
            AuthorOfImage.NavigateUri = new Uri(PinManager.ListOfPhotosAround.photos[id].owner_url, UriKind.Absolute);
            PhotoOnPanoramioUrl = PinManager.ListOfPhotosAround.photos[id].photo_url;
            //GoToPanoramio.NavigateUri = new Uri(PinManager.ListOfPhotosAround.photos[id].photo_url, UriKind.Absolute);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (ImageGrid.Visibility == Visibility.Visible)
            {
                ImageGrid.Visibility = Visibility.Collapsed;
                ApplicationBar = ((ApplicationBar)this.Resources["MainAppBar"]);
                isTappedPushpin = false;
                e.Cancel = true;
            }
            else
            {
                while (NavigationService.BackStack.Any())
                    NavigationService.RemoveBackEntry();
            }
        }
             
        private void DownloadPhoto_Click(object sender, EventArgs e)
        {
            LoadingBar.IsIndeterminate = true;

            var bitmapImage = new BitmapImage { CreateOptions = BitmapCreateOptions.None };
            bitmapImage.ImageOpened += bitmapImage_ImageOpened;
            bitmapImage.ImageFailed += bitmapImage_ImageFailed;
            bitmapImage.DownloadProgress += bitmapImage_DownloadProgress; 
            
            string imageUri = @"http://static.panoramio.com/photos/original/" + PinManager.ListOfPhotosAround.photos[PinManager.CurrentId].photo_id.ToString() + ".jpg";
            bitmapImage.UriSource = new Uri(imageUri);
        }

        private void bitmapImage_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {

        }

        private void bitmapImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            LoadingBar.IsIndeterminate = false;
            MessageBox.Show("Ошибка. Фото не было сохранено");
        }

        private void bitmapImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            var userStoreForApplication = IsolatedStorageFile.GetUserStoreForApplication();
            var writeableBitmap = new WriteableBitmap(sender as BitmapImage);
            var isolatedStorageFileStream = userStoreForApplication.CreateFile("temp.jpg");
            writeableBitmap.SaveJpeg(isolatedStorageFileStream, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight, 0, 85);

            isolatedStorageFileStream.Close();
            isolatedStorageFileStream = userStoreForApplication.OpenFile("temp.jpg", FileMode.Open, FileAccess.Read);
            // Save the image to the camera roll or saved pictures album.
            var mediaLibrary = new MediaLibrary();
            // Save the image to the saved pictures album.
            mediaLibrary.SavePicture(string.Format("SavedPicture{0}.jpg", DateTime.Now), isolatedStorageFileStream);
            isolatedStorageFileStream.Close();

            MessageBox.Show("Фото сохранено");
            LoadingBar.IsIndeterminate = false;
        }

        private void PhotoInfo_Click(object sender, EventArgs e)
        {
            ImageInfoGrid.Visibility = ImageInfoGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SharePhoto_Click(object sender, EventArgs e)
        {

        }

        private void SearchAppBar_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(@"/SearchPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void GoToPanoramio(object sender, EventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(PhotoOnPanoramioUrl, UriKind.Absolute);
            webBrowserTask.Show();
        }
    }

    public enum GoogleTileTypes
    {
        Hybrid,
        Physical,
        Street,
        Satellite,
        WaterOverlay
    }

    public class GoogleTile : Microsoft.Phone.Controls.Maps.TileSource
    {
        private int _server;
        private char _mapmode;
        private GoogleTileTypes _tiletypes;

        public GoogleTileTypes TileTypes
        {
            get { return _tiletypes; }
            set
            {
                _tiletypes = value;
                MapMode = MapModeConverter(value);
            }
        }

        public char MapMode
        {
            get { return _mapmode; }
            set { _mapmode = value; }
        }

        public int Server
        {
            get { return _server; }
            set { _server = value; }
        }

        public GoogleTile()
        {
            UriFormat = @"http://mt{0}.google.com/vt/lyrs={1}&z={2}&x={3}&y={4}";
            Server = 0;
        }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            if (zoomLevel > 0)
            {
                var Url = string.Format(UriFormat, Server, MapMode, zoomLevel, x, y);
                return new Uri(Url);
            }
            return null;
        }

        private char MapModeConverter(GoogleTileTypes tiletype)
        {
            switch (tiletype)
            {
                case GoogleTileTypes.Hybrid:
                    {
                        return 'y';
                    }
                case GoogleTileTypes.Physical:
                    {
                        return 't';
                    }
                case GoogleTileTypes.Satellite:
                    {
                        return 's';
                    }
                case GoogleTileTypes.Street:
                    {
                        return 'm';
                    }
                case GoogleTileTypes.WaterOverlay:
                    {
                        return 'r';
                    }
            }
            return ' ';
        }
    }
}
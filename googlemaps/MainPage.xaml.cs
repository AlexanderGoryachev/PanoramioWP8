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

namespace googlemaps
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            hybrid.Visibility = Visibility.Visible;
            hybridRadioButton.IsChecked = true;
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // await GoToMyLocation();
            googlemap.Center = new GeoCoordinate(51.672000, 39.184300);
            googlemap.ZoomLevel = 14;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                // User has opted in or out of Location
                return;
            }
            else
            {
                MessageBoxResult result =
                    MessageBox.Show("This app accesses your phone's location. Is that ok?",
                    "Location",
                    MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();

            }
        }
        
        private void ButtonZoomIn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	googlemap.ZoomLevel++;
        }

        private void ButtonZoomOut_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	googlemap.ZoomLevel--;
        }

        private void hybridRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            hybrid.Visibility = Visibility.Visible;
            satellite.Visibility = Visibility.Collapsed;
            street.Visibility = Visibility.Collapsed;
            physical.Visibility = Visibility.Collapsed;
            wateroverlay.Visibility = Visibility.Collapsed;
        }

        private void physicalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            hybrid.Visibility = Visibility.Collapsed;
            satellite.Visibility = Visibility.Collapsed;
            street.Visibility = Visibility.Collapsed;
            physical.Visibility = Visibility.Visible;
            wateroverlay.Visibility = Visibility.Collapsed;
        }

        private void waterRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            hybrid.Visibility = Visibility.Collapsed;
            satellite.Visibility = Visibility.Collapsed;
            street.Visibility = Visibility.Collapsed;
            physical.Visibility = Visibility.Collapsed;
            wateroverlay.Visibility = Visibility.Visible;
        }

        private void streetRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            hybrid.Visibility = Visibility.Collapsed;
            satellite.Visibility = Visibility.Collapsed;
            street.Visibility = Visibility.Visible;
            physical.Visibility = Visibility.Collapsed;
            wateroverlay.Visibility = Visibility.Collapsed;
        }

        private void satelliteRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            hybrid.Visibility = Visibility.Collapsed;
            satellite.Visibility = Visibility.Visible;
            street.Visibility = Visibility.Collapsed;
            physical.Visibility = Visibility.Collapsed;
            wateroverlay.Visibility = Visibility.Collapsed;
        }

        private void SettingsAppBar_Click(object sender, EventArgs e)
        {
            SettingsPanel.Visibility = Visibility.Visible;
        }

        private void SettingsPanel_LostFocus(object sender, RoutedEventArgs e)
        {
            //SettingsPanel.Visibility = Visibility.Collapsed; 
        }

        private void googlemap_MouseEnter(object sender, MouseEventArgs e)
        {
            SettingsPanel.Visibility = Visibility.Collapsed; 
        }

        private void ScaleSwitch_Click(object sender, RoutedEventArgs e)
        {
            ScalePanel.Visibility = ScaleSwitch.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void MyLocationAppBar_Click(object sender, EventArgs e)
        {
            await GoToMyLocation();
        }

        private async Task GoToMyLocation()
        {
            LoadingBar.IsIndeterminate = true;

            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
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
            LoadingBar.IsIndeterminate = true;

            var centerOfViewport = e.GetPosition(googlemap);
            var viewportPadding = (googlemap.ViewportSize.Width - 180)/2;
            GeoCoordinate coordinates = googlemap.ViewportPointToLocation(centerOfViewport);
            GeoCoordinate leftBottom = googlemap.ViewportPointToLocation(new Point(centerOfViewport.X - viewportPadding, centerOfViewport.Y + viewportPadding));
            GeoCoordinate rightTop = googlemap.ViewportPointToLocation(new Point(centerOfViewport.X + viewportPadding, centerOfViewport.Y - viewportPadding));    
  
            googlemap.Center = coordinates;

            string miny = leftBottom.Latitude.ToString("0.000000000000000", CultureInfo.InvariantCulture);  //координата x левого нижнего угла
            string minx = leftBottom.Longitude.ToString("0.000000000000000", CultureInfo.InvariantCulture); //координата y левого нижнего угла;
            string maxy = rightTop.Latitude.ToString("0.000000000000000", CultureInfo.InvariantCulture);    //координата x правого верхнего угла
            string maxx = rightTop.Longitude.ToString("0.000000000000000", CultureInfo.InvariantCulture);   //координата y правого верхнего угла;
            
            var size = "square";
            var order = "popularity";
            var set = "public";

            var mapfilter = "true";

            var from = 0;
            var to = 20;            

            var photosUrl = "http://www.panoramio.com/map/get_panoramas.php?order=" + order + "&set=" + set + "&from=" + from + "&to=" + to + "&minx=" + minx + "&miny=" + miny + "&maxx=" + maxx + "&maxy=" + maxy + "&size=" + size + "&mapfilter=" + mapfilter;
            
            await PinManager.GetPhotosAround(photosUrl);

            PushpinsLayer.Children.Clear();

            for (int i = 0; i < PinManager.ListOfPhotosAround.photos.Count; i++)
            {
                var location = new GeoCoordinate(PinManager.ListOfPhotosAround.photos[i].latitude, PinManager.ListOfPhotosAround.photos[i].longitude);
                var pushpin = new Pushpin()
                {
                    Name = i.ToString(),
                    Location = location,
                    Style = Application.Current.Resources["PushpinStyle"] as Style,
                    //Content = new Image()
                    //{
                    //    Source = new BitmapImage() { UriSource = new Uri(PinManager.ListOfPhotosAround.photos[i].photo_file_url, UriKind.Absolute) },
                    //    Height = 60,
                    //    Width = 60,
                    //},
                    Content = new BitmapImage() { UriSource = new Uri(PinManager.ListOfPhotosAround.photos[i].photo_file_url, UriKind.Absolute) },
                };
                pushpin.Tap += pushpin_Tap;
                PushpinsLayer.AddChild(pushpin, location);
            }

            LoadingBar.IsIndeterminate = false;
        }

        private void pushpin_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var tappedPushpin = sender as Pushpin;
            var name = Int32.Parse(tappedPushpin.Name);
            var uriString = PinManager.ListOfPhotosAround.photos[name].photo_file_url.Replace("square", "medium");
            var imageUrl = new Uri(uriString, UriKind.Absolute);

            ApplicationBar.IsVisible = false;
            ImageGrid.Visibility = Visibility.Visible;

            FullImage.Source = new BitmapImage() { UriSource = imageUrl };

            TitleImage.Text = PinManager.ListOfPhotosAround.photos[name].photo_title;

            AuthorOfImage.Content = PinManager.ListOfPhotosAround.photos[name].owner_name;
            AuthorOfImage.NavigateUri = new Uri(PinManager.ListOfPhotosAround.photos[name].owner_url, UriKind.Absolute);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (ImageGrid.Visibility == Visibility.Visible) 
            {
                ImageGrid.Visibility = Visibility.Collapsed;
                ApplicationBar.IsVisible = true;
                e.Cancel = true;
            }
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
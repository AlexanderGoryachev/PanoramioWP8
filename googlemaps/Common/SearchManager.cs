using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace googlemaps.Common
{
    class SearchManager
    {
        public static RootObject SearchResults = new RootObject();

        public class AddressComponent
        {
            public string long_name { get; set; }
            public string short_name { get; set; }
            public ObservableCollection<string> types { get; set; }
        }

        public class Northeast
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Southwest
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Bounds
        {
            public Northeast northeast { get; set; }
            public Southwest southwest { get; set; }
        }

        public class Location
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Northeast2
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Southwest2
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Viewport
        {
            public Northeast2 northeast { get; set; }
            public Southwest2 southwest { get; set; }
        }

        public class Geometry
        {
            public Bounds bounds { get; set; }
            public Location location { get; set; }
            public string location_type { get; set; }
            public Viewport viewport { get; set; }
        }

        public class Result
        {
            public ObservableCollection<AddressComponent> address_components { get; set; }
            public string formatted_address { get; set; }
            public Geometry geometry { get; set; }
            public ObservableCollection<string> types { get; set; }
        }

        public class RootObject
        {
            public ObservableCollection<Result> results { get; set; }
            public string status { get; set; }
        }

        public static async Task Search(string textQuery)
        {
            var url = "http://maps.googleapis.com/maps/api/geocode/json?address=" + textQuery + "&sensor=false";
            var request = WebRequest.Create(new Uri(url)) as HttpWebRequest;
            request.Method = "GET";
            request.Headers["Accept-Language"] = "ru";
            WebResponse responseObject = await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, request);
            var responseStream = responseObject.GetResponseStream();
            var sr = new StreamReader(responseStream);
            string received = await sr.ReadToEndAsync();
            SearchResults = JsonConvert.DeserializeObject<RootObject>(received);
        }
    }
}

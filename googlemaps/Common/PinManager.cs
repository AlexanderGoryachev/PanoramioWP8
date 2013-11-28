using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace googlemaps.Common
{
    
    class PinManager
    {
        public static PhotosAround ListOfPhotosAround = new PhotosAround();

        public class PhotosAround
        {
            public int count { get; set; }
            public bool has_more { get; set; }
            public Map_location map_location { get; set; }
            public ObservableCollection<Photos> photos { get; set; }
        }

        public class Map_location
        {
            public double lat { get; set; }
            public double lon { get; set; }
            public int panoramio_zoom { get; set; }
        }

        public class Photos
        { 
            public int photo_id {get; set;}
            public string photo_title {get; set;}
            public string photo_url {get; set;}
            public string photo_file_url {get; set;}
            public double latitude {get; set;}
            public double longitude {get; set;}
            public int width {get; set;}
            public int height {get; set;}
            public int owner_id {get; set;}
            public string owner_name {get; set;}
            public string owner_url {get; set;}
            public string upload_date {get; set;}
        }

        public static async Task GetPhotosAround(string url)
        {
            var request = WebRequest.Create(new Uri(url)) as HttpWebRequest;
            request.Method = "GET";
            WebResponse responseObject = await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, request);
            var responseStream = responseObject.GetResponseStream();
            var sr = new StreamReader(responseStream);
            string received = await sr.ReadToEndAsync();
            ListOfPhotosAround = JsonConvert.DeserializeObject<PhotosAround>(received);
        }
    }
}

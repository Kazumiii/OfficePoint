using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficePoint.Core.Helpers
{
    public static class LocationHelper
    {
        public static async Task<Geocoordinate> GetCurrentLocationAsync()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();

            try
            {
                if (accessStatus == GeolocationAccessStatus.Allowed)
                {
                    Geolocator geolocator = new Geolocator() { DesiredAccuracy = PositionAccuracy.High };

                    Geoposition pos = await geolocator.GetGeopositionAsync();

                    return pos.Coordinate;
                }
            }
            catch
            {
            }

            return null;
        }

        public static async Task<string> GetThreeLocationAsync(double latitude, double longitude)
        {
            string threeWords = "";

            string url = "https://api.what3words.com/v2/reverse?coords=" + latitude.ToString("R") + "%2C" + longitude.ToString("R") + "&key=LRLWRJ6N&lang=en&format=geojson&display=full";

            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(url);

            Request.Method = "GET";

            try
            {
                HttpWebResponse Response = (HttpWebResponse)await Request.GetResponseAsync();
                StreamReader ResponseDataStream = new StreamReader(Response.GetResponseStream());

                string data = await ResponseDataStream.ReadToEndAsync();

                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(data));

                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Json.RawW3WInformation));
                var result = ser.ReadObject(ms) as Json.RawW3WInformation;

                threeWords = result.properties.words;
            }
            catch
            {

            }

            return threeWords;
        }

        public static async Task<LocationInformation> ResolveLocationAsync(double latitude, double longitude)
        {
            LocationInformation location = null;

            GeocodeServiceClient geocodeClient = Helpers.BindingManager.GetGeocodeClient();

            ReverseGeocodeRequest geocodeRequest = new ReverseGeocodeRequest();

            // Set the credentials using a valid Bing Maps key
            geocodeRequest.Credentials = new GeocodeService.Credentials();
            geocodeRequest.Credentials.ApplicationId = Core.Common.CoreConstants.BingLocationApiKey;

            // Set the full address query
            geocodeRequest.Location = new GeocodeService.Location()
            {
                Latitude = latitude,
                Longitude = longitude,
            };

            // Set the options to only return high confidence results 
            List<FilterBase> filters = new List<FilterBase>();

            filters.Add(new ConfidenceFilter()
            {
                MinimumConfidence = GeocodeService.Confidence.High,

            });

            GeocodeOptions geocodeOptions = new GeocodeOptions();
            geocodeOptions.Filters = filters;

            GeocodeResponse geocodeResponse = await geocodeClient.ReverseGeocodeAsync(geocodeRequest);

            var result = geocodeResponse.Results.FirstOrDefault();

            GeocodeService.Address foundAddress = null;
            GeocodeService.Location foundLocation = null;

            if (result != null)
            {
                foundAddress = result.Address;
                foundLocation = result.Locations.FirstOrDefault();

                if (foundAddress != null)
                {
                    location = new LocationInformation()
                    {
                        DisplayName = foundAddress.FormattedAddress,
                        StreetAddress = foundAddress.AddressLine,
                        City = foundAddress.Locality,
                        PostalCode = foundAddress.PostalCode,
                        State = foundAddress.AdminDistrict,

                        County = foundAddress.District,

                        Latitude = (foundLocation == null) ? 0.0 : foundLocation.Latitude,
                        Longitude = (foundLocation == null) ? 0.0 : foundLocation.Longitude,
                    };
                }


            }

            return location;

        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficePoint.Core.Models
{
    public class LocationResultInformation : System.Data.Common.BindableBase
    {
        private string _id;
        public string Id
        {
            get { return this._id; }
            set { this.SetProperty(ref this._id, value); }
        }

        private string _displayName;
        public string DisplayName
        {
            get { return this._displayName; }
            set { this.SetProperty(ref this._displayName, value); }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get { return this._phoneNumber; }
            set { this.SetProperty(ref this._phoneNumber, value); }
        }

        private string _streetAddress;
        public string StreetAddress
        {
            get { return this._streetAddress; }
            set { this.SetProperty(ref this._streetAddress, value); }
        }

        private string _formattedAddress;
        public string FormattedAddress
        {
            get { return this._formattedAddress; }
            set { this.SetProperty(ref this._formattedAddress, value); }
        }

        private double _distanceAway;
        public double DistanceAway
        {
            get { return this._distanceAway; }
            set { this.SetProperty(ref this._distanceAway, value); }
        }

        private string _district;
        public string District
        {
            get { return this._district; }
            set { this.SetProperty(ref this._district, value); }
        }

        private string _region;
        public string Region
        {
            get { return this._region; }
            set { this.SetProperty(ref this._region, value); }
        }

        private string _locality;
        public string Locality
        {
            get { return this._locality; }
            set { this.SetProperty(ref this._locality, value); }
        }

        private string _postalCode;
        public string PostalCode
        {
            get { return this._postalCode; }
            set { this.SetProperty(ref this._postalCode, value); }
        }

        private string _hoursOfOperation;
        public string HoursOfOperation
        {
            get { return this._hoursOfOperation; }
            set { this.SetProperty(ref this._hoursOfOperation, value); }
        }

        private string _imageUrl;
        public string ImageUrl
        {
            get { return this._imageUrl; }
            set { this.SetProperty(ref this._imageUrl, value); }
        }

        private string _websiteUrl;
        public string WebsiteUrl
        {
            get { return this._websiteUrl; }
            set { this.SetProperty(ref this._websiteUrl, value); }
        }

        private double _latitude;
        public double Latitude
        {
            get { return this._latitude; }
            set { this.SetProperty(ref this._latitude, value); }
        }

        private double _longitude;
        public double Longitude
        {
            get { return this._longitude; }
            set { this.SetProperty(ref this._longitude, value); }
        }

        private bool _isContactUpdating;
        public bool IsContactUpdating
        {
            get { return this._isContactUpdating; }
            set { this.SetProperty(ref this._isContactUpdating, value); }
        }

        private bool _hasBeenAdded;
        public bool HasBeenAdded
        {
            get { return this._hasBeenAdded; }
            set { this.SetProperty(ref this._hasBeenAdded, value); }
        }

    }
}

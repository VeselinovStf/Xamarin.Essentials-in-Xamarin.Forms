﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using RealEstateApp.Models;
using RealEstateApp.ViewModels.Base;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RealEstateApp.ViewModels
{
    public class AddEditPropertyViewModel : ViewModelBase
    {
        public AddEditPropertyViewModel()
        {
            Property = new Property();
            Agents = new ObservableCollection<Agent>(Repository.GetAgents());
        }


        public ObservableCollection<Agent> Agents { get; }

        public ICommand SaveCommand => new Command(SaveAsync);
        public ICommand CancelCommand => new Command(CancelAsync);
        public ICommand GetCurrentAspectCommand => new Command(GetCurrentAspectAsync);
        public ICommand GetLocationCommand => new Command(GetLocationAsync);
        public ICommand GetGeocodeCommand => new Command(GetGeocodAsync);
        public bool IsOnline => Connectivity.NetworkAccess == NetworkAccess.Internet;

        public void CheckBateryStatus()
        {
            if (Battery.ChargeLevel < 0.2)
            {
                StatusMessage = "Battery is low";
                if (Battery.State != BatteryState.Charging)
                {
                    StatusColor = Color.Red;
                }
                else
                {
                    StatusColor = Color.Yellow;
                }
            }
            else
            {
                StatusMessage = null;
            }

            OnPropertyChanged(nameof(StatusMessage));
        }

        public override void OnAppearing()
        {
            Connectivity.ConnectivityChanged += OnConnectivityChanged;

            Battery.BatteryInfoChanged += OnBatteryInfoChange;
            Battery.EnergySaverStatusChanged += OnEnergySaverStatusChanged;
        }

        private void OnEnergySaverStatusChanged(object sender, EnergySaverStatusChangedEventArgs e)
        {
            CheckBateryStatus();
        }

        private void OnBatteryInfoChange(object sender, BatteryInfoChangedEventArgs e)
        {
            CheckBateryStatus();
        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsOnline));
        }

        
        public override void OnDisappearing()
        {
            //UnSubscribe to avoid memory problems
            Connectivity.ConnectivityChanged -= OnConnectivityChanged;
            Battery.BatteryInfoChanged -= OnBatteryInfoChange;
            Battery.EnergySaverStatusChanged -= OnEnergySaverStatusChanged;
        }


        private async void GetGeocodAsync(object obj)
        {
            if (string.IsNullOrWhiteSpace(Property.Address))
            {
                await DialogService.ShowAlertAsync("Please enter an address", "No address");
                return;
            }

            if (IsOnline == false)
            {
                await DialogService.ShowAlertAsync("You must be online to use geocoding", "Ofline");
            }

            var locations = await Geocoding.GetLocationsAsync(Property.Address);
            var location = locations.FirstOrDefault();
            if (location != null)
            {
                Property.Longitude = location.Longitude;
                Property.Latitude = location.Latitude;
            }
        }

        private async void GetLocationAsync(object obj)
        {
            var geolocationRequest =  new GeolocationRequest(GeolocationAccuracy.Best);

            var geolocation = await Geolocation.GetLocationAsync(geolocationRequest);

            Property.Longitude = geolocation.Longitude;
            Property.Latitude = geolocation.Latitude;

            var geocoding = await Geocoding.GetPlacemarksAsync(geolocation);
            var geocodingAddress = geocoding.FirstOrDefault();

            if (geocodingAddress != null)
            {
                Property.Address = $"{geocodingAddress.AdminArea} ," +
                    $"{geocodingAddress.CountryName} ," +
                    $"{geocodingAddress.Locality} ,{geocodingAddress.PostalCode}";
            }
          
            OnPropertyChanged();

        }

        private async void GetCurrentAspectAsync()
        {
            var modal = await NavigationService.NavigateToModalAsync<CompassViewModel>();
            var currentAspect = await modal.GetAspectAsync();
            if (currentAspect != null)
            {
                Property.Aspect = currentAspect;
                OnPropertyChanged(nameof(Property));
            }
        }
        


        private Property _property;

        public Property Property
        {
            get => _property;
            set
            {
                SetProperty(ref _property, value);
                SelectedAgent = Agents?.FirstOrDefault(x => x.Id == _property?.AgentId);
            }
        }

        private Agent _selectedAgent;

        public Agent SelectedAgent
        {
            get => _selectedAgent;
            set
            {
                if (!SetProperty(ref _selectedAgent, value)) return;

                if (Property != null) Property.AgentId = _selectedAgent?.Id;
            }
        }

        private string _statusMessage;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private Color _statusColor = Color.White;

        public Color StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public override async Task InitializeAsync(object navigationData)
        {
            Property = navigationData as Property;

            if (Property == null)
            {
                Title = "Add Property";
                Property = new Property();
            }
            else
            {
                Title = "Edit Property";
            }
        }

        private void SaveAsync()
        {
            if (Save()) NavigationService.PopModalAsync();
        }

        private void CancelAsync()
        {
            Vibration.Cancel();
            NavigationService.PopModalAsync();
        }

        public bool Save()
        {
            if (IsValid() == false)
            {
                StatusMessage = "Please fill in all required fields";
                StatusColor = Color.Red;

                Vibration.Vibrate(5000);

                return false;
            }

            Repository.SaveProperty(Property);

            return true;
        }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Property.Address)
                || Property.Beds == null
                || Property.Price == null
                || Property.AgentId == null)
                return false;

            return true;
        }
    }
}
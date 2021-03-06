﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using RealEstateApp.Models;
using RealEstateApp.Services.Settings;
using RealEstateApp.ViewModels.Base;
using MenuItem = RealEstateApp.Models.MenuItem;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace RealEstateApp.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        static MenuViewModel()
        {
            InitMenuItems();
        }

        public MenuViewModel(ISettingsService settingService)
        {
            LoggedInUser = settingService.LoggedInUser;
        }

        public ICommand ItemSelectedCommand => new Command<MenuItem>(SelectMenuItemAsync);
        public ICommand TurnOnFlashLight => new Command( () => ToggleFlashlight(true));
        public ICommand TurnOffFlashLight => new Command( () => ToggleFlashlight(false));

        private static ObservableCollection<MenuItem> _menuItems = new ObservableCollection<MenuItem>();

        public ObservableCollection<MenuItem> MenuItems
        {
            get => _menuItems;
            set => _menuItems = value;
        }

        private User _loggedInUser;

        public User LoggedInUser
        {
            get => _loggedInUser;
            set => SetProperty(ref _loggedInUser, value);
        }

        public async void ToggleFlashlight(bool on)
        {
            try
            {
                if (on)
                {
                    await Flashlight.TurnOnAsync();
                }
                else
                {
                    await Flashlight.TurnOffAsync();
                }
            }
            catch (Exception ex)
            {

                await DialogService.ShowAlertAsync($"Flashlight not working - {ex.Message}", "Flashlight");
            }
          
        }

        public static bool IsMenuItem(Type viewModelType)
        {
            return _menuItems.Any(x => x.ViewModelType == viewModelType);
        }

        private static void InitMenuItems()
        {
            _menuItems.Add(new MenuItem
            {
                Title = "Properties",
                MenuItemType = MenuItemType.Properties,
                ViewModelType = typeof(PropertyListViewModel),
                IsEnabled = true
            });

            _menuItems.Add(new MenuItem
            {
                Title = "Height Calculator",
                MenuItemType = MenuItemType.CalculateHeight,
                ViewModelType = typeof(HeightCalculatorViewModel),
                IsEnabled = true
            });

            _menuItems.Add(new MenuItem
            {
                Title = "About",
                MenuItemType = MenuItemType.About,
                ViewModelType = typeof(AboutViewModel),
                IsEnabled = true
            });

            _menuItems.Add(new MenuItem
            {
                Title = "Log Out",
                MenuItemType = MenuItemType.LogOut,
                ViewModelType = typeof(LoginViewModel),
                IsEnabled = true,
                NavigationData = "logout"
            });
        }


        private async void SelectMenuItemAsync(MenuItem item)
        {
            if (item.IsEnabled) await NavigationService.NavigateToAsync(item.ViewModelType, item.NavigationData);
        }
    }
}
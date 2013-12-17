using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace googlemaps
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        private IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        public SettingsPage()
        {
            InitializeComponent();
            InitSettings();
        }

        private void InitSettings()
        {
            if (settings.Contains("MapType"))
            {
                switch (settings["MapType"].ToString())
                {
                    case "hybrid":
                        hybridRadioButton.IsChecked = true;
                        break;

                    case "satellite":
                        satelliteRadioButton.IsChecked = true;
                        break;

                    case "street":
                        streetRadioButton.IsChecked = true;
                        break;

                    case "physical":
                        physicalRadioButton.IsChecked = true;
                        break;

                    case "wateroverlay":
                        waterRadioButton.IsChecked = true;
                        break;
                }
            }

            if (settings.Contains("ScaleButton"))
            {
                if ((bool)settings["ScaleButton"])
                    ScaleSwitch.IsChecked = true;
                else
                    ScaleSwitch.IsChecked = false;
            }

            if (settings.Contains("LocationConsent"))
            {
                if ((bool)settings["LocationConsent"])
                    CheckMyLocationSwitch.IsChecked = true;
                else
                    CheckMyLocationSwitch.IsChecked = false;
            }

        }


        private void hybridRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            settings["MapType"] = "hybrid";
        }

        private void satelliteRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            settings["MapType"] = "satellite";
        }

        private void streetRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            settings["MapType"] = "street";
        }

        private void physicalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            settings["MapType"] = "physical";
        }

        private void waterRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            settings["MapType"] = "wateroverlay";
        }

        private void ScaleSwitch_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ScaleSwitch.IsChecked)
                settings["ScaleButton"] = true;
            else
                settings["ScaleButton"] = false;
        }

        private void CheckMyLocationSwitch_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckMyLocationSwitch.IsChecked)
                settings["LocationConsent"] = true;
            else
                settings["LocationConsent"] = false;
        }
    }
}
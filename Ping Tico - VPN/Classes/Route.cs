using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace PingTicoVPN.Classes
{
    public class Route
    {
        public string name { get; set; } //Name of the Route
        public IPAddress ip { get; set; } //IP Address of the Route
        public int port { get; set; } //Port of the Route

        private bool _active;
        public bool active { 
            get => _active; 
            set {
                _active = value;

                Active_Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                Inactive_Visibility = value ? Visibility.Collapsed : Visibility.Visible;

                Item_Color = value ? new SolidColorBrush(Color.FromRgb(255, 255, 255)) : new SolidColorBrush(Color.FromRgb(155, 155, 155));
            } 
        } //Is route active ?

        public Visibility Active_Visibility { get; set; } = Visibility.Collapsed; //If the UI should display this route as active
        public Visibility Inactive_Visibility { get; set; } = Visibility.Visible; //If the UI should display this route as inactive

        public SolidColorBrush Item_Color { get; set; } = new SolidColorBrush(Color.FromRgb(155, 155, 155)); //If the UI should display this route as inactive

        //Sets the current selected status
        public void SetActiveStatus(bool a)
        {
            active = a;
        }

        //Inverts Selection Status
        public void SwitchActiveStatus()
        {
            SetActiveStatus(!active);
        }

    }
}

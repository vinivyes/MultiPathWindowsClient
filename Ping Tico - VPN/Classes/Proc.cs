using PingTicoVPN.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PingTicoVPN.Classes
{
    /// <summary>
    /// Wraps a Process and exposes some of the properties to be used by WPF
    /// </summary>
    public class Proc
    {
       
        public Process process { get; set; } //Holds a Process object

        public string Name { get; set; } //Name of the Process (Main Window Title)
        public int PID { get; set; } //Process PID
        public string ProcessName { get; set; } //Process Name (<PROCESS NAME>.exe)

        private bool _Selected = false;
        private bool Selected { 
            get => _Selected; 
            set {
                _Selected = value;

                Selected_Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                Unselected_Visibility = value ? Visibility.Collapsed : Visibility.Visible;
            }
        } //If this process has been selected in the UI
        public Visibility Selected_Visibility { get; set; } = Visibility.Collapsed; //If the UI should display this process as selected
        public Visibility Unselected_Visibility { get; set; } = Visibility.Visible; //If the UI should display this process as unselected

        public BitmapImage icon { get; set; } = null; //Process Icon

        //To create a Process Wrapper (Proc) a Process Object is required
        public Proc(Process _process)
        {

            //Saves process and extracts Name (Title), PID and Process Name.
            process = _process;
            Name = _process.MainWindowTitle;
            PID = _process.Id;
            ProcessName = _process.ProcessName;

            //Try to get Process Icon
            try
            {
                if (System.IO.File.Exists(_process.MainModule.FileName))
                {
                    icon = Helpers.ToBitmapImage(Icon.ExtractAssociatedIcon(_process.MainModule.FileName).ToBitmap());
                }
            }
            catch { }
        }

        //Sets the current selected status
        public void SetSelected(bool s)
        {
            Selected = s;
        }

        //Inverts Selection Status
        public void SwitchSelected()
        {
            SetSelected(!Selected);
        }
    }
}

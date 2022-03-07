using PingTicoVPN.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PingTicoVPN.Dialogs
{
    /// <summary>
    /// Interaction logic for AddRoute.xaml
    /// </summary>
    public partial class AddRoute : Window
    {

        public Route route = null;

        public AddRoute()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        #region - New Character Input Validation

        private void IPAddress_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = (TextBox)e.Source;
            
            //Determines what is the resulting text
            string oldText = tb.Text;
            oldText = oldText.Remove(tb.SelectionStart, tb.SelectionLength);
            string text = oldText.Insert(tb.CaretIndex, e.Text);

            //Gets the last character and the one previous
            string lastChar = text.Substring(text.Length - 1, 1);
            string beforeLastChar = text.Length >= 2 ? text.Substring(text.Length - 2, 1) : "";

            //Sections of IPv4 separated by dots
            string[] sections = text.Split('.');

            //Regex for number only
            Regex regex = new Regex("^[0-9]+$");
            if (
                (!regex.IsMatch(e.Text) && e.Text != ".") ||  //If not number and not a dot
                (beforeLastChar == "." && lastChar == ".") || //If 2 dots in a row
                (sections.Length > 4) ||                      //If there are more than 4 sections
                (text.Length == 1 && lastChar == ".")         //If first character is a dot
            ) { 

                e.Handled = true;                             //Cancel input
                return;
            }
            foreach (string section in sections)
            {
                if (section.Length > 3) { e.Handled = true; } //If section of IP has more than 3 characters
                if ((String.IsNullOrEmpty(section) ? 0 : int.Parse(section)) > 255) { e.Handled = true; } //If the section to number is higher than 255
            }

        }

        private void Port_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = (TextBox)e.Source;

            //Determines what is the resulting text
            string oldText = tb.Text;
            oldText = oldText.Remove(tb.SelectionStart, tb.SelectionLength);
            string text = oldText.Insert(tb.CaretIndex, e.Text);

            //Regex for number only
            Regex regex = new Regex("^[0-9]+$");
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true;                             //Cancel input
                return;
            }

            if ((String.IsNullOrEmpty(text) ? 0 : int.Parse(text)) > 65535) { e.Handled = true; } //If the section to number is higher than 255
        }

        #endregion - New Character Input Validation

        #region - Application actions such as Paste, Cut, etc...
        private bool isIPAddressPasted = false;
        private string previousToPasteText = "";
        private void IPAddress_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            TextBox tb = (TextBox)e.Source;

            isIPAddressPasted = true;
            previousToPasteText = tb.Text;
        }

        private bool isPortPasted = false;
        private string previousToPasteTextPort = "";
        private void Port_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            TextBox tb = (TextBox)e.Source;

            isPortPasted = true;
            previousToPasteTextPort = tb.Text;
        }
        #endregion - Application actions such as Paste, Cut, etc...

        #region - Make changes to Text Box Validation
        private bool isIPAddressTBBeingHandled = false;
        private void IPAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isIPAddressTBBeingHandled) { return; } //Cancel verification if is being reset by this same function
            if (isIPAddressPasted) //Is there currently a paste action happening
            {
                isIPAddressTBBeingHandled = true; //Enable validation ignore flag

                TextBox tb = (TextBox)e.Source; //Get Text Box Object

                string text = tb.Text; //Load text

                //Sections of IPv4 separated by dots
                string[] sections = text.Split('.');

                Regex regex = new Regex(@"^[\.0-9]*$"); //Only numbers or dots
                
                if (text != tb.Text.Trim())
                {
                    tb.Text = previousToPasteText;
                } 
                else if (sections.Length > 4) 
                { 
                    tb.Text = previousToPasteText; 
                }
                else
                {
                    //Verify if valid IP Address
                    foreach (string section in sections)
                    {
                        if (section.Length > 3) { tb.Text = previousToPasteText; break; } //If section of IP has more than 3 characters
                        if ((String.IsNullOrEmpty(section) && regex.IsMatch(section) ? 0 : int.Parse(section)) > 255) { tb.Text = previousToPasteText; break; } //If the section to number is higher than 255
                    }
                }

                isIPAddressTBBeingHandled = false; //Disable validation ignore flag
            }
            isIPAddressPasted = false;
            previousToPasteText = "";
            ValidateForm();
        }

        private bool isPortTBBeingHandled = false;
        private void Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isPortTBBeingHandled) { return; } //Cancel verification if is being reset by this same function
            if (isPortPasted) //Is there currently a paste action happening
            {
                isPortTBBeingHandled = true; //Enable validation ignore flag

                TextBox tb = (TextBox)e.Source; //Get Text Box Object

                string text = tb.Text; //Load text
                int port = 99999;

                Regex regex = new Regex("^[0-9]+$"); //Only numbers

                if (text != tb.Text.Trim())
                {
                    tb.Text = previousToPasteTextPort;
                }
                else if (!regex.IsMatch(text) && !String.IsNullOrEmpty(text))
                {
                    tb.Text = previousToPasteTextPort;
                }
                else if (!int.TryParse(text, out port) && !String.IsNullOrEmpty(text))
                {
                    tb.Text = previousToPasteTextPort;
                }
                else if (port > 65535) { 
                    tb.Text = previousToPasteTextPort; //If number is higher 65535
                }

                isPortTBBeingHandled = false; //Disable validation ignore flag
            }
            isPortPasted = false;
            previousToPasteTextPort = "";
            ValidateForm();
        }

        #endregion - Make changes to Text Box Validation

        #region - Form Validation

        private void ValidateForm() {
            bool isValid = true;

            Regex regex = new Regex(@"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.|$)){4}\b");

            int port = 99999;
            if (!regex.IsMatch(IPAddressTB.Text)) { 
                isValid = false; 
            }
            if (int.TryParse(PortTB.Text, out port)) {
                if (port > 65535 || port < 0) { 
                    isValid = false; 
                }
            } else { 
                isValid = false; 
            }

            AddBTN.IsEnabled = isValid;
        }

        #endregion - Form Validation

        private void CancelBTN_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddBTN_Click(object sender, RoutedEventArgs e)
        {
            route = new Route(IPAddress.Parse(IPAddressTB.Text), int.Parse(PortTB.Text));
            
            route.name = NameTB.Text;
            route.active = false;

            this.Close();
        }
    }
}

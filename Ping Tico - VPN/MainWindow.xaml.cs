    using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PingTicoVPN.AttachedProperty;
using PingTicoVPN.Classes;
using PingTicoVPN.Dialogs;
using PingTicoVPN.Modules;

public class ViewModel
{
    public ObservableCollection<Interface> InterfaceList { get; set; } = new ObservableCollection<Interface>();
    public ObservableCollection<Proc> ProcessList { get; set; } = new ObservableCollection<Proc>();
    public ObservableCollection<Route> RouteList { get; set; } = new ObservableCollection<Route>();
}

namespace PingTicoVPN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel vm = new ViewModel();

        Task pingRoutes = null;

        public MainWindow()
        {
            InitializeComponent();

            //Load initial data
            LoadProcessList();
            LoadInterfaceList();

            DataContext = vm; //Set context of the Window

            //Testing, quickly load routes
            vm.RouteList.Add(new Route(IPAddress.Parse("127.0.0.1"), 31313) { name = "Test 1", InterfaceList = vm.InterfaceList });
            vm.RouteList.Add(new Route(IPAddress.Parse("127.0.0.1"), 31313) { name = "Test 2", InterfaceList = vm.InterfaceList });

            //Start pinging all routes
            pingRoutes = new Task(PingRoutes);
            pingRoutes.Start();

            //Start communication with Wireguard locally.
            MultiPath.StartReceivingDataFromWireguard(ref vm);

        }

        #region - Interface List 
        //Loads list of current Network Interfaces.
        private void LoadInterfaceList() 
        {
            vm.InterfaceList.Clear(); //Make sure list is currently empty.

            vm.InterfaceList.Add(new Interface() { ifId = 0, ipAddress = IPAddress.Any, name = "Default" }); //Default interface (listen on any)

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            //Loads and populate list of interfaces currently available (support iPv4 only)
            foreach (NetworkInterface adapter in nics)
            {
                try
                {
                    // Only get information for interfaces that support IPv4.
                    if (!adapter.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        continue;
                    }

                    IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                    // Get the IPv4 interface properties.
                    IPv4InterfaceProperties p = adapterProperties.GetIPv4Properties();

                    if (p == null)
                    {
                        continue;
                    }

                    //Create one entry for each valid iPv4 address in the list.
                    foreach (var ip in adapterProperties.UnicastAddresses)
                    {
                        if (ip.Address.ToString().Split('.').Length == 4)
                        {
                            vm.InterfaceList.Add(new Interface() { ifId = p.Index, name = String.Format("{0} - ({1})", adapter.Description, ip.Address.ToString()), ipAddress = ip.Address });
                        }
                    }
                }
                catch(Exception ex) { Log.HandleError(ex); }
               
            }

            //Refresh UI
            RefreshRoutes();
        }
        
        #endregion - Interface List 

        #region - Process List 

        //Loads the list of currently running processes.
        private void LoadProcessList() {
            vm.ProcessList.Clear();    //Makes sure list is currently empty.

            foreach (Process p in Process.GetProcesses()) {
                if (!String.IsNullOrEmpty(p.MainWindowTitle)) //If main window has title...
                {
                    //Add process to process list
                    Proc proc = new Proc(p);
                    vm.ProcessList.Add(proc);
                }
            }
        }

        #endregion - Process List

        #region - Window Events

        //Current window changed state
        private void Window_StateChanged(object sender, EventArgs e)
        {

        }

        //Current window is visible
        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        //Click on a Process in the Process List Box
        private void Process_Click(object sender, RoutedEventArgs e)
        {
            Proc p = (Proc)((Button)sender).DataContext;

            p.SwitchSelected();

            ProcessListBox.Items.Refresh();
        }

        //Clicked button to add a new route
        private void RouteList_Add_Click(object sender, MouseButtonEventArgs e)
        {
            AddRoute dialog = new AddRoute();

            dialog.ShowDialog();

            dialog.route.InterfaceList = vm.InterfaceList;
            if (dialog.route != null)
            {
                vm.RouteList.Add(dialog.route);
            }
        }

        //Clicked button to toggle route state
        private void RouteToggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Route p = (Route)((Image)sender).DataContext;

            p.SwitchActiveStatus();

            RouteListBox.Items.Refresh();
        }
        //Clicked button to delete route
        private void RouteDelete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Route p = (Route)((Image)sender).DataContext;

            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format("Are you sure you want to delete {0}?", p.name), "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes) {
                vm.RouteList.Remove(p);

                RouteListBox.Items.Refresh();
            }
        }

        //For each route, if a new interface is selected. Update route configuration.
        private void RouteInterface_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //Load context
                Route r = (Route)((ComboBox)sender).DataContext;
                Interface i = (Interface)((ComboBox)sender).SelectedItem;

                r.ReconfigureSocket(i); //Reconfigure Socket to use selected Network Interface
            }
            catch (Exception ex)
            {
                Log.HandleError(ex);
            }
        }

        #endregion - Window Events

        #region - Routes
        //Refreshes the UI Displaying routes.
        private void RefreshRoutes()
        {
            RouteListBox.Dispatcher.Invoke(new Action(() => {
                RouteListBox.Items.Refresh();
            }));
        }

        //Pings all the current routes every 5 seconds, refreshes the UI after 1 second.
        private void PingRoutes()
        {
            while (true)
            {
                Thread.Sleep(5000);
                foreach (Route route in vm.RouteList)
                {
                    if(route.active) route.UpdatePing();
                }
                Thread.Sleep(1000);
                RefreshRoutes();
            }
        }

        #endregion - Routes

    }
}

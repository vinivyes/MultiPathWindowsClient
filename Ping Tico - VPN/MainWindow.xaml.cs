using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
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

        public MainWindow()
        {
            InitializeComponent();

            LoadProcessList();
            LoadInterfaceList();

            DataContext = vm;

            vm.RouteList.Add(new Route(IPAddress.Parse("20.112.13.97"), 21213) { name = "Test 1" });
            vm.RouteList.Add(new Route(IPAddress.Parse("20.112.13.97"), 21214) { name = "Test 2" });
            vm.RouteList.Add(new Route(IPAddress.Parse("20.112.13.97"), 21215) { name = "Test 3" });

            MultiPath.StartReceivingDataFromWireguard(ref vm);
        }

        #region - Interface List 

        private void LoadInterfaceList() { 
            
        }
        
        #endregion - Interface List 

        #region - Process List 

        private void LoadProcessList() {
            vm.ProcessList.Clear();

            foreach (Process p in Process.GetProcesses()) {
                if (!String.IsNullOrEmpty(p.MainWindowTitle))
                {
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

        private void RouteList_Add_Click(object sender, MouseButtonEventArgs e)
        {
            AddRoute dialog = new AddRoute();

            dialog.ShowDialog();

            if(dialog.route != null)
            {
                vm.RouteList.Add(dialog.route);
            }
        }

        private void RouteToggle_MouseDown(object sender, MouseButtonEventArgs e)
        {

            Route p = (Route)((Image)sender).DataContext;

            p.SwitchActiveStatus();

            RouteListBox.Items.Refresh();
        }

        private void RouteDelete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Route p = (Route)((Image)sender).DataContext;

            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format("Are you sure you want to delete {0}?", p.name), "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes) {
                vm.RouteList.Remove(p);

                RouteListBox.Items.Refresh();
            } 
        }

        #endregion - Window Events

    }
}

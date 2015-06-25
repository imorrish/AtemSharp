using System.Windows;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;

namespace AtemSharp
{
    public partial class MainWindow : Window
    {
        private CBMDSwitcherDiscovery bSwitcherDiscovery;
        private IBMDSwitcher bSwitcher;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            bSwitcherDiscovery = new CBMDSwitcherDiscovery();
            if(bSwitcherDiscovery == null)
            {
                MessageBox.Show("Could not create Switcher Discovery Instance.\n"
                    + "ATEM Switcher Software may not be installed.", "Error");
                return;
            }

            string deviceAddress = TextBoxSwitcherIPAddress.Text;
            _BMDSwitcherConnectToFailure failReason = 0;

            try
            {
                bSwitcherDiscovery.ConnectTo(deviceAddress, out bSwitcher, out failReason);
            }
            catch(COMException)
            {
                switch(failReason)
                {
                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureNoResponse:
                        MessageBox.Show("No response from Switcher", "Error");
                        break;
                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureIncompatibleFirmware:
                        MessageBox.Show("Switcher has incompatible firmware", "Error");
                        break;
                    default:
                        MessageBox.Show("Connection failed for unknown reason", "Error");
                        break;
                }
                return;
            }

            ButtonConnect.Content = "Connected";
            TextBoxSwitcherIPAddress.IsEnabled = false;

            string bProductName;
            bSwitcher.GetProductName(out bProductName);
            TextBoxProductName.Text = bProductName;
        }
    }
}

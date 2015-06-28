using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;

namespace AtemSharp
{
    public partial class MainWindow : Window
    {
        private void OnSwitcherConnected()
        {
            ButtonConnect.Content = "Connected";
            TextBoxSwitcherIPAddress.IsEnabled = false;

            string productName;
            bSwitcher.GetProductName(out productName);
            TextBoxProductName.Text = productName;

            StackPanelPreview1.IsEnabled = true;
            StackPanelPreview2.IsEnabled = true;
            StackPanelProgram1.IsEnabled = true;
            StackPanelProgram2.IsEnabled = true;

            bSwitcher.AddCallback(bSwitcherCallback);

            GetInputs();
            GetMixEffectBlock();
        }

        private void GetInputs()
        {
            IBMDSwitcherInputIterator inputIterator = null;
            IntPtr inputIteratorPointer;
            Guid inputIteratorInterfaceID = typeof(IBMDSwitcherInputIterator).GUID;

            bSwitcher.CreateIterator(ref inputIteratorInterfaceID, out inputIteratorPointer);
            if (inputIteratorPointer == null)
                return;

            inputIterator = (IBMDSwitcherInputIterator)Marshal.GetObjectForIUnknown(inputIteratorPointer);
            if (inputIterator == null)
                return;

            IBMDSwitcherInput input;
            inputIterator.Next(out input);

            while (input != null)
            {
                long inputId;
                string inputName;

                input.GetInputId(out inputId);
                input.GetString(_BMDSwitcherInputPropertyId.bmdSwitcherInputPropertyIdShortName, out inputName);
                bInputIds.Add(inputName, inputId);

                InputCallback inputCallback = new InputCallback(input);
                inputCallback.LongNameChanged += OnLongNameChanged;
                bInputCallbacks.Add(inputCallback);

                inputIterator.Next(out input);
            }
        }

        private void GetMixEffectBlock()
        {
            bSwitcherMixEffectBlock = null;

            IBMDSwitcherMixEffectBlockIterator mixEffectBlockIterator = null;
            IntPtr mixEffectBlockIteratorPointer;
            Guid mixEffectBlockIteratorInterfaceID = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;

            bSwitcher.CreateIterator(ref mixEffectBlockIteratorInterfaceID, out mixEffectBlockIteratorPointer);
            if (mixEffectBlockIteratorPointer == null)
                return;

            mixEffectBlockIterator =
                (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(mixEffectBlockIteratorPointer);
            if (mixEffectBlockIterator == null)
                return;

            mixEffectBlockIterator.Next(out bSwitcherMixEffectBlock);
            if (bSwitcherMixEffectBlock == null)
                return;

            bSwitcherMixEffectBlock.AddCallback(bMixEffectBlockCallback);
        }

        private void OnSwitcherDisconnected()
        {
            ButtonConnect.Content = "Connect";
            TextBoxSwitcherIPAddress.IsEnabled = true;
            TextBoxProductName.Text = "";

            StackPanelPreview1.IsEnabled = false;
            StackPanelPreview2.IsEnabled = false;
            StackPanelProgram1.IsEnabled = false;
            StackPanelProgram2.IsEnabled = false;
        }

        private void OnPreviewInputChanged()
        {

        }

        private void OnProgramInputChanged()
        {

        }

        private void OnTransitionFramesRemainingChanged()
        {

        }

        private void OnTransitionPositionChanged()
        {

        }

        private void OnInTransitionChanged()
        {

        }

        private void OnLongNameChanged()
        {

        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            bSwitcherDiscovery = new CBMDSwitcherDiscovery();
            if (bSwitcherDiscovery == null)
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
            catch (COMException)
            {
                switch (failReason)
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

            OnSwitcherConnected();
        }

        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            string inputName = ((Button)sender).Tag.ToString();
            long inputId = bInputIds[inputName];

            bSwitcherMixEffectBlock.SetInt
                (_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput, inputId);
        }

        private void ButtonProgram_Click(object sender, RoutedEventArgs e)
        {
            string inputName = ((Button)sender).Tag.ToString();
            long inputId = bInputIds[inputName];

            bSwitcherMixEffectBlock.SetInt
                (_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, inputId);
        }
    }
}

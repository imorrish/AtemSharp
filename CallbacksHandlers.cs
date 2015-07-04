using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;

namespace AtemSharp
{
    public partial class MainWindow : Window
    {
        private void OnSwitcherConnected()
        {
            ButtonConnect.Content = "Connected";
            ButtonConnect.IsEnabled = false;
            TextBoxSwitcherIPAddress.IsEnabled = false;

            string productName;
            bSwitcher.GetProductName(out productName);
            TextBoxProductName.Text = productName;

            GridProgram.IsEnabled = true;
            GridPreview.IsEnabled = true;
            SliderTransition.IsEnabled = true;
            StackPanelTransition.IsEnabled = true;

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

                bInputNamesById.Add(inputId, inputName);
                bInputIdsByName.Add(inputName, inputId);

                InputCallback inputCallback = new InputCallback(input);
                input.AddCallback(inputCallback);
                inputCallback.LongNameChanged +=
                    () => Dispatcher.Invoke((Action)(() => OnLongNameChanged()));

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
            ButtonConnect.IsEnabled = true;
            TextBoxSwitcherIPAddress.IsEnabled = true;
            TextBoxProductName.Text = "";

            GridProgram.IsEnabled = false;
            GridPreview.IsEnabled = false;
            SliderTransition.IsEnabled = false;
            StackPanelTransition.IsEnabled = false;

            foreach (InputCallback inputCallback in bInputCallbacks)
            {
                inputCallback.Input.RemoveCallback(inputCallback);
                inputCallback.LongNameChanged -=
                    () => Dispatcher.Invoke((Action)(() => OnLongNameChanged()));
            }

            bInputNamesById.Clear();
            bInputIdsByName.Clear();
            bInputCallbacks.Clear();

            if (bSwitcherMixEffectBlock != null)
            {
                bSwitcherMixEffectBlock.RemoveCallback(bMixEffectBlockCallback);
                bSwitcherMixEffectBlock = null;
            }

            if (bSwitcher != null)
            {
                bSwitcher.RemoveCallback(bSwitcherCallback);
                bSwitcher = null;
            }
        }

        private void OnProgramInputChanged()
        {
            long programId;
            bSwitcherMixEffectBlock.GetInt
                (_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, out programId);

            foreach (Button button in GridProgram.Children)
            {
                if (button.Background == Brushes.Coral)
                    button.ClearValue(Button.BackgroundProperty);

                if (button.Tag.ToString() == bInputNamesById[programId])
                    button.Background = Brushes.Coral;
            }
        }

        private void OnPreviewInputChanged()
        {
            long previewId;
            bSwitcherMixEffectBlock.GetInt
                (_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput, out previewId);

            SolidColorBrush brush = bInTransition ? Brushes.Coral : Brushes.LightGreen;

            foreach (Button button in GridPreview.Children)
            {
                if (button.Background == brush)
                    button.ClearValue(Button.BackgroundProperty);

                if (button.Tag.ToString() == bInputNamesById[previewId])
                    button.Background = brush;
            }
        }

        private void OnTransitionFramesRemainingChanged()
        {

        }

        private void OnTransitionPositionChanged()
        {
            if (!bMouseDown)
            {
                double position;
                bSwitcherMixEffectBlock.GetFloat
                    (_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdTransitionPosition, out position);

                SliderTransition.Value = position;

                if (position == 1)
                    SliderTransition.IsDirectionReversed = !SliderTransition.IsDirectionReversed;
            }
        }

        private void OnInTransitionChanged()
        {
            int inTransition;
            bSwitcherMixEffectBlock.GetFlag
                (_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdInTransition, out inTransition);

            if (inTransition == 1)
            {
                bInTransition = true;
                ButtonAuto.Background = Brushes.Coral;
            }
            else
            {
                bInTransition = false;
                ButtonAuto.ClearValue(Button.BackgroundProperty);
            }

            OnPreviewInputChanged();
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

        private void ButtonProgram_Click(object sender, RoutedEventArgs e)
        {
            string inputName = ((Button)sender).Tag.ToString();
            long inputId = bInputIdsByName[inputName];

            bSwitcherMixEffectBlock.SetInt
                (_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, inputId);
        }

        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            string inputName = ((Button)sender).Tag.ToString();
            long inputId = bInputIdsByName[inputName];

            bSwitcherMixEffectBlock.SetInt
                (_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput, inputId);
        }

        private void SliderTransition_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bMouseDown = true;
        }

        private void SliderTransition_MouseUp(object sender, MouseButtonEventArgs e)
        {
            bMouseDown = false;
        }

        private void SliderTransition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bMouseDown)
            {
                double position = ((Slider)sender).Value;

                if (position == 1)
                    ((Slider)sender).IsDirectionReversed = !((Slider)sender).IsDirectionReversed;

                bSwitcherMixEffectBlock.SetFloat
                    (_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdTransitionPosition, position);
            }
        }

        private void ButtonAuto_Click(object sender, RoutedEventArgs e)
        {
            bSwitcherMixEffectBlock.PerformAutoTransition();
        }

        private void ButtonCut_Click(object sender, RoutedEventArgs e)
        {
            bSwitcherMixEffectBlock.PerformCut();
        }
    }
}

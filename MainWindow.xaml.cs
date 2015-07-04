using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;

namespace AtemSharp
{
    public partial class MainWindow : Window
    {
        private IBMDSwitcherDiscovery bSwitcherDiscovery;
        private IBMDSwitcher bSwitcher;
        private IBMDSwitcherMixEffectBlock bSwitcherMixEffectBlock;

        private SwitcherCallback bSwitcherCallback;
        private MixEffectBlockCallback bMixEffectBlockCallback;
        private List<InputCallback> bInputCallbacks = new List<InputCallback>();

        private bool bMouseDown = false;

        private Dictionary<long, string> bInputNamesById = new Dictionary<long, string>();
        private Dictionary<string, long> bInputIdsByName = new Dictionary<string, long>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeEventHandlers();
        }

        private void InitializeEventHandlers()
        {
            bSwitcherCallback = new SwitcherCallback();
            bMixEffectBlockCallback = new MixEffectBlockCallback();

            bSwitcherCallback.SwitcherDisconnected +=
                () => Dispatcher.Invoke((Action)(() => OnSwitcherDisconnected()));
            bMixEffectBlockCallback.ProgramInputChanged +=
                () => Dispatcher.Invoke((Action)(() => OnProgramInputChanged()));
            bMixEffectBlockCallback.PreviewInputChanged +=
                () => Dispatcher.Invoke((Action)(() => OnPreviewInputChanged()));
            bMixEffectBlockCallback.TransitionFramesRemainingChanged +=
                () => Dispatcher.Invoke((Action)(() => OnTransitionFramesRemainingChanged()));
            bMixEffectBlockCallback.TransitionPositionChanged +=
                () => Dispatcher.Invoke((Action)(() => OnTransitionPositionChanged()));
            bMixEffectBlockCallback.InTransitionChanged +=
                () => Dispatcher.Invoke((Action)(() => OnInTransitionChanged()));

            SliderTransition.AddHandler
                (Slider.MouseDownEvent, new MouseButtonEventHandler(SliderTransition_MouseDown), true);
            SliderTransition.AddHandler
                (Slider.MouseUpEvent, new MouseButtonEventHandler(SliderTransition_MouseUp), true);
        }
    }
}

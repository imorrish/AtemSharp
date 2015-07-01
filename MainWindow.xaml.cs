using System;
using System.Collections.Generic;
using System.Windows;
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

        private Dictionary<string, long> bInputIds = new Dictionary<string, long>();

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
            bMixEffectBlockCallback.PreviewInputChanged +=
                () => Dispatcher.Invoke((Action)(() => OnPreviewInputChanged()));
            bMixEffectBlockCallback.ProgramInputChanged +=
                () => Dispatcher.Invoke((Action)(() => OnProgramInputChanged()));
            bMixEffectBlockCallback.TransitionFramesRemainingChanged +=
                () => Dispatcher.Invoke((Action)(() => OnTransitionFramesRemainingChanged()));
            bMixEffectBlockCallback.TransitionPositionChanged +=
                () => Dispatcher.Invoke((Action)(() => OnTransitionPositionChanged()));
            bMixEffectBlockCallback.InTransitionChanged +=
                () => Dispatcher.Invoke((Action)(() => OnInTransitionChanged()));
        }
    }
}

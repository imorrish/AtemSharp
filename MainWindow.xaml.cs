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

            bSwitcherCallback.SwitcherDisconnected += OnSwitcherDisconnected;
            bMixEffectBlockCallback.PreviewInputChanged += OnPreviewInputChanged;
            bMixEffectBlockCallback.ProgramInputChanged += OnProgramInputChanged;
            bMixEffectBlockCallback.TransitionFramesRemainingChanged += OnTransitionFramesRemainingChanged;
            bMixEffectBlockCallback.TransitionPositionChanged += OnTransitionPositionChanged;
            bMixEffectBlockCallback.InTransitionChanged += OnInTransitionChanged;
        }
    }
}

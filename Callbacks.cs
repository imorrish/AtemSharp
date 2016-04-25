using System.Windows;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;

namespace AtemSharp
{
    public delegate void SwitcherEventHandler();

    class SwitcherCallback : IBMDSwitcherCallback
    {
        public event SwitcherEventHandler SwitcherDisconnected;

        public void SwitcherMonitor()
        {
        }

        void IBMDSwitcherCallback.Notify(_BMDSwitcherEventType eventType, _BMDSwitcherVideoMode coreVideoMode)
        {
            if (eventType == _BMDSwitcherEventType.bmdSwitcherEventTypeDisconnected)
            {
                if (SwitcherDisconnected != null)
                    SwitcherDisconnected();
            }
        }
    }

    class MixEffectBlockCallback : IBMDSwitcherMixEffectBlockCallback
    {
        public event SwitcherEventHandler ProgramInputChanged;
        public event SwitcherEventHandler PreviewInputChanged;
        public event SwitcherEventHandler TransitionFramesRemainingChanged;
        public event SwitcherEventHandler TransitionPositionChanged;
        public event SwitcherEventHandler InTransitionChanged;

        void IBMDSwitcherMixEffectBlockCallback.PropertyChanged(_BMDSwitcherMixEffectBlockPropertyId propertyId)
        {
            switch (propertyId)
            {
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput:
                    if (ProgramInputChanged != null)
                        ProgramInputChanged();
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput:
                    if (PreviewInputChanged != null)
                        PreviewInputChanged();
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdTransitionFramesRemaining:
                    if (TransitionFramesRemainingChanged != null)
                        TransitionFramesRemainingChanged();
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdTransitionPosition:
                    if (TransitionPositionChanged != null)
                        TransitionPositionChanged();
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdInTransition:
                    if (InTransitionChanged != null)
                        InTransitionChanged();
                    break;
            }
        }
    }

    class InputCallback : IBMDSwitcherInputCallback
    {
        public event SwitcherEventHandler LongNameChanged;

        private IBMDSwitcherInput bInput;
        public IBMDSwitcherInput Input { get { return bInput; } }

        public InputCallback(IBMDSwitcherInput input)
        {
            bInput = input;
        }

        void IBMDSwitcherInputCallback.Notify(_BMDSwitcherInputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeLongNameChanged:
                    if (LongNameChanged != null)
                        LongNameChanged();
                    break;
            }
        }
        //       void IBMDSwitcherInputCallback.PropertyChanged(_BMDSwitcherInputPropertyId propertyId)
        //       {
        //           switch (propertyId)
        //           {
        //               case _BMDSwitcherInputPropertyId.bmdSwitcherInputPropertyIdLongName:
        //                   if (LongNameChanged != null)
        //                       LongNameChanged();
        //                   break;
        //           }
    }
    
}

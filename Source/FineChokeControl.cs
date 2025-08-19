using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Linq;
using UnityEngine;

namespace FineChokeControl
{

    /// <summary>
    /// Mod that enables fine control of the Satsuma choke using the mouse scroll wheel.
    /// </summary>
    public class FineChokeControl : Mod
    {

        /// <summary>
        /// Unique ID of the mod.
        /// </summary>
        public override string ID => "FineChokeControl";
        
        /// <summary>
        /// Display name of the mod.
        /// </summary>
        public override string Name => "Fine Choke Control";
        
        /// <summary>
        /// Mod version.
        /// </summary>
        public override string Version => "1.0.1";
        
        /// <summary>
        /// Author of the mod.
        /// </summary>
        public override string Author => "WilliamIsted";
        
        //public override byte[] Icon => null;
        
        /// <summary>
        /// Description shown in MSCLoader Mod List.
        /// </summary>
        public override string Description => "Fine control over the Satsuma choke via the scroll wheel";

        /*
         * 
         * 
         * 
         */

        private readonly float ChokeMin   =  0.00000f; // Choke fully closed
        private readonly float ChokeMax   =  1.00000f; // Choke fully open
        private readonly float KnobPosMin =  0.00000f; // Knob at Choke = 0
        private readonly float KnobPosMax = -0.03000f; // Lnob at Choke = 1

        private FsmVariables GlobalVars;
        private FsmVariables ChokeUse;
        private FsmVariables ChokeChoke;
        private GameObject   ChokeKnob;
        private FsmString    Subtitle;

        /// <summary>
        /// Sensitivity of the choke adjustment
        /// </summary>
        private SettingsSlider ChokeSensitivitySlider;

        /// <summary>
        /// Invert the scroll wheel
        /// </summary>
        private SettingsCheckBox ScrollWheelInvert;

        /// <summary>
        /// Registers the setup functions for load and update.
        /// </summary>
        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, DoOnLoad);
            SetupFunction(Setup.Update, DoUpdate);
            SetupFunction(Setup.ModSettings, DoModSettings);
        }

        /// <summary>
        /// Called on game load. Initializes FSM references and finds the Choke knob GameObject.
        /// </summary>
        private void DoOnLoad()
        {

            GameObject Choke = GameObject.Find("Choke");

            GlobalVars = PlayMakerGlobals.Instance.Variables;
            ChokeUse   = Choke.GetComponents<PlayMakerFSM>()
                    .FirstOrDefault(fsm => fsm != null && fsm.FsmName == "Use").FsmVariables;
            ChokeChoke = Choke.GetComponents<PlayMakerFSM>()
                    .FirstOrDefault(fsm => fsm != null && fsm.FsmName == "Choke").FsmVariables;

            ChokeKnob  = GameObject.Find("SATSUMA(557kg, 248)/Dashboard/pivot_dashboard/dashboard(Clone)/pivot_meters/dashboard meters(Clone)/Knobs/KnobChoke/knob");

            Subtitle   = GlobalVars.FindFsmString("GUIsubtitle");

        }

        /// <summary>
        /// Called every frame. Checks for scroll wheel input when interacting with the choke
        /// and updates FSM values and knob transform accordingly.
        /// </summary>
        private void DoUpdate()
        {

            bool isDashboard = GlobalVars.FindFsmString("PickedPart").Value.ToUpper() == "DASHBOARD";
            bool isChoke     = GlobalVars.FindFsmString("GUIinteraction").Value.ToUpper() == "CHOKE";

            if (isDashboard && isChoke)
            {

                // Get FSM floats
                FsmFloat chokeLevel = ChokeChoke.FindFsmFloat("ChokeLevel");
                FsmFloat chokeUse = ChokeUse.FindFsmFloat("Choke");
                FsmFloat knobPos = ChokeUse.FindFsmFloat("KnobPos");

                float scroll = Input.GetAxis("Mouse ScrollWheel");

                if (ScrollWheelInvert.GetValue()) scroll *= -1f;

                if (Input.GetMouseButtonDown(2))
                {
                    scroll = (chokeUse.Value < 0.5f) ? +999f : -999f;
                }

                if (Mathf.Abs(scroll) > 0f)
                {

                    // Adjust choke level
                    float chokeStep = 0.05f * ( ChokeSensitivitySlider.GetValue() * 2 );
                    chokeUse.Value += scroll * chokeStep;
                    chokeUse.Value = Mathf.Clamp(chokeUse.Value, 0f, 1f);
                    chokeUse.Value = (float)Math.Round(chokeUse.Value, 3);

                    // Mirror choke level
                    chokeLevel.Value = chokeUse.Value;

                    // Update knobPos from choke
                    knobPos.Value = Mathf.Lerp(0f, -0.03f, chokeUse.Value);
                    knobPos.Value = (float)Math.Round(knobPos.Value, 5);

                    // Update visual knob transform
                    Vector3 pos = ChokeKnob.transform.localPosition;
                    pos.y = Mathf.Clamp(knobPos.Value, -0.03f, 0f);
                    ChokeKnob.transform.localPosition = pos;

                    Subtitle.Value = $"{Mathf.RoundToInt(chokeUse.Value * 100f)}%";

                }

            }

        }

        private void DoModSettings()
        {
            ChokeSensitivitySlider = Settings.AddSlider("ChokeSensitivity", "Choke Sensitivity", 0.1f, 2f, 1f, null, 1);
            ScrollWheelInvert      = Settings.AddCheckBox("ScrollWheelInvert", "Invert Scroll Wheel", false);
        }

    }
}

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
        public override string Version => "0.0.1";
        
        /// <summary>
        /// Author of the mod.
        /// </summary>
        public override string Author => "WilliamIsted";
        
        //public override byte[] Icon => null;
        
        /// <summary>
        /// Description shown in MSCLoader Mod List.
        /// </summary>
        public override string Description => "Fine control over the Satsuma choke via the scroll wheel";

        /// <summary>
        /// Sensitivity of the choke adjustment
        /// </summary>
        private readonly float ChokeSensitivity = 2.0f;

        private FsmVariables globalVars;
        private PlayMakerFSM Choke;
        private PlayMakerFSM ChokePos;
        private GameObject ChokeObject;

        /// <summary>
        /// Registers the setup functions for load and update.
        /// </summary>
        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, DoOnLoad);
            SetupFunction(Setup.Update, DoUpdate);
        }

        /// <summary>
        /// Called on game load. Initializes FSM references and finds the Choke knob GameObject.
        /// </summary>
        private void DoOnLoad()
        {
            
            globalVars = PlayMakerGlobals.Instance.Variables;
            Choke = GameObject.Find("Choke").GetComponents<PlayMakerFSM>()
                .FirstOrDefault(fsm => fsm != null && fsm.FsmName == "Choke");
            ChokePos = GameObject.Find("Choke").GetComponents<PlayMakerFSM>()
                .FirstOrDefault(fsm => fsm != null && fsm.FsmName == "Use");

            ChokeObject = GameObject.Find("SATSUMA(557kg, 248)/Dashboard/pivot_dashboard/dashboard(Clone)/pivot_meters/dashboard meters(Clone)/Knobs/KnobChoke/knob");

        }

        /// <summary>
        /// Called every frame. Checks for scroll wheel input when interacting with the choke
        /// and updates FSM values and knob transform accordingly.
        /// </summary>
        private void DoUpdate()
        {

            string pickedPart = globalVars.FindFsmString("PickedPart").Value.ToUpper();
            string interaction = globalVars.FindFsmString("GUIinteraction").Value.ToUpper();

            if (pickedPart == "DASHBOARD" && interaction == "CHOKE")
            {

                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.00001f)
                {

                    // Get choke related variables
                    var chokeLevel = Choke.FsmVariables.FindFsmFloat("ChokeLevel");
                    var chokePos = ChokePos.FsmVariables.FindFsmFloat("Choke");
                    var knobPos = ChokePos.FsmVariables.FindFsmFloat("KnobPos");

                    // Update choke level
                    chokeLevel.Value += scroll * ChokeSensitivity;
                    chokeLevel.Value = Mathf.Clamp01((float)Math.Round(chokeLevel.Value, 5));

                    // Sync FSM values
                    chokePos.Value = chokeLevel.Value;

                    // KnobPos
                    knobPos.Value = -0.03f * chokeLevel.Value;

                    // Update transform
                    Vector3 pos = ChokeObject.transform.localPosition;
                    float targetY = knobPos.Value;

                    // Smooth knob y-position
                    float smoothSpeed = 10f;
                    pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * smoothSpeed);
                    ChokeObject.transform.localPosition = pos;

                }

            }

        }

    }
}

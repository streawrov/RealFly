using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using RealFly;
using UnityEngine;

[assembly: MelonInfo(typeof(FlyMod), "RealFly", "1.0.0", "streawkceur", "https://github.com/streawrov/RealFly/releases")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace RealFly
{
    public class FlyMod : MelonMod
    {

        private static MelonPreferences_Entry<bool> toggle;
        private static MelonPreferences_Entry<bool> gripControl;
        private static MelonPreferences_Entry<float> vertSlow;
        private static MelonPreferences_Entry<float> horSlow;
        private static MelonPreferences_Entry<float> breakMult;
        private static MelonPreferences_Entry<float> vboost;
        private static MelonPreferences_Entry<float> hboost;
        private static MelonPreferences_Entry<float> viewVertInfluence;

        private bool gotPlayerApi = false;
        private bool contact = true;


        public const string RightTrigger = "Oculus_CrossPlatform_SecondaryIndexTrigger";
        public const string LeftTrigger = "Oculus_CrossPlatform_PrimaryIndexTrigger";

        public const string RightHand = "Oculus_CrossPlatform_SecondaryHandTrigger";
        public const string LeftHand = "Oculus_CrossPlatform_PrimaryHandTrigger";

        private VRC.SDKBase.VRCPlayerApi playerApi;
        private GameObject player;
        private GameObject playerCamera;

        public override void OnApplicationStart()
        {
            MelonPreferences_Category ModSettings = MelonPreferences.CreateCategory("RealFly", "RealFly");

            bool defaultToggle = false;
            float defaultVBoost = 17;
            float defaultHBoost = 25;
            float defaultVSlow = 100;
            float defaultHSlow = 120;
            float defaultVVInfluence = 10;

            toggle = (MelonPreferences_Entry<bool>)ModSettings.CreateEntry("RealFly", defaultToggle, "Real Fly");
            gripControl = (MelonPreferences_Entry<bool>)ModSettings.CreateEntry("gripControl", defaultToggle, "Grip / Index Control");
            viewVertInfluence = (MelonPreferences_Entry<float>)ModSettings.CreateEntry("viewVertInfluence", defaultVVInfluence, "1/x VertView Influence");
            hboost = (MelonPreferences_Entry<float>)ModSettings.CreateEntry("hBoost", defaultHBoost, "Horizontal Boost");
            horSlow = (MelonPreferences_Entry<float>)ModSettings.CreateEntry("hSlow", defaultHSlow, "1/x Horizontal Slow");
            breakMult = (MelonPreferences_Entry<float>)ModSettings.CreateEntry("breakMult", 2f, "Horizontal Brake Mult");
            vboost = (MelonPreferences_Entry<float>)ModSettings.CreateEntry("vBoost", defaultVBoost, "Vertical Boost");
            vertSlow = (MelonPreferences_Entry<float>)ModSettings.CreateEntry("vSlow", defaultVSlow, "1/x Vertical Slow");
        }

        public override void OnApplicationQuit()
        {
            toggle.Value = false;
        }

        public override void OnSceneWasLoaded(int wert, string szene)
        {
            toggle.Value = false;
            gotPlayerApi = false;
        }

        public override void OnUpdate()
        {

            if (toggle.Value && gotPlayerApi && VRChatUtilityKit.Utilities.VRCUtils.AreRiskyFunctionsAllowed)
            {
                float leftPower;
                float rightPower;

                if (gripControl.Value)
                {
                    leftPower = Input.GetAxisRaw(LeftHand);
                    rightPower = Input.GetAxisRaw(RightHand);
                }
                else
                {
                    leftPower = Input.GetAxisRaw(LeftTrigger);
                    rightPower = Input.GetAxisRaw(RightTrigger);
                }
                
                if (Input.GetKey(KeyCode.Space))
                {
                    leftPower = 1f;
                }

                if(Input.GetKey(KeyCode.E))
                {
                    rightPower = 1f;
                }

                if (leftPower != 0 || rightPower != 0)
                {

                    Vector3 playerVec = playerApi.GetVelocity();
                    if (contact && playerVec.y == 0 && leftPower!=0)
                    {
                        player.transform.position += Vector3.up * 0.15f;
                        contact = false;
                    }

                    Vector3 camVec = playerCamera.transform.forward.normalized;

                    Vector3 dirVec = camVec * rightPower * hboost.Value * Time.deltaTime;

                    Vector3 strippedDirVec = new Vector3(dirVec.x,dirVec.y/viewVertInfluence.Value, dirVec.z);

                    Vector3 yVec = new Vector3(0, leftPower * vboost.Value * Time.deltaTime, 0);

                    float rightModifier = 1f;
                    
                    if(rightPower == 0)
                    {
                        rightModifier = breakMult.Value;
                        if (rightModifier < 0.0001f)
                        {
                            rightModifier = 0.0001f;
                        }
                    }

                    Vector3 onlyHorVec = new Vector3(playerVec.x, 0, playerVec.z);
                    Vector3 defaultHorSlow = onlyHorVec * -1 * 1 / (horSlow.Value / rightModifier);


                    Vector3 defaultVertSlow = Vector3.zero;
                    if (leftPower != 0)
                    {
                        Vector3 onlyVertVec = new Vector3(0, playerVec.y, 0);
                        defaultVertSlow = onlyVertVec * -1 * 1 / vertSlow.Value;
                    }

                    playerApi.SetVelocity(playerVec + yVec + strippedDirVec + defaultHorSlow + defaultVertSlow);

                }
                else
                {
                    contact = true;
                }

            }


        }

        public override void OnPreferencesSaved()
        {
            applySettings();
        }

        public override void OnPreferencesLoaded()
        {
            applySettings();
        }
        private void applySettings()
        {
            if (toggle.Value)
            {
                playerApi = VRC.SDKBase.Networking.LocalPlayer;
                player = playerApi.gameObject;
                playerCamera = GameObject.Find("Camera (eye)");
                gotPlayerApi = true;
            }

        }

    }

}
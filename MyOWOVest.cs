﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using MelonLoader;
using OWOHaptic;
//using MyOWOSensations;

namespace MyBhapticsTactsuit
{
    public class TactsuitVR
    {
        /* A class that contains the basic functions for the bhaptics Tactsuit, like:
         * - A Heartbeat function that can be turned on/off
         * - A function to read in and register all .tact patterns in the bHaptics subfolder
         * - A logging hook to output to the Melonloader log
         * - 
         * */
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        // Event to start and stop the heartbeat thread
        public Dictionary<String, ISensation> FeedbackMap = new Dictionary<String, ISensation>();

        public TactsuitVR()
        {
            RegisterAllTactFiles();
            InitializeOWO();
        }

        private async void InitializeOWO()
        {
            LOG("Initializing suit");

            string IPFile = Directory.GetCurrentDirectory() + "\\Mods\\OWO\\IP.txt";
            if(File.Exists(IPFile))
            {
                string IP=File.ReadAllText(IPFile);
                OWO.Connect(IP);
                Thread.Sleep(100);
            }
            else
            {
                await OWO.AutoConnectAsync();
            }

            if (OWO.IsConnected)
            {
                suitDisabled = false;
                LOG("OWO suit connected.");
            }
            if (suitDisabled) LOG("Owo is not enabled?!?!");
        }

        ~TactsuitVR()
        {
            LOG("Destructor called");
            DisconnectOwo();
        }


        public void DisconnectOwo()
        {
            LOG("Disconnecting Owo skin.");
            OWO.Disconnect();
        }

        public void LOG(string logStr)
        {
#pragma warning disable CS0618 // remove warning that the logger is deprecated
            MelonLogger.Msg(logStr);
#pragma warning restore CS0618
        }

        void RegisterAllTactFiles()
        {

            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\OWO";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.owo", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                // LOG("Trying to register: " + prefix + " " + fullName);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    ISensation test = Sensation.FromCode(tactFileStr);
                    //bHaptics.RegisterFeedback(prefix, tactFileStr);
                    LOG("Pattern registered: " + prefix);
                    FeedbackMap.Add(prefix, test);
                }
                catch (Exception e) { LOG(e.ToString()); }

            }

            systemInitialized = true;
        }


        public void PlayBackHit(float xzAngle, float yShift)
        {
            /*
            OWOSensation sensation = OWOSensation.ShotEntry;
            // two parameters can be given to the pattern to move it on the vest:
            // 1. An angle in degrees [0, 360] to turn the pattern to the left
            // 2. A shift [-0.5, 0.5] in y-direction (up and down) to move it up or down
            if ((xzAngle < 90f))
            {
                if (yShift >= 0f) OWO.Send(sensation, OWOMuscle.Pectoral_Right);
                else OWO.Send(sensation, OWOMuscle.Abdominal_Right);
            }
            if ((xzAngle > 90f) && (xzAngle < 180f))
            {
                if (yShift >= 0f) OWO.Send(sensation, OWOMuscle.Dorsal_Right);
                else OWO.Send(sensation, OWOMuscle.Lumbar_Right);
            }
            if ((xzAngle > 180f) && (xzAngle < 270f))
            {
                if (yShift >= 0f) OWO.Send(sensation, OWOMuscle.Dorsal_Left);
                else OWO.Send(sensation, OWOMuscle.Lumbar_Left);
            }
            if ((xzAngle > 270f))
            {
                if (yShift >= 0f) OWO.Send(sensation, OWOMuscle.Pectoral_Right);
                else OWO.Send(sensation, OWOMuscle.Abdominal_Right);
            }
            */
            if ((xzAngle < 180f))
            {
                OWO.Send(FeedbackMap["Hit_Front"]);
            }
            else OWO.Send(FeedbackMap["Hit_Back"]);
        }

        public void Recoil(bool isRightHand, bool isTwoHanded)
        {
            if (isTwoHanded)
            {
                PlayBackFeedback("Recoil_both");
                return;
            }
            if (isRightHand) PlayBackFeedback("Recoil_R");
            else PlayBackFeedback("Recoil_L");
        }

        public void PlayBackFeedback(string feedback)
        {
            if (FeedbackMap.ContainsKey(feedback))
            {
                OWO.Send(FeedbackMap[feedback]);
            }
            else LOG("Feedback not registered: " + feedback);
        }

    }
}

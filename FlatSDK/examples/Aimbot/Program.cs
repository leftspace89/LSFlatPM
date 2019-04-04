using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FlatPMSDK;
using GameOverlay.Drawing;
using GameOverlay.Windows;
using SharpDX;
using Point = GameOverlay.Drawing.Point;
using Color = GameOverlay.Drawing.Color;
using Rectangle = GameOverlay.Drawing.Rectangle;


namespace Aimbot
{
    class Program : Instance
    {
        public override PluginInfo info { get => new PluginInfo() { author = "LeftSpace", name = "Aimbot", version = 1 }; }
        public static string[] aimkeys = { "CAPSLOCK", "LBUTTON", "RBUTTON", "LSHIFT", "V", "E", "Q" };
        private static FlatSDKInternal.Entity LocalPlayer = null;
        [Serializable]
        public static class AimOptions
        {
            public static int bEnabled = 1;
            public static int bDrawFow = 1;
            public static float bFov = 160;
            public static int bSmooth = 1;
            public static int bFovInt = 0;
            public static int bAimFolder = 0;
            public static float[] bFovArray= new float[]{60,90,120,160};
            public static int bPredict = 0;
            public static int bYAxis = 0;
            public static int bAimKeyINT = 0;
            public static System.Windows.Forms.Keys[] bAimKeys = new System.Windows.Forms.Keys[] { System.Windows.Forms.Keys.CapsLock, System.Windows.Forms.Keys.LButton, System.Windows.Forms.Keys.RButton, System.Windows.Forms.Keys.LShiftKey, System.Windows.Forms.Keys.V, System.Windows.Forms.Keys.E, System.Windows.Forms.Keys.Q};
            [NonSerialized]
            public static int bSaveBTN = 0;
            [NonSerialized]
            public static int bLoadBTN = 0;
        }
    
        public class AimTarget
        {
            public Vector2 Screen2D;
            public float CrosshairDistance;
            public int uniqueID;
        }
        private static AimTarget FindAimTargetByUniqueID(AimTarget[]array,int uniqueID)
        {
            var entityList = array;
            for (int i = 0; i < entityList.Length; i++)
            {
                var current = entityList[i];
                if (current == null)
                    continue;

                if (current.uniqueID == uniqueID)
                    return current;
            }
            return null;
        }
        private static FlatSDKInternal.Entity[] GetEntitiesAsArray()
        {
            return new List<FlatSDKInternal.Entity>(FlatSDK.GetEntities()).ToArray(); 
        }

        private static void DrawBlip(GameOverlay.Drawing.Graphics gfx, FlatSDKInternal.Entity go)
        {

        }

        public override async Task Load()
        {
            FlatSDK.DrawGraphics += FlatSDK_DrawGraphics;
            FlatSDK.SetupGraphics += FlatSDK_SetupGraphics;

            var FolderObject = FlatSDK.menuBase.AddFolderElement(ref AimOptions.bAimFolder, "Aimbot Options", FlatSDKInternal.folderonoff);
            FlatSDK.menuBase.AddTextElement(ref AimOptions.bEnabled, "Aimbot", FlatSDKInternal.onoff, FolderObject);
            FlatSDK.menuBase.AddTextElement(ref AimOptions.bDrawFow, "Draw Fov", FlatSDKInternal.onoff, FolderObject);
            FlatSDK.menuBase.AddTextElement(ref AimOptions.bSmooth, "Smooth", FlatSDKInternal.offx2x3x4x5x6, FolderObject);
            FlatSDK.menuBase.AddTextElement(ref AimOptions.bFovInt, "Fov", FlatSDKInternal.aimFov, FolderObject);
            FlatSDK.menuBase.AddTextElement(ref AimOptions.bPredict, "Predict", FlatSDKInternal.offx2x3x4x5x6, FolderObject);
            FlatSDK.menuBase.AddTextElement(ref AimOptions.bYAxis, "Y Axis", FlatSDKInternal.offx2x3x4x5x6, FolderObject);
            FlatSDK.menuBase.AddTextElement(ref AimOptions.bAimKeyINT, "Aim Key", aimkeys, FolderObject);
            FlatSDK.menuBase.AddTextElement(ref AimOptions.bSaveBTN, "Save", FlatSDKInternal.savebtn, FolderObject);
            FlatSDK.menuBase.AddTextElement(ref AimOptions.bLoadBTN, "Load", FlatSDKInternal.loadbtn, FolderObject);


            SerializeStatic.Deserialize(typeof(AimOptions), "AimCFG.xml");
         
        }

        private void FlatSDK_SetupGraphics(object sender, SetupGraphicsEventArgs e)
        {
            var gfx = e.Graphics;
        }
        //Stackoverflow
        private static bool isInside(float circle_x, float circle_y,
                   float rad, float x, float y)
        {
            // Compare radius of circle with distance  
            // of its center from given point 
            if ((x - circle_x) * (x - circle_x) +
                (y - circle_y) * (y - circle_y) <= rad * rad)
                return true;
            else
                return false;
        }


        //uc port
        private static void AimAtPosV2(float x, float y,bool smooth)
        {
            int ScreenCenterX = FlatSDK.Overlay.Width / 2, ScreenCenterY = FlatSDK.Overlay.Height / 2;

            float AimSpeed = (float)AimOptions.bSmooth;
            float TargetX = 0;
            float TargetY = 0;

            //X Axis
            if (x != 0)
            {
                if (x > ScreenCenterX)
                {
                    TargetX = -(ScreenCenterX - x);
                    TargetX /= AimSpeed;
                    if (TargetX + ScreenCenterX > ScreenCenterX * 2) TargetX = 0;
                }

                if (x < ScreenCenterX)
                {
                    TargetX = x - ScreenCenterX;
                    TargetX /= AimSpeed;
                    if (TargetX + ScreenCenterX < 0) TargetX = 0;
                }
            }

            //Y Axis

            if (y != 0)
            {
                if (y > ScreenCenterY)
                {
                    TargetY = -(ScreenCenterY - y);
                    TargetY /= AimSpeed;
                    if (TargetY + ScreenCenterY > ScreenCenterY * 2) TargetY = 0;
                }

                if (y < ScreenCenterY)
                {
                    TargetY = y - ScreenCenterY;
                    TargetY /= AimSpeed;
                    if (TargetY + ScreenCenterY < 0) TargetY = 0;
                }
            }

            if (!smooth)
            {
                FlatSDKInternal.mouse_event(1, (int)TargetX, (int)(TargetY), 0, UIntPtr.Zero);
                return;
            }

            TargetX /= 10;
            TargetY /= 10;

            if (Math.Abs(TargetX) < 1)
            {
                if (TargetX > 0)
                {
                    TargetX = 1;
                }
                if (TargetX < 0)
                {
                    TargetX = -1;
                }
            }
            if (Math.Abs(TargetY) < 1)
            {
                if (TargetY > 0)
                {
                    TargetY = 1;
                }
                if (TargetY < 0)
                {
                    TargetY = -1;
                }
            }
            FlatSDKInternal.mouse_event(1, (int)TargetX, (int)(TargetY), 0, UIntPtr.Zero);
        }


        private static int BestTargetUniqID = -1;
        private static void FlatSDK_DrawGraphics(object sender, DrawGraphicsEventArgs e)
        {
            var gfx = e.Graphics;
        
            if(AimOptions.bSaveBTN == 1)
            {
                SerializeStatic.Serialize(typeof(AimOptions), "AimCFG.xml");
                AimOptions.bSaveBTN = 0;
            }
            if (AimOptions.bLoadBTN == 1)
            {
                SerializeStatic.Deserialize(typeof(AimOptions), "AimCFG.xml");
                AimOptions.bLoadBTN = 0;
            }

            LocalPlayer = FlatSDK.GetLocalPlayer();

            if (LocalPlayer == null)
                return;
            if (LocalPlayer.extra == null)
                return;

            var entList = GetEntitiesAsArray();

            var AimTargets = new AimTarget[entList.Length];

            float fClosestDist = -1;
            
            float ScreenCenterX = FlatSDK.Overlay.Width / 2.0f;
            float ScreenCenterY = FlatSDK.Overlay.Height / 2.0f;


            if (AimOptions.bDrawFow == 1)
               gfx.DrawCircle(FlatSDKInternal.IRenderer._opakwhite, ScreenCenterX, ScreenCenterY, AimOptions.bFovArray[AimOptions.bFovInt], 2);
            
            

            for (int i = 0; i < entList.Length; i++)
            {
                var current = entList[i];
                if (current.type != 82)
                    continue;

 
                var clampPos = current.HeadBone.position - current.position;


                bool w2sHead = FlatSDK.WorldToScreen(FlatSDK.Overlay.Width, FlatSDK.Overlay.Height, new Vector3(current.HeadBone.position.X, current.HeadBone.position.Y - (AimOptions.bPredict*2), current.HeadBone.position.Z  - (AimOptions.bYAxis * 8)), out Vector2 HeadPosition);
                AimTargets[i] = new AimTarget();
                AimTargets[i].Screen2D = HeadPosition;
                AimTargets[i].uniqueID = current.uniqueID;
                AimTargets[i].CrosshairDistance = Vector2.Distance(HeadPosition, new Vector2(ScreenCenterX, ScreenCenterY));

                

                // isInFov
                if(BestTargetUniqID == -1)
                if(isInside(ScreenCenterX, ScreenCenterY,AimOptions.bFovArray[AimOptions.bFovInt], AimTargets[i].Screen2D.X, AimTargets[i].Screen2D.Y))
                {
                    fClosestDist = AimTargets[i].CrosshairDistance;
                    BestTargetUniqID = AimTargets[i].uniqueID;
                }
   
                //if (BestTargetUniqID == -1)
                //{
                //    if (w2sHead)
                //    {
                //        gfx.FillCircle(FlatSDKInternal.IRenderer._opakwhite, HeadPosition.X, HeadPosition.Y, 5);
                //    }
                //}
                //else
                //{
                //    if (w2sHead)
                //    {
                //        gfx.FillCircle(FlatSDKInternal.IRenderer._red, HeadPosition.X, HeadPosition.Y, 5);
                //    }
                //}

            }


            if (FlatSDK.IsKeyPushedDown(AimOptions.bAimKeys[AimOptions.bAimKeyINT]))
            {
                if (BestTargetUniqID != -1)
                {
                    var best = FindAimTargetByUniqueID(AimTargets, BestTargetUniqID);

                    if (best != null)
                    {
                        // kek

                        {
                            var roundPos = new Vector2((float)Math.Round(best.Screen2D.X), (float)Math.Round(best.Screen2D.Y));
                            AimAtPosV2(roundPos.X, roundPos.Y, false);

                        }

                    }

                }
            }
            else
            {
                BestTargetUniqID = -1;
            }


        }
    }
}

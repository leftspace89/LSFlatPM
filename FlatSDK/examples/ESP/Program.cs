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


namespace ESP
{
    class Program : Instance
    {
        public override PluginInfo info { get => new PluginInfo() { author = "LeftSpace", name = "ESP", version = 10001 }; }
        private static FlatSDKInternal.Entity LocalPlayer = null;
        public static class ESPOptions
        {
            public static int bShowHealth = 1;
            public static int bShowState  = 1;
            public static int bShowDistance = 1;
            public static int bSnapLines = 1;
            public static int bESPFolder = 0;
            public static int bShowBox = 1;


            [NonSerialized]
            public static int bSaveBTN = 0;
            [NonSerialized]
            public static int bLoadBTN = 0;
        }
    
        private static void DrawBlip(GameOverlay.Drawing.Graphics gfx, FlatSDKInternal.Entity go)
        {

            FlatSDK.WorldToScreen(FlatSDK.Overlay.Width, FlatSDK.Overlay.Height, go.extra.FootPos, out Vector2 MaxOutput);
            bool w2s = FlatSDK.WorldToScreen(FlatSDK.Overlay.Width, FlatSDK.Overlay.Height, go.position, out Vector2 MinOutput);
            var distance = (go.position - LocalPlayer.position).Length() / 100;
            var fark2 = (go.extra.FootPos - go.position).Length() / 100;
            var fark = MinOutput.Y - MaxOutput.Y;
            string state = string.Empty;
            var FontSize = Math.Max(10, Math.Min(fark, 16.0f));
            string HealthDSTR = ESPOptions.bShowHealth == 1 ? Math.Round(go.extra.health) + "hp" : string.Empty;
            string DistanceDSTR = ESPOptions.bShowDistance == 1? Math.Round(distance) + "mt" : string.Empty;
            if (ESPOptions.bShowState == 1)
            {
                if (fark2 > 3)
                {
                    state = "| Driving |" + Environment.NewLine;
                }
                else if (fark2 > 0.88f && fark2 < 3)
                {
                    state = "| Running |" + Environment.NewLine;
                }

                else if (fark2 < 0.5f && fark2 < 3)
                {
                    state = "| Snake |" + Environment.NewLine;
                }
                else if (fark2 == 0.6f)
                {
                    state = "| Crouch |" + Environment.NewLine;
                }
                else if (fark2 == 0.88f)
                {
                    state = "| Stand |" + Environment.NewLine;
                }
            }
         

            if(w2s)
            {

                var topx = FlatSDK.Overlay.Width / 2;
                var topy = 0;

                if (ESPOptions.bSnapLines == 1)
                {
                    if (fark2 > 3)
                        gfx.DrawLine(FlatSDKInternal.IRenderer._opakwhite, topx, topy, MinOutput.X, MinOutput.Y, 1);
                    else
                        gfx.DrawLine(FlatSDKInternal.IRenderer._opakwhite, topx, topy, MinOutput.X, MinOutput.Y - fark * 2.2f, 1);
                }

                // 2DBOX
                if(ESPOptions.bShowBox == 1)
                {
                    if (fark2 > 3)
                    {
                        gfx.OutlineRectangle(FlatSDKInternal.IRenderer._black, FlatSDKInternal.IRenderer._blue, Rectangle.Create(MinOutput.X, MinOutput.Y, 5, 5), 2);
                    }
                    else
                    {
                        gfx.OutlineRectangle(FlatSDKInternal.IRenderer._black, FlatSDKInternal.IRenderer._blue, Rectangle.Create(MinOutput.X - (fark / 2), MinOutput.Y - fark * 2.2f, fark, fark * 2.2f), 2);
                    }
                }
              

                // TEXT
                if (ESPOptions.bShowState == 1 || ESPOptions.bShowDistance == 1 || ESPOptions.bShowHealth == 1)
                    gfx.DrawText(FlatSDKInternal.IRenderer._font, FontSize, FlatSDKInternal.IRenderer._white, MinOutput.X - FontSize - 10, MinOutput.Y, $"{DistanceDSTR} {HealthDSTR} {Environment.NewLine + state} ");

            }
           

        }

        public override async Task Load()
        {
            FlatSDK.DrawGraphics += FlatSDK_DrawGraphics;
            FlatSDK.SetupGraphics += FlatSDK_SetupGraphics;

            var ESPFolderObject = FlatSDK.menuBase.AddFolderElement(ref ESPOptions.bESPFolder, "ESP Options", FlatSDKInternal.folderonoff);

            FlatSDK.menuBase.AddTextElement(ref ESPOptions.bSnapLines, "SnapLines ESP ", FlatSDKInternal.onoff,ESPFolderObject);
            FlatSDK.menuBase.AddTextElement(ref ESPOptions.bShowHealth, "Health ESP", FlatSDKInternal.onoff, ESPFolderObject);
            FlatSDK.menuBase.AddTextElement(ref ESPOptions.bShowBox,"Box ESP", FlatSDKInternal.onoff,ESPFolderObject);
            FlatSDK.menuBase.AddTextElement(ref ESPOptions.bShowDistance, "Distance ESP", FlatSDKInternal.onoff, ESPFolderObject);
            FlatSDK.menuBase.AddTextElement(ref ESPOptions.bShowState, "State ESP", FlatSDKInternal.onoff, ESPFolderObject);

            FlatSDK.menuBase.AddTextElement(ref ESPOptions.bSaveBTN, "Save", FlatSDKInternal.savebtn, ESPFolderObject);
            FlatSDK.menuBase.AddTextElement(ref ESPOptions.bLoadBTN, "Load", FlatSDKInternal.loadbtn, ESPFolderObject);


            SerializeStatic.Deserialize(typeof(ESPOptions), "ESPCFG.xml");
        }

        private void FlatSDK_SetupGraphics(object sender, SetupGraphicsEventArgs e)
        {
            var gfx = e.Graphics;
        }


        private static void FlatSDK_DrawGraphics(object sender, DrawGraphicsEventArgs e)
        {
            var gfx = e.Graphics;

            if (ESPOptions.bSaveBTN == 1)
            {
                SerializeStatic.Serialize(typeof(ESPOptions), "ESPCFG.xml");
                ESPOptions.bSaveBTN = 0;
            }
            if (ESPOptions.bLoadBTN == 1)
            {
                SerializeStatic.Deserialize(typeof(ESPOptions), "ESPCFG.xml");
                ESPOptions.bLoadBTN = 0;
            }


            LocalPlayer = FlatSDK.GetLocalPlayer();

            if (LocalPlayer == null)
                return;
            if (LocalPlayer.extra == null)
                return;

            foreach (var ent in FlatSDK.GetEntities())
            {
                if (ent.type != 82)
                    continue;

                DrawBlip(gfx, ent);

            }
        }
    }
}

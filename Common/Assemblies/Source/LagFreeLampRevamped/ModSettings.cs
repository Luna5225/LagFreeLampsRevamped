//inspired by https://gist.github.com/erdelf/84dce0c0a1f00b5836a9d729f845298a
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace LagFreeLampsRevamped
{
    public class ExampleSettings : ModSettings
    {
        /// <summary>
        /// The three settings our mod has.
        /// </summary>
        public bool Torches;
        public bool Braziers;
        public bool Dark_Torches;
        public bool Dark_Torches_Fungal;

        /// <summary>
        /// The part that writes our settings to file. Note that saving is by ref.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref Torches, "Include_Torches");
            Scribe_Values.Look(ref Braziers, "Include_Braziers");
            Scribe_Values.Look(ref Dark_Torches, "Include_Dark_Torches");
            Scribe_Values.Look(ref Dark_Torches_Fungal, "Include_Dark_Torches_Fungal");
            base.ExposeData();
        }
    }

    public class LagFreeLampsRevamped : Mod
    {
        /// <summary>
        /// A reference to our settings.
        /// </summary>
        ExampleSettings settings;

        /// <summary>
        /// A mandatory constructor which resolves the reference to our settings.
        /// </summary>
        /// <param name="content"></param>
        public LagFreeLampsRevamped(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<ExampleSettings>();
        }

        /// <summary>
        /// The (optional) GUI part to set your settings.
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Include Torches?", ref settings.Torches);
            listingStandard.CheckboxLabeled("Include Braziers?", ref settings.Braziers);
            listingStandard.CheckboxLabeled("Include Dark Torches?", ref settings.Dark_Torches);
            listingStandard.CheckboxLabeled("Include Dark Torches (Fungal)?", ref settings.Dark_Torches_Fungal);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings.
        /// Using .Translate() is optional, but does allow for localisation.
        /// </summary>
        /// <returns>The (translated) mod name.</returns>
        public override string SettingsCategory()
        {
            return "Lag Free Lamps Revamped";
        }
    }
}
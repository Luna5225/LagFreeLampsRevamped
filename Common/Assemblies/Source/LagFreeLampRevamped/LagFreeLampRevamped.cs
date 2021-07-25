using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LagFreeLampRevamped
{
    public class PatchDisablerMod : Mod
    {
        public static DisablePatch_Settings Settings;

        // create a list of all the patches we want to make controllable.
        public static List<PatchDescription> Patches = new List<PatchDescription>
    {
        new PatchDescription("PatchOnePointThree.xml", "Torches", "Are Torches Exempt to Fuel?"),
        new PatchDescription("campfire.xml", "Campfires", "Are Campfires Exempt to Fuel?"),
        new PatchDescription("cooler.xml", "Passive Coolers", "Are Passive Coolers Exempt to Fuel?"),
        new PatchDescription("chemfuelPoweredGenerator.xml", "Chemfuel-Powered Generators", "Are Chemfuel-Powered Generators Exempt to Fuel?"),
        new PatchDescription("woodFiredGenerator.xml", "Wood-Fired Generators", "Are Wood-Fired Generators Exempt to Fuel?"),
        new PatchDescription("braziers.xml", "Braziers", "Are Braziers Exempt to Fuel?"),
        new PatchDescription("darktorch.xml", "Dark Torches", "Are Dark Torches Exempt to Fuel?"),
        new PatchDescription("darktorchfungus.xml", "Fungal Dark Torches", "Are Fungal Dark Torches Exempt to Fuel?"),
        new PatchDescription("VG_Campfire.xml", "Bamboo Campfires", "Are Bamboo Campfires Exempt to Fuel?"),
        new PatchDescription("VG_torchLamp.xml", "Bamboo Torches", "Are Bamboo Torches Exempt to Fuel?"),
        };


        public PatchDisablerMod(ModContentPack content) : base(content)
        {
            // make the settings available for other code. 
            // Note that GetSettings also loads any previously set settings so that we don't have to.
            Settings = GetSettings<DisablePatch_Settings>();

            // this does several things;
            // 1: getting Patches causes the game to load the files.
            // 2: content.Patches gets passed to us as an IEnumerable<PatchOperation> (which is essentially read-only),
            //  but it is actually a List<PatchOperation> (which we can alter), so we cast it back. Note that this works because
            // List<T> implements IEnumerable<T>, and we know the underlying type. We can't just do this to things that aren't 
            // lists.
            var allPatches = content.Patches as List<PatchOperation>;
            foreach (var patch in Patches)
            {
                if (Settings.PatchDisabled[patch] == false)
                {
                    // find our target patch, null if not found.
                    // note that `sourceFile` is the FULL file path, so we only check that it ends in the correct file name.
                    // make sure that there aren't any false positives here, include some of the folder structure if you need.
                    // and finally, actually remove the patch.
                    allPatches.RemoveAll(p => p.sourceFile.EndsWith(patch.file));
                }
            }
            // profit!
        }

        // the game expects a render function for the settings here, forwarding it to settings (this is personal preference)
        public override void DoSettingsWindowContents(Rect canvas)
        {
            Settings.DoWindowContents(canvas);
        }

        // title of the settings tab for our mod
        public override string SettingsCategory()
        {
            return "Lag Free Lamp Revamped";
        }
    }

    // define a simple struct as a data container for our patches.
    public struct PatchDescription
    {
        public string file;
        public string label;
        public string tooltip;

        public PatchDescription(string file, string label, string tooltip = null)
        {
            this.file = file;
            this.label = label;
            this.tooltip = tooltip;
        }
    }

    public class DisablePatch_Settings : ModSettings
    {
        // expose data is the games way of saving/loading data.
        private Dictionary<string, bool> _scribeHelper;

        // keep a dictionary of patches and a enabled/disabled bool
        // we initialize all patches as not disabled, we'll load their
        // values in ExposeData.
        public Dictionary<PatchDescription, bool> PatchDisabled = PatchDisablerMod.Patches.ToDictionary(p => p, p => true);
        private Vector2 scrollPosition;
        private Rect viewRect;

        // I like putting the 'render function' for settings in the settings class, vanilla would put it in the mod class. 
        public void DoWindowContents(Rect canvas)
        {
            // listing_standard is the simplest way to make an interface. It's not pretty, but it works.
            // I use it for most of my settings, as people shouldn't be spending much time there anyway.
            var options = new Listing_Standard();

            // tells the listing where it can render
            options.Begin(canvas);
            options.Label("Game has to be restarted in order for the changes to be applied!");
            options.Label("");
            options.Label("Choose what should be enabled:");

            // for each patch in the list of patches, render a checkbox.
            // this is one of the things that is super easy to do in options.
            // NOTE: if you have a lot of patches, you may want to try out 
            // options.BeginScrollView
            foreach (var patch in PatchDisablerMod.Patches)
            {
                // we can't use ref on a dictionary value, so pull it out for a sec.
                var status = PatchDisabled[patch];
                options.CheckboxLabeled(patch.label, ref status, patch.tooltip);

                PatchDisabled[patch] = status;
            }
            

            // see also other functions on `options`, for textboxes, radio buttons, etc.
            options.End();
        }

        public override void ExposeData()
        {
            // store any values in the base ModSettings class (there aren't any, but still good practice).
            base.ExposeData();

            // save/load now becomes a bit more complicated, as we need to associate each of the patches with 
            // a specific value, while dealing with updates and such.
            // the 'proper' way to do this would be to use ILoadReferencable, but that is WAY overkill for this 
            // scenario.

            // we're going to store the filename, because that's a relatively unique identifier.
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                // create the data structure we're going to save.
                _scribeHelper = PatchDisabled.ToDictionary(
                    // delegate to transform a dict item into a key, we want the file property of the old key. ( PatchDescription => string )
                    k => k.Key.file,

                    // same for the value, which is just the value. ( bool => bool )
                    v => v.Value);
            }

            // and store it. Notice that both the keys and values of our collection are strings, so simple values.
            // Note that we want this step to take place in all scribe stages, so it's not in a Scribe.mode == xxx block.
            Scribe_Collections.Look(ref _scribeHelper, "patches", LookMode.Value, LookMode.Value);

            // finally, when the scribe finishes, we need to transform this back to a data structure we understand.
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // for each stored patch, update the value in our dictionary.
                foreach (var storedPatch in _scribeHelper)
                {
                    var index = PatchDisablerMod.Patches.FindIndex(p => p.file == storedPatch.Key);
                    if (index >= 0) // match found
                    {
                        var patch = PatchDisablerMod.Patches[index];
                        PatchDisabled[patch] = storedPatch.Value;
                    }
                }
            }
        }
    }
}

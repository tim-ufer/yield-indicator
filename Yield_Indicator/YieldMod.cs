using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace Yield_Indicator
{
    /// <summary>The mod entry point.</summary>
    internal sealed class YieldMod : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            //helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            //helper.Events.Player.Warped += this.drawOnWarp;
            //helper.Events.GameLoop.TimeChanged += this.CheckLocationsOnEvent;
            helper.Events.Player.Warped += this.CheckLocationsOnEvent;
        }


        /*********
        Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);
        }

        private void CheckLocationsOnEvent(object? sender, WarpedEventArgs e)
        {
            CheckAllLocations();
        }

        private void CheckLocationsOnEvent(object? sender, TimeChangedEventArgs e)
        {
            CheckAllLocations();
        }

        private void CheckAllLocations()
        {
            var locationContents = new Dictionary<string, Dictionary<string, int>>() { };

            if (Context.IsMainPlayer)
            {
                Utility.ForEachLocation((GameLocation loc) =>
                {
                    if (!loc.IsOutdoors || loc.IsFarm)
                    {
                        var locDict = CheckLocation(loc);
                        if (locDict.Count > 0)
                        {
                            locationContents[loc.NameOrUniqueName] = locDict;
                        }
                    }
                    return true;
                });
            }
            else
            {
                foreach (GameLocation loc in this.Helper.Multiplayer.GetActiveLocations())
                {
                    if (!loc.IsOutdoors || loc.IsFarm)
                    {
                        var locDict = CheckLocation(loc);
                        if (locDict.Count > 0)
                        {
                            locationContents[loc.NameOrUniqueName] = locDict;
                        }
                    }
                }
            }
            PrintState(locationContents);
        }

        private void PrintState(Dictionary<string, Dictionary<string, int>> globalState)
        {
            foreach (var loc in globalState)
            {
                this.Monitor.Log("\n", LogLevel.Debug);
                this.Monitor.Log($"List of ready entities in {loc.Key}:", LogLevel.Info);
                foreach (var item in loc.Value)
                {
                    this.Monitor.Log($"{item.Key} : {item.Value}", LogLevel.Info);
                }
            }
        }

        private Dictionary<string, int> CheckLocation(GameLocation loc)
        {
            var readyEntities = new Dictionary<string, int>() { };
            //this.Monitor.Log($"{loc.Name} contains {loc.Objects.Count()} objects and {loc.terrainFeatures.Count()} terrain features.", LogLevel.Debug);
            
            CheckMachines(loc, readyEntities);
            if (loc.isGreenhouse.Value)
            {
                CheckPlants(loc, readyEntities);
            }
            return readyEntities;
        }

        private void CheckMachines(GameLocation loc, Dictionary<string, int> dict)
        {
            foreach (var pair in loc.Objects.Pairs)
            {
                if (pair.Value is IndoorPot pot && pot.hoeDirt.Value.readyForHarvest())
                {
                    //this.Monitor.Log($"----------------{loc.Name} {pot.hoeDirt.Value.crop.indexOfHarvest}---------------", LogLevel.Debug);
                    var cropName = new StardewValley.Object(pot.hoeDirt.Value.crop.indexOfHarvest.Value, 0).Name;
                    //this.Monitor.Log($"{cropName}", LogLevel.Debug);
                    
                    if (dict.ContainsKey(cropName))
                    {
                        dict[cropName] += 1;
                    }
                    else
                    {
                        dict.Add(cropName, 1);
                    }
                }
                var name = pair.Value.name;
                if (pair.Value.IsConsideredReadyMachineForComputer())
                {
                    if (dict.ContainsKey(name))
                    {
                        dict[name] += 1;
                    }
                    else
                    {
                        dict.Add(name, 1);
                    }
                }
            }
        }

        private void CheckPlants(GameLocation loc, Dictionary<string, int> dict)
        {
            foreach (var terrain in loc.terrainFeatures.Pairs)
            {
                if (terrain.Value is HoeDirt dirt && dirt.readyForHarvest())
                {
                    //this.Monitor.Log($"{terrain.Value.GetType().Name} : Harvst_index={dirt.crop.indexOfHarvest.Value}", LogLevel.Debug);
                    var cropName = new StardewValley.Object(dirt.crop.indexOfHarvest.Value, 0).Name;
                    //this.Monitor.Log($"{cropName}", LogLevel.Debug);
                    
                    if (dict.ContainsKey(cropName))
                    {
                        dict[cropName] += 1;
                    }
                    else
                    {
                        dict.Add(cropName, 1);
                    }
                    
                }
                
                if (terrain.Value is FruitTree tree && tree.fruit.Count == FruitTree.maxFruitsOnTrees)
                {
                    //this.Monitor.Log($"{terrain.Value.GetType().Name} : Fruit_index={tree.indexOfFruit.Value}, Fruits={tree.fruitsOnTree.Value}", LogLevel.Debug);
                    var fruitName = tree.fruit.Name;
                    //this.Monitor.Log($"{fruitName}", LogLevel.Debug);

                    if (dict.ContainsKey(fruitName))
                    {
                        dict[fruitName] += 1;
                    }
                    else
                    {
                        dict.Add(fruitName, 1);
                    }
                }
            }
        }

        private void drawOnWarp(object sender, WarpedEventArgs e)
        {
            //TODO
            this.Monitor.Log($"{Game1.player.Name} is now in {e.NewLocation}.", LogLevel.Debug);
            /*if (kvp.Key == "Keg")
                {
                    loc.GetLocationContext()
                    Vector2 scaleFactor = Vector2.Zero;
                    scaleFactor *= 4f;
                    Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
                    Rectangle destination = new Rectangle((int)(position.X + 32f - 8f - scaleFactor.X / 2f) + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y + 64f + 8f - scaleFactor.Y / 2f) + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(16f + scaleFactor.X), (int)(16f + scaleFactor.Y / 2f));
                    spriteBatch.Draw(Game1.mouseCursors, destination, ((int)base.heldObject.Value.quality < 4) ? new Rectangle(338 + ((int)base.heldObject.Value.quality - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8), Color.White * 0.95f, 0f, Vector2.Zero, SpriteEffects.None, (float)((y + 1) * 64) / 10000f);
                }
                    
                    
            if (loc is BuildableGameLocation bl)
            {                
                this.Monitor.Log($"\n{bl.name}", LogLevel.Debug);
                foreach (var pair in bl.buildings)
                {
                    //this.Monitor.Log($"{pair.Key} : {pair.Value}", LogLevel.Debug);
                    //this.Monitor.Log($"{pair} {pair.nameOfIndoors} ({pair.tileX},{pair.tileY})", LogLevel.Debug);
                }
            }
            if (loc is Beach hama)
            {
                
                foreach (var pair in hama.doors.Pairs)
                {
                    //this.Monitor.Log($"{pair.Key} : {pair.Value}", LogLevel.Debug);
                    this.Monitor.Log($"{pair.Key} : {pair.Value})", LogLevel.Debug);
                }
            }
                    */
        }
    }
}

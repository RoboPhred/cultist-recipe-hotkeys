﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using BepInEx.Configuration;
using UnityEngine;

namespace CultistRecipeHotkeys
{
    [BepInEx.BepInPlugin("net.robophreddev.CultistSimulator.CultistRecipeHotkeys", "CultistRecipeHotkeys", "1.0.1")]
    public class CultistRecipeHotkeysMod : BepInEx.BaseUnityPlugin
    {
        private List<ConfigEntry<KeyboardShortcut>> ExecuteRecipeHotkeys = new List<ConfigEntry<KeyboardShortcut>>();
        private List<ConfigEntry<KeyboardShortcut>> RestoreRecipeHotkeys = new List<ConfigEntry<KeyboardShortcut>>();
        private List<ConfigEntry<KeyboardShortcut>> LearnRecipeHotkeys = new List<ConfigEntry<KeyboardShortcut>>();

        readonly Dictionary<int, RecipeConfig> RecipesByHotkeyIndex = new Dictionary<int, RecipeConfig>();

        private TabletopTokenContainer TabletopTokenContainer
        {
            get
            {
                {
                    var tabletopManager = Registry.Get<TabletopManager>();
                    if (tabletopManager == null)
                    {
                        this.Logger.LogError("Could not fetch TabletopManager");
                    }

                    return tabletopManager._tabletop;
                }
            }
        }

        void Start()
        {
            for (var i = 0; i <= 11; i++)
            {
                ExecuteRecipeHotkeys.Add(Config.Bind(new ConfigDefinition("Hotkeys", "ExecuteRecipe" + i), new KeyboardShortcut(KeyCode.F1 + i)));
                RestoreRecipeHotkeys.Add(Config.Bind(new ConfigDefinition("Hotkeys", "RestoreRecipe" + i), new KeyboardShortcut(KeyCode.F1 + i, KeyCode.LeftShift)));
                LearnRecipeHotkeys.Add(Config.Bind(new ConfigDefinition("Hotkeys", "LearnRecipe" + i), new KeyboardShortcut(KeyCode.F1 + i, KeyCode.LeftControl)));
            }

            if (!File.Exists(Config.ConfigFilePath))
            {
                Config.Save();
            }

            this.Logger.LogInfo("CultistRecipeHotkeys initialized.");
        }

        void Update()
        {
            for (var i = 0; i < this.ExecuteRecipeHotkeys.Count; i++)
            {
                if (this.ExecuteRecipeHotkeys[i].Value.IsDown())
                {
                    this.RestoreRecipe(i, true);
                }
                else if (this.RestoreRecipeHotkeys[i].Value.IsDown())
                {
                    this.RestoreRecipe(i, false);
                }
                else if (this.LearnRecipeHotkeys[i].Value.IsDown())
                {
                    this.StoreRecipe(i);
                }
            }
        }

        void StoreRecipe(int index)
        {
            if (TabletopManager.IsInMansus())
            {
                return;
            }

            var situation = this.GetOpenSituation();
            if (situation == null)
            {
                return;
            }

            if (situation.SituationClock.State != SituationState.Unstarted)
            {
                this.Notify("I cannot remember this", "Memory only serves to start the unstarted.");
                return;
            }

            var slots = situation.situationWindow.GetStartingSlots();
            var elements = slots.Select(x => ValidRecipeSlotOrNull(x)).Select(x => x?.GetElementStackInSlot()?.EntityId);

            this.RecipesByHotkeyIndex[index] = new RecipeConfig
            {
                Situation = situation.GetTokenId(),
                RecipeElements = elements.ToArray()
            };

            this.Notify("Repetition breeds familiarity", "I will remember this recipe for later.");
        }

        void RestoreRecipe(int index, bool executeOnRestore)
        {
            if (TabletopManager.IsInMansus())
            {
                return;
            }

            RecipeConfig recipe;
            if (!this.RecipesByHotkeyIndex.TryGetValue(index, out recipe))
            {
                return;
            }

            var situation = this.GetSituation(recipe.Situation);
            if (situation == null)
            {
                return;
            }

            switch (situation.SituationClock.State)
            {
                case SituationState.Complete:
                    situation.situationWindow.DumpAllResultingCardsToDesktop();
                    break;
                case SituationState.Unstarted:
                    situation.situationWindow.DumpAllStartingCardsToDesktop();
                    break;
                default:
                    SoundManager.PlaySfx("CardDragFail");
                    this.Notify("I am busy", "I cannot start a recipe while I am busy doing somthing else.");
                    return;
            }

            // The first slot is the primary slot, so slot it independently.
            //  A successful slot here may cause new slots to be added.
            var primaryElement = recipe.RecipeElements.FirstOrDefault();
            if (primaryElement != null)
            {
                var slot = situation.situationWindow.GetStartingSlots().FirstOrDefault();
                if (!slot || !this.TryPopulateSlot(slot, primaryElement))
                {
                    this.Notify("Something is missing", "I cannot start this recipe, as I am missing a critical component.");
                    return;
                }
            }

            // Slot the remainder of the elements, now that
            //  the primary has opened up new slots for us.
            var slots = situation.situationWindow.GetStartingSlots();
            for (var i = 1; i < Math.Min(slots.Count, recipe.RecipeElements.Length); i++)
            {
                var element = recipe.RecipeElements[i];
                var slot = slots[i];
                this.TryPopulateSlot(slot, element);
            }

            if (executeOnRestore)
            {
                situation.AttemptActivateRecipe();
                if (situation.SituationClock.State == SituationState.Unstarted)
                {
                    this.Notify("Something went wrong", "I could not start the recipe.");
                    situation.OpenWindow();
                }

                // If we started the recipe, there is no need to open the window.
            }
            else
            {
                situation.OpenWindow();
            }
        }

        bool TryPopulateSlot(RecipeSlot slot, string elementId)
        {
            if (slot.Defunct || slot.IsGreedy || slot.IsBeingAnimated)
            {
                return false;
            }

            var stack = this.GetStackForElement(elementId);
            if (stack == null)
            {
                return false;
            }

            this.PopulateSlot(slot, stack);
            return true;
        }

        void PopulateSlot(RecipeSlot slot, ElementStackToken stack)
        {
            stack.lastTablePos = new Vector2?(stack.RectTransform.anchoredPosition);
            if (stack.Quantity != 1)
            {
                var newStack = stack.SplitAllButNCardsToNewStack(stack.Quantity - 1, new Context(Context.ActionSource.PlayerDrag));
                slot.AcceptStack(newStack, new Context(Context.ActionSource.PlayerDrag));
            }
            else
            {
                slot.AcceptStack(stack, new Context(Context.ActionSource.PlayerDrag));
            }
        }

        ElementStackToken GetStackForElement(string elementId)
        {
            var tokens = this.TabletopTokenContainer.GetTokens();
            var elementStacks =
                from token in tokens
                let stack = token as ElementStackToken
                where stack != null && stack.EntityId == elementId
                select stack;
            return elementStacks.FirstOrDefault();
        }

        RecipeSlot ValidRecipeSlotOrNull(RecipeSlot slot)
        {
            if (slot.Defunct || slot.IsGreedy || slot.IsBeingAnimated)
            {
                return null;
            }
            return slot;
        }

        SituationController GetSituation(string entityId)
        {
            var situation = Registry.Get<SituationsCatalogue>().GetRegisteredSituations().FirstOrDefault(x => x.situationToken.EntityId == entityId);
            var token = situation.situationToken as SituationToken;
            if (token.Defunct || token.IsBeingAnimated)
            {
                return null;
            }

            return situation;
        }

        SituationController GetOpenSituation()
        {
            var situation = Registry.Get<SituationsCatalogue>().GetOpenSituation();
            var token = situation.situationToken as SituationToken;
            if (token.Defunct || token.IsBeingAnimated)
            {
                return null;
            }

            return situation;
        }

        void Notify(string title, string text)
        {
            Registry.Get<INotifier>().ShowNotificationWindow(title, text, false);
        }
    }

    struct RecipeConfig
    {
        public string Situation;
        public string[] RecipeElements;
    }
}
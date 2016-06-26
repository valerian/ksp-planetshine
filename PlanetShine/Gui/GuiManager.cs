/*
* (C) Copyright 2014, Valerian Gaudeau
* 
* Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
* project is in no way associated with nor endorsed by Squad.
* 
* This code is licensed under the Apache License Version 2.0. See the LICENSE.txt
* file for more information.
*/
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using UnityEngine;
using KSP.UI.Screens;


namespace PlanetShine
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class GuiManager : MonoBehaviour
    {
        public bool isConfigDisplayed
        {
            get
            {
                return _isConfigDisplayed;
            }

            set
            {
                if (_isConfigDisplayed == value)
                    return;
                _isConfigDisplayed = value;
                UpdateButtonIcons();
            }
        }
        private bool _isConfigDisplayed = false;

        private Config config = Config.Instance;
        private PlanetShine planetShine;
        private IButton blizzyButton;
        private ApplicationLauncherButton stockButton;
        private GuiRenderer guiRenderer;

        public void Start()
        {
            guiRenderer = new GuiRenderer(this);
            UpdateToolbarBlizzy();
            UpdateToolbarStock();
        }

        public void UpdateToolbarStock()
        {
            if (stockButton != null)
            {
                if (!config.stockToolbarEnabled && config.blizzyToolbarInstalled)
                {
                    ApplicationLauncher.Instance.RemoveModApplication(stockButton);
                    stockButton = null;
                }
                return;
            }
            else if (!config.stockToolbarEnabled && config.blizzyToolbarInstalled)
                return;
            stockButton = ApplicationLauncher.Instance.AddModApplication(
                () =>
                {
                    planetShine = PlanetShine.Instance;
                    isConfigDisplayed = true;
                },
                () =>
                {
                    isConfigDisplayed = false;
                },
                null,
                null,
                null,
                null,
                ApplicationLauncher.AppScenes.FLIGHT,
                GameDatabase.Instance.GetTexture("PlanetShine/Icons/ps_toolbar", false)
                );
            if (isConfigDisplayed)
                stockButton.SetTrue();
        }

        public void UpdateToolbarBlizzy()
        {
            if (!config.blizzyToolbarInstalled)
                return;
            if (blizzyButton != null)
                return;
            blizzyButton = ToolbarManager.Instance.add("PlanetShine", "Gui");
            blizzyButton.TexturePath = "PlanetShine/Icons/ps_disabled";
            blizzyButton.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
            blizzyButton.ToolTip = "PlanetShine Settings";
            blizzyButton.OnClick += (e) =>
            {
                planetShine = PlanetShine.Instance;
                isConfigDisplayed = !isConfigDisplayed;
            };
        }

        private void UpdateButtonIcons() {
            if (blizzyButton != null)
                blizzyButton.TexturePath = isConfigDisplayed ? "PlanetShine/Icons/ps_enabled" : "PlanetShine/Icons/ps_disabled";
            if (stockButton != null)
                if (_isConfigDisplayed)
                    stockButton.SetTrue();
                else
                    stockButton.SetFalse();       
        }

        private void OnGUI(){
            if (isConfigDisplayed) {
                guiRenderer.Render(planetShine);
            }
        }
        
        private void OnDestroy() {
            if (stockButton != null)
                ApplicationLauncher.Instance.RemoveModApplication(stockButton);
            if (blizzyButton != null)
                blizzyButton.Destroy();
        }
    }
}


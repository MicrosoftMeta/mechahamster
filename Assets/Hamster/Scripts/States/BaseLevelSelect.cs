﻿// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using System.Collections.Generic;

namespace Hamster.States {
  class BaseLevelSelect : BaseState {
    Menus.LevelSelectGUI menuComponent;

    protected int mapSelection = 0;
    protected int currentPage = 0;
    protected LevelMap currentLevel;
    protected string[] levelNames;

    protected int currentLoadedMap = -1;

    // Layout constants.
    private const int ButtonsPerPage = 5;
    private const float ColumnPadding = 50;

    Dictionary<int, GameObject> levelButtons = new Dictionary<int, GameObject>();

    // Update function, which gets called once per frame.
    public override void Update() {
      // If they've got a different map selected than the one we have loaded,
      // load the new one!
      if (currentLoadedMap != mapSelection) {
        currentLoadedMap = mapSelection;
        LoadLevel(mapSelection);
      }
    }

    // This needs to be called with a list of maps to display.
    protected virtual void MenuStart(string[] levelNames, string title) {
      this.levelNames = levelNames;

      menuComponent = SpawnUI<Menus.LevelSelectGUI>(StringConstants.PrefabsLevelSelectMenu);
      menuComponent.SelectionText.text = title;

      SpawnLevelButtons(currentPage);
      ChangePage(0);
    }

    // Called whenever a level is selected in the menu.
    protected virtual void LoadLevel(int i) { }

    // Initialization method.  Called after the state is added to the stack.
    public override void Initialize() { }

    // Removes all buttons from the screen.
    void ClearCurrentButtons() {
      foreach (KeyValuePair<int, GameObject> pair in levelButtons) {
        GameObject.Destroy(pair.Value);
      }
      levelButtons.Clear();
    }

    // Creates one page worth of level buttons for a given page.  Sets their names
    // and properties, and makes sure they're in the correct part of the window.
    // Also removes any existing level buttons.
    void SpawnLevelButtons(int page) {
      ClearCurrentButtons();
      int maxButtonIndex = (currentPage + 1) * ButtonsPerPage;
      if (maxButtonIndex > levelNames.Length) maxButtonIndex = levelNames.Length;
      for (int i = currentPage * ButtonsPerPage; i < maxButtonIndex; i++) {
        GameObject button = GameObject.Instantiate(
            CommonData.prefabs.menuLookup[StringConstants.PrefabsLevelSelectButton]);
        Menus.LevelSelectButtonGUI component = button.GetComponent<Menus.LevelSelectButtonGUI>();
        if (component != null) {
          component.buttonId = i;
          levelButtons[i] = button;
          button.transform.SetParent(menuComponent.Panel.transform, false);
          component.ButtonText.text = levelNames[i];
        } else {
          Debug.LogError("Level select button prefab had no LevelSelectButtionGUI component.");
        }

        gui.transform.SetParent(CommonData.mainCamera.transform, false);
      }
    }

    public override void Resume(StateExitValue results) {
      menuComponent.gameObject.SetActive(true);
    }

    public override void Suspend() {
      menuComponent.gameObject.SetActive(false);
    }

    void ChangePage(int delta) {
      currentPage += delta;
      int pageMax = (int)((levelNames.Length) / ButtonsPerPage);
      if (currentPage <= 0) currentPage = 0;
      if (currentPage >= pageMax) currentPage = pageMax;

      menuComponent.BackButton.gameObject.SetActive(currentPage != 0);
      menuComponent.ForwardButton.gameObject.SetActive(currentPage != pageMax);
      SpawnLevelButtons(currentPage);
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      Menus.LevelSelectButtonGUI buttonComponent =
          source.GetComponent<Menus.LevelSelectButtonGUI>();
      if (source == menuComponent.MainButton.gameObject) {
        manager.SwapState(new MainMenu());
      } else if (source == menuComponent.PlayButton.gameObject) {
        if (CommonData.inVrMode) {
          manager.PushState(new ControllerHelp());
        } else {
          manager.PushState(new Gameplay());
        }
      } else if (source == menuComponent.BackButton.gameObject) {
        ChangePage(-1);
      } else if (source == menuComponent.ForwardButton.gameObject) {
        ChangePage(1);
      } else if (buttonComponent != null) {
        // They pressed one of the buttons for a level.
        mapSelection = buttonComponent.buttonId;
      }
    }

    // Clean up when we exit the state.
    public override StateExitValue Cleanup() {
      DestroyUI();
      CommonData.gameWorld.DisposeWorld();
      return null;
    }
  }

}

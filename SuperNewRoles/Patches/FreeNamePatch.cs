// Taken from https://github.com/NuclearPowered/Reactor/ , licensed under the LGPLv3

using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patches
{
    class FreeNamePatch
    {
        public static void Initialize()
        {
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) =>
            {
                if (!scene.name.Equals("MMOnline")) return;
                if (!TryMoveObjects()) return;
                var editName = FastDestroyableSingleton<AccountManager>.Instance.accountTab.editNameScreen;
                var NameText = Object.Instantiate(editName.nameText.gameObject);
                NameText.transform.localPosition += Vector3.up * 2.2f;
                var textBox = NameText.GetComponent<TextBoxTMP>();
                textBox.outputText.alignment = TextAlignmentOptions.CenterGeoAligned;
                textBox.outputText.transform.position = NameText.transform.position;
                textBox.outputText.fontSize = 4f;
                textBox.OnChange.AddListener((Action)(() =>
                {
                    DataManager.Player.Customization.Name = textBox.text;
                }));
                textBox.OnEnter = textBox.OnFocusLost = textBox.OnChange;
                textBox.Pipe.GetComponent<TextMeshPro>().fontSize = 4f;
            }));
        }

        private static bool TryMoveObjects()
        {
            var toMove = new List<string>
            {
                "HostGameButton",
                "FindGameButton",
                "JoinGameButton"
            };
            var yStart = Vector3.up;
            var yOffset = Vector3.down * 1.5f;
            var gameObjects = toMove.Select(x => GameObject.Find("NormalMenu/" + x)).ToList();
            if (gameObjects.Any(x => x == null)) return false;
            for (var i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].transform.position = yStart + (yOffset * i);
            }
            return true;
        }
    }
}
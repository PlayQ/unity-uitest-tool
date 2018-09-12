using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using PlayQ.TG.Bundles;
using PlayQ.TG.Bundles.Downloader;
using PlayQ.TG.Bundles.Events;
using PlayQ.TG.EditorTools;
using PlayQ.TG.Tools;
using UnityEditor;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class GameEventImporter : EditorWindow
    {
        private const string FULL_EVENT_ASSET_NAME = "Assets/Tests/Editor/Resources/" + EVENT_ASSET_NAME + ".asset";
        private const string EVENT_ASSET_NAME = "ArchivedEvents";

        private GameEventsHolder eventsHolder;
        private UITestToolWindowFooter footer;

        private void OnEnable()
        {
            eventsHolder = Resources.Load<GameEventsHolder>(EVENT_ASSET_NAME);
            footer = new UITestToolWindowFooter();
            if (eventsHolder == null)
            {
                eventsHolder = CreateInstance<GameEventsHolder>();
                AssetDatabase.CreateAsset(eventsHolder, FULL_EVENT_ASSET_NAME);
                EditorUtility.SetDirty(eventsHolder);
            }
        }

        private Vector2 scrollPosition;

        private void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.Space(5);
            
            if (eventsHolder.Events.Count > 0)
            {
                EditorGUILayout.LabelField("Imported game events: ");

                GUILayout.BeginVertical(GUI.skin.box);
                for (var i = eventsHolder.Events.Count - 1; i >= 0; i--)
                {
                    var gameEvent = eventsHolder.Events[i];
                    GUILayout.BeginHorizontal();

                    var label = StringTool.Format("{0}. {1} ({2})", gameEvent.Id.ToString(), gameEvent.Name, gameEvent.UpdatedAt.ToString());
                    GUILayout.Label(label);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        eventsHolder.Events.RemoveAt(i);
                        EditorUtility.SetDirty(eventsHolder);
                    }

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }

            if (GUILayout.Button("Import new event"))
            {
                ImportEvent();
            }
            GUILayout.EndScrollView();
            GUILayout.Space(UITestToolWindowFooter.HEIGHT + 5);
            footer.Draw(position);
        }

        private void ImportEvent()
        {
            var archivedGameEvent = new ImportedGameEvent();

            var pathToFolder =
                EditorUtility.OpenFolderPanel("Choose folder with game event", string.Empty, string.Empty);
            if (pathToFolder.Length == 0)
            {
                return;
            }

            var manifestPath = StringTool.CombinePath(pathToFolder, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                EditorUtils.ShowErrorDialog("Can't find manifest.json in folder \"{0}\"", false, pathToFolder);
                return;
            }

            var parseFailed = true;
            var manifestText = File.ReadAllText(manifestPath);
            BundleManifest manifest = null;
            try
            {
                manifest = JsonConvert.DeserializeObject<BundleManifest>(manifestText);
                parseFailed = manifest == null;
            }
            catch (Exception)
            {
                parseFailed = true;
            }

            if (parseFailed)
            {
                EditorUtils.ShowErrorDialog("Can't parse manifest", false);
                return;
            }

            archivedGameEvent.Files.Add(new ImportedFile
            {
                Location = "manifest.json",
                Bytes = Encoding.UTF8.GetBytes(manifestText)
            });

            var configPath = StringTool.CombinePath(pathToFolder, "config.json");
            if (!File.Exists(configPath))
            {
                EditorUtils.ShowErrorDialog("Can't find config.json in folder \"{0}\"", false, pathToFolder);
                return;
            }

            var configText = File.ReadAllText(configPath);
            GameEvent gameEvent = null;
            try
            {
                gameEvent = JsonConvert.DeserializeObject<GameEvent>(configText);
                parseFailed = gameEvent == null;
            }
            catch (Exception)
            {
                parseFailed = true;
            }

            if (parseFailed)
            {
                EditorUtils.ShowErrorDialog("Can't parse game event config", false);
                return;
            }

            archivedGameEvent.Id = gameEvent.Id;
            archivedGameEvent.Name = gameEvent.Name;
            archivedGameEvent.UpdatedAt = gameEvent.UpdatedAt.ToString(StringTool.DATE_TIME_SHORT_FORMAT);

            foreach (var manifestFile in manifest.Files)
            {
                if (manifestFile.DelayedDownload)
                {
                    continue;
                }
                var filePath = StringTool.CombinePath(pathToFolder,
                    FileStorageController.LocationToFileName(manifestFile.Location));
                if (!File.Exists(filePath))
                {
                    EditorUtils.ShowErrorDialog("Can't find file \"{0}\" ({1}) in folder \"{2}\"", false,
                        manifestFile.Name, FileStorageController.LocationToFileName(manifestFile.Location),
                        pathToFolder);
                    return;
                }
                var bytes = File.ReadAllBytes(filePath);
                var gettedSha = SHA256.Get(bytes);
                if (gettedSha != manifestFile.Sha256)
                {
                    EditorUtils.ShowErrorDialog(
                        "Sha256 for file \"{0}\" ({1}) is not equals with sha256 in manifest ({2}).", false,
                        manifestFile.Name, gettedSha, manifestFile.Sha256);
                    return;
                }

                archivedGameEvent.Files.Add(new ImportedFile
                {
                    Location = manifestFile.Location,
                    Bytes = bytes
                });
            }

            var oldEventIndex = eventsHolder.Events.FindIndex(e => e.Id == archivedGameEvent.Id);
            if (oldEventIndex != -1)
            {
                eventsHolder.Events.RemoveAt(oldEventIndex);
            }
            eventsHolder.Events.Add(archivedGameEvent);
            EditorUtility.SetDirty(eventsHolder);
        }
    }
}
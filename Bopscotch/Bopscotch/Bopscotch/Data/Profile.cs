﻿using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework.GamerServices;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;
using Leda.Core.Asset_Management;

using Bopscotch.Interface;

namespace Bopscotch.Data
{
    public sealed class Profile : ISerializable
    {
        private static Profile _instance = null;
        private static Profile Instance
        {
            get
            {
                if (_instance == null) { _instance = new Profile().GetProfile(); _instance.SaveData(); }
                return _instance;
            }
        }

        public static PCSettings Settings { get { return Instance._settings; } }

        public static int GoldenTickets { get { return Instance._goldenTicketCount; } set { Instance._goldenTicketCount = value; } }
        public static bool PlayingRaceMode { get; set; }
        public static bool PauseOnSceneActivation { get; set; }

        public static string CurrentAreaName { set { Instance._currentArea = value; } }
        public static AreaDataContainer CurrentAreaData { get { return Instance._areaLevelData[Instance._currentArea]; } }
        public static List<XElement> SimpleAreaData { get { return Instance.GetAreaBaseData(); } }
        public static Dictionary<string, string> AvailableAreas { get { return Instance.AreaSelectorData; } }
        public static string SelectedAreaName { set { Instance._currentArea = value; Instance.SaveData(); } }
        public static List<XElement> NewlyUnlockedContent { get { return Instance.GetContentForLockState(LockState.NewlyUnlocked); } }
        public static List<XElement> FullVersionOnlyUnlocks { get { return Instance.GetContentForLockState(LockState.FullVersionOnly); } }
        public static List<XElement> UnlockedContent { get { return Instance.GetContentForLockState(LockState.Unlocked); } }
        public static List<XElement> LockedContent { get { return Instance.GetContentForLockState(LockState.Locked); } }

        public static void Load() { string temp = Instance.ID; }
        public static void Save() { Instance.SaveData(); }
        public static void UnlockCurrentAreaContent() { Instance.UnlockLockedContentForCurrentArea(); }
        public static string AreaSelectionTexture(string areaName) { return Instance._areaLevelData[areaName].SelectionTexture; }
        public static bool AvatarComponentUnlocked(string set, string component) { return Instance.CheckForAvatarComponentUnlock(set, component); }
        public static bool AvatarCostumeUnlocked(string name) { return Instance.CheckForAvatarCostumeUnlock(name); }
        public static void ResetAreas() { Instance.ResetAllAreas(); }

        public static bool AreaHasBeenCompleted(string areaName) { return Instance._areaLevelData[areaName].Completed; }
        public static bool AreaIsLocked(string areaName) 
        { 
            return (bool)(((from a in SimpleAreaData where a.Attribute("name").Value == areaName select a).First<XElement>()).Attribute("locked")); 
        }

        public static bool UnlockNamedArea(string areaName)
        {
            if (!Instance._areaLevelData[areaName].Locked) { return false; }

            Instance._areaLevelData[areaName].Locked = false;
            Save();
            return true;
        }

        public static void UnlockCostume(string costumeName) { Instance.UnlockAvatarCostume(costumeName); }

        private Dictionary<string, AreaDataContainer> _areaLevelData;
        private string _currentArea;

        private int _goldenTicketCount;
        private List<XElement> _newlyUnlockedItems;
        private List<XElement> _fullVersionOnlyUnlockables;
        private List<XElement> _unlockedAvatarComponents;

        private PCSettings _settings;

        public string ID { get { return Profile_ID; } set { } }

        private List<XElement> GetAreaBaseData()
        {
            List<XElement> areaData = new List<XElement>();
            string[] difficultyTagSequence = Difficulty_Sequence_CSV.Split(',');

            foreach (KeyValuePair<string, AreaDataContainer> kvp in _areaLevelData)
            {
                XElement el = new XElement("area-base");
                el.Add(new XAttribute("name", kvp.Value.Name));
                el.Add(new XAttribute("difficulty", kvp.Value.DifficultyTag));
                el.Add(new XAttribute("speed", kvp.Value.SpeedStep));
                el.Add(new XAttribute("texture", kvp.Value.SelectionTexture));
                el.Add(new XAttribute("last", kvp.Value.LevelScores.Count));
                el.Add(new XAttribute("locked", kvp.Value.Locked));
                el.Add(new XAttribute("no-race", kvp.Value.DoesNotHaveRaceCourse));

                for (int i = 0; i < difficultyTagSequence.Length; i++)
                {
                    if (difficultyTagSequence[i] == kvp.Value.DifficultyTag.ToLower())
                    {
                        el.Add(new XAttribute("index", i));
                        break;
                    }
                }

                areaData.Add(el);
            }

            return areaData.OrderBy(el => (int)el.Attribute("index")).ToList();
        }

        private Dictionary<string, string> AreaSelectorData
        {
            get
            {
                Dictionary<string, string> areaData = new Dictionary<string, string>();
                foreach (KeyValuePair<string, AreaDataContainer> kvp in _areaLevelData) { areaData.Add(kvp.Key, kvp.Value.SelectionTexture); }
                return areaData;
            }
        }

        public Profile()
        {
            _areaLevelData = new Dictionary<string, AreaDataContainer>();
            _settings = new PCSettings();

            _goldenTicketCount = 0;
            _newlyUnlockedItems = new List<XElement>();
            _unlockedAvatarComponents = new List<XElement>();
            _fullVersionOnlyUnlockables = new List<XElement>();
        }

        private Profile GetProfile()
        {
            if (Definitions.Overwrite_Profile) { FileManager.DeleteFile(Profile_FileName); }

            if (FileManager.FileExists(Profile_FileName)) { PopulateFromXml(FileManager.LoadXMLFile(Profile_FileName)); }
            else { CreateProfile(); }

            CreateAreaData(FileManager.LoadXMLContentFile(Additional_Areas_FileName));
            EnsureAreaLockStateSynchronisation();

            return this;
        }

        private void PopulateFromXml(XDocument profileData)
        {
            Deserialize(profileData.Element("profile-data").Element("object"));
        }

        public void Deserialize(XElement serializedData)
        {
            Serializer serializer = new Serializer(serializedData);
            _settings = serializer.GetDataItem<PCSettings>("config-settings");
            _goldenTicketCount = serializer.GetDataItem<int>("golden-tickets");
            _currentArea = serializer.GetDataItem<string>("last-area");

            LoadAreaDataFromXml(serializer.GetDataElement("survival-area-data"));
            LoadAvatarComponentDataFromXml(serializer.GetDataElement("avatar-component-data"));
        }

        private void LoadAreaDataFromXml(XElement areaData)
        {
            if (areaData != null)
            {
                _areaLevelData.Clear();

                foreach (XElement a in areaData.Elements("area")) { _areaLevelData.Add(a.Attribute("name").Value, AreaDataContainer.CreateFromXml(a)); }
            }
        }

        private void LoadAvatarComponentDataFromXml(XElement componentData)
        {
            _unlockedAvatarComponents.Clear();

            if ((componentData != null) && (componentData.Elements("component") != null))
            {
                foreach (XElement el in componentData.Elements("component")) { _unlockedAvatarComponents.Add(el); }
            }
        }

        private void CreateProfile()
        {
            CreateAreaData(FileManager.LoadXMLContentFile(Default_Areas_FileName));
            _currentArea = _areaLevelData.Keys.First();
        }

        private void CreateAreaData(XDocument areaData)
        {
            foreach (XElement el in areaData.Element("areas").Elements("area"))
            {
                if (!_areaLevelData.ContainsKey(el.Attribute("name").Value)) 
                { 
                    _areaLevelData.Add(el.Attribute("name").Value, AreaDataContainer.CreateFromXml(el)); 
                }
                else if (el.Element("completion-unlockables") != null) 
                { 
                    _areaLevelData[el.Attribute("name").Value].SetCompletionUnlockables(el.Element("completion-unlockables")); 
                }
            }
        }

        private void EnsureAreaLockStateSynchronisation()
        {
            foreach (KeyValuePair<string, AreaDataContainer> kvp in _areaLevelData)
            {
                if ((kvp.Value.Completed) && (kvp.Value.ContentToUnlockOnCompletion != null))
                {
                    foreach (XElement el in kvp.Value.ContentToUnlockOnCompletion.Elements())
                    {
                        if (el.Attribute("type").Value == "area") { _areaLevelData[el.Attribute("name").Value].Locked = false; }
                    }
                }
            }

            SaveData();
        }

        private void SaveData()
        {
            XDocument profileData = new XDocument(new XDeclaration("1.0", "utf", "yes"));
            profileData.Add(new XElement("profile-data"));

            profileData.Element("profile-data").Add(Serialize());

            FileManager.SaveXMLFile(Profile_FileName, profileData);
        }

        public XElement Serialize()
        {
            Serializer serializer = new Serializer(this);

            serializer.AddDataItem("config-settings", _settings);
            serializer.AddDataItem("golden-tickets", _goldenTicketCount);
            serializer.AddDataItem("last-area", _currentArea);
            serializer.AddDataElement(SerializedAreaData);
            serializer.AddDataElement(SerializedAvatarComponentData);

            return serializer.SerializedData;
        }

        private XElement SerializedAreaData
        {
            get
            {
                XElement data = new XElement("survival-area-data");

                foreach (KeyValuePair<string, AreaDataContainer> kvp in _areaLevelData) { data.Add(kvp.Value.Xml); }

                return data;
            }
        }

        private XElement SerializedAvatarComponentData
        {
            get
            {
                XElement data = new XElement("avatar-component-data");

                foreach (XElement el in _unlockedAvatarComponents) { data.Add(el); }

                return data;
            }
        }

        private void UnlockLockedContentForCurrentArea()
        {
            _newlyUnlockedItems.Clear();
            _fullVersionOnlyUnlockables.Clear();

            if (_areaLevelData[_currentArea].ContentToUnlockOnCompletion != null)
            {
                foreach (XElement el in _areaLevelData[_currentArea].ContentToUnlockOnCompletion.Elements("unlockable"))
                {
                    if (!(bool)el.Attribute("unlocked"))
                    {
                        if (ItemCanBeUnlocked(el))
                        {
                            el.SetAttributeValue("unlocked", true);
                            _newlyUnlockedItems.Add(el);

                            switch (el.Attribute("type").Value)
                            {
                                case "area": UnlockNamedArea(el.Attribute("name").Value); break;
                                case "golden-ticket": UnlockGoldenTickets((int)el.Attribute("units")); break;
                                case "avatar-component": UnlockAvatarComponent(el.Attribute("set").Value, el.Attribute("name").Value); break;
                                case "avatar-costume": UnlockAvatarCostume(el.Attribute("name").Value); break;
                            }
                        }
                        else
                        {
                            _fullVersionOnlyUnlockables.Add(el);
                        }
                    }
                }

                SaveData();
            }
        }

        private bool ItemCanBeUnlocked(XElement item)
        {
            if (!Definitions.Simulate_Trial_Mode) { return true; }
            if (item.Attribute("trial-unlockable") == null) { return false; }
            if ((bool)item.Attribute("trial-unlockable")) { return true; }

            return false;
        }

        private void UnlockArea(string areaName)
        {
            _areaLevelData[areaName].Locked = false;
        }

        private void UnlockGoldenTickets(int numberOfUnits)
        {
            _goldenTicketCount += numberOfUnits;
        }

        private void UnlockAvatarComponent(string componentSet, string componentName)
        {
            XElement unlockedComponent = new XElement("component");
            unlockedComponent.Add(new XAttribute("set", componentSet));
            unlockedComponent.Add(new XAttribute("name", componentName));

            _unlockedAvatarComponents.Add(unlockedComponent);

            Avatar.AvatarComponentManager.UnlockComponent(componentSet, componentName);
        }

        private void UnlockAvatarCostume(string costumeName)
        {
            if (Avatar.AvatarComponentManager.CostumeComponents.ContainsKey(costumeName))
            {
                foreach (XElement el in Avatar.AvatarComponentManager.CostumeComponents[costumeName].Elements("component"))
                {
                    UnlockAvatarComponent(el.Attribute("set").Value, el.Attribute("name").Value);
                }
            }
        }

        private List<XElement> GetContentForLockState(LockState state)
        {
            List<XElement> content = new List<XElement>();

            foreach (XElement el in _areaLevelData[_currentArea].ContentToUnlockOnCompletion.Elements("unlockable"))
            {
                switch (state)
                {
                    case LockState.Locked:
                        if ((!(bool)el.Attribute("unlocked")) && (ItemCanBeUnlocked(el))) { content.Add(el); }
                        break;
                    case LockState.Unlocked:
                        if (((bool)el.Attribute("unlocked")) && (!_newlyUnlockedItems.Contains(el))) { content.Add(el); }
                        break;
                    case LockState.NewlyUnlocked:
                        if (((bool)el.Attribute("unlocked")) && (_newlyUnlockedItems.Contains(el))) { content.Add(el); }
                        break;
                    case LockState.FullVersionOnly:
                        if ((!(bool)el.Attribute("unlocked")) && (!ItemCanBeUnlocked(el))) { content.Add(el); }
                        break;

                }
            }

            return content;
        }

        private bool CheckForAvatarComponentUnlock(string set, string component)
        {
            if (_unlockedAvatarComponents.Count() < 1) { return false; }

            return ((from el in _unlockedAvatarComponents
                     where ((el.Attribute("set").Value == set) && (el.Attribute("name").Value == component))
                     select el).Count() > 0);
        }

        private bool CheckForAvatarCostumeUnlock(string name)
        {
            if (!Avatar.AvatarComponentManager.CostumeComponents.ContainsKey(name)) { return false; }

            XElement firstComponent = (XElement)Avatar.AvatarComponentManager.CostumeComponents[name].FirstNode;

            return CheckForAvatarComponentUnlock(firstComponent.Attribute("set").Value, firstComponent.Attribute("name").Value);
        }

        private void ResetAllAreas()
        {
            foreach (KeyValuePair<string, AreaDataContainer> kvp in _areaLevelData) { kvp.Value.Reset(); }
            GoldenTickets = 0;
            SaveData();
        }

        public enum LockState
        {
            Locked,
            FullVersionOnly,
            NewlyUnlocked,
            Unlocked
        }

        private const string Profile_ID = "profile-id";
        private const string Profile_FileName = "profile.xml";
        private const string Default_Areas_FileName = "Content/Files/Levels/DefaultAreas.xml";
        private const string Additional_Areas_FileName = "Content/Files/Levels/AdditionalAreas.xml";
        private const string Difficulty_Sequence_CSV = "n/a,easy,simple,medium,hard,insane";
    }
}

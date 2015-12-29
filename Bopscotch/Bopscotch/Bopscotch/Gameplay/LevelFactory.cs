using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Game_Objects.Tile_Map;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;
using Leda.Core.Timing;

using Bopscotch.Data;
using Bopscotch.Effects.Particles;
using Bopscotch.Gameplay.Objects.Environment;
using Bopscotch.Gameplay.Objects.Environment.Blocks;
using Bopscotch.Gameplay.Objects.Environment.Collectables;
using Bopscotch.Gameplay.Objects.Environment.Signposts;
using Bopscotch.Gameplay.Objects.Environment.Flags;
using Bopscotch.Gameplay.Objects.Characters;
using Bopscotch.Gameplay.Objects.Characters.Player;

namespace Bopscotch.Gameplay
{
    public class LevelFactory : IObjectCreator, IGameObject
    {
        private Scene.ObjectRegistrationHandler _registerGameObject;
        private TimerController.TickCallbackRegistrationHandler _registerTimerTick;

        public Player Player { get; private set; }
        public TileMap Map { get; private set; }
        public AnimationController AnimationController { set { BlockFactory.AnimationController = value; } }
        public SmashBlock.SmashCallbackMethod SmashBlockCallback { set { BlockFactory.SmashBlockCallback = value; } }
        public AdditiveLayerParticleEffectManager.CloudBurstEffectInitiator SmashBlockRegenrationCallback { set { BlockFactory.SmashBlockRegerationCallback = value; } }

        public Point BackgroundDimensions { private get; set; }

        public int RaceLapCount { get; private set; }
        public string RaceAreaName { private get; set; }

        public LevelFactory(Scene.ObjectRegistrationHandler registerGameObject, 
            TimerController.TickCallbackRegistrationHandler registerTimerTick)
        {
            _registerGameObject = registerGameObject;
            _registerTimerTick = registerTimerTick;

            BackgroundDimensions = new Point(Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height);
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
        }

        public void LoadAndInitializeLevel()
        {
            XElement levelData = LoadLevelData();
            RaceLapCount = (Data.Profile.PlayingRaceMode ? GetLapsForRace(levelData) : 0);

            WireUpCallElementFactoryCallbacks();

            Map = BlockFactory.LoadBlocks(levelData.Element(Terrain_Data_Element).Element(BlockFactory.Data_Group_Node_Name));
            Map.ViewportDimensionsInTiles = new Point(
                (BackgroundDimensions.X / Definitions.Grid_Cell_Pixel_Size) + 1, BackgroundDimensions.Y / Definitions.Grid_Cell_Pixel_Size);

            CollectableFactory.LoadCollectables(levelData.Element(Terrain_Data_Element).Element(CollectableFactory.Data_Group_Node_Name));
            SignpostFactory.LoadSignposts(levelData.Element(Terrain_Data_Element).Element(SignpostFactory.Data_Group_Node_Name));
            FlagFactory.LoadFlags(levelData.Element(Terrain_Data_Element).Element(FlagFactory.Data_Group_Node_Name));

            Player = CharacterFactory.LoadPlayer(levelData.Element(CharacterFactory.Player_Data_Node_Name));
            Player.Map = Map;

            CreateBackground(levelData.Element(Background_Data_Element));

            _registerGameObject(Map);
        }

        private XElement LoadLevelData()
        {
            if (FileManager.FileExists(string.Concat(Data.Profile.Settings.Identity, SelectedLevelFile)))
            {
                return FileManager.LoadXMLFile(string.Concat(Data.Profile.Settings.Identity, SelectedLevelFile)).Element(Data_Root_Element);
            }
            else
            {
                return FileManager.LoadXMLContentFile(string.Concat(Content_Path, SelectedLevelFile)).Element(Data_Root_Element);
            }
        }

        private void WireUpCallElementFactoryCallbacks()
        {
            BlockFactory.TimerTickHandler = _registerTimerTick;
            CharacterFactory.TimerTickHandler = _registerTimerTick;

            CharacterFactory.ObjectRegistrationHandler = _registerGameObject;
            CollectableFactory.ObjectRegistrationHandler = _registerGameObject;
            SignpostFactory.ObjectRegistrationHandler = _registerGameObject;
            FlagFactory.ObjectRegistrationHandler = _registerGameObject;
        }

        private string SelectedLevelFile
        {
            get
            {
                if (Profile.PlayingRaceMode) { return string.Format("{0}/{1}/RaceStage.xml", Level_Data_Path_Base, RaceAreaName); }
                return string.Format("{0}/{1}/Survival/{2}.xml", Level_Data_Path_Base, Profile.CurrentAreaData.Name, Profile.CurrentAreaData.LastSelectedLevel);
            }
        }

        private int GetLapsForRace(XElement levelData)
        {
            if (levelData.Element(Race_Data_Element) == null)
            {
                throw new Exception("Race data has not been defined for current level");
            }

            return (int)levelData.Element(Race_Data_Element);
        }

        private void CreateBackground(XElement backgroundData)
        {
            Background inGameBackground = new Background();
            inGameBackground.TextureReference = backgroundData.Attribute("texture").Value;
            inGameBackground.TargetArea = new Rectangle(0,0, BackgroundDimensions.X, BackgroundDimensions.Y);
            _registerGameObject(inGameBackground);
        }

        public void ReinstateDynamicObjects(XElement serializedData)
        {
            WireUpCallElementFactoryCallbacks();

            BlockFactory.ReinstateSerializedBlocks(GetSerializedFactoryData(serializedData, BlockFactory.Serialized_Data_Identifier));
            CollectableFactory.ReinstateSerializedCollectables(GetSerializedFactoryData(serializedData, CollectableFactory.Serialized_Data_Identifier));
            FlagFactory.ReinstateSerializedFlags(GetSerializedFactoryData(serializedData, FlagFactory.Serialized_Data_Identifier));
            SignpostFactory.ReinstateSerializedSignposts(GetSerializedFactoryData(serializedData, SignpostFactory.Signpost_Serialized_Data_Identifier));
            SignpostFactory.ReinstateSerializedRouteMarkers(GetSerializedFactoryData(serializedData, SignpostFactory.Route_Marker_Serialized_Data_Identifier));

            _registerGameObject(new Background());

            Player = CharacterFactory.ReinstatePlayer();
        }

        private List<XElement> GetSerializedFactoryData(XElement serializedData, string identifierTag)
        {
            return (from el
                    in serializedData.Elements()
                    where el.Attribute("id").Value.ToString().StartsWith(identifierTag)
                    select el).ToList();
        }

        private const string Level_Data_Path_Base = "/Files/Levels";
        private const string Content_Path = "Content";
        private const string Data_Root_Element = "leveldata";
        private const string Background_Data_Element = "background";
        private const string Terrain_Data_Element = "terrain";
        private const string Player_Data_Element = "player";
        private const string Race_Data_Element = "race-laps";
    }
}

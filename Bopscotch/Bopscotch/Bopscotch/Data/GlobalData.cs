using System.Xml.Linq;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;

namespace Bopscotch.Data
{
    public sealed class GlobalData : ISerializable
    {
        private static GlobalData _instance = null;

        public static GlobalData Instance
        {
            get
            {
                if (_instance == null) { _instance = new GlobalData(); }
                return _instance;
            }
        }

        public string ID { get; set; }

        public bool PlayingInRaceMode { get; set; }
        public int SurvivalModeHighestLevel { get; set; }
        public int RaceModeHighestLevel { get; set; }
        public int SelectedLevel { get; set; }

        public bool CalibrationMode { get { return false; } }

        public string SelectedLevelFile 
        { 
            get 
            {
                if (CalibrationMode) { return "Content/Files/Levels/Calibrator.xml"; }
                else if (PlayingInRaceMode) { return string.Concat(Race_Level_Data_Path, SelectedLevel, ".xml"); }
                else { return string.Concat(Survival_Level_Data_Path, SelectedLevel, ".xml"); }
            } 
        }

        public GlobalData()
        {
            ID = "global-data";

            SurvivalModeHighestLevel = 0;
            RaceModeHighestLevel = 0;

            PlayingInRaceMode = true;
        }

        public void SetForNewGame()
        {
            SelectedLevel = 0;
        }

        public XElement Serialize()
        {
            Serializer serializer = new Serializer(this);

            serializer.AddDataItem("selected-level", SelectedLevel);

            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Serializer serializer = new Serializer(serializedData);

            SelectedLevel = serializer.GetDataItem<int>("selected-level");
        }

        public const string Survival_Level_Data_Path = "Content/Files/Levels/Survival/";
        public const string Race_Level_Data_Path = "Content/Files/Levels/Race/";
    }
}

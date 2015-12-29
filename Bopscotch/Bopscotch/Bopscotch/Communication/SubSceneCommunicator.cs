namespace Bopscotch.Communication
{
    public class SubSceneCommunicator : ICommunicator
    {
        public Data.RacePlayerCommunicationData OwnPlayerData { get; set; }
        public Data.RacePlayerCommunicationData OtherPlayerData { get { return OtherPlayerDataSource.OwnPlayerData; } }

        public SubSceneCommunicator OtherPlayerDataSource { private get; set; }
        public bool ConnectionLost { get { return false; } }

        public SubSceneCommunicator()
        {
        }

        public void Update()
        {
        }
    }
}

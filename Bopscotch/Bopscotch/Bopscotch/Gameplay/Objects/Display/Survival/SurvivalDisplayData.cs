using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;

using Bopscotch.Data;

namespace Bopscotch.Gameplay.Objects.Display.Survival
{
    public class SurvivalDataDisplay : StatusText
    {
        public SurvivalLevelData CurrentLevelData 
        {
            private get { return (SurvivalLevelData)_currentLevelData; }
            set { _currentLevelData = value; }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            TextWriter.Write(string.Concat("Score: ", TotalScore), spriteBatch, _position, Color.White, Color.Black, Outline_Thickness, 
                Text_Scale, Render_Depth, TextWriter.Alignment.Left);

            TextWriter.Write(string.Concat("Level: ", GlobalData.Instance.SelectedLevel + 1), spriteBatch, _position + new Vector2(0.0f, Line_Height), 
                Color.White, Color.Black, Outline_Thickness, Text_Scale, Render_Depth, TextWriter.Alignment.Left);
        }

        private long TotalScore
        {
            get { return Profile.StartingScoreForLevel(GlobalData.Instance.SelectedLevel) + CurrentLevelData.PointsScoredThisLevel; }
        }
    }
}

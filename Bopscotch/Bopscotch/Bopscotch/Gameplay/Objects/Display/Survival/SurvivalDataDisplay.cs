﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;

using Bopscotch.Data;
using Bopscotch.Interface;

namespace Bopscotch.Gameplay.Objects.Display.Survival
{
    public class SurvivalDataDisplay : StatusDisplay
    {
        private long _displayedScore;

        public SurvivalLevelData CurrentLevelData { private get; set; }
        public bool FreezeDisplayedScore { private get; set; }

        public override void Draw(SpriteBatch spriteBatch)
        {
            TextWriter.Write(string.Concat(Translator.Translation("Score:"), " ", TotalScore), spriteBatch, _position, Color.White, Color.Black, 
                Outline_Thickness, Text_Scale, Render_Depth, TextWriter.Alignment.Left);

            TextWriter.Write(string.Concat(Translator.Translation("Level:"), " ", Profile.CurrentAreaData.LastSelectedLevel + 1), spriteBatch, 
                _position + new Vector2(0.0f, Line_Height), Color.White, Color.Black, Outline_Thickness, Text_Scale, Render_Depth, TextWriter.Alignment.Left);

            spriteBatch.Draw(TextureManager.Textures[Golden_Ticket_Texture], _position + new Vector2(0.0f, Line_Height * 2.3f), null, Color.White, 0.0f,
                Vector2.Zero, 1.0f, SpriteEffects.None, Render_Depth);

            TextWriter.Write(Translator.Translation("unit-count").Replace("[QUANTITY]", Profile.GoldenTickets.ToString()), spriteBatch,
                _position + new Vector2(TextureManager.Textures[Golden_Ticket_Texture].Width, Line_Height * 2.0f), Color.White, Color.Black,
                Outline_Thickness, Text_Scale, Render_Depth, TextWriter.Alignment.Left);

            base.Draw(spriteBatch);
        }

        private long TotalScore
        {
            get
            {
                if (!FreezeDisplayedScore) { _displayedScore = Profile.CurrentAreaData.ScoreAtCurrentLevelStart + CurrentLevelData.PointsScoredThisLevel; }
                return _displayedScore;
            }
        }

        private const string Golden_Ticket_Texture = "golden-ticket";
    }
}

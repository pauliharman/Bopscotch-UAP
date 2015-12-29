﻿using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Bopscotch.Data.Avatar;
using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Dialogs.EditCharacter
{
    public class ComponentSetSelectionCarouselDialog : CarouselDialog
    {
        private List<AvatarComponentSet> _selectableComponentSets;

        public AvatarComponentSet SelectedComponentSet { get { return _selectableComponentSets[SelectedItem]; } }

        public ComponentSetSelectionCarouselDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            _selectableComponentSets = new List<AvatarComponentSet>();

            Height = Dialog_Height;
            TopYWhenActive = (Definitions.Back_Buffer_Height * (1.0f - Data.Profile.Settings.DisplaySafeAreaFraction)) - (Dialog_Height + Bottom_Margin);
            CarouselCenter = new Vector2(Definitions.Back_Buffer_Center.X, Carousel_Center_Y);
            CarouselRadii = new Vector2(Carousel_Horizontal_Radius, Carousel_Vertical_Radius);
            _itemRenderDepths = new Range(Minimum_Item_Render_Depth, Maximum_Item_Render_Depth);
            _itemScales = new Range(Minimum_Item_Scale, Maximum_Item_Scale);

            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 360), Button.ButtonIcon.B, Color.Red, 0.7f);
            AddButton("Change", new Vector2(Definitions.Right_Button_Column_X, 360), Button.ButtonIcon.A, Color.LawnGreen, 0.7f);

            AButtonPressedValue = "Change";

            ActionButtonPressHandler = HandleActionButtonPress;
            TopYWhenInactive = Definitions.Back_Buffer_Height;

            registrationHandler(this);
        }

        private void HandleActionButtonPress(string action)
        {
            DismissWithReturnValue(action);
        }

        public override void Activate()
        {
            FlushItems();
            _selectableComponentSets.Clear();

            base.Activate();

            AddAvailableComponentTypes();

            InitializeForSpin();
        }

        private void AddAvailableComponentTypes()
        {
            foreach (KeyValuePair<string, AvatarComponentSet> kvp in AvatarComponentManager.ComponentSets)
            {
                if (kvp.Value.HasUnlockedComponents) { AddComponentSetDisplaySkeleton(kvp.Value); }
            }
        }

        private void AddComponentSetDisplaySkeleton(AvatarComponentSet componentSet)
        {
            ComponentSetDisplayAvatar avatar = new ComponentSetDisplayAvatar(componentSet.Name, 0.0f);
            avatar.CreateBonesFromDataManager(componentSet.DisplaySkeleton);
            avatar.Name = componentSet.DisplaySkeleton;

            if (componentSet.Name != "body") { avatar.Components.Add(AvatarComponentManager.Component("body", "Blue")); }
            avatar.Components.Add((from c in componentSet.Components where c.Unlocked == true select c).First());
            avatar.SkinFromComponents();
            avatar.RenderLayer = RenderLayer;
            avatar.Annotation = componentSet.Name;

            AddItem(avatar);

            _selectableComponentSets.Add(componentSet);
        }

        private const float Carousel_Center_Y = 160.0f;
        private const float Carousel_Horizontal_Radius = 300.0f;
        private const float Carousel_Vertical_Radius = 50.0f;
        private const float Maximum_Item_Render_Depth = 0.1f;
        private const float Minimum_Item_Render_Depth = 0.05f;
        private const float Maximum_Item_Scale = 1.0f;
        private const float Minimum_Item_Scale = 0.75f;

        private const int Dialog_Height = 450;
        private const float Bottom_Margin = 20.0f;
    }
}

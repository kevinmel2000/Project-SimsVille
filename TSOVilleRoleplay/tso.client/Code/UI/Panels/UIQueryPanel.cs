﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSOVille.Code.UI.Framework;
using TSOVille.Code.UI.Controls;
using Microsoft.Xna.Framework.Graphics;
using TSOVille.LUI;
using Microsoft.Xna.Framework;
using TSO.Common.rendering.framework.model;
using TSO.Simantics;
using TSO.Files.formats.iff.chunks;
using TSO.HIT;
using TSOVille.Code.UI.Model;
using tso.world;
using tso.world.components;

namespace TSOVille.Code.UI.Panels
{
    public class UIQueryPanel : UIContainer
    {

        public Texture2D BackgroundImageCatalog { get; set; }
        public Texture2D BackgroundImageTrade { get; set; }
        public Texture2D BackgroundImagePanel { get; set; }
        public UIImage QuerybackPanel;
        public UIImage QuerybackCatalog;
        public UIImage QuerybackTrade;

        public UIButton GeneralTabButton { get; set; }
        public UIButton SpecificTabButton { get; set; }

        //general tab only
        public Texture2D ImageDescriptionBackground { get; set; }
        public Texture2D ImageMotivesBackground { get; set; }
        public Texture2D ImageGeneralTab { get; set; }

        public UIImage DescriptionBackgroundImage;
        public UIImage MotivesBackgroundImage;
        public UIImage GeneralTabImage;

        public UITextEdit DescriptionText { get; set; }
        public UISlider DescriptionSlider { get; set; }
        public UIButton DescriptionScrollUpButton { get; set; }
        public UIButton DescriptionScrollDownButton { get; set; }

        public UITextEdit MotivesText { get; set; }
        public UISlider MotivesSlider { get; set; }
        public UIButton MotivesScrollUpButton { get; set; }
        public UIButton MotivesScrollDownButton { get; set; }

        //specific tab only
        public Texture2D ImageSpecificTab { get; set; }
        public UIImage SpecificTabImage;

        public UILabel ObjectIsBrokenText { get; set; }
        public UILabel ObjectNeedsUpgradeText { get; set; }
        public UILabel ObjectIsDisabledText { get; set; }

        public UIProgressBar WearProgressBar { get; set; }
        public UILabel WearValueText { get; set; }
        public UILabel WearLabelText { get; set; }

        public UITextEdit ForSalePrice { get; set; }

        public UIButton SellBackButton { get; set; }
        public UIButton InventoryButton { get; set; }

        public UILabel ObjectNameText { get; set; }
        public UILabel ObjectOwnerText { get; set; }
        public UILabel ObjectValueText { get; set; }
        public UILabel ObjectCrafterText { get; set; }

        //async sale stuff, as in "once I know what this actually does I'll care"

        public UIButton AsyncSaleButton { get; set; }
        public UIButton AsyncCancelSaleButton { get; set; }
        public UIButton AsyncEditPriceButton { get; set; }
        public UIButton AsyncBuyButton { get; set; }

        public Texture2D GeneralOwnerPriceBack { get; set; }
        public UIImage OwnerPriceBack;

        public event ButtonClickDelegate OnSellBackClicked;

        //world required for drawing thumbnails
        public World World;
        public UIImage Thumbnail;

        private bool _Active;
        public bool Active
        {
            set
            {
                if (_Active != value) HITVM.Get().PlaySoundEvent(UISounds.Whoosh);
                _Active = value;
            }
            get
            {
                return _Active;
            }
        }

        private int _Tab;
        public int Tab
        {
            set
            {
                //0:general, 1:specific, 2:trade
                DescriptionBackgroundImage.Visible = (value == 0);
                MotivesBackgroundImage.Visible = (value == 0);
                GeneralTabImage.Visible = (value == 0);

                DescriptionText.Visible = (value == 0);
                DescriptionSlider.Visible = (value == 0);
                DescriptionScrollUpButton.Visible = (value == 0);
                DescriptionScrollDownButton.Visible = (value == 0);

                MotivesText.Visible = (value == 0);
                MotivesSlider.Visible = (value == 0);
                MotivesScrollUpButton.Visible = (value == 0);
                MotivesScrollDownButton.Visible = (value == 0);

                GeneralTabButton.Selected = (value == 0);

                //specific value only
                SpecificTabImage.Visible = (value == 1);

                //TODO: detect these cases and display where relevant
                ObjectIsBrokenText.Visible = false;
                ObjectNeedsUpgradeText.Visible = false;
                ObjectIsDisabledText.Visible = false;

                SpecificTabButton.Selected = (value == 1);

                //null for some reason?
                //WearProgressBar.Visible = (value == 1);
                WearValueText.Visible = (value == 1);
                WearLabelText.Visible = (value == 1);

                ForSalePrice.Visible = (value == 1);

                SellBackButton.Visible = (value == 1);
                InventoryButton.Visible = (value == 1);

                ObjectNameText.Visible = (value == 1);
                ObjectOwnerText.Visible = (value == 1);
                ObjectValueText.Visible = (value == 1);
                ObjectCrafterText.Visible = (value == 1);

                //async sale stuff, as in "once I know what this actually does I'll care"

                AsyncSaleButton.Visible = false;
                AsyncCancelSaleButton.Visible = false;
                AsyncEditPriceButton.Visible = false;
                AsyncBuyButton.Visible = false;

                //uh..
                OwnerPriceBack.Visible = false;
                _Tab = value;
                UpdateImagePosition();
            }
            get
            {
                return _Tab;
            }
        }

        private string[] AdStrings;

        private int _Mode;
        public int Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                this.Y = GlobalSettings.Default.GraphicsHeight - ((value == 0) ? 114 : 228);
                QuerybackPanel.Visible = (value == 0);
                QuerybackCatalog.Visible = (value == 1);
                QuerybackTrade.Visible = (value == 2);
                _Mode = value;
            }
        }

        public UIQueryPanel(World world) {
            World = world;
            Active = false;
            Opacity = 0;
            Visible = false;

            AdStrings = new string[14];
            for (int i = 0; i < 14; i++)
            {
                string str = GameFacade.Strings.GetString("206", (i + 4).ToString());
                AdStrings[i] = ((i<7)?str.Substring(0,str.Length-2)+"{0}":str) + "\r\n";
            }

            var script = this.RenderScript("querypanel"+((GlobalSettings.Default.GraphicsWidth < 1024)?"":"1024")+".uis");

            //NOTE: the background and position of this element changes with the context it is used in.
            //other elements that are only used for certain modes will be flagged as such with comments.

            QuerybackPanel = new UIImage(BackgroundImagePanel);
            QuerybackPanel.Y = 0;
            QuerybackPanel.BlockInput();
            this.AddAt(0, QuerybackPanel);

            QuerybackCatalog = new UIImage(BackgroundImageCatalog);
            QuerybackCatalog.Position = new Vector2(-22, 0);
            QuerybackCatalog.BlockInput();
            this.AddAt(1, QuerybackCatalog);

            QuerybackTrade = new UIImage(BackgroundImageTrade);
            QuerybackTrade.Y = 0;
            QuerybackTrade.BlockInput();
            this.AddAt(2, QuerybackTrade);

            //init general tab specific backgrounds

            DescriptionBackgroundImage = new UIImage(ImageDescriptionBackground);
            DescriptionBackgroundImage.Position = new Microsoft.Xna.Framework.Vector2(119, 7);
            this.AddAt(3, DescriptionBackgroundImage);

            MotivesBackgroundImage = new UIImage(ImageMotivesBackground);
            MotivesBackgroundImage.Position = new Microsoft.Xna.Framework.Vector2((GlobalSettings.Default.GraphicsWidth < 1024)?395:619, 7);
            this.AddAt(3, MotivesBackgroundImage);

            GeneralTabImage = new UIImage(ImageGeneralTab);
            GeneralTabImage.Position = new Microsoft.Xna.Framework.Vector2((GlobalSettings.Default.GraphicsWidth < 1024) ? 563 : 787, 0);
            this.AddAt(3, GeneralTabImage);

            SpecificTabImage = new UIImage(ImageSpecificTab);
            SpecificTabImage.Position = new Microsoft.Xna.Framework.Vector2((GlobalSettings.Default.GraphicsWidth < 1024) ? 563 : 787, 0);
            this.AddAt(3, SpecificTabImage);

            OwnerPriceBack = new UIImage(GeneralOwnerPriceBack);
            OwnerPriceBack.Position = new Microsoft.Xna.Framework.Vector2((GlobalSettings.Default.GraphicsWidth < 1024) ? 501 : 725, 80);
            this.AddAt(3, OwnerPriceBack);

            Thumbnail = new UIImage();
            Thumbnail.Position = new Vector2(24, 11);
            Thumbnail.SetSize(90, 90);
            this.Add(Thumbnail);

            DescriptionText.CurrentText = "No Object Selected"; //user should not see this.
            DescriptionSlider.AttachButtons(DescriptionScrollUpButton, DescriptionScrollDownButton, 1);
            DescriptionText.AttachSlider(DescriptionSlider);

            MotivesText.CurrentText = "";
            MotivesSlider.AttachButtons(MotivesScrollUpButton, MotivesScrollDownButton, 1);
            MotivesText.AttachSlider(MotivesSlider);

            GeneralTabButton.OnButtonClick += new ButtonClickDelegate(GeneralTabButton_OnButtonClick);
            SpecificTabButton.OnButtonClick += new ButtonClickDelegate(SpecificTabButton_OnButtonClick);
            SellBackButton.OnButtonClick += new ButtonClickDelegate(SellBackButton_OnButtonClick);

            Mode = 1;
            Tab = 0;
        }

        void SellBackButton_OnButtonClick(UIElement button)
        {
            if (OnSellBackClicked != null) OnSellBackClicked(button);
        }

        void SpecificTabButton_OnButtonClick(UIElement button)
        {
            Tab = 1;
        }

        void GeneralTabButton_OnButtonClick(UIElement button)
        {
            Tab = 0;
        }

        public override void Update(UpdateState state)
        {
            if (Mode == 1)
            {
                if (Active)
                {
                    Visible = true;
                    if (Opacity < 1f) Opacity += 1f / 20f;
                    else
                    {
                        Opacity = 1;
                    }
                }
                else
                {
                    if (Opacity > 0f) Opacity -= 1f / 20f;
                    else
                    {
                        Visible = false;
                        Opacity = 0f;
                    }
                }
            }
            else
            {
                Opacity = 1;
                if (Active) Visible = true;
            }
            base.Update(state);
        }

        public void SetInfo(VMEntity entity, bool bought)
        {
            var obj = entity.Object;
            var def = entity.MasterDefinition;
            if (def == null) def = entity.Object.OBJ;
            CTSS catString = obj.Resource.Get<CTSS>(def.CatalogStringsID);
            if (catString != null)
            {
                DescriptionText.CurrentText = catString.GetString(0) + "\r\n" + catString.GetString(1);
                ObjectNameText.Caption = catString.GetString(0);
            }
            else
            {
                DescriptionText.CurrentText = entity.ToString();
                ObjectNameText.Caption = entity.ToString();
            }
            StringBuilder motivesString = new StringBuilder();
            motivesString.AppendFormat(GameFacade.Strings.GetString("206", "19") + "${0}\r\n", def.Price);
            if (def.RatingHunger != 0) { motivesString.AppendFormat(AdStrings[0], def.RatingHunger); }
            if (def.RatingComfort != 0) { motivesString.AppendFormat(AdStrings[1], def.RatingComfort); }
            if (def.RatingHygiene != 0) { motivesString.AppendFormat(AdStrings[2], def.RatingHygiene); }
            if (def.RatingBladder != 0) { motivesString.AppendFormat(AdStrings[3], def.RatingBladder); }
            if (def.RatingEnergy != 0) { motivesString.AppendFormat(AdStrings[4], def.RatingEnergy); }
            if (def.RatingFun != 0) { motivesString.AppendFormat(AdStrings[5], def.RatingFun); }
            if (def.RatingRoom != 0) { motivesString.AppendFormat(AdStrings[6], def.RatingRoom); }

            var sFlags = def.RatingSkillFlags;
            for (int i = 0; i < 7; i++)
            {
                if ((sFlags & (1 << i)) > 0) motivesString.Append(AdStrings[i+7]);
            }

            MotivesText.CurrentText = motivesString.ToString();
            ObjectOwnerText.Caption = GameFacade.Strings.GetString("206", "24", new string[] { "You" });

            SpecificTabButton.Disabled = !bought;

            if (entity is VMGameObject) {
                var objects = entity.MultitileGroup.Objects;
                ObjectComponent[] objComps = new ObjectComponent[objects.Count];
                for (int i=0; i<objects.Count; i++) {
                    objComps[i] = (ObjectComponent)objects[i].WorldUI;
                }
                var thumb = World.GetObjectThumb(objComps, entity.MultitileGroup.GetBasePositions(), GameFacade.GraphicsDevice);
                if (Thumbnail.Texture != null) Thumbnail.Texture.Dispose();
                Thumbnail.Texture = thumb;
                UpdateImagePosition();
            } else
            {
                if (Thumbnail.Texture != null) Thumbnail.Texture.Dispose();
                Thumbnail.Texture = null;
            }
        }

        private void UpdateImagePosition() {
            var thumb = Thumbnail.Texture;
            if (thumb == null) return;
            float scale = Math.Min(1, 90f / Math.Max(thumb.Height, thumb.Width));
            Thumbnail.SetSize(thumb.Width * scale, thumb.Height * scale);
            var baseLoc = (Tab == 0) ? new Vector2(24, 11) : new Vector2(64, 11);
            Thumbnail.Position = baseLoc + new Vector2(45 - (thumb.Width * scale / 2), 45 - (thumb.Height * scale / 2));
        }
    }
    
}
﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the TSOVille.

The Initial Developer of the Original Code is
ddfczm. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSOVille.Code.UI.Framework;
using Microsoft.Xna.Framework.Graphics;
using TSOVille.Code.UI.Controls;
using TSOVille.LUI;
using TSOVille.Code.UI.Framework.Parser;
using TSOVille.Code.Utils;
using System.IO;
using TSOVille.Code.UI.Screens;
using TSO.Common.rendering.framework.model;
using TSOVille.Network;
using TSO.Vitaboy;
using TSO.Content;
using tso.world.model;

namespace TSOVille.Code.UI.Panels
{
    public class UIGizmoPropertyFilters : UIContainer
    {
        public UIImage Background;

        public UIGizmoPropertyFilters(UIScript script, UIGizmo parent)
        {
            Background = script.Create<UIImage>("BackgroundImageFilters");
            this.Add(Background);

            var filterChildren = parent.GetChildren().Where(x => x.ID != null && x.ID.StartsWith("PropertyFilterButton_")).ToList();
            foreach (var child in filterChildren)
            {
                child.Parent.Remove(child);
                this.Add(child);
            }
        }
    }

    public class UIGizmoSearch : UIContainer
    {
        public UISlider SearchSlider { get; set; }
        public UIButton WideSearchUpButton { get; set; }
        public UIButton NarrowSearchButton { get; set; }
        public UIButton SearchScrollUpButton { get; set; }
        public UIButton SearchScrollDownButton { get; set; }
        public UIListBox SearchResult { get; set; }
        public UITextEdit SearchText { get; set; }
        public UILabel NoSearchResultsText { get; set; }


        private UIImage Background;

        public UIGizmoSearch(UIScript script, UIGizmo parent)
        {
            Background = script.Create<UIImage>("BackgroundImageSearch");
            this.Add(Background);
            

            script.LinkMembers(this, true);
        }
    }

    public class UIGizmoTop100 : UIContainer
    {
        public UISlider Top100Slider { get; set; }
        public UIButton Top100ListScrollUpButton { get; set; }
        public UIButton Top100ListScrollDownButton { get; set; }
        public UIButton Top100SubListScrollUpButton { get; set; }
        public UIButton Top100SubListScrollDownButton { get; set; }
        public UIListBox Top100SubList { get; set; }
        public UIListBox Top100ResultList { get; set; }
        public List<string> CharList;
        public UIGizmo Parent;

        private int UpdateCooldown;

        public UIImage Background; //public so we can disable visibility when not selected... workaround to stop background mouse blocking still happening when panel is hidden

        public UIGizmoTop100(UIScript script, UIGizmo parent)
        {
            CharList = new List<string>();

            DirectoryInfo dir = new DirectoryInfo(GlobalSettings.Default.DocumentsPath + "//Characters");
            foreach (var file in dir.GetFiles())
                CharList.Add(Path.GetFileNameWithoutExtension(file.Name));

            Parent = parent;

            Background = script.Create<UIImage>("BackgroundImageTop100Lists");
            this.Add(Background);
            
            script.LinkMembers(this, true);

            Top100Slider.AttachButtons(Top100ListScrollUpButton, Top100ListScrollDownButton, 1);
            Top100ResultList.AttachSlider(Top100Slider);

            if (Parent.TabV == UIGizmoTab.Property)
                populateWithXMLHouses();
            else if (Parent.TabV == UIGizmoTab.People)
                populateWithCharacters();
            UpdateCooldown = 100;
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            if (UpdateCooldown-- < 0)
            {
                if (Parent.TabV == UIGizmoTab.Property)
                    populateWithXMLHouses();
                else if (Parent.TabV == UIGizmoTab.People)
                    populateWithCharacters();
                UpdateCooldown = 100;
            }
        }

        public void populateWithXMLHouses()
        {
            var xmlHouses = new List<UIXMLLotEntry>();

            string[] paths = Directory.GetFiles(@"Content\Blueprints\", "*.xml", SearchOption.AllDirectories);
            for (int i = 0; i < paths.Length; i++)
            {
                string entry = paths[i];
                string filename = Path.GetFileName(entry);
                xmlHouses.Add(new UIXMLLotEntry { Filename = filename, Path = entry });
            }

            paths = Directory.GetFiles(Path.Combine(GlobalSettings.Default.StartupPath, @"housedata\"), "*_00.xml", SearchOption.AllDirectories);
            for (int i=0; i<paths.Length; i++)
            {
                string entry = paths[i];
                string filename = Path.GetFileName(entry);
                xmlHouses.Add(new UIXMLLotEntry { Filename = filename, Path = entry });
            }

            paths = Directory.GetFiles(Path.Combine(GlobalSettings.Default.DocumentsPath, @"Houses\"), "*.xml", SearchOption.AllDirectories);
            for (int i = 0; i < paths.Length; i++)
            {
                string entry = paths[i];
                string filename = Path.GetFileName(entry);
                xmlHouses.Add(new UIXMLLotEntry { Filename = filename, Path = entry });
            }

            Top100ResultList.Items = xmlHouses.Select(x => new UIListBoxItem(x, x.Filename)).ToList();
            Top100ResultList.OnDoubleClick += Top100ItemSelect;
        }

        public void populateWithCharacters()
        {

            if (CharList != null)
            {
                Top100ResultList.Items = CharList.Select(x => new UIListBoxItem(x, x.ToString())).ToList();
                Top100ResultList.OnDoubleClick += Top100ItemSelect;

            }
        }

        private void Top100ItemSelect(UIElement button)
        {

            if (Parent.TabV == UIGizmoTab.Property)
                ((CoreGameScreen)(Parent.Parent)).InitTestLot(((UIXMLLotEntry)Top100ResultList.SelectedItem.Data).Path);
            else if (Parent.TabV == UIGizmoTab.People)
                {
                    Parent.SimBoxSelect(Top100ResultList.SelectedItem.Data.ToString());
                   
                }

        }
    }

 

    public struct UIXMLLotEntry
    {
        public string Filename;
        public string Path;
    }

    public enum UIGizmoTab
    {
        People,
        Property
    }

    public enum UIGizmoView
    {
        Filters,
        Search,
        Top100
    }

    public class UIGizmo : UIContainer
    {
        private UIImage BackgroundImageGizmo;
        private UIImage BackgroundImageGizmoPanel;
        private UIImage BackgroundImagePanel;

        private UIContainer ButtonContainer;

        public UIButton ExpandButton { get; set; }
        public UIButton ContractButton { get; set; }

        public UIButton FiltersButton { get; set; }
        public UIButton SearchButton { get; set; }
        public UIButton Top100ListsButton { get; set; }

        public UIButton PeopleTabButton { get; set; }
        public UIButton HousesTabButton { get; set; }

        public UIGizmoPropertyFilters FiltersProperty;
        public UIGizmoSearch Search;
        public UIGizmoTop100 Top100;

        public UIGizmoTab TabV;

        public XmlCharacter SelectedCharInfo;
        public UISim SimBox;

        public UIGizmo()
        {
            TabV = Tab;

            var ui = this.RenderScript("gizmo.uis");

            BackgroundImageGizmo = ui.Create<UIImage>("BackgroundImageGizmo");
            this.AddAt(0, BackgroundImageGizmo);

            BackgroundImageGizmoPanel = ui.Create<UIImage>("BackgroundImageGizmoPanel");
            this.AddAt(0, BackgroundImageGizmoPanel);

            BackgroundImagePanel = ui.Create<UIImage>("BackgroundImagePanel");
            this.AddAt(0, BackgroundImagePanel);

            UIUtils.MakeDraggable(BackgroundImageGizmo, this);
            UIUtils.MakeDraggable(BackgroundImageGizmoPanel, this);
            UIUtils.MakeDraggable(BackgroundImagePanel, this);

            ButtonContainer = new UIContainer();
            this.Remove(ExpandButton);
            ButtonContainer.Add(ExpandButton);
            this.Remove(ContractButton);
            ButtonContainer.Add(ContractButton);
            this.Remove(FiltersButton);
            ButtonContainer.Add(FiltersButton);
            this.Remove(SearchButton);
            ButtonContainer.Add(SearchButton);
            this.Remove(Top100ListsButton);
            ButtonContainer.Add(Top100ListsButton);
            this.Add(ButtonContainer);

            FiltersProperty = new UIGizmoPropertyFilters(ui, this);
            FiltersProperty.Visible = false;
            this.Add(FiltersProperty);

            Search = new UIGizmoSearch(ui, this);
            Search.Visible = false;
            this.Add(Search);

            Top100 = new UIGizmoTop100(ui, this);
            Top100.Visible = false;
            Top100.Background.Visible = false;
            this.Add(Top100);


            ExpandButton.OnButtonClick += new ButtonClickDelegate(ExpandButton_OnButtonClick);
            ContractButton.OnButtonClick += new ButtonClickDelegate(ContractButton_OnButtonClick);

            PeopleTabButton.OnButtonClick += new ButtonClickDelegate(PeopleTabButton_OnButtonClick);
            HousesTabButton.OnButtonClick += new ButtonClickDelegate(HousesTabButton_OnButtonClick);

            FiltersButton.OnButtonClick += new ButtonClickDelegate(FiltersButton_OnButtonClick);
            SearchButton.OnButtonClick += new ButtonClickDelegate(SearchButton_OnButtonClick);
            Top100ListsButton.OnButtonClick += new ButtonClickDelegate(Top100ListsButton_OnButtonClick);


            View = UIGizmoView.Filters;
            SetOpen(true);

            TabV = UIGizmoTab.Property;
        }

        public void SimBoxSelect(string charname)
        {
            this.Remove(SimBox);
            SimBox = null;
            if (PlayerAccount.CurrentlyActiveSim != null)
                SimBox = new UISim(PlayerAccount.CurrentlyActiveSim.GUID.ToString(), "1");
            else
                SimBox = new UISim("", "");

            string charfile = GlobalSettings.Default.DocumentsPath + "Characters//" + charname + ".xml";
            var sim = NetworkFacade.Data.SelectCharacter(charfile);
            SelectedCharInfo = XmlCharacter.Parse(charfile);

            SimBox = sim;
            SimBox.SimScale = 0.4f;
            SimBox.Position = new Microsoft.Xna.Framework.Vector2(60, 160);


            this.Add(SimBox);



        }

        void Top100ListsButton_OnButtonClick(UIElement button)
        {
            View = UIGizmoView.Top100;
            SetOpen(true);
        }

        void SearchButton_OnButtonClick(UIElement button)
        {
            View = UIGizmoView.Search;
            SetOpen(true);
        }

        void FiltersButton_OnButtonClick(UIElement button)
        {
            View = UIGizmoView.Filters;
            SetOpen(true);
        }

        void HousesTabButton_OnButtonClick(UIElement button)
        {
            TabV = UIGizmoTab.Property;
            SetOpen(true);
        }

        void PeopleTabButton_OnButtonClick(UIElement button)
        {
            TabV = UIGizmoTab.People;
            SetOpen(true);
        }

        void ContractButton_OnButtonClick(UIElement button)
        {
            SetOpen(false);
        }

        void ExpandButton_OnButtonClick(UIElement button)
        {
            SetOpen(true);
        }

        private bool m_Open = false;
        private UIGizmoView View = UIGizmoView.Filters;
        private UIGizmoTab Tab = UIGizmoTab.Property;

        private void SetOpen(bool open)
        {
            m_Open = open;
            Redraw();
        }

        private void Redraw()
        {
            var isOpen = m_Open;
            var isClosed = !m_Open;

            if (isOpen)
            {
                if (SimBox != null)
                SimBox.Position = new Microsoft.Xna.Framework.Vector2(60, 160);
            }
            else
            {
                if (SimBox != null)
                SimBox.Position = new Microsoft.Xna.Framework.Vector2(60, 160);
            }

            PeopleTabButton.Disabled = View == UIGizmoView.Filters;
            FiltersButton.Selected = isOpen && View == UIGizmoView.Filters;
            SearchButton.Selected = isOpen && View == UIGizmoView.Search;
            Top100ListsButton.Selected = isOpen && View == UIGizmoView.Top100;

            ButtonContainer.Y = isOpen ? 6 : 0;

            BackgroundImageGizmo.Visible = isClosed;
            BackgroundImageGizmoPanel.Visible = isOpen;
            BackgroundImagePanel.Visible = isOpen;
            ExpandButton.Visible = isClosed;
            ContractButton.Visible = isOpen;

            FiltersProperty.Visible = false;
            Top100.Visible = false;
            Top100.Background.Visible = false;
            Search.Visible = false;

            PeopleTabButton.Visible = isOpen;
            HousesTabButton.Visible = isOpen;

            if (TabV == UIGizmoTab.People && View == UIGizmoView.Filters)
            {
                View = UIGizmoView.Search;
            }

            if (isOpen)
            {
                switch (View)
                {
                    case UIGizmoView.Filters:
                        FiltersProperty.Visible = true;
                        break;

                    case UIGizmoView.Search:
                        Search.Visible = true;
                        break;

                    case UIGizmoView.Top100:

                        
                        Top100.Visible = true;
                        Top100.Background.Visible = true;
                        
                        break;

                }
                        
            }
        }
    }
}
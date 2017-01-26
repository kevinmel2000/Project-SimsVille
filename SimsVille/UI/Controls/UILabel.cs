﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the TSOVille.

The Initial Developer of the Original Code is
Mats 'Afr0' Vederhus. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TSOVille.Code.UI.Framework;
using TSOVille.Code.UI.Framework.Parser;
using TSOVille.Code.Utils;

namespace TSOVille.Code.UI.Controls
{
    /// <summary>
    /// A drawable label containing text.
    /// </summary>
    public class UILabel : UIElement
    {
        /// <summary>
        /// The font to use when rendering the label
        /// </summary>
        [UIAttribute("font", typeof(TextStyle))]
        public TextStyle CaptionStyle { get; set; }
        private string m_Text = "";

        [UIAttribute("text", DataType=UIAttributeType.StringTable)]
        public string Caption
        {
            get { return m_Text; }
            set { m_Text = value; }
        }

        /// <summary>
        /// If size is set you can make use of alignment settings
        /// </summary>
        [UIAttribute("size")]
        public new virtual Vector2 Size {
            get
            {
                if (m_Size != null)
                {
                    return new Vector2(m_Size.X, m_Size.Y);
                }
                return Vector2.Zero;
            }
            set
            {
                m_Size = new Rectangle(0, 0, (int)value.X, (int)value.Y);
            }
        }
        private Rectangle m_Size;

        public UILabel()
        {
            CaptionStyle = TextStyle.DefaultLabel;
        }

        public TextAlignment Alignment = TextAlignment.Center;

        [UIAttribute("alignment")]
        public int _Alignment
        {
            set
            {
                switch (value)
                {
                    case 3:
                        Alignment = TextAlignment.Center | TextAlignment.Middle;
                        break;
                }
            }
        }


        public override void Draw(UISpriteBatch SBatch)
        {
            if (!Visible)
            {
                return;
            }

            if (m_Text != null && CaptionStyle != null)
            {
                if (m_Size != Rectangle.Empty)
                {
                    DrawLocalString(SBatch, m_Text, Vector2.Zero, CaptionStyle, m_Size, Alignment);
                }
                else
                {
                    DrawLocalString(SBatch, m_Text, Vector2.Zero, CaptionStyle);
                }
            }
        }
    }
}
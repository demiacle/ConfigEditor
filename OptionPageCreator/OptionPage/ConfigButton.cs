﻿using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;

namespace Demiacle.OptionPageCreator.OptionPage {
    internal class ConfigButton : ModOption {
        public ConfigButton( string label, ModOptionsWindow page ) : base ( label, page ){
        }

        public override void receiveLeftClick( int x, int y ) {
            page.changePageTo( label );
        }

        /// <summary>
        /// Draws the option.
        /// </summary>
        /// <param name="slotX">Unused</param>
        /// <param name="slotY">Unused</param>
        public override void draw( SpriteBatch b, int slotX, int slotY ) {
            b.Draw( Game1.staminaRect, new Rectangle( bounds.X, bounds.Y + bounds.Height, bounds.Width, 1 ), Color.IndianRed * 0.2f );
            SpriteText.drawString( b, label, bounds.X, bounds.Y + 4 );
        }

    }
}
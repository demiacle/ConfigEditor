using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace Demiacle.OptionPageCreator.OptionPage {
    public class Checkbox : ModOption {

        public static Rectangle sourceRectUnchecked = new Rectangle( 227, 425, 9, 9 );
        public static Rectangle sourceRectChecked = new Rectangle( 236, 425, 9, 9 );
        public const int pixelsWide = 9;
        public bool isChecked;

        /// <summary>
        /// This class creates a checkbox to be used in the ModOptionsPage
        /// </summary>
        public Checkbox( string label, bool isChecked ) : base( label ) {
            this.isChecked = isChecked;
            bounds.Width = 9 * Game1.pixelZoom;
            bounds.Height = 9* Game1.pixelZoom;
        }

        /// <summary>
        /// Changes the checkbox and updates ModData
        /// </summary>
        public override void receiveLeftClick( int x, int y ) {
            if( greyedOut || bounds.Contains( x, y ) == false ) {
                return;
            }
            
            Game1.playSound( "drumkit6" );
            isChecked = !isChecked;
        }

        public override void draw( SpriteBatch b ) {
            b.Draw( Game1.mouseCursors, new Vector2( bounds.X, bounds.Y  ), new Rectangle?( isChecked ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked ), Color.White * ( greyedOut ? 0.33f : 1f ), 0.0f, Vector2.Zero, ( float ) Game1.pixelZoom, SpriteEffects.None, 0.4f );
            int paddingX = 8;
            b.DrawString( Game1.smallFont, prettyLabel, new Vector2( bounds.X + bounds.Width + paddingX, bounds.Y + 8 ), Color.Black);
        }

        public override string getHoverText() {
            if( isChecked ) {
                return "true";
            } else {
                return "false";
            }
        }

    }
}

using Demiacle.OptionPageCreator.OptionPage;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using Microsoft.Xna.Framework;

namespace Demiacle.OptionPageCreator.OptionPage {
    internal class ButtonChooser : ModOption {

        private string value;

        public ButtonChooser( string label, string value )
          : base( label ) {
            this.value = value;

        }

        public override void draw( SpriteBatch b, int slotX, int slotY ) {
            b.DrawString( Game1.smallFont, label, new Vector2( bounds.X + bounds.Width, bounds.Y + 8 ), Color.Black);
        }

        public override void receiveLeftClick( int x, int y ) {
            if( listeningToKey == true ) {
                return;
            }
            listeningToKey = true;
        }

        public override void receiveKeyPress( Keys key ) {
            if( listeningToKey ) {
                value = key.ToString();
            }
        }

    }
}
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using Microsoft.Xna.Framework;

namespace Demiacle.OptionPageCreator.OptionPage {
    internal class ButtonChooser : ModOption {

        public string value;
        private string message;

        public ButtonChooser( string label, string value )
          : base( label ) {
            this.value = value;
            message = value;
        }

        public override void draw( SpriteBatch b ) {
            b.DrawString( Game1.smallFont, prettyLabel, new Vector2( bounds.X, bounds.Y + 8 ), Color.Black );
            b.DrawString( Game1.smallFont, message, new Vector2( bounds.X + bounds.Width - Game1.smallFont.MeasureString( message ).X, bounds.Y + 8 ), Color.Black );
        }

        public override void receiveLeftClick( int x, int y ) {
            if( bounds.Contains( x, y ) && listeningToKey == false ) {
                listeningToKey = true;
                message = "Press new key...";
                return;
            }

            listeningToKey = false;
            message = value;
        }

        public Keys getBoundedKey() {
            return ( Keys ) Enum.Parse( typeof( Keys ), value );
        }

        public override void receiveKeyPress( Keys key ) {
            if( listeningToKey ) {

                string modsThatUseKey = page.getModsThatUseKey( key );
                if( modsThatUseKey != "" ) {
                    page.setError( $"{modsThatUseKey} is already using {key.ToString()}" );
                } else {
                    value = key.ToString();
                    message = value;
                }

                listeningToKey = false;
            }
        }

        public override string getHoverText() {
            if( listeningToKey == false ) {
                return "Click to change keybind";
            } else {
                return "";
            }
        }
    }
}
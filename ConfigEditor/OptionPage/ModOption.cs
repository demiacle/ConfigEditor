using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Text;

namespace Demiacle.OptionPageCreator.OptionPage {
    public class ModOption {

        public const int defaultX = 8;
        public const int defaultY = 4;
        public const int defaultPixelWidth = 9;
        public Rectangle bounds;
        public string label;
        public string prettyLabel;
        public bool greyedOut;
        public Action toggleOptionDelegate;
        public const int BUTTON_HEIGHT = 50;
        public bool listeningToKey = false;
        protected ModOptionsWindow page;

        /// <summary>
        /// The base class for all custom ModOptions. Width and height are automatically set and dependant upon the page.
        /// </summary>
        public ModOption( string label ) {
            this.label = label;
            this.prettyLabel = beutifyString( label );
            this.bounds = new Rectangle( 0,0, ModOptionsWindow.WIDTH - 100, BUTTON_HEIGHT );
        }

        public string beutifyString( string label ) {
            StringBuilder newText = new StringBuilder( label.Length * 2 );
            newText.Append( Char.ToUpper( label[ 0 ] ) );

            // Convert camel case into properly formated sentence
            for( int i = 1; i < label.Length; i++ ) {
                if( char.IsUpper( label[ i ] ) && label[ i - 1 ] != '.' && char.IsUpper( label[ i - 1 ] ) == false ) {
                    newText.Append( ' ' );
                }
                if( label[ i ] == '.' ) {
                    newText.Append( ": " );
                } else {
                    if( label[ i - 1 ] == '.' ) {
                        newText.Append( Char.ToUpper( label[ i ] ) );
                    } else {
                        newText.Append( Char.ToLower( label[ i ] ) );
                    }
                }
            }

            return newText.ToString();
        }

        public void setPage( ModOptionsWindow page ) {
            this.page = page;
        }

        public virtual void receiveLeftClick( int x, int y ) {
        }

        public virtual void leftClickHeld( int x, int y ) {
        }

        public virtual void leftClickReleased( int x, int y ) {
        }

        public virtual void receiveKeyPress( Keys key ) {
        }

        public virtual void draw( SpriteBatch b ) {
        }

        public virtual void update( GameTime time ) { 
        }

        public virtual string getHoverText() {
            return "";
        }
    }
}

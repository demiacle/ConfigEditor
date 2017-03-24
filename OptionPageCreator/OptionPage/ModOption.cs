using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
 
namespace Demiacle.OptionPageCreator.OptionPage {
    public class ModOption {

        public const int defaultX = 8;
        public const int defaultY = 4;
        public const int defaultPixelWidth = 9;
        public Rectangle bounds;
        public string label;
        public int whichOption;
        public bool greyedOut;
        public Action toggleOptionDelegate;
        protected ModOptionsWindow page;
        public const int BUTTON_HEIGHT = 50;
        protected bool listeningToKey = false;
        /// <summary>
        /// The base class for all custom ModOptions
        /// </summary>
        public ModOption( string label, ModOptionsWindow page ) {
            this.page = page;
            this.label = label;
            this.bounds = new Rectangle( 0,0, page.width - 100, BUTTON_HEIGHT );
            this.whichOption = -1;
        }

        public ModOption( string label ) : this ( label, null ) {
        }

        public ModOption( string label, int x, int y, int width, int height, int whichOption = -1 ) {

            if( x == -1 )
                x = 8 * Game1.pixelZoom;
            if( y == -1 )
                y = 4 * Game1.pixelZoom;

            this.bounds = new Rectangle( x, y, width, height );
            this.label = label;
            this.whichOption = whichOption;
        }

        public ModOption( string label, Rectangle bounds, int whichOption ) {

            this.whichOption = whichOption;
            this.label = label;
            this.bounds = bounds;
        }

        public virtual void receiveLeftClick( int x, int y ) {
        }

        public virtual void leftClickHeld( int x, int y ) {
        }

        public virtual void leftClickReleased( int x, int y ) {
        }

        public virtual void receiveKeyPress( Keys key ) {
        }

        public virtual void draw( SpriteBatch b, int slotX, int slotY ) {
        }
    }
}

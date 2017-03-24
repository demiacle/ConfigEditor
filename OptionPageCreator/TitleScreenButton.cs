using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using Demiacle.OptionPageCreator.OptionPage;

namespace Demiacle.OptionPageCreator  {
    class TitleScreenButton : IClickableMenu {

        public Rectangle bounds = new Rectangle();

        private void setDefaultVariables() {
            width = 168;
            height = 70;
            int offsetX = 20;
            int offsetY = -32;
            xPositionOnScreen = offsetX;
            yPositionOnScreen = Game1.viewport.Height - height + offsetY;

            bounds.Width = width;
            bounds.Height = height;
            bounds.X = xPositionOnScreen;
            bounds.Y = yPositionOnScreen;
        }

        public override void receiveRightClick( int x, int y, bool playSound = true ) {
        }

        public override void receiveLeftClick( int x, int y, bool playSound = true ) {
            base.receiveLeftClick( x, y, playSound );
        }

        public override void draw( SpriteBatch b ) {
            base.draw( b );

            // Find position every draw in case viewport is changed... lazy lazy
            setDefaultVariables();

            if ( bounds.Contains( Game1.getMouseX(), Game1.getMouseY() ) ) {
                IClickableMenu.drawTextureBox( Game1.spriteBatch, xPositionOnScreen - 4, yPositionOnScreen - 4, width + 8, height + 8, Color.White );
            } else {
                IClickableMenu.drawTextureBox( Game1.spriteBatch, xPositionOnScreen, yPositionOnScreen, width, height, Color.White );
            }
            Game1.spriteBatch.DrawString( Game1.smallFont, "Mod Config", new Vector2( xPositionOnScreen + 20, yPositionOnScreen + 24 ), Color.Black );
            drawMouse( b );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace Demiacle.OptionPageCreator.OptionPage {
    public class ModOptionsWindow : IClickableMenu {

        private const int WIDTH = 800;

        private Dictionary<string, List<ModOption>> modConfigs = new Dictionary<string, List<ModOption>>();

        private List<ModOption> currentOptions = new List<ModOption>();
        private List<ModOption> loadedMods = new List<ModOption>();

        private string hoverText = "";
        private int currentItemIndex;

        private bool scrolling;
        private ClickableTextureComponent upArrow;
        private ClickableTextureComponent downArrow;
        private ClickableTextureComponent scrollBar;
        private Rectangle scrollBarRunner;
        private int visibleOptions; // calculate this base on window height
        private Vector2 innerTopLeftCorner;

        private Rectangle hoverHighlighter = new Rectangle();


        // TODO abstract arrows scroll bar and scrollbar runner out of this

        /// <summary>
        /// Base class of ModOptionsPage, mostly copy paste
        /// </summary>
        public ModOptionsWindow ( List<ModConfig> loadedConfigs )
          : base( Game1.activeClickableMenu.xPositionOnScreen, Game1.activeClickableMenu.yPositionOnScreen + 10, WIDTH, Game1.activeClickableMenu.height ) {
            upArrow = new ClickableTextureComponent( new Rectangle( xPositionOnScreen + width + Game1.tileSize / 4, yPositionOnScreen + Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom ), Game1.mouseCursors, new Rectangle( 421, 459, 11, 12 ), ( float ) Game1.pixelZoom, false );
            downArrow = new ClickableTextureComponent( new Rectangle( xPositionOnScreen + width + Game1.tileSize / 4, yPositionOnScreen + height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom ), Game1.mouseCursors, new Rectangle( 421, 472, 11, 12 ), ( float ) Game1.pixelZoom, false );
            scrollBar = new ClickableTextureComponent( new Rectangle( upArrow.bounds.X + Game1.pixelZoom * 3, upArrow.bounds.Y + upArrow.bounds.Height + Game1.pixelZoom, 6 * Game1.pixelZoom, 10 * Game1.pixelZoom ), Game1.mouseCursors, new Rectangle( 435, 463, 6, 10 ), ( float ) Game1.pixelZoom, false );
            scrollBarRunner = new Rectangle( scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + Game1.pixelZoom, scrollBar.bounds.Width, height - Game1.tileSize * 2 - upArrow.bounds.Height - Game1.pixelZoom * 2 );

            // Split loadedMods into a usable dictionary and create a list of buttons for all loaded mods to display as the default screen
            foreach( var item in loadedConfigs ) {
                modConfigs.Add( item.configName, item.options );
                loadedMods.Add( new ConfigButton( item.configName, this ) );
            }

            changePageToModConfigList();
            calculateVisibleOptions();
            yPositionOnScreen = 0;
            innerTopLeftCorner = new Vector2( 16, 116 );
        }

        internal void changePageTo( string label ) {
            currentOptions = modConfigs[ label ];
            calculateVisibleOptions();
        }

        internal void changePageToModConfigList() {
            currentOptions = loadedMods;
            calculateVisibleOptions();
        }

        internal void calculateVisibleOptions() {
            visibleOptions = Math.Min( currentOptions.Count, 10 );
        }

        private void setScrollBarToCurrentIndex() {
            if( currentOptions.Count <= 0 )
                return;
            scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max( 1, currentOptions.Count - 7 + 1 ) * currentItemIndex + upArrow.bounds.Bottom + Game1.pixelZoom;
            if( currentItemIndex != currentOptions.Count - 7 )
                return;
            scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - Game1.pixelZoom;

        }


        /// <summary>
        /// Handles left click held down. Currently only usable effects scrollbars.
        /// </summary>
        public override void leftClickHeld( int x, int y ) {
            if( GameMenu.forcePreventClose )
                return;

            base.leftClickHeld( x, y );

            if( scrolling ) {
                int y1 = scrollBar.bounds.Y;
                scrollBar.bounds.Y = Math.Min( yPositionOnScreen + height - Game1.tileSize - Game1.pixelZoom * 3 - scrollBar.bounds.Height, Math.Max( y, yPositionOnScreen + upArrow.bounds.Height + Game1.pixelZoom * 5 ) );
                currentItemIndex = Math.Min( currentOptions.Count - 7, Math.Max( 0, ( int ) ( ( double ) currentOptions.Count * ( double ) ( ( float ) ( y - scrollBarRunner.Y ) / ( float ) scrollBarRunner.Height ) ) ) );
                setScrollBarToCurrentIndex();
                int y2 = scrollBar.bounds.Y;
                if( y1 == y2 )
                    return;
                Game1.playSound( "shiny4" );
            }
        }

        public override void receiveKeyPress( Keys key ) {
            // Handle joystick navigation

            // Let options handle keypress if they need to
            foreach( var item in currentOptions ) {
                item.receiveKeyPress( key );
            }
        }

        public override void receiveScrollWheelAction( int direction ) {
            if( GameMenu.forcePreventClose )
                return;

            base.receiveScrollWheelAction( direction );
            
            if( direction > 0 && currentItemIndex > 0 ) {
                upArrowPressed();
                Game1.playSound( "shiny4" );
            } else {
                if( direction >= 0 || currentItemIndex >= Math.Max( 0, currentOptions.Count - visibleOptions ) ) {
                    return;
                }
                downArrowPressed();
                Game1.playSound( "shiny4" );
            }
        }

        public override void releaseLeftClick( int x, int y ) {
            if( GameMenu.forcePreventClose ){
                return;
            }

            base.releaseLeftClick( x, y );
            scrolling = false;
        }

        private void downArrowPressed() {
            downArrow.scale = downArrow.baseScale;
            currentItemIndex = currentItemIndex + 1;
            setScrollBarToCurrentIndex();
        }

        private void upArrowPressed() {
            upArrow.scale = upArrow.baseScale;
            currentItemIndex = currentItemIndex - 1;
            setScrollBarToCurrentIndex();
        }

        public override void receiveLeftClick( int x, int y, bool playSound = true ) {
            if( GameMenu.forcePreventClose ) {
                return;
            }

            if( downArrow.containsPoint( x, y ) && currentItemIndex < Math.Max( 0, currentOptions.Count - 7 ) ) {
                downArrowPressed();
                Game1.playSound( "shwip" );
            } else if( upArrow.containsPoint( x, y ) && currentItemIndex > 0 ) {
                upArrowPressed();
                Game1.playSound( "shwip" );
            } else if( scrollBar.containsPoint( x, y ) ) {
                scrolling = true;
            } else if( !downArrow.containsPoint( x, y ) && x > xPositionOnScreen + width && ( x < xPositionOnScreen + width + Game1.tileSize * 2 && y > yPositionOnScreen ) && y < yPositionOnScreen + height ) {
                scrolling = true;
                leftClickHeld( x, y );
                releaseLeftClick( x, y );
            }

            currentItemIndex = Math.Max( 0, Math.Min( currentOptions.Count - visibleOptions, currentItemIndex ) );

            for( int index = currentItemIndex; index < visibleOptions; ++index ) {
                if( currentOptions[ index ].bounds.Contains( x, y ) ) {
                    currentOptions[ index ].receiveLeftClick( x, y );
                    break;
                }
            }
        }

        /// <summary>
        /// Updates the current position of each button.
        /// </summary>
        public override void update( GameTime time ) {
            int count = 0;
            for( int i = 0; i < currentOptions.Count; i++ ) {
                if( currentItemIndex <= i && i < currentItemIndex + visibleOptions ) {
                    currentOptions[ i ].bounds.X = ( int ) innerTopLeftCorner.X + 32;
                    currentOptions[ i ].bounds.Y = ( int ) innerTopLeftCorner.Y + ( count * ModOption.BUTTON_HEIGHT );
                    count++;

                // Else option is not visible so place bounds off screen
                } else {
                    currentOptions[ i ].bounds.X = 100000;
                }
            }
        }

        public override void receiveRightClick( int x, int y, bool playSound = true ) { }

        public override void performHoverAction( int x, int y ) {
            if( GameMenu.forcePreventClose ) {
                return;
            }

            hoverText = "";
            hoverHighlighter = Rectangle.Empty;

            for( int i = currentItemIndex; i < currentOptions.Count; i++ ) {
                if( i > visibleOptions ) {
                    return;
                }
                if( currentOptions[i].bounds.Contains( Game1.getMouseX(), Game1.getMouseY() ) ) {
                    hoverHighlighter = currentOptions[i].bounds;
                }
            }

            upArrow.tryHover( x, y, 0.1f );
            downArrow.tryHover( x, y, 0.1f );
            scrollBar.tryHover( x, y, 0.1f );
            int num = scrolling ? 1 : 0;
        }

        public override void draw( SpriteBatch b ) {
            Game1.drawDialogueBox( xPositionOnScreen, yPositionOnScreen, width, height, false, true );

            b.End();
            b.Begin( SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, ( DepthStencilState ) null, ( RasterizerState ) null );

            b.End();
            b.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, ( DepthStencilState ) null, ( RasterizerState ) null );


            // Draw highlighter
            Game1.spriteBatch.Draw( Game1.staminaRect, hoverHighlighter, Color.White * 0.24f );

            // Draw option slot
            for( int index = currentItemIndex; index < visibleOptions; ++index ) {
                currentOptions[ index ].draw( b, 0, 0 );
            }
            // Draw Arrow navigation
            if( !GameMenu.forcePreventClose ) {
                upArrow.draw( b );
                downArrow.draw( b );
                if( currentOptions.Count > 7 ) {
                    IClickableMenu.drawTextureBox( b, Game1.mouseCursors, new Rectangle( 403, 383, 6, 6 ), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, ( float ) Game1.pixelZoom, false );
                    scrollBar.draw( b );
                }
            }

            // Draw title bg
            var titlePosition = new Vector2( innerTopLeftCorner.X + 80, innerTopLeftCorner.Y - 72 );
            int titleWidth = 260;
            int titleHeight = 50;
            int fadeWidth = 2;
            float bgTransparency = 0.5f;
            float fadeAmount = 0.05f;
            ;
            b.Draw( Game1.staminaRect, new Rectangle( ( int ) titlePosition.X - 4, ( int ) titlePosition.Y - 4, titleWidth, titleHeight ), Color.CadetBlue * bgTransparency );

            for( int i = 0; i < 10; i++ ) {
                b.Draw( Game1.staminaRect, new Rectangle( ( int ) titlePosition.X - 4 + titleWidth + i * fadeWidth, ( int ) titlePosition.Y - 4, fadeWidth, titleHeight ), Color.CadetBlue * ( bgTransparency - ( i * fadeAmount ) ) );
            }
            for( int i = 1; i < 10; i++ ) {
                b.Draw( Game1.staminaRect, new Rectangle( ( int ) titlePosition.X - 4 - fadeWidth * i, ( int ) titlePosition.Y - 4, fadeWidth, titleHeight ), Color.CadetBlue * ( bgTransparency - ( i * fadeAmount ) ) );
            }

            // Draw title
            Game1.drawWithBorder( $"Loaded   Mods", new Color( 30, 30, 30), Color.MintCream, titlePosition, 0, 1.1f, 1f, false );

            // Draw hover text
            if( !( hoverText.Equals( "" ) ) )
                IClickableMenu.drawHoverText( b, hoverText, Game1.smallFont, 0, 0, -1, ( string ) null, -1, ( string[] ) null, ( Item ) null, 0, -1, -1, -1, -1, 1f, ( CraftingRecipe ) null );

            // Draw mouse
            if ( !Game1.options.hardwareCursor )
                Game1.spriteBatch.Draw( Game1.mouseCursors, new Vector2( ( float ) Game1.getMouseX(), ( float ) Game1.getMouseY() ), new Microsoft.Xna.Framework.Rectangle?( Game1.getSourceRectForStandardTileSheet( Game1.mouseCursors, Game1.mouseCursor, 16, 16 ) ), Color.White * Game1.mouseCursorTransparency, 0.0f, Vector2.Zero, ( float ) Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f );
        }
    }
}
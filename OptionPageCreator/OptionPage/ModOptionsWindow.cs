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

        public const int WIDTH = 960;
        private const int buttonsPerPage = 11;

        private Dictionary<string, List<ModOption>> modConfigs = new Dictionary<string, List<ModOption>>();

        private List<ModOption> currentOptions = new List<ModOption>();
        private List<ModOption> loadedMods = new List<ModOption>();

        private string hoverText = "";
        private int currentItemIndex;

        private bool scrolling;
        private ClickableTextureComponent backButton;
        private ClickableTextureComponent upArrow;
        private ClickableTextureComponent downArrow;
        private ClickableTextureComponent scrollBar;
        private Rectangle scrollBarRunner;
        private int visibleOptions; // calculate this base on window height
        private Vector2 innerTopLeftCorner;

        private Rectangle hoverHighlighter = new Rectangle();
        private string title;
        private string error = "";
        private Point errorLocation = new Point( 0, 0 );

        private List<Tuple<Keys, string>> keysAlreadyUsedByMods = new List<Tuple<Keys, string>>();

        internal string getModsThatUseKey( Keys key ) {
            StringBuilder sb = new StringBuilder();
            foreach( var item in keysAlreadyUsedByMods ) {
                if( item.Item1.Equals( key ) ) {
                    sb.Append( item.Item2 );
                    sb.Append( ", " );
                }
            }
            if( sb.Length > 1 ) {
                sb.Remove( sb.Length - 2, 2 );
            }
            return sb.ToString();
        }

        // TODO abstract arrows scroll bar and scrollbar runner out of this

        /// <summary>
        /// Base class of ModOptionsPage, mostly copy paste
        /// </summary>
        public ModOptionsWindow ( List<ModConfig> loadedConfigs )
          : base( Game1.activeClickableMenu.xPositionOnScreen, Game1.activeClickableMenu.yPositionOnScreen + 10, WIDTH, Game1.activeClickableMenu.height ) {
            backButton = new ClickableTextureComponent( new Rectangle( 64, 46, 32, 32 ), Game1.mouseCursors, new Rectangle( 480, 96, 32, 32 ), 1f, false );
            upArrow = new ClickableTextureComponent( new Rectangle( xPositionOnScreen + width + Game1.tileSize / 4, yPositionOnScreen + Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom ), Game1.mouseCursors, new Rectangle( 421, 459, 11, 12 ), ( float ) Game1.pixelZoom, false );
            downArrow = new ClickableTextureComponent( new Rectangle( xPositionOnScreen + width + Game1.tileSize / 4, yPositionOnScreen + height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom ), Game1.mouseCursors, new Rectangle( 421, 472, 11, 12 ), ( float ) Game1.pixelZoom, false );
            scrollBar = new ClickableTextureComponent( new Rectangle( upArrow.bounds.X + Game1.pixelZoom * 3, upArrow.bounds.Y + upArrow.bounds.Height + Game1.pixelZoom, 6 * Game1.pixelZoom, 10 * Game1.pixelZoom ), Game1.mouseCursors, new Rectangle( 435, 463, 6, 10 ), ( float ) Game1.pixelZoom, false );
            scrollBarRunner = new Rectangle( scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + Game1.pixelZoom, scrollBar.bounds.Width, height - Game1.tileSize * 2 - upArrow.bounds.Height - Game1.pixelZoom * 2 );

            // Split loadedMods into a usable dictionary and create a list of buttons for all loaded mods to display as the default screen
            foreach( var loadedConfig in loadedConfigs ) {
                modConfigs.Add( loadedConfig.configName, loadedConfig.options );
                loadedMods.Add( new ConfigButton( loadedConfig.configName, this ) );

                // Inject this page into all options
                foreach( var item in loadedConfig.options ) {
                    if( item is ButtonChooser ) {
                        var keyAndName = new Tuple<Keys, string>( ( item as ButtonChooser ).getBoundedKey(), loadedConfig.configName );
                        keysAlreadyUsedByMods.Add( keyAndName ) ;
                    }
                    item.setPage( this );
                }
            }

            yPositionOnScreen = 0;
            innerTopLeftCorner = new Vector2( 16, 116 );
            changePageToModConfigList();
        }

        public bool isKeyInUse( Keys key ) {
            foreach( var item in keysAlreadyUsedByMods ) {
                if( item.Item1.Equals( key ) ) {
                    return true;
                }
            }
            return false;
        }

        internal void setError( string errorMessage ) {
            error = errorMessage;
        }

        internal void changePageTo( string label ) {
            title = label;
            currentOptions = modConfigs[ label ];
            calculateVisibleOptions();
            updateOptionLocations();
        }

        internal void changePageToModConfigList() {
            title = "Loaded Mods";
            currentOptions = loadedMods;
            calculateVisibleOptions();
            updateOptionLocations();
        }

        internal void calculateVisibleOptions() {
            visibleOptions = Math.Min( currentOptions.Count, buttonsPerPage );
        }

        private void setScrollBarToCurrentIndex() {
            updateOptionLocations();
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
            // Scroll up
            if( direction > 0 ) {
                upArrowPressed();
                Game1.playSound( "shiny4" );

            // Scroll down
            } else {
                downArrowPressed();
                Game1.playSound( "shiny4" );
            }
        }

        public override void releaseLeftClick( int x, int y ) {
            base.releaseLeftClick( x, y );
            scrolling = false;
        }

        private void downArrowPressed() {
            downArrow.scale = downArrow.baseScale;
            if( currentItemIndex + currentOptions.Count + 1 < visibleOptions ) {
                currentItemIndex = currentItemIndex + 1;
                setScrollBarToCurrentIndex();
            }
        }

        private void upArrowPressed() {
            upArrow.scale = upArrow.baseScale;
            if( currentItemIndex - 1 > 0 ) {
                currentItemIndex = currentItemIndex - 1;
                setScrollBarToCurrentIndex();
            }
        }

        public override void receiveLeftClick( int x, int y, bool playSound = true ) {
            error = "";

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
            } else if( backButton.containsPoint( x, y ) ) {
                backButtonPressed();
            }

            //currentItemIndex = Math.Max( 0, Math.Min( currentOptions.Count - visibleOptions, currentItemIndex ) );

            for( int index = currentItemIndex; index < visibleOptions; ++index ) {
                currentOptions[ index ].receiveLeftClick( x, y );
            }
        }

        private void backButtonPressed() {
            changePageToModConfigList();
            backButton.scale = backButton.baseScale;
        }

        /// <summary>
        /// Updates the current position of each button.
        /// </summary>
        public void updateOptionLocations() {
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

        public override void update( GameTime time ) {
            foreach( var item in currentOptions ) {
                item.update( time );
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
                    hoverText = currentOptions[ i ].getHoverText();
                }
            }

            backButton.tryHover( x, y, 0.1f );
            upArrow.tryHover( x, y, 0.1f );
            downArrow.tryHover( x, y, 0.1f );
            scrollBar.tryHover( x, y, 0.1f );
            int num = scrolling ? 1 : 0;
        }

        public override void draw( SpriteBatch b ) {
            // Draw bg
            Game1.drawDialogueBox( xPositionOnScreen, yPositionOnScreen, width, height, false, true );

            b.End();
            b.Begin( SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, ( DepthStencilState ) null, ( RasterizerState ) null );

            b.End();
            b.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, ( DepthStencilState ) null, ( RasterizerState ) null );


            // Draw highlighter
            Game1.spriteBatch.Draw( Game1.staminaRect, hoverHighlighter, Color.White * 0.24f );

            // Draw option slot
            for( int index = currentItemIndex; index < visibleOptions; ++index ) {
                currentOptions[ index ].draw( b );
            }
            // Draw Arrow navigation
            if( currentOptions.Count > buttonsPerPage ) {
                upArrow.draw( b );
                downArrow.draw( b );
                IClickableMenu.drawTextureBox( b, Game1.mouseCursors, new Rectangle( 403, 383, 6, 6 ), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, ( float ) Game1.pixelZoom, false );
                scrollBar.draw( b );
            }

            // Draw title bg
            var titlePosition = new Vector2( innerTopLeftCorner.X + 80, innerTopLeftCorner.Y - 72 );
            int titleWidth = (int) Game1.borderFont.MeasureString( title ).X;
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
            Game1.drawWithBorder( title, new Color( 30, 30, 30), Color.MintCream, titlePosition );

            // Back button
            if( currentOptions != loadedMods ) {
                backButton.draw( b );
            }
            // Draw error
            if( error != "" ) {
                errorLocation = new Point( 500, 20 );
                IClickableMenu.drawTextureBox( Game1.spriteBatch, errorLocation.X, errorLocation.Y, ( int ) Game1.smallFont.MeasureString( error ).X + 32, 58, Color.White );
                Game1.spriteBatch.DrawString( Game1.smallFont, error, new Vector2( errorLocation.X + 16, errorLocation.Y + 16 ), Color.Red );
            }

            // Draw hover text
            if( !( hoverText.Equals( "" ) ) )
                IClickableMenu.drawHoverText( b, hoverText, Game1.smallFont, 0, 0, -1, ( string ) null, -1, ( string[] ) null, ( Item ) null, 0, -1, -1, -1, -1, 1f, ( CraftingRecipe ) null );


            // Draw mouse
            if ( !Game1.options.hardwareCursor )
                Game1.spriteBatch.Draw( Game1.mouseCursors, new Vector2( ( float ) Game1.getMouseX(), ( float ) Game1.getMouseY() ), new Microsoft.Xna.Framework.Rectangle?( Game1.getSourceRectForStandardTileSheet( Game1.mouseCursors, Game1.mouseCursor, 16, 16 ) ), Color.White * Game1.mouseCursorTransparency, 0.0f, Vector2.Zero, ( float ) Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f );
        }
    }
}
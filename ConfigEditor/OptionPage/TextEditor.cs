using System;
using Demiacle.OptionPageCreator.OptionPage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using System.Text;
using StardewValley.Menus;

namespace Demiacle.OptionPageCreator.OptionsPage {
    internal class TextEditor : ModOption, IKeyboardSubscriber  {

        private string oldValue;
        public string value;
        private bool isSelected = false;

        private int cursorBlinkTime;
        private Vector2 cursorLocation = Vector2.Zero;
        private bool isCursorVisible = false;
        private int cursorCharPosition;

        public bool valueIsInt;
        public bool valueIsFloat;

        // Substring fails if starting position is the end of the string
        private string charsRightOfCursor { get => 
            ( cursorCharPosition == value.Length ) ? "" : value.Substring( cursorCharPosition, value.Length - cursorCharPosition ); }
        private string charsLeftOfCursor { get => value.Substring( 0, cursorCharPosition ); }

        public bool Selected { get; set; }

        public TextEditor( string label, string value, bool valueIsInt = false, bool valueIsFloat = false ) : base( label ) {
            this.value = value;
            this.valueIsInt = valueIsInt;
            this.valueIsFloat = valueIsFloat;
        }
/****************/
/*PUBLIC METHODS*/
/****************/
        public override void receiveLeftClick( int x, int y ) {
            isCursorVisible = true;
            if( bounds.Contains( x, y ) ) {
                isSelected = true;
                cursorCharPosition = 0;
                calculateCursorLocationWhenClicked( x, y );
                updateCursorDrawLocation();
                Game1.keyboardDispatcher.Subscriber = this;
                oldValue = value;
            } else if( isSelected ) {
                registerNewValue();
            }
        }

        public override string getHoverText() {
            if( isSelected == false ) {
                string message = "Left click to edit text";
                if( valueIsFloat == false && valueIsInt == false ) {
                    message += "\nRight click to assign a key or button( Not implemented yet";
                }
                return message;
            } else {
                return "";
            }
        }

        /// <summary>
        /// Advances the blinking of the cursor.
        /// </summary>
        public override void update( GameTime time ) {
            if( isSelected == false ) {
                return;
            }

            cursorBlinkTime += time.ElapsedGameTime.Milliseconds;

            if( cursorBlinkTime > 800 ) {
                cursorBlinkTime = 0;

                if( isCursorVisible ) {
                    isCursorVisible = false;
                } else {
                    isCursorVisible = true;
                }
            }
        }

        public override void draw( SpriteBatch b ) {
            // Draw cursor
            if( isSelected ) {
                b.Draw( Game1.staminaRect, new Rectangle( bounds.X, bounds.Y, bounds.Width, bounds.Height ), Color.White * 0.7f );

                if( isCursorVisible ) {
                    b.Draw( Game1.staminaRect, new Rectangle( ( int ) cursorLocation.X, ( int ) cursorLocation.Y + 8, 2, bounds.Height - 26 ), Color.Black * 0.7f );
                }
            }

            // Draw title
            b.DrawString( Game1.smallFont, prettyLabel, new Vector2( bounds.X, bounds.Y + 8 ), Color.Black );

            // Draw value
            if( value == "" && isSelected == false ) {
                b.DrawString( Game1.smallFont, "No value", new Vector2( bounds.X + bounds.Width - Game1.smallFont.MeasureString( "No value" ).X, bounds.Y + 8 ), Color.Red );
            } else {
                b.DrawString( Game1.smallFont, value, new Vector2( bounds.X + bounds.Width - Game1.smallFont.MeasureString( value ).X, bounds.Y + 8 ), Color.Black );
            }
        }

        /// <summary>
        /// Adds a char to the value after the cursor. Validates a keystroke if input is loaded as an int or float.
        /// </summary>
        /// <param name="inputChar">The input character. Uppercase has already been calculated</param>
        public void RecieveTextInput( char inputChar ) {
            // Allow only digits and '-' for int types
            if( valueIsInt && Char.IsDigit( inputChar ) == false ) {
                page.setError( "Only digits are valid" );
                return;
            }

            // Allow digits and '.' and '-' for float types
            if( valueIsFloat ) {
                if( Char.IsDigit( inputChar ) == false || inputChar != ( int ) Keys.OemPeriod || inputChar != ( int ) Keys.OemMinus ) {
                    page.setError( "Only digits and . and - keys are valid" );
                    return;
                }
            }

            // Keystroke
            string newValue = charsLeftOfCursor + inputChar + charsRightOfCursor;
            value = newValue;
            cursorCharPosition++;
            page.setError( "" );
        }

        public void RecieveTextInput( string text ) {
        }

        public void RecieveCommandInput( char command ) {
        }

        /// <summary>
        /// Handle all keystrokes that have behavior.
        /// </summary>
        public void RecieveSpecialInput( Keys key ) {
            // Backspace
            if( key.Equals( Keys.Back ) ) {
                if( charsLeftOfCursor == "" ) {
                    return;
                }

                string newValue = charsLeftOfCursor.Substring( 0, charsLeftOfCursor.Length - 1 ) + charsRightOfCursor;
                value = newValue;
                cursorCharPosition--;
            }

            // Delete
            if( key.Equals( Keys.Delete ) ) {
                if( charsRightOfCursor == "" ) {
                    return;
                }

                string newValue = charsLeftOfCursor + charsRightOfCursor.Remove( 0, 1 );
                value = newValue;
            }

            // Left
            if( key.Equals( Keys.Left ) ) {
                cursorCharPosition = Math.Max( 0, cursorCharPosition - 1 );
            }

            // Right
            if( key.Equals( Keys.Right ) ) {
                cursorCharPosition = Math.Min( value.Length, cursorCharPosition + 1 );
            }

            // Enter
            if( key.Equals( Keys.Enter ) ) {
                registerNewValue();
            }

            // Escape
            if( key.Equals( Keys.Escape ) ) {
                resetValue();
                registerNewValue();
            }

            isCursorVisible = true;
            updateCursorDrawLocation();
        }
/********************/
/*NON-PUBLIC METHODS*/
/********************/
        private void registerNewValue() {
            if( validateNewValue() == false ) {
                resetValue();
            }

            isSelected = false;

            // Do not remove keyboard dispatch if another button is already handling it
            if( Game1.keyboardDispatcher.Subscriber == this ) {
                Game1.keyboardDispatcher.Subscriber = null;
            }
        }

        private void resetValue() {
            value = oldValue;
            cursorCharPosition = 0;
        }

        /// <summary>
        /// Validates the new value to an int or a float if the value was loaded as an int or a float.
        /// </summary>
        /// <returns>True if value is acceptable</returns>
        private bool validateNewValue() {
            if( valueIsInt ) {
                int intValue;
                if( int.TryParse( value, out intValue ) == false ) {
                    page.setError( $"New value must be within {int.MinValue} and {int.MaxValue}" );
                    return false;
                }
            }

            if( valueIsFloat ) {
                float floatValue;
                if( float.TryParse( value, out floatValue ) == false ) {
                    page.setError( $"New value must be within {float.MinValue} and {float.MinValue}" );
                    return false;
                }
            }

            return true;
        }

        private void calculateCursorLocationWhenClicked( int x, int y ) {
            StringBuilder stringBuilder = new StringBuilder();
            for( int i = value.Length - 1; i > 0; i-- ) {
                stringBuilder.Insert( 0, value[ i ] );
                int stringLocationX = bounds.X + bounds.Width - ( int ) Game1.smallFont.MeasureString( stringBuilder ).X;
                if( stringLocationX < x ) {
                    cursorCharPosition = i;
                    return;
                }
            }
        }

        private void updateCursorDrawLocation() {
            cursorLocation.X = bounds.X + bounds.Width - Game1.smallFont.MeasureString( charsRightOfCursor ).X;
            cursorLocation.Y = bounds.Y;
        }

    }
}
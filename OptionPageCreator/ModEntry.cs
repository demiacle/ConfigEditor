using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Newtonsoft.Json.Linq;
using Demiacle.OptionPageCreator.OptionPage;
using Demiacle.OptionPageCreator.OptionsPage;

namespace Demiacle.OptionPageCreator {
    class ModEntry : Mod {

        public static IModHelper helper;
        private TitleScreenButton configOptionButton;
        public List<ModConfig> modConfigs = new List<ModConfig>();

        public override void Entry( IModHelper helper ) {
            base.Entry( helper );
            ModEntry.helper = helper;

            GameEvents.QuarterSecondTick += checkTitleScreen;
        }

        private void checkTitleScreen( object sender, EventArgs e ) {
            if( Game1.activeClickableMenu is TitleMenu == false ) {
                return;
            }

            var titleScreen = ( TitleMenu ) Game1.activeClickableMenu;
            var fadeFromWhiteDuration = ( int ) typeof( TitleMenu ).GetField( "fadeFromWhiteTimer", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( Game1.activeClickableMenu );
            var logoSwipeTimer = ( float ) typeof( TitleMenu ).GetField( "logoSwipeTimer", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( Game1.activeClickableMenu );

            // Mods are fully loaded and at title screen
            if( fadeFromWhiteDuration < 1 && logoSwipeTimer < 1 ) {
                Type ModRegistryType = Assembly.GetAssembly( typeof( Mod ) ).GetType( "StardewModdingAPI.Framework.ModRegistry" );
                var loadedMods = (List<IMod>) ModRegistryType.GetField( "Mods", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( helper.ModRegistry );

                // Parse config data of each mod
                foreach( var item in loadedMods ) {
                    string configFile = item.Helper.DirectoryPath + "\\config.json"; 

                    if( File.Exists( configFile ) ) {

                        dynamic configData = item.Helper.ReadConfig<object>();
                        var jObject = ( JObject ) configData;
                        var options = new List<ModOption>();

                        foreach( var config in jObject.Children() ) {
                            parseJsonObject( config, options );
                        }

                        // Ignore empty configs
                        if( options.Count < 1  ) {
                            continue;
                        }

                        var modConfigElements = new ModConfig( item.ModManifest.Name, options, jObject, item.Helper );
                        modConfigs.Add( modConfigElements );
                    }
                }

                configOptionButton = new TitleScreenButton();
                GraphicsEvents.OnPostRenderGuiEvent += drawButton;
                ControlEvents.MouseChanged += handleButtonClick;

                GameEvents.QuarterSecondTick -= checkTitleScreen;
            }

        }

        /// <summary>
        /// Removes button handlers and creates the in game option menu.
        /// </summary>
        private void handleButtonClick( object sender, EventArgsMouseStateChanged e ) {
            if( e.NewState.LeftButton == ButtonState.Pressed && configOptionButton.bounds.Contains( e.NewPosition.X, e.NewPosition.Y ) ) {
                // Play sound
                configOptionButton.receiveLeftClick( e.NewPosition.X, e.NewPosition.Y );

                var titleMenu = ( TitleMenu ) Game1.activeClickableMenu;
                var modOptionsWindow = new ModOptionsWindow( modConfigs );

                typeof( TitleMenu ).GetField( "subMenu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).SetValue( titleMenu, modOptionsWindow );

                ControlEvents.MouseChanged -= handleButtonClick;
                GraphicsEvents.OnPostRenderGuiEvent -= drawButton;
                GameEvents.SecondUpdateTick += listenForBackButton;
                typeof( TitleMenu ).GetField( "buttonsToShow", BindingFlags.NonPublic | BindingFlags.Instance ).SetValue( titleMenu, 0 );
            }
        }

        private void listenForBackButton( object sender, EventArgs e ) {
            var titleMenu = ( TitleMenu ) Game1.activeClickableMenu;
            var subMenu = typeof( TitleMenu ).GetField( "subMenu", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( titleMenu );

            if( subMenu == null ) {
                typeof( TitleMenu ).GetField( "buttonsToShow", BindingFlags.NonPublic | BindingFlags.Instance ).SetValue( titleMenu, 3 );
                ControlEvents.MouseChanged -= handleButtonClick;
                ControlEvents.MouseChanged += handleButtonClick;
                GraphicsEvents.OnPostRenderGuiEvent -= drawButton;
                GraphicsEvents.OnPostRenderGuiEvent += drawButton;
                GameEvents.SecondUpdateTick -= listenForBackButton;
                updateConfigs();
            }
        }

        private void updateConfigs() {
            foreach( var config in modConfigs ) {
                foreach( var option in config.options ) {
                    // Use dynamic for readability. All used types are within this block
                    dynamic value = null;

                    // Get value
                    if( option is TextEditor ) {
                        var textEditor = ( TextEditor ) option;
                        if( textEditor.valueIsInt ) {
                            value = Convert.ToInt32( textEditor.value );
                        } else if( textEditor.valueIsFloat ) {
                            value = Convert.ToDecimal( textEditor.value );
                        } else {
                            value = textEditor.value;
                        }
                    } else if( option is Checkbox ) {
                        value = ( option as Checkbox ).isChecked;
                    } else if( option is ButtonChooser ) {
                        value = ( option as ButtonChooser ).value;
                    }

                    updateJSon( option.label, value, config.json );
                }

                config.helper.WriteJsonFile<JObject>( config.helper.DirectoryPath + "\\config.json", config.json );
            }
        }

        /// <summary>
        /// Updates the loaded JSON data for a config.
        /// </summary>
        /// <param name="varName">Name of the variable. Containers are represented with dot format.</param>
        /// <param name="value">The updated value.</param>
        /// <param name="json">The json object that will be updated.</param>
        private void updateJSon( string varName, dynamic value, JObject json ) {
            if( value == null ) {
                Monitor.Log( $"The value for option {varName} is null and will not be updated. Please " );
                return;
            }

            // Validate dynamic value?

            if( varName.Contains( "." ) ) {
                string[] splitLabel = varName.Split( '.' );
                Type t = json[ splitLabel[ 0 ] ][ splitLabel[ 1 ] ].GetType();
                json[ splitLabel[ 0 ] ][ splitLabel[ 1 ] ] = value;
            } else {
                json[ varName ] = value;
            }
        }

        private void drawButton( object sender, EventArgs e ) {
            if( Game1.activeClickableMenu is TitleMenu == false ) {
                return;
            }
            configOptionButton.draw( Game1.spriteBatch );
        }

        // create optionElement based on type
        // cache config file for saving after menu interactions
        // cache configData
        // optionElement interacts with configData and on updateData will save to config


        /// <summary>
        /// Parses the json object and infer json types to option page buttons. Called recursively to parse objects within objects.
        /// </summary>
        /// <param name="configData">The json object.</param>
        /// <param name="options">Json nodes found will be added to this list</param>
        private void parseJsonObject( JToken configData, List<ModOption> options ) {

            switch( configData.Type ) {

                // Parse all children first
                case JTokenType.Property:

                // Arrays and objects may be used for row/column checkbox data... Check and infer type from bool or 0,1 implementations eventually
                // Arrays and objects must also be allowed to expand and retract within the menu... that is add and delete entries
                case JTokenType.Object:
                case JTokenType.Array:
                    foreach( var item in configData.Children() ) {
                        parseJsonObject( item, options );
                    }
                    break;

                // Infer int
                case JTokenType.Integer:
                    var intValue = configData.Value<string>();
                    this.Monitor.Log( $"Parse int {configData.Path} to value {intValue}" );
                    options.Add( new TextEditor( configData.Path, intValue, valueIsInt: true ) );
                    break;

                // Infer float
                case JTokenType.Float:
                    var floatValue = configData.Value<string>();
                    this.Monitor.Log( $"Parse float {configData.Path} to value {floatValue}" );
                    options.Add( new TextEditor( configData.Path, floatValue, valueIsFloat: true ) );
                    break;

                // Infer string
                case JTokenType.String:
                    var stringValue = configData.Value<string>();
                    this.Monitor.Log( $"Parse string {configData.Path} to value {stringValue}" );

                    // Infer key
                    foreach( var keyName in Enum.GetNames( typeof( Keys ) ) ) {
                        if( keyName == configData.Value<string>() ) {
                            options.Add( new ButtonChooser( configData.Path, stringValue ) );
                            goto EndStringParse;
                        }
                    }

                    // WARNING a,b,x,y are overllaped for the two so make sure button chooser is flexible with both!

                    // Infer joypad
                    foreach( var keyName in Enum.GetNames( typeof( Buttons ) ) ) {
                        if( keyName == configData.Value<string>() ) {
                            options.Add( new ButtonChooser( configData.Path, stringValue ) );
                            goto EndStringParse;
                        }
                    }

                    // Infer editable text
                    options.Add( new TextEditor( configData.Path, stringValue ) );

                    EndStringParse:
                    break;

                // Infer checkbox
                case JTokenType.Boolean:
                    var boolValue = configData.Value<bool>();
                    this.Monitor.Log( $"Parse bool for type {configData.Path} to value {boolValue}" );
                    options.Add( new Checkbox( configData.Path, boolValue ) );
                    break;

                // Ignore cases
                case JTokenType.None:
                case JTokenType.Comment: // Since json parses comments maybe use them so people can add their own comments to the menu
                case JTokenType.Constructor:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Date:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                default:
                    break;
            }
        }

    }
}

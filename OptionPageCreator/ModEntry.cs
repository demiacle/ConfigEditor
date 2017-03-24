using Entoarox.Framework.UI;
using Microsoft.Xna.Framework;
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
using System.Runtime.Remoting;
using Demiacle.OptionPageCreator.OptionPage;

namespace Demiacle.OptionPageCreator {
    class ModEntry : Mod {

        FrameworkMenu test = new FrameworkMenu( new Microsoft.Xna.Framework.Rectangle( 0,0,500,500) );
        public static IModHelper helper;
        private TitleScreenButton configOptionButton;
        public List<ModConfig> modConfigs = new List<ModConfig>();

        public override void Entry( IModHelper helper ) {
            base.Entry( helper );
            ModEntry.helper = helper;

            this.Monitor.Log( "HELLO WORLD" );
            ConfigStub config = helper.ReadConfig<ConfigStub>();

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

                // parse config data of each mod
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

                        var modConfigElements = new ModConfig( item.ModManifest.Name, options );
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
            
            // Use Path var for fully qualified name

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
                    this.Monitor.Log( $"Parse int {configData.Path} to value {configData.Value<int>()}" );
                    break;

                // Infer float
                case JTokenType.Float:
                    this.Monitor.Log( $"Parse float {configData.Path} to value {configData.Value<float>()}" );
                    break;

                // Infer string
                case JTokenType.String:
                    this.Monitor.Log( $"Parse string {configData.Path} to value {configData.Value<string>()}" );

                    // Infer key
                    foreach( var keyName in Enum.GetNames( typeof( Keys ) ) ) {
                        if( keyName == configData.Value<string>() ) {
                            options.Add( new ButtonChooser( configData.Path, null) );
                            break;
                        }
                    }

                    // WARNING a,b,x,y are overllaped for the two so make sure button chooser is flexible with both!

                    // Infer joypad
                    foreach( var keyName in Enum.GetNames( typeof( Buttons ) ) ) {
                        if( keyName == configData.Value<string>() ) {
                            options.Add( new ButtonChooser( configData.Path, null ) );
                            break;
                        }
                    }

                    break;

                // Infer checkbox
                case JTokenType.Boolean:
                    this.Monitor.Log( $"Parse bool for type {configData.Path} to value {configData.Value<bool>()}" );
                    options.Add( new Checkbox( configData.Path, null ) );
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

using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace PlantAnywhere {
    class ModEntry : Mod {

        private bool hasAlteredTiles = false;

        public override void Entry( IModHelper helper ) {
            base.Entry( helper );
            
            LocationEvents.CurrentLocationChanged += alterTiles;
        }

        private void alterTiles( object sender, EventArgsCurrentLocationChanged e ) {
            if( e.NewLocation is Farm == false || hasAlteredTiles == true ) {
                return;
            }

            string layerName = "Back";
            int xTile = Game1.player.getTileX();
            int yTile = Game1.player.getTileY();

            if( Game1.currentLocation.map.GetLayer( layerName ) == null ) {
                return;
            }

            xTile.Layers.Layer backLayer = Game1.currentLocation.map.GetLayer( layerName );
            int mapWidth = backLayer.TileWidth;
            int mapHeight = backLayer.TileHeight;

            var tileToAddDiggable = new List<Tile>();

            for( int i = 0; i < mapWidth - 1; i++ ) {
                for( int k = 0; k < mapHeight - 1; k++ ) {

                    Tile currentTile = backLayer.Tiles[ i, k ];

                    if( currentTile == null || currentTile.TileIndexProperties == null) {
                        continue;
                    }

                    foreach( var item in currentTile.TileIndexProperties ) {

                        // Do not add another diggable property
                        if( item.Key == "Diggable" ) {
                            continue;
                        }

                        // If tile has buildable or grass property its probably ok to dig here
                        if( item.Key == "Buildable" || item.Value.ToString() == "Grass" ) {
                            tileToAddDiggable.Add( currentTile );
                            currentTile.TileIndexProperties.Add( "Diggable", new PropertyValue( "T" ) );
                            
                        }

                    }

                }
            }

            hasAlteredTiles = true;
        }

    }
}

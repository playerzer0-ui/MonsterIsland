using Microsoft.Xna.Framework;

namespace NodeTesting.models
{
    public class MapConnection
    {
        /// <summary>Which edge of the source map triggers the transition.</summary>
        public Direction ExitDirection;

        /// <summary>
        /// The column (for Up/Down exits) or row (for Left/Right exits) that is active.
        /// </summary>
        public int ExitCoordinate;

        /// <summary>CSV path of the destination map.</summary>
        public string TargetMapCsv;

        /// <summary>Tileset texture used by the destination map.</summary>
        public string TargetMapTexture;

        /// <summary>Grid tile the player lands on in the target map.</summary>
        public Point LandingTile;

        /// <summary>
        /// Content path of the background sprite for the destination map,
        /// e.g. "Maps/sea". Game1 uses this to swap the visible background.
        /// </summary>
        public string BackgroundSprite;
    }
}
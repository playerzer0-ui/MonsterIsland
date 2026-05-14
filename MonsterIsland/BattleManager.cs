using NodeTesting.models;
using Microsoft.Xna.Framework;

namespace MonsterIsland
{
    public class BattleManager
    {
        private Sprite[] background;
        private Sprite action;

        public BattleManager () 
        {
            background =
            [
                new Sprite("background/plains1", new Vector2(480, 240)),
                new Sprite("background/plains2", new Vector2(480, 240)),
                new Sprite("background/beach", new Vector2(480, 240)),
                new Sprite("background/forest", new Vector2(480, 240)),
                new Sprite("background/sea", new Vector2(480, 240)),
                new Sprite("background/deepsea", new Vector2(480, 240)),
                new Sprite("background/deepest", new Vector2(480, 240)),
                new Sprite("background/cave", new Vector2(480, 240)),
                new Sprite("background/stronghold", new Vector2(480, 240)),
                new Sprite("background/plains1", new Vector2(480, 240)),
            ];
            action = new Sprite("background/actionbar", new Vector2(480, 560));
        }

        public void Draw()
        {
            background[2].Draw(Color.White);
            action.Draw(Color.White);
        }
    }
}

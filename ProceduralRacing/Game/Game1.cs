using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProceduralRacing
{
    // Owns the window and whichever screen is currently active.
    // Flow: Main Menu -> Race Setup (seed + difficulty) -> Garage (car parts) -> Race -> back to Main Menu

    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Screen activeScreen;

        public int ScreenWidth => GraphicsDevice.PresentationParameters.BackBufferWidth;
        public int ScreenHeight => GraphicsDevice.PresentationParameters.BackBufferHeight;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1920,
                PreferredBackBufferHeight = 1080,
                PreferMultiSampling = true,
                SynchronizeWithVerticalRetrace = true,
            };
            Content.RootDirectory = "Content";
            IsFixedTimeStep = false;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Interface.Initialize(Content, GraphicsDevice);
            PieceLibrary.LoadContent(Content);
            GoToMenu();
        }

        // --- Screen navigation ---

        public void GoToMenu() => activeScreen = new MainMenuScreen(this);

        public void GoToSetup(int playerCount) => activeScreen = new RaceSetupScreen(this, playerCount);

        public void GoToGarage(int playerCount, int seed, TrackDifficulty difficulty) =>
            activeScreen = new GarageScreen(this, playerCount, loadouts => StartRace(seed, difficulty, loadouts));

        public void StartRace(int seed, TrackDifficulty difficulty, CarLoadout[] loadouts) =>
            activeScreen = new GameScreen(this, seed, difficulty, loadouts);

        // --- Loop ---

        protected override void Update(GameTime gameTime)
        {
            if (IsActive) activeScreen.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Interface.Background);
            activeScreen.Draw(spriteBatch, gameTime);
            base.Draw(gameTime);
        }
    }
}

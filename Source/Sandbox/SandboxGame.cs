﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Maps.Tiled;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.ViewportAdapters;

namespace Sandbox
{
    public class SandboxGame : Game
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        // ReSharper disable once NotAccessedField.Local
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private SpriteBatch _spriteBatch;
        private Camera2D _camera;
        private Sprite _sprite;
        private ViewportAdapter _viewportAdapter;
        private BitmapFont _bitmapFont;
        private TiledMap _tiledMap;
        private FramesPerSecondCounter _fpsCounter;
        private SpriteAnimator _spriteAnimator;
        private Zombie _zombie;
        //private CollisionGrid _collisionGrid;
        //private Point _mousePoint;
        private readonly CollisionWorld _world;

        public SandboxGame()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Position = new Point(50, 50);
            Window.Title = string.Format("MonoGame.Extended - {0}", GetType().Name);

            _world = new CollisionWorld(new Vector2(0, 900));
        }

        protected override void Dispose(bool disposing)
        {
            _world.Dispose();

            base.Dispose(disposing);
        }

        protected override void Initialize()
        {
            _fpsCounter = new FramesPerSecondCounter();
            _viewportAdapter = new BoxingViewportAdapter(GraphicsDevice, 800, 480);
            _camera = new Camera2D(_viewportAdapter)
            {
                MinimumZoom = 0.5f,
                MaximumZoom = 2.0f,
                Zoom = 0.5f,
                Origin = new Vector2(400, 240),
                Position = new Vector2(408, 270)
            };

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += (s, e) => _viewportAdapter.OnClientSizeChanged();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _bitmapFont = Content.Load<BitmapFont>("Fonts/courier-new-32");
            _tiledMap = Content.Load<TiledMap>("Tilesets/level01");

            _world.CreateGrid(_tiledMap.GetLayer<TiledTileLayer>("Tile Layer 1"));

            var fireballTexture = Content.Load<Texture2D>("Sprites/fireball");
            var spriteSheetAtlas = TextureAtlas.Create(fireballTexture, 512, 197);

            _sprite = new Sprite(spriteSheetAtlas[0])
            {
                Position = new Vector2(850, 200),
                Scale = new Vector2(0.5f)
            };
            _spriteAnimator = new SpriteAnimator(_sprite, spriteSheetAtlas, 15);

            var zombieSheet = Content.Load<TextureAtlas>("Sprites/zombie-atlas");
            _zombie = new Zombie(zombieSheet)
            {
                Position = new Vector2(300, 500)
            };

            _world.CreateBody(_zombie);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            //_mousePoint = _camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y)).ToPoint();

            // camera
            if (keyboardState.IsKeyDown(Keys.R))
                _camera.ZoomIn(deltaSeconds);

            if (keyboardState.IsKeyDown(Keys.F))
                _camera.ZoomOut(deltaSeconds);

            // zombie
            if (keyboardState.IsKeyDown(Keys.Left))
                _zombie.Walk(-1.0f);

            if (keyboardState.IsKeyDown(Keys.Right))
                _zombie.Walk(1.0f);

            if (keyboardState.IsKeyDown(Keys.Space))
                _zombie.Attack();

            if (keyboardState.IsKeyDown(Keys.Up))
                _zombie.Jump();
            
            if (keyboardState.IsKeyDown(Keys.Enter))
                _zombie.Die();

            // update must be called before collision detection
            _zombie.Update(gameTime);

            _world.Update(gameTime);

            //var boundingBox = _zombie.GetAxisAlignedBoundingBox();
            //_collisionGrid.CollidesWith(boundingBox, _zombie.OnCollision);

            _camera.LookAt(_zombie.Position);
            
            // fireball
            _sprite.Position += new Vector2(-500, 0) * deltaSeconds;
            _spriteAnimator.Update(gameTime);

            if (_sprite.Position.X < 0 - _sprite.GetBoundingRectangle().Width)
                _sprite.Position = new Vector2(1900, _sprite.Position.Y);
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _fpsCounter.Update(gameTime);

            GraphicsDevice.Clear(Color.Black);

            _tiledMap.Draw(_camera);

            _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix());
            _spriteBatch.Draw(_sprite);
            _zombie.Draw(_spriteBatch);
            _spriteBatch.End();

            //var x = _mousePoint.X / _collisionGrid.CellWidth;
            //var y = _mousePoint.Y / _collisionGrid.CellHeight;

            _spriteBatch.Begin();            
            //_spriteBatch.DrawString(_bitmapFont, string.Format("CD: {0}, {1} = {2}", x, y, _collisionGrid.GetDataAt(x, y)), new Vector2(5, 35), new Color(0.5f, 0.5f, 0.5f));
            _spriteBatch.DrawString(_bitmapFont, string.Format("FPS: {0} Zoom: {1}", _fpsCounter.AverageFramesPerSecond, _camera.Zoom), new Vector2(5, 5), new Color(0.5f, 0.5f, 0.5f));
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}

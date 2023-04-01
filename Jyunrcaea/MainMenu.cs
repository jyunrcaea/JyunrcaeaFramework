using JyunrcaeaFramework;
using System.Transactions;

namespace Jyunrcaea
{

    namespace MainMenu
    {
        public class MainScene : Scene
        {
            Title title;
            PlayGame game;
            Setting setting;
            Exit exit;
            Select select;
            StartRange startRange;
            SettingRange settingrange;
            ExitRange exitRange;

            int beforeselect = 0;
            public int nowselect = 0;

            public MainScene()
            {
                this.AddSprite(new Background());
                this.AddSprite(new Menu());
                this.AddSprite(title = new());
                this.AddSprite(select = new Select());
                this.AddSprite(startRange = new StartRange());
                this.AddSprite(settingrange = new SettingRange());
                this.AddSprite(exitRange = new ExitRange());    
                this.AddSprite(game = new());
                this.AddSprite(setting = new Setting());
                this.AddSprite(exit = new Exit());
                this.AddSprite(new Version());
                //this.AddSprite(new TestGruop());
            }

            public override void Start()
            {
                base.Start();
                Resize();
            }

            public override void MouseMove()
            {
                if (Convenience.MouseOver(startRange)) nowselect = 1;
                else if (Convenience.MouseOver(settingrange)) nowselect = 2;
                else if (Convenience.MouseOver(exitRange)) nowselect = 3;
                else nowselect = 0;

                if (nowselect != beforeselect)
                {
                    if (beforeselect == 0) select.Opacity(100, 100f);
                    switch (nowselect)
                    {
                        case 0:
                            select.Opacity(0, 100f);
                            break;
                        case 1:
                            select.Move(select.X, game.Y, 100f);
                            break;
                        case 2:
                            select.Move(select.X, setting.Y, 100f);
                            break;
                        case 3:
                            select.Move(select.X, exit.Y, 100f);
                            break;
                    }
                    beforeselect = nowselect;
                }

                base.MouseMove();
            }

            public override void Resize()
            {
                title.X = game.X = setting.X = exit.X = select.X = startRange.X =settingrange.X = exitRange.X = (int)(250 * Window.AppropriateSize);
                base.Resize();
                if (!select.MoveAnimationState.Complete)
                {
                    if (nowselect == 1)
                    {
                        select.MoveAnimationState.ModifyArrivalPoint(select.X, game.Y);
                    }
                    else if (nowselect == 2) select.MoveAnimationState.ModifyArrivalPoint(select.X, setting.Y);
                    else if (nowselect == 3) select.MoveAnimationState.ModifyArrivalPoint(select.X, exit.Y);
                }
            }
        }

        public class Background : Sprite, MouseMoveEventInterface
        {
            

            public Background() : base(new TextureFromFile("cache/background.png"))
            {
                
            }

            public override void Start()
            {
                base.Start();
            }

            float ratio;

            public override void Resize()
            {
                if (this.Texture.Width * (ratio = (float)Window.Height / (float)this.Texture.Height) < Window.Width) ratio = (float)Window.Width / (float)this.Texture.Width;
                this.Size = ratio * 1.1f;
                base.Resize();
            }

            public void MouseMove()
            {
                this.X = (int)((Input.Mouse.X - Window.Width * 0.5f) * 0.1f);
                this.Y = (int)((Input.Mouse.Y - Window.Height * 0.5f) * 0.1f);
            }
        }

        public class Version : TextboxForAnimation
        {
            public Version() : base("cache/font.ttf", 20, $"Jyunrcaea! ({Jyunrcaea.Store.Version})")
            {
                OriginY = VerticalPositionType.Bottom;
                DrawY = VerticalPositionType.Top;
                this.MoveAnimationState.CalculationFunction = Animation.GetAnimation(AnimationType.Ease_Out);
                this.FontColor = new(31, 30, 51);
                this.Move(0, 20);
                this.Opacity(0);
            }

            public override void Start()
            {
                base.Start();
                this.Move(0, 1, 300f, 500f);
                this.Opacity(255, 300f, 500f);
            }

            public override void Resize()
            {
                this.Size = (int)(20 * Window.AppropriateSize);
                base.Resize();
            }
        }

        public class Title : TextboxForAnimation
        {
            public Title() : base("cache/font.ttf",48,"Jyunrcaea!")
            {
                this.OriginX = HorizontalPositionType.Left;
                //this.DrawX = HorizontalPositionType.Right;
                this.OriginY = VerticalPositionType.Top;
                this.DrawY = VerticalPositionType.Bottom;
                this.FontColor = new(31, 30, 51);
                this.Opacity(0);
            }

            public override void Start()
            {
                base.Start();
                this.Opacity(255, 300f, 500f);
            }

            public override void Resize()
            {
                Y = (int)(100 * Window.AppropriateSize);
                this.Size = (int)(48 * Window.AppropriateSize);
                base.Resize();
            }
        }

        public class PlayGame : TextboxForAnimation
        {
            public PlayGame() : base("cache/font.ttf", 30, "Music Play")
            {
                this.OriginX = HorizontalPositionType.Left;
                this.FontColor = new(31, 30, 51);
                this.Opacity(0);
            }

            public override void Start()
            {
                base.Start();
                this.Opacity(255, 300f, 500f);
            }

            public override void Resize()
            {
                this.Size = (int)(30 * Window.AppropriateSize);
                base.Resize();
            }   
        }

        public class Setting : TextboxForAnimation
        {
            public Setting() : base("cache/font.ttf",30,"Setting")
            {
                this.OriginX = HorizontalPositionType.Left;
                this.FontColor = new(31, 30, 51);
                this.Opacity(0);
            }

            public override void Start()
            {
                base.Start();
                this.Opacity(255, 300f,500f);
            }

            public override void Resize()
            {
                this.Size = (int)(30 * Window.AppropriateSize);
                this.Y = this.Size + 1;
                base.Resize();
            }
        }

        public class Exit : TextboxForAnimation
        {
            public Exit() : base("cache/font.ttf", 30, "Exit")
            {
                this.OriginX = HorizontalPositionType.Left;
                this.FontColor = new(31, 30, 51);
                this.Opacity(0);
            }

            public override void Start()
            {
                base.Start();
                this.Opacity(255, 300f, 500f);
            }

            public override void Resize()
            {
                this.Size = (int)(30 * Window.AppropriateSize);
                this.Y = this.Size * 2 + 2;
                base.Resize();
            }
        }

        public class Menu : RectangleForAnimation
        {
            public Menu()
            {
                this.OriginX = HorizontalPositionType.Left;
                this.DrawX = HorizontalPositionType.Right;
                this.Color = new(200, 200, 200,0);
            }

            public override void Start()
            {
                base.Start();
                this.Opacity(150, 300f, 450f);
            }

            public override void Resize()
            {
                this.Height = Window.Height;
                this.Width = (int)(500 * Window.AppropriateSize);
                base.Resize();
            }
        }

        public class StartRange : GhostObject, MouseButtonDownEventInterface
        {
            public StartRange()
            {
                this.OriginX = HorizontalPositionType.Left;
            }

            public override void Resize()
            {
                this.Width = (int)(300 * Window.AppropriateSize);
                this.Height = (int)(30 * Window.AppropriateSize);
                base.Resize();
            }
            
            public void MouseButtonDown(Input.Mouse.Key key)
            {
                if (key != Input.Mouse.Key.Left) return;
                if (Convenience.MouseOver(this))
                {
                    MainScene sc = (MainScene)this.InheritedObject;
                    Jyunrcaea.Program.musiclistscene.EventRejection = Jyunrcaea.Program.musiclistscene.Hide = false;
                    sc.EventRejection = sc.Hide = true;
                }
            }
        }

        public class SettingRange : GhostObject
        {
            public SettingRange()
            {
                this.OriginX = HorizontalPositionType.Left;
            }

            public override void Resize()
            {
                this.Width = (int)(300 * Window.AppropriateSize);
                this.Height = (int)(30 * Window.AppropriateSize);
                this.Y = this.Height + 1;
                base.Resize();
            }
        }

        public class ExitRange : GhostObject, MouseButtonDownEventInterface
        {
            public ExitRange()
            {
                this.OriginX = HorizontalPositionType.Left;
            }

            public override void Resize()
            {
                this.Width = (int)(300 * Window.AppropriateSize);
                this.Height = (int)(30 * Window.AppropriateSize);
                this.Y = this.Height * 2 + 2;
                base.Resize();
            }

            public void MouseButtonDown(Input.Mouse.Key key)
            {
                if (key == Input.Mouse.Key.Left && Convenience.MouseOver(this)) Framework.Stop(); 
            }
        }

        public class Select : RectangleForAnimation
        {
            public Select()
            {
                this.OriginX = HorizontalPositionType.Left;
                this.Color = new(alpha: 0);
                this.OpacityAnimationState.CalculationFunction = this.MoveAnimationState.CalculationFunction = Animation.GetAnimation(AnimationType.Ease_Out);
            }

            

            public override void Resize()
            {
                this.Width = (int)(300 * Window.AppropriateSize);
                this.Height = (int)(30 * Window.AppropriateSize) + 2;
                base.Resize();
            }
        }
    }
}

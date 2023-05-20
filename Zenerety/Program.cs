using JyunrcaeaFramework;
Framework.Init("Zenerety", 1280, 720);
Framework.NewRenderingSolution = true;
Window.BackgroundColor = new();
Image Icon = new Image { Texture = new TextureFromFile("Icon.png") };
Display.Target.ObjectList.Add(Icon);
Animation.Add(new Animation.Info.Rotation(Icon,360,1000,1000,0,null,Animation.GetAnimation(AnimationType.Easing)));
Framework.Run();
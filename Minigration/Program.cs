using JyunrcaeaFramework;

Framework.Init("Minigration", 1200, 614);
Window.BackgroundColor = new(200,200,250);
Display.Target.Objects.Add(new Minigration.Home.Scene());
Framework.Run(true);
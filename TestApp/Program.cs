using JyunrcaeaFrameworkApp.Editor;

RenderOption renderOption = new(false, true);

Framework.Init("Jyunrcaea Framework App", 1280, 720, renderOption: renderOption);
Font.DefaultPath = @"Fonts\SUIT-Variable.ttf";
Display.Target.Objects.Add(new EditorScene());
Display.FrameLateLimit = (Display.FrameLateLimit = 0) * 2;
Framework.Run();
using JyunrcaeaFramework;

namespace JyunrcaeaFrameworkApp.Editor;

public class CodeBlockObject : Group
{
    public CodeBlockObject(string text, Color color, int width = 200) {
        Objects.Add(background = new(width, 50, color));
        Objects.Add(foreground = new(text));

        background.DrawX = HorizontalPositionType.Right;
        foreground.DrawX = HorizontalPositionType.Right;
        foreground.X = 15;
    }

    public bool IsMouseOver() => background.MouseOver();
    public int Width => background.DisplayedWidth;
    public int Height => background.DisplayedHeight;
    public CodeBlockObject? ParentBlock { get; set; } = null;
    public CodeBlockObject? ChildBlock { get; set; } = null;
    public CodeBlockObject LeafBlock => ChildBlock ?? this;

    public void UpdateFollow() {
        if (ParentBlock is not null) {
            this.X = ParentBlock.X;
            this.Y = ParentBlock.Y + background.DisplayedHeight;
        }

        if (ChildBlock is not null) {
            ChildBlock.UpdateFollow();
        }
    }

    public void RemoveChaning() {
        Parent?.Objects.Remove(this);
        if (ChildBlock is not null) {
            ChildBlock.RemoveChaning();
        }
    }

    public void AddChaning(in Group parent) {
        parent.Objects.Add(this);
        if (ChildBlock is not null) {
            ChildBlock.AddChaning(parent);
        }
    }

    private Box background;
    private Text foreground;
}
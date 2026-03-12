using JyunrcaeaFramework;

namespace JyunrcaeaFrameworkApp.Editor;

public class EditorScene : Group, Events.IMouseKeyDown, Events.IMouseKeyUp
{
    public EditorScene() {
        Objects.Add(new CodeBlockObject("Func A", Color.Lilac) { Y = 120, X = -110 });
        Objects.Add(new CodeBlockObject("Func B", Color.Lavender) { Y = 60, X = -110 });
        Objects.Add(new CodeBlockObject("Func C", Color.Periwinkle) { Y = 0, X = -110 });
        Objects.Add(new CodeBlockObject("Func D", Color.LightPeriwinkle) { Y = -60, X = -110 });
        
        Objects.Add(new CodeBlockObject("Func E", Color.Lilac) { Y = 120, X = 110});
        Objects.Add(new CodeBlockObject("Func F", Color.Lavender) { Y = 60, X = 110});
        Objects.Add(new CodeBlockObject("Func G", Color.Periwinkle) { Y = 0, X = 110});
        Objects.Add(new CodeBlockObject("Func H", Color.LightPeriwinkle) { Y = -60, X = 110});
    }

    private CodeBlockObject? selectedBlock = null;
    private int pointerHorizonDistant = 0;
    private int pointerVerticalDistant = 0;
    public void MouseKeyDown(MouseKey key) {
        if (key != MouseKey.Left) return;

        foreach (var obj in Enumerable.Reverse(Objects)) {
            if (obj is not CodeBlockObject codeBlock) return;
            if (codeBlock.IsMouseOver()) {
                selectedBlock = codeBlock;
                break;
            }
        }

        if (selectedBlock is not null) {
            pointerHorizonDistant = selectedBlock.X - Input.Mouse.X;
            pointerVerticalDistant = selectedBlock.Y - Input.Mouse.Y;
            selectedBlock.RemoveChaning();
            if (selectedBlock.ParentBlock is not null) selectedBlock.ParentBlock.ChildBlock = null;
            selectedBlock.ParentBlock = null;
            selectedBlock.AddChaning(this);
        }
    }

    public void MouseKeyUp(MouseKey key) {
        if (key != MouseKey.Left) return;

        if (selectedBlock is not null) {
            UpdateSelectBlock();
            
            CodeBlockObject? insertParent = null;
            int minimumVerticalDistance = int.MaxValue;
            foreach (var obj in Objects) {
                if (obj == selectedBlock) continue;
                if (Math.Abs(obj.X - selectedBlock.X) >= selectedBlock.Width) continue;

                int vd = selectedBlock.Y - obj.Y;
                if (0 <= vd && vd < minimumVerticalDistance) {
                    insertParent = obj as CodeBlockObject;
                    minimumVerticalDistance = vd;
                }
            }

            if (minimumVerticalDistance >= selectedBlock.Height) {
                insertParent = null;
            }

            if (insertParent is not null) {
                selectedBlock.ParentBlock = insertParent;

                if (insertParent.ChildBlock is CodeBlockObject prevBlock) {
                    var leaf = selectedBlock.LeafBlock;
                    prevBlock.ParentBlock = leaf;
                    leaf.ChildBlock = prevBlock;
                }
                insertParent.ChildBlock = selectedBlock;
                insertParent.UpdateFollow();
            }
            
            selectedBlock = null;
        }
    }

    public override void Update(float ms) {
        base.Update(ms);

        UpdateSelectBlock();
    }

    private void UpdateSelectBlock() {
        if (selectedBlock is null) return;
        
        selectedBlock.X = Input.Mouse.X + pointerHorizonDistant;
        selectedBlock.Y = Input.Mouse.Y + pointerVerticalDistant;
        selectedBlock.UpdateFollow();
    }
}
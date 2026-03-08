using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JyunrcaeaFramework.Collections;

public class EventList
{
    internal Queue<BaseObject> ObjectQueue = new();

    internal List<Events.IResized> Resized = new();
    internal List<Events.IResize> Resize = new();
    internal List<Events.IUpdate> Update = new();
    internal List<Events.IKeyDown> KeyDown = new();
    internal List<Events.IKeyUp> keyUp = new();
    internal List<Events.IMouseMove> mouseMoves = new();
    internal List<Events.IMouseKeyDown> mouseKeyDowns = new();
    internal List<Events.IMouseKeyUp> mouseKeyUps = new();

    internal List<Events.IWindowMove> windowMoves = new();
    internal List<Events.IWindowQuit> windowQuits = new();
    internal List<Events.IWindowMaximized> windowMaximizeds = new();
    internal List<Events.IWindowMinimized> windowMinimizeds = new();
    internal List<Events.IWindowRestore> windowRestores = new();
    internal List<Events.IDropFile> dropFiles = new();
    internal List<Events.IKeyFocusIn> keyFocusIns = new();
    internal List<Events.IKeyFocusOut> keyFocusOuts = new();

    public void Add(BaseObject obj)
    {
        if (obj is not Group)
        {
            Ad(Resize , obj);
            Ad(Update , obj);
        }
        Ad(Resized , obj);
        Ad(KeyDown , obj);
        Ad(keyUp , obj);
        Ad(mouseMoves , obj);
        Ad(mouseKeyDowns , obj);
        Ad(mouseKeyUps , obj);

        Ad(windowMoves , obj);
        Ad(windowQuits , obj);
        Ad(windowMaximizeds , obj);
        Ad(windowMinimizeds , obj);
        Ad(windowRestores , obj);
        Ad(dropFiles , obj);
        Ad(keyFocusIns , obj);
        Ad(keyFocusOuts , obj);
    }

    public void Remove(object obj)
    {
        Rd(Resized , obj);
        Rd(Resize , obj);
        Rd(Update , obj);
        Rd(KeyDown , obj);
        Rd(keyUp , obj);
        Rd(mouseMoves , obj);
        Rd(mouseKeyDowns , obj);
        Rd(mouseKeyUps , obj);

        Rd(windowMoves , obj);
        Rd(windowQuits , obj);
        Rd(windowMaximizeds , obj);
        Rd(windowMinimizeds , obj);
        Rd(windowRestores , obj);
        Rd(dropFiles , obj);
        Rd(keyFocusIns , obj);
        Rd(keyFocusOuts , obj);
    }

    internal void Ad<T>(List<T> li , object obj)
    {
        if (obj is not T)
            return;
        li.Add((T)obj);
    }

    internal bool Rd<T>(List<T> li , object obj)
    {
        if (obj is not T)
            return false;
        return li.Remove((T)obj);
    }

    public void Refresh()
    {
        while (ObjectQueue.Count != 0)
            Add(ObjectQueue.Dequeue());
    }
}

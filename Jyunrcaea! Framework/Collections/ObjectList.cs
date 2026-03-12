using JyunrcaeaFramework.Core;
using JyunrcaeaFramework.Interfaces;
using JyunrcaeaFramework.Objects;

namespace JyunrcaeaFramework.Collections;

/// <summary>
/// 오브젝트 리스트입니다. 추가 및 제거시 자동으로 리소스를 할당/해제 해줍니다.
/// </summary>
public class ObjectList : List<BaseObject>, IDisposable
{
    internal TopGroup? Ancestor;

    Group Parent;
    public ObjectList(Group group)
    {
        Parent = group;
        Ancestor = group.Ancestor;
    }

    Queue<AddTarget> AddList = new();
    public int AddQueueCount => AddList.Count;

    /// <summary>
    /// 객체 목록에 변경해야될 사항이 있는 경우 목록을 새로 고칩니다.
    /// 만약 즉시 추가해야될 객체가 있는경우 이 함수를 호출하여 즉시 추가하세요.
    /// </summary>
    public void Update()
    {
        if (AddList.Count == 0) return;
        while (AddList.Count != 0)
        {
            var target = AddList.Dequeue();
            if (target.index == -1) base.Add(target.target);
            else base.Insert(target.index, target.target);
        }
    }

    bool RemoveFromPendingQueue(BaseObject obj)
    {
        if (AddList.Count == 0) return false;

        bool removed = false;
        Queue<AddTarget> newQueue = new(AddList.Count);
        while (AddList.Count != 0)
        {
            var target = AddList.Dequeue();
            if (!removed && ReferenceEquals(target.target, obj))
            {
                removed = true;
                continue;
            }
            newQueue.Enqueue(target);
        }

        AddList = newQueue;
        return removed;
    }

    void AddProcedure(BaseObject obj)
    {
        if (this.Ancestor is not null) this.Ancestor.EventManager.Add(obj);
        if (obj is Group group)
        {
            group.Ancestor = this.Ancestor;
            group.Prepare();
            return;
        }
    }

    /// <summary>
    /// 객체를 추가합니다.
    /// </summary>
    /// <param name="obj">객체</param>
    public new void Add(BaseObject obj)
    {
        if (obj.Parent is not null) throw new JyunrcaeaFrameworkException("이미 다른 부모 객체에게 상속된 객체입니다.");
        obj.Parent = this.Parent;
        if (this.Parent.ResourceReady)
        {
            AddProcedure(obj);
            this.AddList.Enqueue(new(obj));
        }
        else base.Add(obj);
    }

    public new void AddRange(IEnumerable<BaseObject> objs)
    {
        foreach (var a in objs)
        {
            Add(a);
        }
    }

    public void AddRange(params BaseObject[] objs)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            this.Add(objs[i]);
        }
    }

    public void AddRange(int index = 0, params BaseObject[] objs)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            this.Insert(index + i, objs[i]);
        }
    }

    public new void Insert(int index, BaseObject zo)
    {
        if (zo.Parent is not null) throw new JyunrcaeaFrameworkException("이미 다른 부모 객체에게 상속된 객체입니다.");
        zo.Parent = this.Parent;
        if (this.Parent.ResourceReady)
        {
            AddProcedure(zo);
            this.AddList.Enqueue(new(zo, index));
        }
        else base.Insert(index, zo);
    }


    public bool Switch(BaseObject target, int index = -1)
    {
        if (!base.Remove(target)) return false;

        if (index == -1)
        {
            base.Add(target);
        }
        else
        {
            base.Insert(index, target);
        }
        return true;
    }

    /// <summary>
    /// 객체를 제거합니다.
    /// </summary>
    /// <param name="obj">객체</param>
    public new bool Remove(BaseObject obj)
    {
        bool removed = base.Remove(obj);
        if (!removed)
        {
            removed = RemoveFromPendingQueue(obj);
        }
        if (!removed) return false;

        obj.Destroy();
        this.Ancestor?.EventManager.Remove(obj);
        obj.Parent = null;
        return true;
    }

    public new void RemoveAt(int index)
    {
        Remove(this[index]);
    }

    public new void Clear()
    {
        while (this.Count != 0)
        {
            RemoveAt(this.Count - 1);
        }

        while (AddList.Count != 0)
        {
            var target = AddList.Dequeue().target;
            target.Destroy();
            this.Ancestor?.EventManager.Remove(target);
            target.Parent = null;
        }
    }

    public void Dispose()
    {
        Clear();
    }
}

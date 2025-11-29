namespace JyunrcaeaFramework;

class AddTarget
{
    public BaseObject target;
    public int index;

    public AddTarget(BaseObject obj,int pos = -1)
    {
        this.index = pos;
        this.target = obj;
    }
}
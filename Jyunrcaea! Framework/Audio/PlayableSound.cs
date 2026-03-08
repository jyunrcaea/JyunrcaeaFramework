using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JyunrcaeaFramework.Audio;

public abstract class PlayableSound : IDisposable
{
    internal IntPtr sound;
    public abstract void Dispose();
    ~PlayableSound()
    {
        Dispose();
    }
}


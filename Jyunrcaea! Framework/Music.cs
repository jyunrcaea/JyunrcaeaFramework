using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JyunrcaeaFramework;

public class Music : PlayableSound
{
    public Music(string FileName)
    {
        this.sound = SDL_mixer.Mix_LoadMUS(FileName);
        if (this.sound == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"음악을 불러오는데 실패하였습니다. SDL mixer Error: {SDL_mixer.Mix_GetError()}");
    }

    public override void Dispose()
    {
        SDL_mixer.Mix_FreeMusic(this.sound);
        GC.SuppressFinalize(this);
    }
    /// <summary>
    /// 음악을 재생합니다.
    /// </summary>
    /// <param name="music">음악</param>
    /// <returns>성공시 true</returns>
    public static bool Play(Music music)
    {
        playingmusic = music;
        if (SDL_mixer.Mix_PlayMusic(music.sound , 1) == -1)
            return false;
        return true;
    }
    public bool Play() => Music.Play(this);

    public static bool Resume()
    {
        if (SDL_mixer.Mix_PlayingMusic() != 0)
            return false;
        SDL_mixer.Mix_ResumeMusic();
        return true;
    }

    static Music? playingmusic = null;
    public static Music? NowPlaying => playingmusic;

    /// <summary>
    /// 음악 제목을 가져옵니다. (시간이 다소 걸립니다.)
    /// 'PlayReady' 가 켜져있거나, 이 음악이 재생중일 경우 좀 더 빠르게 불러올수 있습니다.
    /// </summary>
    public string Title => SDL_mixer.Mix_GetMusicTitle(this.sound);

    public static void Skip()
    {
        SDL_mixer.Mix_HaltMusic();
    }

    public static bool Paused => SDL_mixer.Mix_PausedMusic() == 1;

    public static void Pause()
    {
        SDL_mixer.Mix_PauseMusic();
    }

    /// <summary>
    /// 원하는 시간대로 이동합니다.
    /// </summary>
    public static double NowTime {
        get { return NowPlaying == null ? -1 : SDL_mixer.Mix_GetMusicPosition(NowPlaying.sound); }
        set {
            if (SDL_mixer.Mix_SetMusicPosition(value) == -1)
                throw new JyunrcaeaFrameworkException("잘못된 위치");
        }
    }

    internal static void Finished()
    {
        if (SDL_mixer.Mix_PlayingMusic() == 0)
            return;
        if (NowPlaying != null)
            NowPlaying.Dispose();
        if (MusicFinished != null)
            MusicFinished();
    }

    public static FunctionWhenMusicFinished? MusicFinished = null;
}

public delegate Music? FunctionWhenMusicFinished();
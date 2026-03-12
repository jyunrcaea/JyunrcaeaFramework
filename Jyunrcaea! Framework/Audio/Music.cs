using JyunrcaeaFramework.Core;
using SDL2;

namespace JyunrcaeaFramework.Audio;

public class Music : PlayableSound
{
    public Music(string FileName)
    {
        this.sound = SDL_mixer.Mix_LoadMUS(FileName);
        if (this.sound == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"음악 파일을 불러오는데 실패했습니다. SDL mixer Error: {SDL_mixer.Mix_GetError()}");
    }

    public override void Dispose()
    {
        if (this.sound == IntPtr.Zero)
        {
            return;
        }

        SDL_mixer.Mix_FreeMusic(this.sound);
        this.sound = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 음악을 재생합니다.
    /// </summary>
    /// <param name="music">음악</param>
    /// <returns>성공시 true</returns>
    public static bool Play(Music music)
    {
        if (SDL_mixer.Mix_PlayMusic(music.sound, 0) == -1)
            return false;

        playingmusic = music;
        return true;
    }

    public bool Play() => Music.Play(this);

    public static bool Resume()
    {
        if (SDL_mixer.Mix_PlayingMusic() == 0)
            return false;
        SDL_mixer.Mix_ResumeMusic();
        return true;
    }

    static Music? playingmusic = null;
    public static Music? NowPlaying => playingmusic;

    /// <summary>
    /// 음악 제목을 반환합니다. (지원하지 않을 수 있음)
    /// </summary>
    /// <remarks>
    /// PlayReady 를 사용해야, 이 속성이 올바른 값을 반환할 수 있는 파일인지 확인할 수 있습니다.
    /// </remarks>
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
    /// 현재 음악의 위치를 가져오거나 설정합니다.
    /// </summary>
    public static double NowTime
    {
        get { return NowPlaying == null ? -1 : SDL_mixer.Mix_GetMusicPosition(NowPlaying.sound); }
        set
        {
            if (SDL_mixer.Mix_SetMusicPosition(value) == -1)
                throw new JyunrcaeaFrameworkException("음악 이동 실패");
        }
    }

    internal static void Finished()
    {
        var finishedMusic = playingmusic;
        playingmusic = null;

        finishedMusic?.Dispose();

        if (MusicFinished is null)
        {
            return;
        }

        var nextMusic = MusicFinished();
        if (nextMusic is not null)
        {
            Play(nextMusic);
        }
    }

    public static FunctionWhenMusicFinished? MusicFinished = null;
}

public delegate Music? FunctionWhenMusicFinished();

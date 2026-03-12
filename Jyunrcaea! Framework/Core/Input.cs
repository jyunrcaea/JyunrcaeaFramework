using JyunrcaeaFramework.Structs;
using SDL2;

namespace JyunrcaeaFramework.Core;

/// <summary>
    /// 키보드, 마우스 및 텍스트 입력 상태에 대한 접근 지점.
    /// </summary>
    public static class Input
{
    /// <summary>
    /// 현재 키가 눌려있는지 여부를 반환합니다.
    /// </summary>
    public static unsafe bool IsKeyPressed(Keycode key)
    {
        var scancode = SDL.SDL_GetScancodeFromKey((SDL.SDL_Keycode)key);
        return ((byte*)SDL.SDL_GetKeyboardState(out _))[(int)scancode] != 0;
    }

    /// <summary>
    /// 마우스 입력 상태 및 헬퍼.
    /// </summary>
    public static class Mouse
    {
        static readonly IntPtr[] CursorCache = new IntPtr[(int)CursorType.SYSTEM_CURSORS];

        /// <summary>
        /// 윈도우가 포커스를 다시 얻을 때 포커스-클릭 이벤트가 전달되는지 여부를 제어합니다.
        /// </summary>
        public static bool BlockEventAtToFocus
        {
            get => SDL.SDL_GetHint(SDL.SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH) == "1";
            set => SDL.SDL_SetHint(SDL.SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, value ? "1" : "0");
        }

        internal static SDL.SDL_Point position = new();

        /// <summary>
        /// 윈도우 좌표계에서 현재 마우스의 x 위치.
        /// </summary>
        public static int X => position.x;

        /// <summary>
        /// 윈도우 좌표계에서 현재 마우스의 y 위치.
        /// </summary>
        public static int Y => position.y;

        static bool cursorhide = false;

        /// <summary>
        /// 활성화된 동안 윈도우 내 마우스 움직임을 캡처합니다.
        /// </summary>
        public static bool Grab
        {
            set => SDL.SDL_SetWindowMouseGrab(Framework.window, value ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
            get => SDL.SDL_GetWindowMouseGrab(Framework.window) == SDL.SDL_bool.SDL_TRUE;
        }

        /// <summary>
        /// OS 커서를 숨기거나 표시합니다.
        /// </summary>
        public static bool HideCursor
        {
            get => cursorhide;
            set => SDL.SDL_ShowCursor((cursorhide = value) ? 0 : 1);
        }

        /// <summary>
        /// 숨겨진 커서 상태가 윈도우 프레임 상호작용을 비활성화하는지 여부를 제어합니다.
        /// </summary>
        public static bool BlockWindowFrame
        {
            set => SDL.SDL_SetHint(SDL.SDL_HINT_WINDOW_FRAME_USABLE_WHILE_CURSOR_HIDDEN, value ? "0" : "1");
            get => SDL.SDL_GetHint(SDL.SDL_HINT_WINDOW_FRAME_USABLE_WHILE_CURSOR_HIDDEN) == "0";
        }

        /// <summary>
        /// 시스템 커서 모양을 설정합니다.
        /// 커서는 SDL 커서 할당 누수를 방지하기 위해 캐시됩니다.
        /// </summary>
        public static void SetCursor(CursorType t)
        {
            int index = (int)t;
            if (index < 0 || index >= CursorCache.Length)
            {
                throw new JyunrcaeaFrameworkException($"Invalid cursor type index: {index}");
            }

            if (CursorCache[index] == IntPtr.Zero)
            {
                CursorCache[index] = SDL.SDL_CreateSystemCursor((SDL.SDL_SystemCursor)t);
                if (CursorCache[index] == IntPtr.Zero)
                {
                    throw new JyunrcaeaFrameworkException($"Failed to create system cursor. SDL Error: {SDL.SDL_GetError()}");
                }
            }

            SDL.SDL_SetCursor(CursorCache[index]);
        }

        /// <summary>
        /// 캐시된 모든 SDL 커서 핸들을 해제합니다.
        /// </summary>
        internal static void DisposeCachedCursors()
        {
            for (int i = 0; i < CursorCache.Length; i++)
            {
                if (CursorCache[i] != IntPtr.Zero)
                {
                    SDL.SDL_FreeCursor(CursorCache[i]);
                    CursorCache[i] = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// SDL 시스템 커서 목록.
        /// </summary>
        public enum CursorType
        {
            Arrow,
            Ibeam,
            Wait,
            Crosshair,
            WaitArrow,
            SIZENWSE,
            SIZENESW,
            HorizonSizing,
            VericalSizing,
            Move,
            NO,
            HAND,
            SYSTEM_CURSORS
        }
    }

    /// <summary>
    /// IME 및 UTF-8 텍스트 입력을 위한 텍스트 입력 상태.
    /// </summary>
    public static class Text
    {
        /// <summary>
        /// SDL_TEXTINPUT 이벤트에서 누적된 현재 텍스트 버퍼.
        /// </summary>
        public static string InputedText = string.Empty;

        public static int CursorPosition = 0;
        public static int SelectionLenght = 0;
        public static string SelectedText = string.Empty;

        public static uint WaitTime = 614;
        public static uint RemoveRepeatTime = 100;
        public static bool Removing { get; internal set; } = false;

        /// <summary>
        /// SDL 텍스트 입력 모드가 현재 활성화되어 있는지 여부.
        /// </summary>
        internal static bool ti = false;

        [Obsolete("Not implemented yet")]
        public static bool Enable
        {
            get => ti;
            set
            {
                if (ti = value)
                {
                    SDL.SDL_StartTextInput();
                }
                else
                {
                    SDL.SDL_StopTextInput();
                }
            }
        }

        [Obsolete("Not implemented yet")]
        public static bool BlockKeyEvent { get; set; } = false;
    }
}

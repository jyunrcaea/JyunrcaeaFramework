using SDL2;

namespace JyunrcaeaFramework;

/// <summary>
/// 입력과 관련된 대부분의 클래스가 있습니다.
/// </summary>
public static class Input
{
    public static unsafe bool IsKeyPressed(Keycode key)
    {
        var scancode = SDL.SDL_GetScancodeFromKey((SDL.SDL_Keycode)key);
        return ((byte*)SDL.SDL_GetKeyboardState(out _))[(int)scancode] != 0;
    }

    /// <summary>
    /// 마우스와 관련된 클래스입니다.
    /// </summary>
    public static class Mouse
    {
        /// <summary>
        /// 창 포커스를 얻기위해 마우스를 클릭했을때 생긴 입력 이벤트를 차단할지에 대한 여부입니다.
        /// </summary>
        public static bool BlockEventAtToFocus
        {
            get => SDL.SDL_GetHint(SDL.SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH) == "1";

            set => SDL.SDL_SetHint(SDL.SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, value ? "1":"0");
        }

        internal static SDL.SDL_Point position = new();

        public static int X => position.x;

        public static int Y => position.y;

        static bool cursorhide = false;
        

        /// <summary>
        /// 마우스를 창 밖으로 나오지 못하게 가둘지에 대한 여부입니다.
        /// </summary>
        public static bool Grab
        {
            set
            {
                SDL.SDL_SetWindowMouseGrab(Framework.window, value ? SDL.SDL_bool.SDL_TRUE: SDL.SDL_bool.SDL_FALSE);
            }
            get => SDL.SDL_GetWindowMouseGrab(Framework.window) == SDL.SDL_bool.SDL_TRUE;
        }

        /// <summary>
        /// 커서를 숨길지에 대한 여부입니다.
        /// </summary>
        public static bool HideCursor
        {
            get => cursorhide;
            set
            {
                SDL.SDL_ShowCursor((cursorhide = value) ? 0 : 1);
            }
        }

        /// <summary>
        /// 커서가 숨겨져 있을때 창 테두리를 조작할수 있게 할지에 대한 여부입니다. (창 조절, 이동 등)
        /// </summary>
        public static bool BlockWindowFrame
        {
            set
            {
                SDL.SDL_SetHint(SDL.SDL_HINT_WINDOW_FRAME_USABLE_WHILE_CURSOR_HIDDEN, value ? "0" : "1");
            }
            get => SDL.SDL_GetHint(SDL.SDL_HINT_WINDOW_FRAME_USABLE_WHILE_CURSOR_HIDDEN) == "0";
        }

        public static void SetCursor(CursorType t)
        {
            SDL.SDL_SetCursor(SDL.SDL_CreateSystemCursor((SDL.SDL_SystemCursor)t));
        }

        public enum CursorType
        {
            Arrow,    // Arrow
            Ibeam,    // I-beam
            Wait,     // Wait
            Crosshair,    // Crosshair
            WaitArrow,    // Small wait cursor (or Wait if not available)
            SIZENWSE, // Double arrow pointing northwest and southeast
            SIZENESW, // Double arrow pointing northeast and southwest
            HorizonSizing,   // Double arrow pointing west and east
            VericalSizing,   // Double arrow pointing north and south
            Move,  // Four pointed arrow pointing north, south, east, and west
            NO,       // Slashed circle or crossbones
            HAND,     // Hand
            SYSTEM_CURSORS
        }
    }
    /// <summary>
    /// 텍스트 입력과 관련된 클래스입니다.
    /// 한국어 및 여러 문자들을 입력받기 위한 기능이 존재합니다.
    /// (0.8 이후부터 지원될 예정입니다.)
    /// </summary>
    public static class Text
    {
        public static string InputedText = string.Empty;

        public static int CursorPosition = 0;
        public static int SelectionLenght = 0;
        public static string SelectedText = string.Empty;

        public static uint WaitTime = 614;
        public static uint RemoveRepeatTime = 100;
        public static bool Removing { get; internal set; } = false;

        /// <summary>
        /// 텍스트 입력 활성/비활성
        /// </summary>
        internal static bool ti = false;
        [Obsolete("아직 구현되지 않은 기능")]
        public static bool Enable
        {
            get => ti;
            set
            {
                if (ti = value) SDL.SDL_StartTextInput();
                else SDL.SDL_StopTextInput();
            }
        }

        /// <summary>
        /// 텍스트가 입력되는 동안 KeyDown/KeyUp 이벤트를 무시합니다. (아직 구현되지 않음, 다음 업데이트를 위해 미리 생성)
        /// </summary>
        [Obsolete("미구현")]
        public static bool BlockKeyEvent{ get; set; } = false;
    }
}
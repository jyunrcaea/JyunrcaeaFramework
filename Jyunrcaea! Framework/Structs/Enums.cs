using SDL2;

namespace JyunrcaeaFramework.Structs;

public enum HorizontalPositionType
{
    Middle = 0,
    Left = 1,
    Right = 2
}

public enum VerticalPositionType
{
    Middle = 0,
    Top = 1,
    Bottom = 2
}

[Flags]
public enum FontStyle
{
    Normal = SDL_ttf.TTF_STYLE_NORMAL,
    Bold = SDL_ttf.TTF_STYLE_BOLD,
    Italic = SDL_ttf.TTF_STYLE_ITALIC,
    Underline = SDL_ttf.TTF_STYLE_UNDERLINE,
    Strikethrough = SDL_ttf.TTF_STYLE_STRIKETHROUGH
}

public enum AnimationType : byte
{
    /// <summary>
    /// 시간에 비례해 이동합니다. (기본값)
    /// </summary>
    Normal = 0,
    /// <summary>
    /// 중간 지점과 가까워질수록 빨라집니다.
    /// 멀어질수록 느려집니다.
    /// </summary>
    Easing = 1,
    /// <summary>
    /// 처음에 느리게 시작합니다.
    /// 그리고 점점 빨라집니다.
    /// 마지막에 다다를수록 기하급수적으로 빨라집니다.
    /// </summary>
    Ease_In = 2,
    /// <summary>
    /// 처음에 빠르게 시작합니다.
    /// 그리고 점점 느려집니다.
    /// (Ease_In 를 반대로 하는것과 같습니다.)
    /// </summary>
    Ease_Out = 3,
    
    EaseInQuad = 4,
    EaseOutQuad = 5,
    EaseInOutQuad = 6,
}

/// <summary>
/// 마우스 버튼 목록
/// </summary>
public enum MouseKey : byte
{
    /// <summary>
    /// 왼쪽
    /// </summary>
    Left = 1,
    /// <summary>
    /// 중간 (마우스 휠)
    /// </summary>
    Middle = 2,
    /// <summary>
    /// 오른쪽
    /// </summary>
    Right = 3
}

/// <summary>
/// 키보드 키코드
/// </summary>
public enum Keycode :int
{
    UNKNOWN = 0,

    RETURN = 13,         // '\r'의 ASCII 값
    ESCAPE = 27,         // 27 (Escape의 ASCII 값)
    BACKSPACE = 8,       // '\b'의 ASCII 값
    TAB = 9,             // '\t'의 ASCII 값
    SPACE = 32,          // ' ' (공백)의 ASCII 값
    EXCLAIM = 33,        // '!'의 ASCII 값
    QUOTEDBL = 34,       // '"'의 ASCII 값
    HASH = 35,           // '#'의 ASCII 값
    PERCENT = 37,        // '%'의 ASCII 값
    DOLLAR = 36,         // '$'의 ASCII 값
    AMPERSAND = 38,      // '&'의 ASCII 값
    QUOTE = 39,          // '\''의 ASCII 값
    LEFTPAREN = 40,      // '('의 ASCII 값
    RIGHTPAREN = 41,     // ')'의 ASCII 값
    ASTERISK = 42,       // '*'의 ASCII 값
    PLUS = 43,           // '+'의 ASCII 값
    COMMA = 44,          // ','의 ASCII 값
    MINUS = 45,          // '-'의 ASCII 값
    PERIOD = 46,         // '.'의 ASCII 값
    SLASH = 47,          // '/'의 ASCII 값
    _0 = 48,             // '0'의 ASCII 값
    _1 = 49,             // '1'의 ASCII 값
    _2 = 50,             // '2'의 ASCII 값
    _3 = 51,             // '3'의 ASCII 값
    _4 = 52,             // '4'의 ASCII 값
    _5 = 53,             // '5'의 ASCII 값
    _6 = 54,             // '6'의 ASCII 값
    _7 = 55,             // '7'의 ASCII 값
    _8 = 56,             // '8'의 ASCII 값
    _9 = 57,             // '9'의 ASCII 값
    COLON = 58,          // ':'의 ASCII 값
    SEMICOLON = 59,      // ';'의 ASCII 값
    LESS = 60,           // '<'의 ASCII 값
    EQUALS = 61,         // '='의 ASCII 값
    GREATER = 62,        // '>'의 ASCII 값
    QUESTION = 63,       // '?'의 ASCII 값
    AT = 64,             // '@'의 ASCII 값
    LEFTBRACKET = 91,    // '['의 ASCII 값
    BACKSLASH = 92,      // '\\'의 ASCII 값
    RIGHTBRACKET = 93,   // ']'의 ASCII 값
    CARET = 94,          // '^'의 ASCII 값
    UNDERSCORE = 95,     // '_'의 ASCII 값
    BACKQUOTE = 96,      // '`'의 ASCII 값
    A = 97,              // 'a'의 ASCII 값
    B = 98,              // 'b'의 ASCII 값
    C = 99,              // 'c'의 ASCII 값
    D = 100,             // 'd'의 ASCII 값
    E = 101,             // 'e'의 ASCII 값
    F = 102,             // 'f'의 ASCII 값
    G = 103,             // 'g'의 ASCII 값
    H = 104,             // 'h'의 ASCII 값
    I = 105,             // 'i'의 ASCII 값
    J = 106,             // 'j'의 ASCII 값
    K = 107,             // 'k'의 ASCII 값
    L = 108,             // 'l'의 ASCII 값
    M = 109,             // 'm'의 ASCII 값
    N = 110,             // 'n'의 ASCII 값
    O = 111,             // 'o'의 ASCII 값
    P = 112,             // 'p'의 ASCII 값
    Q = 113,             // 'q'의 ASCII 값
    R = 114,             // 'r'의 ASCII 값
    S = 115,             // 's'의 ASCII 값
    T = 116,             // 't'의 ASCII 값
    U = 117,             // 'u'의 ASCII 값
    V = 118,             // 'v'의 ASCII 값
    W = 119,             // 'w'의 ASCII 값
    X = 120,             // 'x'의 ASCII 값
    Y = 121,             // 'y'의 ASCII 값
    Z = 122,              // 'z'의 ASCII 값

    CAPSLOCK = SDL.SDL_Keycode.SDLK_CAPSLOCK,

    F1 = SDL.SDL_Keycode.SDLK_F1,
    F2 = SDL.SDL_Keycode.SDLK_F2,
    F3 = SDL.SDL_Keycode.SDLK_F3,
    F4 = SDL.SDL_Keycode.SDLK_F4,
    F5 = SDL.SDL_Keycode.SDLK_F5,
    F6 = SDL.SDL_Keycode.SDLK_F6,
    F7 = SDL.SDL_Keycode.SDLK_F7,
    F8 = SDL.SDL_Keycode.SDLK_F8,
    F9 = SDL.SDL_Keycode.SDLK_F9,
    F10 = SDL.SDL_Keycode.SDLK_F10,
    F11 = SDL.SDL_Keycode.SDLK_F11,
    F12 = SDL.SDL_Keycode.SDLK_F12,

    PRINTSCREEN = SDL.SDL_Keycode.SDLK_PRINTSCREEN,
    SCROLLLOCK = SDL.SDL_Keycode.SDLK_SCROLLLOCK,
    PAUSE = SDL.SDL_Keycode.SDLK_PAUSE,
    INSERT = SDL.SDL_Keycode.SDLK_INSERT,
    HOME = SDL.SDL_Keycode.SDLK_HOME,
    PAGEUP = SDL.SDL_Keycode.SDLK_PAGEUP,
    DELETE = 127,
    END = SDL.SDL_Keycode.SDLK_END,
    PAGEDOWN = SDL.SDL_Keycode.SDLK_PAGEDOWN,
    RIGHT = SDL.SDL_Keycode.SDLK_RIGHT,
    LEFT = SDL.SDL_Keycode.SDLK_LEFT,
    DOWN = SDL.SDL_Keycode.SDLK_DOWN,
    UP = SDL.SDL_Keycode.SDLK_UP,

    NUMLOCKCLEAR = SDL.SDL_Keycode.SDLK_NUMLOCKCLEAR,
    // KP_DIVIDE = (int)SDL_Scancode.SDL_SCANCODE_KP_DIVIDE |  SCANCODE_MASK,
    // KP_MULTIPLY = (int)SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY |  SCANCODE_MASK,
    // KP_MINUS = (int)SDL_Scancode.SDL_SCANCODE_KP_MINUS |  SCANCODE_MASK,
    KP_PLUS = SDL.SDL_Keycode.SDLK_KP_PLUS,
    NUM_ENTER = SDL.SDL_Keycode.SDLK_KP_ENTER,
    NUM_1 = SDL.SDL_Keycode.SDLK_KP_1,
    NUM_2 = SDL.SDL_Keycode.SDLK_KP_2,
    NUM_3 = SDL.SDL_Keycode.SDLK_KP_3,
    NUM_4 = SDL.SDL_Keycode.SDLK_KP_4,
    NUM_5 = SDL.SDL_Keycode.SDLK_KP_5,
    NUM_6 = SDL.SDL_Keycode.SDLK_KP_6,
    NUM_7 = SDL.SDL_Keycode.SDLK_KP_7,
    NUM_8 = SDL.SDL_Keycode.SDLK_KP_8,
    NUM_9 = SDL.SDL_Keycode.SDLK_KP_9,
    NUM_0 = SDL.SDL_Keycode.SDLK_KP_0,
    // KP_PERIOD = (int)SDL_Scancode.SDL_SCANCODE_KP_PERIOD |  SCANCODE_MASK,

    // APPLICATION = (int)SDL_Scancode.SDL_SCANCODE_APPLICATION |  SCANCODE_MASK,
    POWER = SDL.SDL_Keycode.SDLK_POWER,
    // KP_EQUALS = (int)SDL_Scancode.SDL_SCANCODE_KP_EQUALS |  SCANCODE_MASK,
    // F13 = (int)SDL_Scancode.SDL_SCANCODE_F13 |  SCANCODE_MASK,
    // F14 = (int)SDL_Scancode.SDL_SCANCODE_F14 |  SCANCODE_MASK,
    // F15 = (int)SDL_Scancode.SDL_SCANCODE_F15 |  SCANCODE_MASK,
    // F16 = (int)SDL_Scancode.SDL_SCANCODE_F16 |  SCANCODE_MASK,
    // F17 = (int)SDL_Scancode.SDL_SCANCODE_F17 |  SCANCODE_MASK,
    // F18 = (int)SDL_Scancode.SDL_SCANCODE_F18 |  SCANCODE_MASK,
    // F19 = (int)SDL_Scancode.SDL_SCANCODE_F19 |  SCANCODE_MASK,
    // F20 = (int)SDL_Scancode.SDL_SCANCODE_F20 |  SCANCODE_MASK,
    // F21 = (int)SDL_Scancode.SDL_SCANCODE_F21 |  SCANCODE_MASK,
    // F22 = (int)SDL_Scancode.SDL_SCANCODE_F22 |  SCANCODE_MASK,
    // F23 = (int)SDL_Scancode.SDL_SCANCODE_F23 |  SCANCODE_MASK,
    // F24 = (int)SDL_Scancode.SDL_SCANCODE_F24 |  SCANCODE_MASK,
    // EXECUTE = (int)SDL_Scancode.SDL_SCANCODE_EXECUTE |  SCANCODE_MASK,
    // HELP = (int)SDL_Scancode.SDL_SCANCODE_HELP |  SCANCODE_MASK,
    // MENU = (int)SDL_Scancode.SDL_SCANCODE_MENU |  SCANCODE_MASK,
    // SELECT = (int)SDL_Scancode.SDL_SCANCODE_SELECT |  SCANCODE_MASK,
    // STOP = (int)SDL_Scancode.SDL_SCANCODE_STOP |  SCANCODE_MASK,
    // AGAIN = (int)SDL_Scancode.SDL_SCANCODE_AGAIN |  SCANCODE_MASK,
    // UNDO = (int)SDL_Scancode.SDL_SCANCODE_UNDO |  SCANCODE_MASK,
    // CUT = (int)SDL_Scancode.SDL_SCANCODE_CUT |  SCANCODE_MASK,
    // COPY = (int)SDL_Scancode.SDL_SCANCODE_COPY |  SCANCODE_MASK,
    // PASTE = (int)SDL_Scancode.SDL_SCANCODE_PASTE |  SCANCODE_MASK,
    // FIND = (int)SDL_Scancode.SDL_SCANCODE_FIND |  SCANCODE_MASK,
    // MUTE = (int)SDL_Scancode.SDL_SCANCODE_MUTE |  SCANCODE_MASK,
    // VOLUMEUP = (int)SDL_Scancode.SDL_SCANCODE_VOLUMEUP |  SCANCODE_MASK,
    // VOLUMEDOWN = (int)SDL_Scancode.SDL_SCANCODE_VOLUMEDOWN |  SCANCODE_MASK,
    // KP_COMMA = (int)SDL_Scancode.SDL_SCANCODE_KP_COMMA |  SCANCODE_MASK,
    // KP_EQUALSAS400 =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_EQUALSAS400 |  SCANCODE_MASK,

    // ALTERASE = (int)SDL_Scancode.SDL_SCANCODE_ALTERASE |  SCANCODE_MASK,
    // SYSREQ = (int)SDL_Scancode.SDL_SCANCODE_SYSREQ |  SCANCODE_MASK,
    // CANCEL = (int)SDL_Scancode.SDL_SCANCODE_CANCEL |  SCANCODE_MASK,
    // CLEAR = (int)SDL_Scancode.SDL_SCANCODE_CLEAR |  SCANCODE_MASK,
    // PRIOR = (int)SDL_Scancode.SDL_SCANCODE_PRIOR |  SCANCODE_MASK,
    // RETURN2 = (int)SDL_Scancode.SDL_SCANCODE_RETURN2 |  SCANCODE_MASK,
    // SEPARATOR = (int)SDL_Scancode.SDL_SCANCODE_SEPARATOR |  SCANCODE_MASK,
    // OUT = (int)SDL_Scancode.SDL_SCANCODE_OUT |  SCANCODE_MASK,
    // OPER = (int)SDL_Scancode.SDL_SCANCODE_OPER |  SCANCODE_MASK,
    // CLEARAGAIN = (int)SDL_Scancode.SDL_SCANCODE_CLEARAGAIN |  SCANCODE_MASK,
    // CRSEL = (int)SDL_Scancode.SDL_SCANCODE_CRSEL |  SCANCODE_MASK,
    // EXSEL = (int)SDL_Scancode.SDL_SCANCODE_EXSEL |  SCANCODE_MASK,

    // KP_00 = (int)SDL_Scancode.SDL_SCANCODE_KP_00 |  SCANCODE_MASK,
    // KP_000 = (int)SDL_Scancode.SDL_SCANCODE_KP_000 |  SCANCODE_MASK,
    // THOUSANDSSEPARATOR =
    //(int)SDL_Scancode.SDL_SCANCODE_THOUSANDSSEPARATOR |  SCANCODE_MASK,
    // DECIMALSEPARATOR =
    //(int)SDL_Scancode.SDL_SCANCODE_DECIMALSEPARATOR |  SCANCODE_MASK,
    // CURRENCYUNIT = (int)SDL_Scancode.SDL_SCANCODE_CURRENCYUNIT |  SCANCODE_MASK,
    // CURRENCYSUBUNIT =
    //(int)SDL_Scancode.SDL_SCANCODE_CURRENCYSUBUNIT |  SCANCODE_MASK,
    // KP_LEFTPAREN = (int)SDL_Scancode.SDL_SCANCODE_KP_LEFTPAREN |  SCANCODE_MASK,
    // KP_RIGHTPAREN = (int)SDL_Scancode.SDL_SCANCODE_KP_RIGHTPAREN |  SCANCODE_MASK,
    // KP_LEFTBRACE = (int)SDL_Scancode.SDL_SCANCODE_KP_LEFTBRACE |  SCANCODE_MASK,
    // KP_RIGHTBRACE = (int)SDL_Scancode.SDL_SCANCODE_KP_RIGHTBRACE |  SCANCODE_MASK,
    // KP_TAB = (int)SDL_Scancode.SDL_SCANCODE_KP_TAB |  SCANCODE_MASK,
    // KP_BACKSPACE = (int)SDL_Scancode.SDL_SCANCODE_KP_BACKSPACE |  SCANCODE_MASK,
    // KP_A = (int)SDL_Scancode.SDL_SCANCODE_KP_A |  SCANCODE_MASK,
    // KP_B = (int)SDL_Scancode.SDL_SCANCODE_KP_B |  SCANCODE_MASK,
    // KP_C = (int)SDL_Scancode.SDL_SCANCODE_KP_C |  SCANCODE_MASK,
    // KP_D = (int)SDL_Scancode.SDL_SCANCODE_KP_D |  SCANCODE_MASK,
    // KP_E = (int)SDL_Scancode.SDL_SCANCODE_KP_E |  SCANCODE_MASK,
    // KP_F = (int)SDL_Scancode.SDL_SCANCODE_KP_F |  SCANCODE_MASK,
    // KP_XOR = (int)SDL_Scancode.SDL_SCANCODE_KP_XOR |  SCANCODE_MASK,
    // KP_POWER = (int)SDL_Scancode.SDL_SCANCODE_KP_POWER |  SCANCODE_MASK,
    // KP_PERCENT = (int)SDL_Scancode.SDL_SCANCODE_KP_PERCENT |  SCANCODE_MASK,
    // KP_LESS = (int)SDL_Scancode.SDL_SCANCODE_KP_LESS |  SCANCODE_MASK,
    // KP_GREATER = (int)SDL_Scancode.SDL_SCANCODE_KP_GREATER |  SCANCODE_MASK,
    // KP_AMPERSAND = (int)SDL_Scancode.SDL_SCANCODE_KP_AMPERSAND |  SCANCODE_MASK,
    // KP_DBLAMPERSAND =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_DBLAMPERSAND |  SCANCODE_MASK,
    // KP_VERTICALBAR =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_VERTICALBAR |  SCANCODE_MASK,
    // KP_DBLVERTICALBAR =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_DBLVERTICALBAR |  SCANCODE_MASK,
    // KP_COLON = (int)SDL_Scancode.SDL_SCANCODE_KP_COLON |  SCANCODE_MASK,
    // KP_HASH = (int)SDL_Scancode.SDL_SCANCODE_KP_HASH |  SCANCODE_MASK,
    // KP_SPACE = (int)SDL_Scancode.SDL_SCANCODE_KP_SPACE |  SCANCODE_MASK,
    // KP_AT = (int)SDL_Scancode.SDL_SCANCODE_KP_AT |  SCANCODE_MASK,
    // KP_EXCLAM = (int)SDL_Scancode.SDL_SCANCODE_KP_EXCLAM |  SCANCODE_MASK,
    // KP_MEMSTORE = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMSTORE |  SCANCODE_MASK,
    // KP_MEMRECALL = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMRECALL |  SCANCODE_MASK,
    // KP_MEMCLEAR = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMCLEAR |  SCANCODE_MASK,
    // KP_MEMADD = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMADD |  SCANCODE_MASK,
    // KP_MEMSUBTRACT =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_MEMSUBTRACT |  SCANCODE_MASK,
    // KP_MEMMULTIPLY =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_MEMMULTIPLY |  SCANCODE_MASK,
    // KP_MEMDIVIDE = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMDIVIDE |  SCANCODE_MASK,
    // KP_PLUSMINUS = (int)SDL_Scancode.SDL_SCANCODE_KP_PLUSMINUS |  SCANCODE_MASK,
    // KP_CLEAR = (int)SDL_Scancode.SDL_SCANCODE_KP_CLEAR |  SCANCODE_MASK,
    // KP_CLEARENTRY = (int)SDL_Scancode.SDL_SCANCODE_KP_CLEARENTRY |  SCANCODE_MASK,
    // KP_BINARY = (int)SDL_Scancode.SDL_SCANCODE_KP_BINARY |  SCANCODE_MASK,
    // KP_OCTAL = (int)SDL_Scancode.SDL_SCANCODE_KP_OCTAL |  SCANCODE_MASK,
    // KP_DECIMAL = (int)SDL_Scancode.SDL_SCANCODE_KP_DECIMAL |  SCANCODE_MASK,
    // KP_HEXADECIMAL =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_HEXADECIMAL |  SCANCODE_MASK,

    LCTRL = SDL.SDL_Keycode.SDLK_LCTRL,
    LSHIFT = SDL.SDL_Keycode.SDLK_LSHIFT,
    LALT = SDL.SDL_Keycode.SDLK_LALT,
    LGUI = SDL.SDL_Keycode.SDLK_LGUI,
    RCTRL = SDL.SDL_Keycode.SDLK_RCTRL,
    RSHIFT = SDL.SDL_Keycode.SDLK_RSHIFT,
    RALT = SDL.SDL_Keycode.SDLK_RALT,
    RGUI = SDL.SDL_Keycode.SDLK_RGUI,

    // MODE = (int)SDL_Scancode.SDL_SCANCODE_MODE |  SCANCODE_MASK,

    // AUDIONEXT = (int)SDL_Scancode.SDL_SCANCODE_AUDIONEXT |  SCANCODE_MASK,
    // AUDIOPREV = (int)SDL_Scancode.SDL_SCANCODE_AUDIOPREV |  SCANCODE_MASK,
    // AUDIOSTOP = (int)SDL_Scancode.SDL_SCANCODE_AUDIOSTOP |  SCANCODE_MASK,
    // AUDIOPLAY = (int)SDL_Scancode.SDL_SCANCODE_AUDIOPLAY |  SCANCODE_MASK,
    // AUDIOMUTE = (int)SDL_Scancode.SDL_SCANCODE_AUDIOMUTE |  SCANCODE_MASK,
    // MEDIASELECT = (int)SDL_Scancode.SDL_SCANCODE_MEDIASELECT |  SCANCODE_MASK,
    // WWW = (int)SDL_Scancode.SDL_SCANCODE_WWW |  SCANCODE_MASK,
    // MAIL = (int)SDL_Scancode.SDL_SCANCODE_MAIL |  SCANCODE_MASK,
    // CALCULATOR = (int)SDL_Scancode.SDL_SCANCODE_CALCULATOR |  SCANCODE_MASK,
    // COMPUTER = (int)SDL_Scancode.SDL_SCANCODE_COMPUTER |  SCANCODE_MASK,
    // AC_SEARCH = (int)SDL_Scancode.SDL_SCANCODE_AC_SEARCH |  SCANCODE_MASK,
    // AC_HOME = (int)SDL_Scancode.SDL_SCANCODE_AC_HOME |  SCANCODE_MASK,
    // AC_BACK = (int)SDL_Scancode.SDL_SCANCODE_AC_BACK |  SCANCODE_MASK,
    // AC_FORWARD = (int)SDL_Scancode.SDL_SCANCODE_AC_FORWARD |  SCANCODE_MASK,
    // AC_STOP = (int)SDL_Scancode.SDL_SCANCODE_AC_STOP |  SCANCODE_MASK,
    // AC_REFRESH = (int)SDL_Scancode.SDL_SCANCODE_AC_REFRESH |  SCANCODE_MASK,
    // AC_BOOKMARKS = (int)SDL_Scancode.SDL_SCANCODE_AC_BOOKMARKS |  SCANCODE_MASK,

    BrightnessDown = SDL.SDL_Keycode.SDLK_BRIGHTNESSDOWN,
    BrightnessUp = SDL.SDL_Keycode.SDLK_BRIGHTNESSUP,
    // DISPLAYSWITCH = (int)SDL_Scancode.SDL_SCANCODE_DISPLAYSWITCH |  SCANCODE_MASK,
    // KBDILLUMTOGGLE =
    //(int)SDL_Scancode.SDL_SCANCODE_KBDILLUMTOGGLE |  SCANCODE_MASK,
    // KBDILLUMDOWN = (int)SDL_Scancode.SDL_SCANCODE_KBDILLUMDOWN |  SCANCODE_MASK,
    // KBDILLUMUP = (int)SDL_Scancode.SDL_SCANCODE_KBDILLUMUP |  SCANCODE_MASK,
    // EJECT = (int)SDL_Scancode.SDL_SCANCODE_EJECT |  SCANCODE_MASK,
    // SLEEP = (int)SDL_Scancode.SDL_SCANCODE_SLEEP |  SCANCODE_MASK,
    // APP1 = (int)SDL_Scancode.SDL_SCANCODE_APP1 |  SCANCODE_MASK,
    // APP2 = (int)SDL_Scancode.SDL_SCANCODE_APP2 |  SCANCODE_MASK,

    // AUDIOREWIND = (int)SDL_Scancode.SDL_SCANCODE_AUDIOREWIND |  SCANCODE_MASK,
    // AUDIOFASTFORWARD = (int)SDL_Scancode.SDL_SCANCODE_AUDIOFASTFORWARD |  SCANCODE_MASK
}

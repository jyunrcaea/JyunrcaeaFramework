using JyunrcaeaFramework.Structs;

namespace JyunrcaeaFramework.EventSystem;

/// <summary>
/// 이벤트 인터페이스가 모여있는 곳입니다.
/// (인터페이스 이름과 그 안에 있는 함수와 이름이 같습니다. 편하게 코딩하세요!)
/// </summary>
public class Events
{
    public interface IPrepare
    {
        public void Prepare();
    }
    public interface IDestroy
    {
        public void Destroy();
    }

    /// <summary>
    /// 글자를 입력함.
    /// </summary>
    public interface IInputText
    {
        public void InputText();
    }

    /// <summary>
    /// 마우스 포커스를 잃음
    /// </summary>
    public interface IMouseFocusOut
    {
        public void MouseFocusOut();
    }

    /// <summary>
    /// 마우스 포커스가 잡힘
    /// </summary>
    public interface IMouseFocusIn
    {
        public void MouseFocusIn();
    }

    /// <summary>
    /// 키보드 포커스를 잃음
    /// </summary>
    public interface IKeyFocusOut
    {
        public void KeyFocusOut();
    }

    /// <summary>
    /// 키보드 포커스가 잡힘
    /// </summary>
    public interface IKeyFocusIn
    {
        public void KeyFocusIn();
    }

    /// <summary>
    /// 창에 파일을 끌어서 놓음
    /// </summary>
    public interface IDropFile
    {
        public void DropFile(string filename);
    }

    /// <summary>
    /// 창 크기가 바뀜 (아직 조정중)
    /// </summary>
    public interface IResize
    {
        public void Resize();
    }

    /// <summary>
    /// 창 크기가 조정됨
    /// </summary>
    public interface IResized
    {
        public void Resized();
    }

    /// <summary>
    /// 업데이트 (렌더링과 같은 주기로 반복됨)
    /// </summary>
    public interface IUpdate
    {
        public void Update(float millisecond);
    }

    /// <summary>
    /// 창이 이동됨
    /// </summary>
    public interface IWindowMove
    {
        public void WindowMove();
    }

    /// <summary>
    /// 키보드에서 키를 누름
    /// </summary>
    public interface IKeyDown
    {
        public void KeyDown(Keycode key);
    }

    /// <summary>
    /// 마우스 포인터가 움직임
    /// </summary>
    public interface IMouseMove
    {
        public void MouseMove();
    }

    /// <summary>
    /// 창에서 나가기를 누름
    /// </summary>
    public interface IWindowQuit
    {
        public void WindowQuit();
    }

    /// <summary>
    /// 마우스의 버튼이 눌림
    /// </summary>
    public interface IMouseKeyDown
    {
        public void MouseKeyDown(MouseKey key);
    }

    /// <summary>
    /// 마우스의 버튼이 완화됨
    /// </summary>
    public interface IMouseKeyUp
    {
        public void MouseKeyUp(MouseKey key);
    }

    /// <summary>
    /// 키보드에서 키가 완화됨
    /// </summary>
    public interface IKeyUp
    {
        public void KeyUp(Keycode key);
    }

    /// <summary>
    /// 창이 최대화됨
    /// </summary>
    public interface IWindowMaximized
    {
        public void WindowMaximized();
    }

    /// <summary>
    /// 창이 최소화됨
    /// </summary>
    public interface IWindowMinimized
    {
        public void WindowMinimized();
    }

    /// <summary>
    /// 창이 복구됨
    /// </summary>
    public interface IWindowRestore
    {
        public void WindowRestore();
    }
}

/// <summary>
/// 모든 이벤트를 모아둔 인터페이스입니다.
/// </summary>
public interface IAllEventInterface :
    Events.IResized,
    Events.IWindowMove,
    Events.IDropFile,
    Events.IUpdate,
    Events.IKeyDown,
    Events.IMouseMove,
    Events.IWindowQuit,
    Events.IMouseKeyDown,
    Events.IMouseKeyUp,
    Events.IKeyUp,
    Events.IWindowMaximized,
    Events.IWindowMinimized,
    Events.IWindowRestore,
    Events.IKeyFocusIn,
    Events.IKeyFocusOut,
    Events.IMouseFocusIn,
    Events.IMouseFocusOut,
    Events.IInputText,
    Events.IPrepare,
    Events.IDestroy
{

}


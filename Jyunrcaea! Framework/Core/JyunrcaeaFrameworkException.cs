namespace JyunrcaeaFramework.Core;

/// <summary>
/// 쥰르케아 프레임워크 내에 발생하는 예외적인 오류입니다.
/// 프레임워크의 예외 오류는 작동 원리만 잘 파악하면 예방할수 있습니다.
/// </summary>
public class JyunrcaeaFrameworkException : Exception
{
    public JyunrcaeaFrameworkException() { }
    /// <summary>
    /// 쥰르케아 프레임워크 예외 오류
    /// </summary>
    /// <param name="message">오류 내용</param>
    public JyunrcaeaFrameworkException(string message) : base(message) { }
}

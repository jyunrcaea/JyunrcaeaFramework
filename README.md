<div align="center">
	<img src='Jyunrcaea/Jyunrcaea!FrameworkIcon.png' alt='쥰르케아 프레임워크 아이콘'>
</div>

# Jyunrcaea! Framework
C# 으로 2D 게임 또는 앱을 쉽게 개발할수 있도록 도와주는 프레임워크.

## Jyunrcaea! Framework 의 이점
* 다른 게임엔진이나 프레임워크와 달리개발 환경 세팅이 매우 간단합니다.
* 블록코딩에서 볼수있는 장면/오브젝트 시스템을 갖고왔습니다.
* 멀티코어(또는 멀티스레딩)을 제공해줍니다.
* 가볍습니다.
* C# 이라는 프로그래밍 언어만 제대로 알면, 처음 써보는 사람도 쉽게 사용할수 있습니다.

## Jyunrcaea! Framework 의 단점
* 기능이 많이 부족합니다. (개발버전)
* 다른 게임 엔진이나 프레임워크에 비해 편의성이 좋진 않습니다.
* 렌더링은 싱글스레딩으로 진행되어 객체가 너무 많아질수록 프레임이 저하될수 있습니다.
* 비디오를 지원하지 않습니다.
* 두개 이상의 창을 생성해 관리할수 없습니다.

## 사용법
(Visual Studio 2022 기준)<br>
Visual Studio 에서 C# 프로젝트를 생성합니다.<br>
그리고 릴리즈에서 제공하는 압축파일을 받고 압축해제 한뒤,<br>
그 파일 안에 있던 Jyunrcaea! Framework.dll 프로젝트에 참조하세요,<br>
그리고 나머지 파일들은 프로젝트에 복사 후 속성에서 '항상 복사'로 설정하신뒤<br>
코딩하시고 디버깅 하면 끝!

## 기여
### 이름
public으로 설정된 변수/함수명은 언더바 없이 지어야 하며,<br>
단어의 첫글자는 대문자여야 합니다.<br>
(예시: ThisIsVariableName)
### 변수
접근성과 관련된 이유를 제외하고는 Get/Set 함수를 생성하는 대신에 프로퍼티를 사용해주세요.<br>
<br>
접근성과 관련된 이유를 제외하고는 숨겨진 변수와 그 변수를 접근하게 해줄 공개 프로퍼티를 생성하는 대신에<br>
변수 자체를 프로터리로 생성해주세요.<br>
#### 잘못된 예
``` c#
class my_phone_is_samsung_galaxy_note_9 {
	int ram;
	public GetRam() => ram;
	public SetRam(int gb) => ram = value * 1024 * 1024 * 1024;

	internal storage;
	public Memory => storage;
}
```
#### 잘된 예
```c#
class MyPhoneIsSamsungGalaxyNote9 {
	int ram;
	public Ram { get => ram; set => ram = value * 1024 * 1024 * 1024 }

	public Memory { get; internal set; };
}
```
___
기여는 언제나 환영합니다.

## 라이선스
이 프로젝트 및 코드들은 MIT License가 적용됩니다.<br><br>
즉 요약하자면
* 복제, 배포, 수정 가능
* 배포시 이 라이선스 사본 첨부해야됨
* 이 프로젝트의 개발자 본인은 보증 및 책임을 지지 않음
* 결론적으로 오픈 소스임.
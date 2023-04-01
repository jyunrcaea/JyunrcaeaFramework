<div align="center">
	<img src="Jyunrcaea! Framework/src/Icon.png" alt='쥰르케아 프레임워크 아이콘'>
</div>

자유로운 2D 프레임워크.

# Jyunrcaea! Framework
C# 으로 2D 게임 또는 앱을 쉽게 개발할수 있도록 도와주는 프레임워크.

## Jyunrcaea! Framework 의 이점
* 다른 게임엔진이나 프레임워크와 달리, 개발 환경 세팅이 매우 간단합니다.
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

## 출시일
딱히 정하진 않았으나, 2024년 초로 예상됨. (갈길이 멀음)

## 플랫폼
### Windows
이 프레임워크는 Windows OS 그리고 .NET 6.0 이 설치된 운영체제에서만 돌아갑니다.<br>
즉 현재까지는 Windows OS 7 이상인 운영체제에서만 실행 가능합니다.
### Mac
지원하지 않습니다. 그리고 지원할 생각도 없습니다.
### Linux
아직 지원하진 않으며, 추후 지원해볼 예정입니다.<br>
SDL2 라이브러리를 .dll 대신 .so 를 사용하면 실행 가능할것으로 보고 있습니다.<br>
(실행은 .NET Runtime 만 있으면 되므로, Wine 은 필요하지 않겠습니다.)
### Android
.NET 6.0 Runtime도 안되고, 최신 Mono도 C# 8.0 밖에 지원 안해서 (Jyunrcaea! Framework는 C# 10.0 을 씀)<br>
지원하고 싶어도 못함... Xamarin 또는 Osu! Framework 쓰셈;;

## 라이선스
이 프로젝트 및 코드들은 MIT License가 적용됩니다.<br><br>
즉 요약하자면
* 복제, 배포, 수정 가능
* 배포시 이 라이선스 사본 첨부해야됨
* 이 프로젝트의 개발자 본인은 보증 및 책임을 지지 않음
* 결론적으로 오픈 소스임.

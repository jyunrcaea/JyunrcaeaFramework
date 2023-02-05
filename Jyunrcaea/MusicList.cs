using Jyunrcaea.MainMenu;
using JyunrcaeaFramework;

namespace Jyunrcaea
{
    namespace MusicList
    {
        public enum AudioFormatType : byte
        {
            FLAC,
            MP3,
            OGG,
            VOC,
            WAV
        }

        public class BitMapInfo
        {
            public AudioFormatType format;
            public string? thumbnail = null;
            public string name;
            public string composer;
            public uint? bpm = null;
            public uint? bpm2 = null;
            public string illustrator_name;
            public double highlight_starttime;
            public double? highlight_endtime;

            public BitMapInfo(AudioFormatType format, string? thumbnail, string name, string composer, uint? bpm,uint? bpm2, string illustrator_name, double highlight_starttime, double? highlight_endtime)
            {
                this.format = format;
                this.thumbnail = thumbnail;
                this.name = name;
                this.composer = composer;
                this.bpm = bpm;
                this.bpm2 = bpm2;
                this.illustrator_name = illustrator_name;
                this.highlight_starttime = highlight_starttime;
                this.highlight_endtime = highlight_endtime;
            }
        }

        public static class MusicList
        {
            public static List<BitMapInfo> list = new();

            public static int allworkcount = -1;
            public static int processedcount = -1;

            public static void Arrange(string musicfolder_path)
            {
                Console.WriteLine("비트맵 읽기 시작");
                var directories = Directory.GetDirectories(musicfolder_path);
                allworkcount = directories.Length;
                for (processedcount = 0; processedcount < allworkcount; processedcount++)
                {
                    //string path = musicfolder_path + "\\" + path + "\\";
                    Console.WriteLine("=== 비트맵 읽는중... 위치: {0} ===", directories[processedcount]);
                    string datapath = directories[processedcount] + "\\data.txt";
                    if (!File.Exists(datapath)) { Console.WriteLine("data.txt 파일이 존재하지 않음. 건너뜀"); continue; }
                    Console.WriteLine("data.txt 파일 발견");
                    var bm = InterpretSettingFile(datapath, out var e,out var el);
                    if (e != null)
                    {
                        Console.WriteLine($"비트맵 읽기 오류 발생 (파일위치: {datapath}, 줄: {el})");
                        Console.Write("원인: ");
                        switch(e)
                        {
                            case BitMapReadErrorType.UnknownCommand:
                                Console.Write("알수없는 명령어");
                                break;
                            case BitMapReadErrorType.BPMIsNotUInt:
                                Console.Write("자연수가 아닌 값으로 BPM 설정을 시도했습니다.");
                                break;
                            case BitMapReadErrorType.BPM2IsNotUInt:
                                Console.Write("자연수가 아닌 값으로 최대 BPM 설정을 시도했습니다.");
                                break;
                            case BitMapReadErrorType.NotExistArgument:
                                Console.Write("명령어에 들어갈 인자를 제공하지 않았습니다.");
                                break;
                            case BitMapReadErrorType.HighlightStarttimeIsNotDouble:
                                Console.Write("하이라이트 시작시간을 실수가 아닌 값으로 설정을 시도했습니다.");
                                break;
                            case BitMapReadErrorType.HighlightEndtimeIsNotDouble:
                                Console.Write("하이라이트 종료시간을 실수가 아닌 값으로 설정을 시도했습니다.");
                                break;
                            case BitMapReadErrorType.HighlightStarttimeIsNegative:
                                Console.Write("하이라이트 시작시간을 음의 실수로 설정을 시도하였습니다.");
                                break;
                            case BitMapReadErrorType.HighlightEndtimeIsNegative:
                                Console.Write("하이라이트 종료시간을 음의 실수로 설정을 시도하였습니다.");
                                break;
                            case BitMapReadErrorType.HighlightTimeIsNegative:
                                Console.Write("하이라이트 시작시간이 종료시간 이후에 위치해있습니다.");
                                break;
                            case BitMapReadErrorType.OutOfHighlightTime:
                                Console.Write("하이라이트 시간이 너무 짧습니다. (2초 미만)");
                                break;
                            case BitMapReadErrorType.UnknownFormat:
                                Console.Write("오디오 포맷이 설정되지 않았습니다.");
                                break;
                            case BitMapReadErrorType.NotExistAudioFile:
                                Console.Write("오디오 파일이 존재하지 않습니다.");break;
                            case BitMapReadErrorType.UnknownFormatCode:
                                Console.Write("알수없는 오디오 포맷입니다. (오디오 포맷은 FLAC, MP3, OGG, VOC, WAV 만 사용가능합니다.)");
                                break;
                            default:
                                Console.Write("알수없음");
                                break;
                        }
                        continue;
                    }
                    Console.WriteLine("성공적으로 읽었습니다.");
                    list.Add(bm);
                    Thread.Sleep(1000);
                }
            }

            static string[] settingfiledata;
            //static int readindex;


            public enum BitMapReadErrorType : byte
            {
                UnknownCommand,
                BPMIsNotUInt,
                BPM2IsNotUInt,
                NotExistArgument,
                HighlightStarttimeIsNotDouble,
                HighlightEndtimeIsNotDouble,
                HighlightStarttimeIsNegative,
                HighlightEndtimeIsNegative,
                HighlightTimeIsNegative,
                OutOfHighlightTime,
                UnknownFormat,
                NotExistAudioFile,
                UnknownFormatCode,
                NotExistMusicFile
            }

            internal static BitMapInfo? InterpretSettingFile(string file_path,out BitMapReadErrorType? error,out int errorline)
            {
                errorline = -1;
                settingfiledata = File.ReadAllLines(file_path);
                AudioFormatType? format = null;
                string? thumbnail = null;
                string name = "(알수없음)";
                string composer = "(알수없음)";
                uint? bpm = null;
                uint? bpm2 = null;
                string illustrator_name = "(알수없음)";
                double highlight_starttime = 0;
                double? highlight_endtime = null;
                error = null;

                while (++errorline < settingfiledata.Length)
                {
                    //주석
                    if (settingfiledata[errorline].StartsWith("//")) continue;
                    string[] commands = settingfiledata[errorline].Split(' ');
                    if (commands.Length < 1) continue;
                    if (commands.Length == 1)
                    {
                        error = BitMapReadErrorType.NotExistArgument;
                        return null;
                    }
                    switch (commands[0].ToLower())
                    {
                        case "format":
                            switch (commands[1].ToLower())
                            {
                                case "flac":
                                    format = AudioFormatType.FLAC;
                                    break;
                                case "mp3":
                                    format = AudioFormatType.MP3;
                                    break;
                                case "ogg":
                                    format = AudioFormatType.OGG;
                                    break;
                                case "wav":
                                    format = AudioFormatType.WAV;
                                    break;
                                case "voc":
                                    format = AudioFormatType.VOC;
                                    break;
                                default:
                                    error = BitMapReadErrorType.UnknownFormatCode;
                                    return null;
                            }
                            break;
                        case "thumbnail":
                            thumbnail = commands[1];
                            break;
                        case "bpm":
                            if (commands[1] == "null") bpm = null;
                            if (!uint.TryParse(commands[1],out var r))
                            {
                                error = BitMapReadErrorType.BPMIsNotUInt;
                                return null;
                            }
                            bpm = r;
                            if (commands.Length > 2)
                            {
                                if (!uint.TryParse(commands[1], out r))
                                {
                                    error = BitMapReadErrorType.BPM2IsNotUInt; return null;
                                }
                                bpm2 = r;
                            }
                            break;
                        case "name":
                            name = commands[1];
                            break;
                        case "composer":
                            composer = commands[1];
                            break;
                        case "illustrator":
                            illustrator_name = commands[1];
                            break;
                        case "highlight":
                            if (!double.TryParse(commands[1],out var d))
                            {
                                error = BitMapReadErrorType.HighlightStarttimeIsNotDouble; return null;
                            }
                            if (d < 0)
                            {
                                error = BitMapReadErrorType.HighlightStarttimeIsNegative; return null;
                            }
                            highlight_starttime = d;
                            if (commands.Length == 2) break;
                            if (!double.TryParse(commands[2],out d))
                            {
                                error = BitMapReadErrorType.HighlightEndtimeIsNotDouble; return null;
                            }
                            if (d < 0)
                            {
                                error = BitMapReadErrorType.HighlightEndtimeIsNegative; return null;
                            }
                            if ( highlight_starttime > (highlight_endtime = d))
                            {
                                error = BitMapReadErrorType.HighlightTimeIsNegative; return null;
                            }
                            if ( highlight_endtime - highlight_starttime < 1)
                            {
                                error = BitMapReadErrorType.OutOfHighlightTime; return null;
                            }
                            break;
                        default:
                            error = BitMapReadErrorType.UnknownCommand;
                            return null;
                    }
                }

                if (format == null)
                {
                    error = BitMapReadErrorType.UnknownFormat; return null;
                }
                string filename = file_path.Remove(file_path.Length - 8) + "audio.";
                switch (format)
                {
                    case AudioFormatType.FLAC:
                        filename += "flac";
                        break;
                    case AudioFormatType.MP3:
                        filename += "mp3";
                        break;
                    case AudioFormatType.OGG:
                        filename += "ogg";
                        break;
                    case AudioFormatType.VOC:
                        filename += "voc";
                        break;
                    case AudioFormatType.WAV:
                        filename += "wav";
                        break;
                }
                if (!File.Exists(filename))
                {
                    error = BitMapReadErrorType.NotExistAudioFile; return null;
                }
                errorline = -1;
                BitMapInfo info = new((AudioFormatType)format, thumbnail, name, composer, bpm, bpm2, illustrator_name, highlight_starttime, highlight_endtime);
                return info;
            }
        }

        public class MainScene : Scene
        {
            public Task arrangebitmap;

            public MainScene() {
                this.EventRejection = true;
                this.Hide = true;
                arrangebitmap = new(LoadBitmap);
                this.AddSprite(new ArrangePrograss());
            }

            public override void Start()
            {
                arrangebitmap.Start();
                base.Start();
                Resize();
            }

            public void LoadBitmap()
            {
                MusicList.Arrange("cache\\data\\music");
                Console.WriteLine("현재 불러온 비트맵 목록");
                if (MusicList.list.Count == 0) 
                    Console.WriteLine("...이 없습니다...");
                else
                    foreach (var i in MusicList.list)
                        Console.WriteLine(
                            $"=== name: {i.name} ===\ncomposer: {i.composer}\nthumbnail path: {i.thumbnail}\nformat: {i.format}\nbpm: {(i.bpm == null ? "(알수없음)" : i.bpm.ToString())}"
                            );
            }
        }

        public class LoadInfoText : TextboxForAnimation
        {
            public LoadInfoText() : base("cache\\font.ttf",0)
            {
                this.OriginX = HorizontalPositionType.Right;
                this.DrawX = HorizontalPositionType.Left;
                this.OriginY = VerticalPositionType.Bottom;
                this.DrawY = VerticalPositionType.Top;
                this.OpacityAnimationState.CompleteFunction = () =>
                {
                    ((MainScene)this.InheritedObject).RemoveSprite(this);
                };
                this.X = -1;
            }

            public override void Start()
            {
                base.Start();
                Resize();
            }

            public override void Resize()
            {
                this.Y = (int)(-5 * Window.AppropriateSize) - 1;
                this.Size = (int)(16 * Window.AppropriateSize);
                base.Resize();
            }

            public override void Update(float ms)
            {
                this.Text = $"비트맵 불러오는중... ({MusicList.processedcount}/{MusicList.allworkcount})";
                base.Update(ms);
            }
        }

        public class ArrangePrograss : RectangleForAnimation
        {
            public ArrangePrograss ()
            {
                this.Color = new(100, 255, 100);
                this.OriginY = VerticalPositionType.Bottom;
                this.DrawY = VerticalPositionType.Top;
                this.OriginX = HorizontalPositionType.Left;
                this.DrawX = HorizontalPositionType.Right;
            }

            float perwidth;

            public void Refresh()
            {
                perwidth = (float)Window.Width / (float)MusicList.allworkcount;
            }

            bool refresh = true;

            LoadInfoText lit;

            public override void Update(float ms)
            {
                if (MusicList.allworkcount == -1) return;
                if ( refresh)
                {
                    refresh = false;
                    Refresh();
                    ((MainScene)this.InheritedObject).AddSprite(lit = new());
                }
                this.Width = (int)(perwidth * MusicList.processedcount);
                if (!removemyself && MusicList.allworkcount == MusicList.processedcount)
                {
                    removemyself = true;
                    this.OpacityAnimationState.CompleteFunction = () =>
                    {
                        ((MainScene)this.InheritedObject).RemoveSprite(this);
                    };
                    lit.Opacity(0, 300f);
                    this.Opacity(0, 300f);
                }
                base.Update(ms);
            }

            bool removemyself = false;

            public override void Resize()
            {
                Refresh();
                this.Height = (int)(5 * Window.AppropriateSize);
                base.Resize();
            }
        }

        public class MusicBar : SpriteForAnimation
        {
            public MusicBar()
            {

            }
        }
    }
}

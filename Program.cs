using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScannedCardDenoiser
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 처리되지 않은 예외를 로그로 남긴다. 스택 트레이스가 없으면 배포판 크래시는 사실상 추적할 수 없다.
            //  - ThreadException   : UI 스레드(메시지 루프)에서 올라온 예외
            //  - UnhandledException: 그 외 스레드(배치/프리뷰 작업 스레드 등)
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ReportCrash(e.Exception, false);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ReportCrash(e.ExceptionObject as Exception, true);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        // 설치 폴더는 쓰기 권한이 없을 수 있어 사용자 로컬 경로에 기록한다.
        private static readonly string CrashLogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ScannedCardDenoiser", "crash.log");

        // fatal이면 CLR이 프로세스를 종료시키므로 계속 실행할 수 없다(기록만 남긴다).
        private static void ReportCrash(Exception ex, bool fatal)
        {
            string text = ex != null ? ex.ToString() : "(알 수 없는 예외)";

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CrashLogPath));
                File.AppendAllText(CrashLogPath,
                    "==== " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + (fatal ? " [fatal]" : "") +
                    Environment.NewLine + text + Environment.NewLine + Environment.NewLine);
            }
            catch { /* 로그 기록 실패가 다시 크래시로 이어지지 않도록 */ }

            try
            {
                MessageBox.Show(
                    (fatal ? "복구할 수 없는 오류로 종료합니다." : "오류가 발생했습니다.") +
                    Environment.NewLine + Environment.NewLine + text +
                    Environment.NewLine + Environment.NewLine + "로그: " + CrashLogPath,
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { /* 종료 중이면 대화상자를 띄울 수 없다 */ }
        }
    }
}

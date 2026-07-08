using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using StudentCourse.Bridge;

namespace StudentCourse.Shell
{
    public partial class MainShellForm : Form
    {
        private WebMessageBridge _messageBridge;

        public MainShellForm()
        {
            InitializeComponent();
            AppRouter.Attach(this);
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await InitializeWebViewAsync();
        }

        private async Task InitializeWebViewAsync()
        {
            try
            {
                string webRoot = ResolveWebRoot();

                await webView.EnsureCoreWebView2Async();
                webView.ZoomFactor = 1.0D;
                webView.CoreWebView2.Settings.IsZoomControlEnabled = false;
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "app.local",
                    webRoot,
                    CoreWebView2HostResourceAccessKind.Allow);

                _messageBridge = new WebMessageBridge(webView);
                NavigateToTeacherHome();
            }
            catch (Exception ex)
            {
                ShowStartupError(ex);
            }
        }

        public void NavigateToTeacherHome()
        {
            if (webView.CoreWebView2 == null)
            {
                return;
            }

            webView.Source = new Uri("https://app.local/teacher.html");
            webView.ZoomFactor = 1.0D;
        }

        private static string ResolveWebRoot()
        {
            string outputWebRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
            if (Directory.Exists(outputWebRoot))
            {
                return outputWebRoot;
            }

            string projectWebRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\wwwroot"));
            if (Directory.Exists(projectWebRoot))
            {
                return projectWebRoot;
            }

            throw new DirectoryNotFoundException("未找到 wwwroot 页面资源目录。");
        }

        private void ShowStartupError(Exception ex)
        {
            string message = System.Net.WebUtility.HtmlEncode(ex.ToString());
            webView.NavigateToString(
                "<!doctype html><html><head><meta charset=\"utf-8\"><style>" +
                "body{font-family:'Microsoft YaHei',Segoe UI,sans-serif;background:#f4f8ff;color:#17324d;padding:40px;}" +
                ".panel{max-width:760px;margin:auto;background:#fff;border:1px solid #d8e6f8;border-radius:12px;padding:28px;box-shadow:0 18px 45px rgba(26,95,180,.12);}" +
                "h1{font-size:22px;color:#0b66d8;margin:0 0 12px;}pre{white-space:pre-wrap;line-height:1.6;font-family:Consolas,'Microsoft YaHei UI',sans-serif;}" +
                "</style></head><body><div class=\"panel\"><h1>页面加载失败</h1><pre>" + message + "</pre></div></body></html>");
        }
    }
}

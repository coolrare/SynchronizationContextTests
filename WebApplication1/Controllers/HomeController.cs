using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Task1(bool continueOnCapturedContext = false)
        {
            System.Web.HttpContext.Current.Items["data"] = "TEST";
            ViewBag.SynchronizationContext = SynchronizationContext.Current;
            ViewBag.ThreadID1 = Thread.CurrentThread.ManagedThreadId;
            // 不要使用 Blocking Thread 的方式在「非同步」方法上
            // 如果「非同步」方法尚未結束就呼叫 Wait() 或 Result 就會產生 Deadlock
            ViewBag.Data = GetAsync(continueOnCapturedContext).Result;
            ViewBag.ThreadID2 = Thread.CurrentThread.ManagedThreadId;
            ViewBag.HttpContext = System.Web.HttpContext.Current;
            return View();
        }

        public async Task<ActionResult> Task2(bool continueOnCapturedContext = false)
        {
            System.Web.HttpContext.Current.Items["data"] = "TEST";
            ViewBag.SynchronizationContext = SynchronizationContext.Current;
            ViewBag.ThreadID1 = Thread.CurrentThread.ManagedThreadId;
            // 不要使用 Blocking Thread 的方式在「非同步」的方法上
            // 如果「非同步」方法尚未結束就呼叫 Wait() 或 Result 就會產生 Deadlock
            ViewBag.Data = GetAsync(continueOnCapturedContext).Result;
            ViewBag.ThreadID2 = Thread.CurrentThread.ManagedThreadId;
            ViewBag.HttpContext = System.Web.HttpContext.Current;
            return View("Task1");
        }

        public async Task<ActionResult> Task3(bool continueOnCapturedContext = false)
        {
            System.Web.HttpContext.Current.Items["data"] = "TEST";
            ViewBag.SynchronizationContext = SynchronizationContext.Current;
            ViewBag.ThreadID1 = Thread.CurrentThread.ManagedThreadId;

            Task<string> t = GetAsync(continueOnCapturedContext);

            // 等到非同步方法執行完畢
            await t;

            // 如果「非同步」方法已經處於完成狀態，就算呼叫 Wait() 或 Result 也不會產生 Deadlock
            ViewBag.Data = t.Result;

            ViewBag.ThreadID2 = Thread.CurrentThread.ManagedThreadId;
            ViewBag.HttpContext = System.Web.HttpContext.Current;
            return View("Task1");
        }

        public async Task<ActionResult> Task4(bool continueOnCapturedContext = false)
        {
            System.Web.HttpContext.Current.Items["data"] = "TEST";
            ViewBag.SynchronizationContext = SynchronizationContext.Current;
            ViewBag.ThreadID1 = Thread.CurrentThread.ManagedThreadId;
            // 可透過 SynchronizationContext.Current 的 Send/Post 方法帶 CurrentThread 過去
            ViewBag.Data = await GetAsync(SynchronizationContext.Current, continueOnCapturedContext);
            ViewBag.ThreadID2 = Thread.CurrentThread.ManagedThreadId;
            return View("Task1");
        }

        public async Task<ActionResult> Task5(bool continueOnCapturedContext = false)
        {
            System.Web.HttpContext.Current.Items["data"] = "TEST";
            ViewBag.SynchronizationContext = SynchronizationContext.Current;
            ViewBag.ThreadID1 = Thread.CurrentThread.ManagedThreadId;
            // 直接傳送 System.Web.HttpContext.Current 過去可以在不同執行緒中共用該物件 (Thread Safe)
            ViewBag.Data = await GetAsync(System.Web.HttpContext.Current, continueOnCapturedContext);
            ViewBag.ThreadID2 = Thread.CurrentThread.ManagedThreadId;
            return View("Task1");
        }


        private async Task<string> GetAsync(bool continueOnCapturedContext = false)
        {
            return await Task.Run(() =>
            {
                // 由於在不同執行緒，所以抓不到 System.Web.HttpContext.Current
                if (System.Web.HttpContext.Current != null)
                {
                    ViewBag.ItemsData = System.Web.HttpContext.Current.Items["data"];
                }

                Thread.Sleep(1000);

                return $"GetAsync's ThreadID = {Thread.CurrentThread.ManagedThreadId}";
            })
            .ConfigureAwait(continueOnCapturedContext);
        }


        private async Task<string> GetAsync(SynchronizationContext sc, bool continueOnCapturedContext = false)
        {
            return await Task.Run(() =>
            {
                Thread.Sleep(1000);

                // 如果是 WPF/WinForm 的話，就可以使用 sc.Post() 非同步方法
                sc.Send((x) =>
                {
                    // 由於透過「同步內容」所以可以得到 HttpContext.Current 物件
                    ViewBag.HttpContext = System.Web.HttpContext.Current;
                    ViewBag.ItemsData = System.Web.HttpContext.Current.Items["data"];
                }, null);

                return $"GetAsync's ThreadID = {Thread.CurrentThread.ManagedThreadId}";

            })
            .ConfigureAwait(continueOnCapturedContext);
        }

        private async Task<string> GetAsync(System.Web.HttpContext context, bool continueOnCapturedContext = false)
        {
            return await Task.Run(() =>
            {
                Thread.Sleep(1000);

                ViewBag.HttpContext = context;
                ViewBag.ItemsData = context.Items["data"];

                return $"GetAsync's ThreadID = {Thread.CurrentThread.ManagedThreadId}";
            })
            .ConfigureAwait(continueOnCapturedContext);
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> About()
        {
            var sc = SynchronizationContext.Current;

            ViewBag.ThreadID1 = Thread.CurrentThread.ManagedThreadId;

            await Task.Run(() =>
            {
                ViewBag.ThreadID2 = Thread.CurrentThread.ManagedThreadId;
                ViewBag.Message = "測試頁面";
            }).ConfigureAwait(false);

            sc.Post((x) =>
            {
                ViewBag.ThreadID3 = Thread.CurrentThread.ManagedThreadId;
            }, null);

            ViewBag.ThreadID4 = Thread.CurrentThread.ManagedThreadId;

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
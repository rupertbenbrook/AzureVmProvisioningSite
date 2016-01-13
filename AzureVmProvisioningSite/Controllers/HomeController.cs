using System;
using AzureVmProvisioningSite.Models;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AzureVmProvisioningSite.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IVmManager _vmManager;

        public HomeController(IVmManager vmManager)
        {
            _vmManager = vmManager;
        }

        public HomeController()
            : this(new VmManager())
        {
        }

        public async Task<ActionResult> Index()
        {
            var vmList = await _vmManager.GetVmsAsync();
            return View(vmList);
        }

        public async Task<ActionResult> ConnectVm(string name)
        {
            await _vmManager.ConnectAsync(name);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> CreateVm(string name)
        {
            await _vmManager.CreateAsync(name);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> StartVm(string name)
        {
            await _vmManager.StartAsync(name);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> StopVm(string name)
        {
            await _vmManager.StopAsync(name);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeleteVm(string name)
        {
            await _vmManager.DeleteAsync(name);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> RestartVm(string name)
        {
            await _vmManager.RestartAsync(name);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> SnapshotVm(string name, string snapshotName)
        {
            await _vmManager.SnapshotAsync(name, snapshotName);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> RestoreVmSnapshot(string name, int snapshotIndex)
        {
            await _vmManager.RestoreSnapshotAsync(name, snapshotIndex);
            return RedirectToAction("Index");
        }
    }
}
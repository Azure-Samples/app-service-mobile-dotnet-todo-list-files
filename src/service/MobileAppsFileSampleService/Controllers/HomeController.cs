using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MobileAppsFileSampleService.DataObjects;
using MobileAppsFileSampleService.Models;

namespace MobileAppsFileSampleService
{
    public class HomeController : Controller
    {
        private MobileAppsFileSampleContext db = new MobileAppsFileSampleContext();

        // GET: Home
        public async Task<ActionResult> Index()
        {
            return View(await db.TodoItems.ToListAsync());
        }

        // GET: Home/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TodoItem todoItem = await db.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return HttpNotFound();
            }
            return View(todoItem);
        }

        // GET: Home/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Home/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Text,Complete,Version,CreatedAt,UpdatedAt,Deleted")] TodoItem todoItem)
        {
            if (ModelState.IsValid)
            {
                if (todoItem.Id == null)
                    todoItem.Id = new Guid().ToString();

                db.TodoItems.Add(todoItem);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(todoItem);
        }

        // GET: Home/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TodoItem todoItem = await db.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return HttpNotFound();
            }
            return View(todoItem);
        }

        // POST: Home/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Text,Complete,Version,CreatedAt,UpdatedAt,Deleted")] TodoItem todoItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(todoItem).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(todoItem);
        }

        // GET: Home/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TodoItem todoItem = await db.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return HttpNotFound();
            }
            return View(todoItem);
        }

        // POST: Home/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            TodoItem todoItem = await db.TodoItems.FindAsync(id);
            todoItem.Deleted = true;
            db.Entry(todoItem).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

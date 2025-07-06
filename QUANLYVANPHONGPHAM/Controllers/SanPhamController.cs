using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QUANLYVANPHONGPHAM.Models;

namespace QUANLYVANPHONGPHAM.Controllers
{
    public class SanPhamController : Controller
    {
        // GET: SanPham
        DB_QLVPPEntities db = new DB_QLVPPEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ShowSanPham()
        {
            List<SanPham> lst_sp = db.SanPhams.ToList();
            return View(lst_sp);  
        }
 
        public ActionResult ChiTietSanPham(int id)
        {   
            SanPham tmp = db.SanPhams.FirstOrDefault(t => t.MaSP == id);
            if (tmp == null) { return RedirectToAction("ShowSanPham"); }
            else { return View(tmp); }
        }
        [HttpGet]
        public ActionResult TimSanPham(string Tensp) 
        {
            List<SanPham> sp = db.SanPhams
            .Where(s => s.MoTa.ToLower().Contains(Tensp.ToLower()))
            .ToList();
            if (sp.Count() == 0)
            {
                return RedirectToAction("KhongTimThay");
            }
            else
            {
                return View(sp);
            }
            
        }
        public ActionResult KhongTimThay()
        {         
            return View();
        }
    }
}
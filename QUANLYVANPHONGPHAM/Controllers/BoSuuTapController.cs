using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QUANLYVANPHONGPHAM.Models;

namespace QUANLYVANPHONGPHAM.Controllers
{
    public class BoSuuTapController : Controller
    {
        // GET: BoSuuTap
        DB_QLVPPEntities db = new DB_QLVPPEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult BoSuuTapPartial()
        {
            List<LoaiSP> lst = db.LoaiSPs.ToList();
            return View(lst); 
        }
        public ActionResult SanPhamTheoLoai(int MaLoai)
        {
            List<SanPham> sp = db.SanPhams.Where(s => s.MaLoai == MaLoai).ToList<SanPham>();
            return View(sp);
        }
        public ActionResult ChiTietSanPham(int id)
        {
            return RedirectToAction("ChiTietSanPham", "SanPham", new { @id = id });
        }

    }
}
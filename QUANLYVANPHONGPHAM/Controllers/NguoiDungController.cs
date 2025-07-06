using QUANLYVANPHONGPHAM.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Protocols;
using static System.Net.Mime.MediaTypeNames;

namespace QUANLYVANPHONGPHAM.Controllers
{
    public class NguoiDungController : Controller
    {
        // GET: NguoiDung
        public ActionResult Index()
        {
            return View();
        }

        DB_QLVPPEntities dbcontext = new DB_QLVPPEntities();

        //action, view DangKy
        public ActionResult DangKy()
        {
            return View();
        }

        //hàm kiểm tra tên tài khoản không được trùng
        public bool ktTenTK(string ptentk)
        {
            var user = dbcontext.NguoiDungs.FirstOrDefault(m=>m.TenTaiKhoan == ptentk);
            if (user == null) 
            {
                return false; // chưa có tên tài khoản này
            }
            else return true; // có tên tài khoản này tồn tại trong csdl
        }

        [HttpPost]
        public ActionResult DangKy(KhachHang pkh)
        {
            if (ModelState.IsValid)
            {
                if (ktTenTK(pkh.TenTaiKhoan)==false)
                {
                    NguoiDung utemp = new NguoiDung();
                    utemp.HoTen = pkh.HoTen;
                    utemp.TenTaiKhoan = pkh.TenTaiKhoan;
                    utemp.MatKhau = pkh.MatKhau;
                    utemp.DiaChi = pkh.DiaChi;
                    utemp.Email = pkh.Email;
                    utemp.SDT = pkh.SDT;
                    
                    
                    ViewBag.SignUpSuccess = true;
                   
                    //gán giá trị mặc định cho cột MaVaiTro là VT003 (mặc định đăng ký luôn là khách hàng)
                    utemp.MaVaiTro = "VT003";

                    //thêm người dùng này vào database
                    dbcontext.NguoiDungs.Add(utemp);
                    dbcontext.SaveChanges();
                    return View();
                }
                else
                {
                    ViewBag.SignUpSuccess = false;
                    return View(pkh);
                }
            }
            return View();
        }

        //action, view DangNhap
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(NguoiDung pnd) 
        {
            if (ModelState.IsValid)
            {
                var user = dbcontext.NguoiDungs.FirstOrDefault(m => m.TenTaiKhoan == pnd.TenTaiKhoan && m.MatKhau == pnd.MatKhau);
                
                if (user!= null) //có tài khoản người dùng này
                {
                    //lưu thông tin người dùng đó vào biến session
                    Session["user"] = user;
                    ViewBag.Login = true;
                   
                    return View();
                }
                else
                {
                    ViewBag.Login = false;
                    ModelState.AddModelError("", "Tên tài khoản hoặc mật khẩu không đúng.");
                }
            }
            return View(pnd);
        }

        //action, view ThongTinCaNhan
        public ActionResult ThongTinCaNhan()
        {
            var user = Session["user"];
            return View(user);
        }

        [HttpPost]
        public ActionResult ThongTinCaNhan(NguoiDung pnd)
        {
            if (ModelState.IsValid)
            {
                var user = (NguoiDung)Session["user"];
                if (user != null)
                {
                    var dbuser = dbcontext.NguoiDungs.SingleOrDefault(m => m.MaND == user.MaND);
                    if (dbuser != null)
                    {
                        //cập nhật thông tin 
                        dbuser.TenTaiKhoan = pnd.TenTaiKhoan;
                        dbuser.HoTen = pnd.HoTen;
                        dbuser.Email = pnd.Email;
                        dbuser.SDT = pnd.SDT;
                        dbuser.DiaChi = pnd.DiaChi;

                        dbcontext.SaveChanges();

                        Session["user"] = dbuser;

                        // Hiển thị thông báo thành công
                        TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    }
                }
            }
            return View(pnd);
        }

        //action DangXuat
        public ActionResult DangXuat()
        {
            // Xóa thông tin người dùng khỏi Session
            Session["user"] = null;

            // Xóa tất cả dữ liệu trong Session (nếu có nhiều dữ liệu cần xóa)
            Session.Clear();
            Session.Abandon();

            // Chuyển hướng người dùng về trang đăng nhập hoặc trang chủ
            return RedirectToAction("ShowSanPham","SanPham");
        }


        //action, view QuenMatKhau
        public ActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        public ActionResult QuenMatKhau(string contactInfo)
        {
            // Kiểm tra email hoặc số điện thoại có tồn tại không
            var user = dbcontext.NguoiDungs.FirstOrDefault(u => u.Email == contactInfo || u.SDT == contactInfo);

            if (user != null)
            {
                // Tạo mã ngẫu nhiên gồm 4 ký tự
                var randomCode = GenerateRandomCode(4);

                // Lưu mật khẩu mới vào csdl
                user.MatKhau = randomCode;
                dbcontext.SaveChanges();

                // Gửi mã ngẫu nhiên tới view để hiển thị
                ViewBag.GeneratedCode = randomCode;

                // Thông báo thành công cho người dùng
                TempData["SuccessMessage"] = "Mã đặt lại mật khẩu đã được tạo. Vui lòng sử dụng mã dưới đây.";
            }
            else
            {
                ModelState.AddModelError("", "Email hoặc số điện thoại không tồn tại trong hệ thống.");
            }

            return View();
        }

        //hàm sinh ra mã ngẫu nhiên
        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }


        //action, view DoiMatKhau
        public ActionResult DoiMatKhau()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DoiMatKhau(DoiMatKhau model)
        {
            if (ModelState.IsValid)
            {
                // Lấy người dùng từ Session
                var user = (NguoiDung)Session["user"];
                if (user == null)
                {
                    return RedirectToAction("DangNhap", "NguoiDung");
                }

                // Kiểm tra mật khẩu cũ có đúng không
                if (user.MatKhau != model.MatKhauCu)
                {
                    TempData["ErrorMessage"] = "Mật khẩu cũ không đúng.";
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                user.MatKhau = model.MatKhauMoi;
                var dbuser = dbcontext.NguoiDungs.FirstOrDefault(u => u.MaND == user.MaND );
                dbuser.MatKhau = user.MatKhau;

                Session["user"] = dbuser;
                dbcontext.SaveChanges();

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return View() ;
            }

            return View(model);
        }
    }
}
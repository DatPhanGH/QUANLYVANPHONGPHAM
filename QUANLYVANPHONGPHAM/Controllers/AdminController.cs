using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using QUANLYVANPHONGPHAM.Models;

namespace QUANLYVANPHONGPHAM.Controllers
{

    public class AdminController : Controller
    {
        // GET: Admin
        DB_QLVPPEntities db = new DB_QLVPPEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AdminPartial() 
        {
            return View();
        }
        public ActionResult TaiSan() 
        {
            List<SanPham> lst_sp = db.SanPhams.ToList();
            return View(lst_sp);
        }
        public ActionResult NhapHang()
        {
            List<SanPham> lst_sp = db.SanPhams.ToList();
            return View(lst_sp);
        }
        
        //hamf chuyển file json thành list sanphamnhap
        public static List<SanPhamNhap> JsonToSanPhamNhapList(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<SanPhamNhap>>(json);
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine("Lỗi định dạng JSON: " + ex.Message);
                return new List<SanPhamNhap>(); // Hoặc xử lý lỗi theo cách khác
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi không xác định: " + ex.Message);
                return new List<SanPhamNhap>();
            }
        }
        [HttpPost]
        public JsonResult NhapHang(string selectedItems, string ngNhapHang, string maNCC)
        {
            // Deserialize chuỗi JSON thành một danh sách các đối tượng SanPhamNhap
            List<SanPhamNhap> receivedItems = JsonToSanPhamNhapList(selectedItems);

            if (receivedItems == null || receivedItems.Count == 0)
            {
                return Json(new { success = false, message = "Không có sản phẩm nào được gửi." });
            }
            // Tạo biến để chứa mã mới
            string newMaPN;

            // Lấy mã mới từ stored procedure
            var maPNParam = new SqlParameter("@NewMaPN", System.Data.SqlDbType.VarChar, 10)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            db.Database.ExecuteSqlCommand("EXEC SP_GetNextMaPN @NewMaPN OUTPUT", maPNParam);
            newMaPN = (string)maPNParam.Value; // Lấy giá trị mã mới
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Tạo phiếu nhập
                    DateTime ngayNhap = DateTime.ParseExact(ngNhapHang, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    var phieuNhap = new PHIEUNHAP { MaPN = newMaPN, MaNCC = maNCC, NgNhapHang = ngayNhap };
                    db.PHIEUNHAPs.Add(phieuNhap);
                    db.SaveChanges();

                    // Thêm chi tiết phiếu nhập bằng bulk insert
                    var chiTietPhieuNhaps = receivedItems.Select(item => new ChiTietPhieuNhap
                    {
                        MaPN = newMaPN,
                        MaSP = item.MaSP,
                        SoLuong = item.SoLuong
                    }).ToList();
                    db.ChiTietPhieuNhaps.AddRange(chiTietPhieuNhaps);
                    db.SaveChanges();

                    transaction.Commit();
                    return Json(new { success = true, message = "Dữ liệu đã được xử lý thành công." });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // Ghi log lỗi và trả về thông báo lỗi phù hợp
                    return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
                }
            }
        }

        //action, view đăng nhập tài khoản admin
        //action, view DangNhap
        public ActionResult DangNhapAdmin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhapAdmin(NguoiDung pnd)
        {
            if (ModelState.IsValid)
            {
                var admin = db.NguoiDungs.FirstOrDefault(m => m.TenTaiKhoan == pnd.TenTaiKhoan && m.MatKhau == pnd.MatKhau);

                if (admin != null) //có tài khoản người dùng này
                {
                    //lưu thông tin người dùng đó vào biến session
                    
                    if (admin.MaVaiTro == "VT001")
                    {
                        Session["admin"] = admin;
                        ViewBag.Login = true;
                        return View();
                    }
                    else
                    {
                        ViewBag.Login = false;
                        ModelState.AddModelError("", "Tài khoản này không có quyền truy cập !");
                    }
                }
                else
                {
                    ViewBag.Login = false;
                    ModelState.AddModelError("", "Tên tài khoản hoặc mật khẩu không đúng.");
                }
            }
            return View(pnd);
        }

        //action, view ThongTinAdmin
        public ActionResult ThongTinAdmin()
        {
            var admin = Session["admin"];
            return View(admin);
        }

        [HttpPost]
        public ActionResult ThongTinAdmin(NguoiDung pnd)
        {
            if (ModelState.IsValid)
            {
                var user = (NguoiDung)Session["admin"];
                if (user != null)
                {
                    var dbuser = db.NguoiDungs.SingleOrDefault(m => m.MaND == user.MaND);
                    if (dbuser != null)
                    {
                        //cập nhật thông tin 
                        dbuser.TenTaiKhoan = pnd.TenTaiKhoan;
                        dbuser.HoTen = pnd.HoTen;
                        dbuser.Email = pnd.Email;
                        dbuser.SDT = pnd.SDT;
                        dbuser.DiaChi = pnd.DiaChi;

                        db.SaveChanges();

                        Session["user"] = dbuser;

                        // Hiển thị thông báo thành công
                        TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    }
                }
            }
            return View(pnd);
        }

        //action DangXuatAdmin
        public ActionResult DangXuatAdmin() 
        {
            // Xóa thông tin người dùng khỏi Session
            Session["admin"] = null;

            // Xóa tất cả dữ liệu trong Session (nếu có nhiều dữ liệu cần xóa)
            Session.Clear();
            Session.Abandon();

            // Chuyển hướng người dùng về trang đăng nhập hoặc trang chủ
            return RedirectToAction("Index", "Admin");
        }

        //action, view QuenMatKhau
        public ActionResult QuenMatKhauAdmin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult QuenMatKhauAdmin(string contactInfo)
        {
            // Kiểm tra email hoặc số điện thoại có tồn tại không
            var user = db.NguoiDungs.FirstOrDefault(u => u.Email == contactInfo || u.SDT == contactInfo);

            if (user != null)
            {
                // Tạo mã ngẫu nhiên gồm 4 ký tự
                var randomCode = GenerateRandomCode(4);

                // Lưu mật khẩu mới vào csdl
                user.MatKhau = randomCode;
                db.SaveChanges();

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
        public ActionResult DoiMatKhauAdmin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DoiMatKhauAdmin(DoiMatKhau model)
        {
            if (ModelState.IsValid)
            {
                // Lấy người dùng từ Session
                var user = (NguoiDung)Session["admin"];
                if (user == null)
                {
                    return RedirectToAction("DangNhapAdmin", "Admin");
                }

                // Kiểm tra mật khẩu cũ có đúng không
                if (user.MatKhau != model.MatKhauCu)
                {
                    TempData["ErrorMessage"] = "Mật khẩu cũ không đúng.";
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                user.MatKhau = model.MatKhauMoi;
                var dbuser = db.NguoiDungs.FirstOrDefault(u => u.MaND == user.MaND);
                dbuser.MatKhau = user.MatKhau;

                Session["admin"] = dbuser;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return View();
            }

            return View(model);
        }


        //action, view DonHangDangChoAdmin hiển thị danh sách các đơn hàng được đặt từ phía khách hàng đang chờ admin xác nhận
        public ActionResult DonHangDangChoAdmin( string sortNgayDat = "NgayDatHang", string iconsort = "fa-sort-up")
        {
            // Truy vấn các đơn hàng có trạng thái là "Chờ xác nhận"
            var donHangsDangChoXacNhan = db.DonHangs.Where(dh => dh.TrangThaiDH == "Đơn hàng đang chờ xác nhận").ToList();

            //sắp xếp cột ngày đặt hàng
            ViewBag.sortNgayDat = sortNgayDat;
            ViewBag.iconsort = iconsort;
            if(sortNgayDat == "NgayDatHang")
            {
                if(iconsort == "fa-sort-asc")
                {
                    donHangsDangChoXacNhan = donHangsDangChoXacNhan.OrderBy(r=>r.NgayDatHang).ToList();
                }
                else
                {
                   donHangsDangChoXacNhan= donHangsDangChoXacNhan.OrderByDescending(r=>r.NgayDatHang).ToList();
                }
            }
            return View(donHangsDangChoXacNhan); // Trả về danh sách đơn hàng cho view
        }

        //hiển thị trang xác nhận của đơn hàng được chọn và cần chọn ngày giao hàng
        [HttpGet]
        public ActionResult XacNhanDonHang(string maDH)
        {
            // Lấy thông tin đơn hàng cần xác nhận
            var donHang = db.DonHangs.Include(dh=>dh.NguoiDung).FirstOrDefault(dh => dh.MaDH == maDH);

            if (donHang == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("DonHangDangChoAdmin");
            }

            // Trả về view với thông tin đơn hàng
            return View(donHang);
        }

        // Xử lý khi admin xác nhận đơn hàng và chọn ngày giao
        [HttpPost]
        public ActionResult XacNhanDonHang(string maDH, DateTime ngayGiaoHang)
        {
            var donHang = db.DonHangs.Include(dh=>dh.NguoiDung).FirstOrDefault(dh => dh.MaDH == maDH);

            if (donHang != null)
            {
                // Cập nhật trạng thái và ngày giao hàng
                donHang.TrangThaiDH = "Đã xác nhận và đang giao hàng";
                donHang.NgayGiao = ngayGiaoHang.Date;

                // Lưu thay đổi vào database
                db.SaveChanges();
                TempData["Message"] = "Đơn hàng đã được xác nhận và cập nhật ngày giao thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy đơn hàng!";
            }

            return RedirectToAction("DonHangDangChoAdmin");
        }

        //quản lý mặt hàng văn phòng phẩm
        //thêm sản phẩm mới
        public ActionResult themSP()
        {
            ViewBag.LoaiSP = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai");
            return View();
        }


        //copy file vao thu muc hinh
        public ActionResult layhinhanh(SanPham s, HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                string path = Path.Combine(Server.MapPath("~/images"), Path.GetFileName(file.FileName));
                file.SaveAs(path);
                s.HinhAnh = "/images/" + Path.GetFileName(file.FileName);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult themSP(SanPham psp, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                if(file != null && file.ContentLength > 0)
                {
                    string path = Path.Combine(Server.MapPath("~/images"), Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    psp.HinhAnh = "images/"+Path.GetFileName(file.FileName);
                }
                db.SanPhams.Add(psp); // Thêm sản phẩm vào DbContext
                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction("TaiSan","Admin"); // Chuyển hướng sau khi lưu
            }

            ViewBag.LoaiSP = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai", psp.MaLoai);
            return View(psp);
        }


        // Hiển thị form chỉnh sửa sản phẩm
        public ActionResult EditSP(int id)
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }

            ViewBag.LoaiSP = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai", sanPham.MaLoai);
            return View(sanPham);
        }

        // Cập nhật thông tin sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSP(SanPham psp, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                var sp = db.SanPhams.Find(psp.MaSP);
                if (sp == null)
                {
                    return HttpNotFound("Sản phẩm không tồn tại.");
                }

                sp.TenSP = psp.TenSP;
                sp.MaLoai = psp.MaLoai;
                sp.GiaBan = psp.GiaBan;
                sp.SoLuongTon = psp.SoLuongTon;
                sp.MoTa = psp.MoTa;
                if (file != null && file.ContentLength > 0)
                {
                    string path = Path.Combine(Server.MapPath("~/images"), Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    psp.HinhAnh = "images/" + Path.GetFileName(file.FileName);
                    sp.HinhAnh = psp.HinhAnh;
                }


                db.SaveChanges();
                return RedirectToAction("TaiSan", "Admin");
            }

            ViewBag.LoaiSP = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai", psp.MaLoai);
            return View(psp);
        }

        // Hiển thị xác nhận xóa sản phẩm
        public ActionResult DeleteSP(int id)
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }

            return View(sanPham);
        }

        // Xóa sản phẩm khỏi cơ sở dữ liệu
        [HttpPost, ActionName("DeleteSP")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDeleteSP(int id)
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham != null)
            {
                db.SanPhams.Remove(sanPham); // Xóa sản phẩm
                db.SaveChanges(); // Lưu thay đổi
            }

            return RedirectToAction("TaiSan", "Admin"); // Chuyển hướng sau khi xóa
        }


        //action, view doanhthu. Hiển thị doanh thu theo tháng
        public ActionResult DoanhThu()
        {
            var model = new DoanhThuViewModel
            {
                LoaiThongKe = "Ngay",
                NgayBatDau = DateTime.Today.AddDays(-7),
                NgayKetThuc = DateTime.Today,
                DanhSachDoanhThu = new List<DoanhThuChiTiet>()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult DoanhThu(DoanhThuViewModel model)
        {
            //lấy ra ds sách các hóa đơn (trong bảng ThanhToan) mà có trạng thái là đã "Thành Công"
            var thanhtoan = db.ThanhToans.AsNoTracking().Where(tt => tt.TrangThai == "Thành Công");

            if(model.NgayBatDau.HasValue&& model.NgayKetThuc.HasValue)
            {
                var ngayBatDau = model.NgayBatDau.Value;
                var ngayKetThuc =model.NgayKetThuc.Value;
                thanhtoan = thanhtoan.Where(tt => DbFunctions.TruncateTime(tt.NgayTT) >= ngayBatDau.Date
                && DbFunctions.TruncateTime(tt.NgayTT) <= ngayKetThuc.Date);
            }
            else if (model.NgayBatDau.HasValue)
            {
                var ngayBatDau = model.NgayBatDau.Value.Date;

                // Lọc các bản ghi trong khoảng ngày đã chọn, bỏ qua phần thời gian
                thanhtoan = thanhtoan.Where(tt => DbFunctions.TruncateTime(tt.NgayTT) == ngayBatDau);
            }

            if(model.LoaiThongKe == "Ngay")
            {
                model.DanhSachDoanhThu = thanhtoan.GroupBy(tt=> DbFunctions.TruncateTime(tt.NgayTT)).Select(g=>new DoanhThuChiTiet 
                { Ngay=g.Key,DoanhThu = g.Sum(x=>x.SoTien)}).OrderBy(x=>x.Ngay).ToList();
            }
            else if(model.LoaiThongKe =="Thang")
            {
                model.DanhSachDoanhThu = thanhtoan.GroupBy(tt => new {tt.NgayTT.Year,tt.NgayTT.Month}).Select(g=>new DoanhThuChiTiet 
                {Nam= g.Key.Year,Thang = g.Key.Month,DoanhThu= g.Sum(x=>x.SoTien) }).OrderBy(x=>x.Nam).ThenBy(x=>x.Thang).ToList();
            }

            return View(model);
        }
    }
}
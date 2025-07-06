using QUANLYVANPHONGPHAM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QUANLYVANPHONGPHAM.Controllers
{
    public class DonHangController : Controller
    {
        // GET: DonHang
        public ActionResult Index()
        {
            return View();
        }

        DB_QLVPPEntities db = new DB_QLVPPEntities();

        //action, view ThongTinDonHang view này dùng để lấy phương thức thanh toán
        public ActionResult ThongTinDonHang()
        {
            string madh = Session["MaDH"] as string;
            var donhang = db.DonHangs.FirstOrDefault(dh => dh.MaDH == madh);
            var thanhtoan = db.ThanhToans.FirstOrDefault(tt => tt.MaDH == madh);

            ThongTinDonHang ttdh = new ThongTinDonHang();
            ttdh.MaDH = donhang.MaDH;
            ttdh.TongTien = float.Parse(donhang.TongTien.ToString());
            ttdh.GiamGia = float.Parse(donhang.GiamGia.ToString());
            ttdh.PhaiTra = float.Parse(donhang.PhaiTra.ToString());
            //mặc định phương thức thanh toán là tiền mặt
            ttdh.PhuongThucTT = "Tiền mặt";

            ttdh.pttt = new List<SelectListItem>
            {
                new SelectListItem {Value = "Tiền mặt", Text = "Thanh toán tiền mặt khi nhận hàng"},
                new SelectListItem { Value = "Chuyển khoản", Text = "Chuyển khoản khi nhận hàng" }
            };

            //lấy giỏ hàng từ session["GioHang"]
            var giohang = Session["GioHang"] as List<GioHang>;

            //gộp vào ttdh
            ttdh.GioHang = giohang;


            return View(ttdh);
        }

        //action, view LuuPTTT dùng để lưu phương thức thanh toán 
        [HttpPost]
        public ActionResult LuuPTTT(ThongTinDonHang ttdh)
        {
            Session.Remove("GioHang");
            var thanhtoan = db.ThanhToans.FirstOrDefault(tt => tt.MaDH == ttdh.MaDH);
            thanhtoan.PhuongThucTT = ttdh.PhuongThucTT;
            if(thanhtoan.PhuongThucTT == null)
            {
                return RedirectToAction("ThongTinDonHang", "DonHang");
            }
            db.SaveChanges();
            return RedirectToAction("ShowSanPham", "SanPham");
        }

        //action, view DonHangDangCho để hiện ra danh sách các đơn hàng của người dùng đang chờ được xác nhận
        public ActionResult DonHangDangCho()
        {
            // Lấy thông tin người dùng từ Session
            var user = Session["user"] as NguoiDung;

            if (user == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Lấy danh sách đơn hàng của người dùng với trạng thái "Đơn hàng đang chờ xác nhận"
            var donHangChoXacNhan = db.DonHangs
                .Where(dh => dh.MaND == user.MaND && dh.TrangThaiDH == "Đơn hàng đang chờ xác nhận")
                .OrderByDescending(dh => dh.NgayDatHang)
                .ToList();

            return View(donHangChoXacNhan);
        }

        //action, view ChiTietDonHang để xem các mặt hàng được đặt
        public ActionResult ChiTietDonHang(string id)
        {
            var donHang = db.DonHangs.FirstOrDefault(dh => dh.MaDH == id);

            if (Session["user"]==null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Lấy danh sách chi tiết đơn hàng
            var chiTietDonHang = db.ChiTietDonHangs.Where(ct => ct.MaDH == id).ToList();

            ViewBag.DonHang = donHang;
            return View(chiTietDonHang);
        }

        //action, view DonHangDangVanChuyen hiển thị các đơn hàng đang giao đến
        public ActionResult DonHangDangVanChuyen()
        {
            // Truy vấn các đơn hàng có trạng thái là "Đã xác nhận và đang giao hàng"
            var donHangsDangVanChuyen = db.DonHangs.Where(dh => dh.TrangThaiDH == "Đã xác nhận và đang giao hàng").ToList();

            return View(donHangsDangVanChuyen); // Trả về danh sách đơn hàng cho view
        }

        //action XacNhanDaNhanHang để cập nhật trạng thái cuối cùng "Đã giao thành công"
        public ActionResult XacNhanDaNhanHang(string maDH)
        {
            // Tìm đơn hàng cần cập nhật
            var donHang = db.DonHangs.FirstOrDefault(dh => dh.MaDH == maDH);
            var thanhtoan = db.ThanhToans.FirstOrDefault(dh=>dh.MaDH == maDH);
            if (donHang != null)
            {
                // Cập nhật trạng thái đơn hàng thành "Đã giao thành công"
                donHang.TrangThaiDH = "Đã giao thành công";

                //cập nhật lại trạng thái và ngày thanh toán
                thanhtoan.TrangThai = "Thành Công";
                thanhtoan.NgayTT =DateTime.Now;

                // Cập nhật lại ngày giao hàng (tức là khi hàng được giao đến thành công thì ngày giao vẫn nên cập nhật )
                donHang.NgayGiao = DateTime.Now;

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();
            }

            return RedirectToAction("DonHangDangVanChuyen"); // Quay lại danh sách đơn hàng
        }

        //
        //action, view LichSuMuaHang để xem lịch sử mua hàng
        public ActionResult LichSuMuaHang()
        {
            // Lấy thông tin người dùng từ Session
            var user = Session["user"] as NguoiDung;

            if (user == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Lấy danh sách đơn hàng của người dùng với trạng thái "Đơn hàng đang chờ xác nhận"
            var donHangChoXacNhan = db.DonHangs
                .Where(dh => dh.MaND == user.MaND && dh.TrangThaiDH == "Đã giao thành công")
                .OrderByDescending(dh => dh.NgayDatHang)
                .ToList();

            return View(donHangChoXacNhan);
        }
    }
}
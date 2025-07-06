using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QUANLYVANPHONGPHAM.Models;

namespace QUANLYVANPHONGPHAM.Controllers
{
    public class GioHangController : Controller
    {
        // GET: GioHang
        DB_QLVPPEntities db = new DB_QLVPPEntities();
        
        public ActionResult Index()
        {
            return View();
        }

        //lấy giỏ hàng
        public List<GioHang> LayGioHang()
        {
            List<GioHang> lst = Session["GioHang"] as List<GioHang>;
            if (lst == null)
            {
                lst = new List<GioHang>();
                Session["GioHang"] = lst;
            }
            return lst;
        }

        public ActionResult XoaSanPham(int id)
        {
            List<GioHang> lst_gh = LayGioHang();
            GioHang sp = lst_gh.Single(s => s.Masp == id);
            if (sp != null)
            {
                lst_gh.RemoveAll(s => s.Masp == id);
                if (lst_gh.Count == 0)
                {
                    return RedirectToAction("GioHangRong", "GioHang");
                }
                return RedirectToAction("GioHang", "GioHang");
            }
            //neu sau khi xoa gio hang khong con san pham

            return RedirectToAction("GioHang", "GioHang");
        }

        //Thêm giỏ hàng
        public ActionResult ThemGioHang(int masp, string url)
        {
            List<GioHang> lst = LayGioHang();
            GioHang sp = lst.Find(s => s.Masp == masp);
            if (sp == null)
            {
                sp = new GioHang(masp);
                lst.Add(sp);
                return Redirect(url);
            }
            else
            {
                sp.Soluong++;
                return Redirect(url);
            }
        }

        private int TongSoLuong()
        {
            int s = 0;
            List<GioHang> lst = Session["GioHang"] as List<GioHang>;
            if (lst != null)
            {
                s = lst.Sum(sp => sp.Soluong);
            }
            return s;
        }
        private double TongThanhTien()
        {
            double s = 0;
            List<GioHang> lst = Session["GioHang"] as List<GioHang>;
            if (lst != null)
            {
                s = lst.Sum(sp => sp.Thanhtien);
            }
            return s;
        }
        public ActionResult GioHang()
        {
            if (Session["GioHang"] == null)
            {
                return RedirectToAction("GioHangRong", "GioHang");
            }
            List<GioHang> lst = LayGioHang();
            if (lst.Count == 0)
            {
                return RedirectToAction("GioHangRong", "GioHang");
            }

            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongThanhTien = TongThanhTien();

            return View(lst);
        }
        public ActionResult GioHangRong()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CapNhatGioHang(int MaSP, FormCollection f)
        {
            List<GioHang> lst_giohang = LayGioHang();
            GioHang sp = lst_giohang.Single(s => s.Masp == MaSP);
            if (sp != null)
            {
                string txt_sl = "txtSoLuong_" + MaSP.ToString();
                sp.Soluong = int.Parse(f[txt_sl].ToString());
            }
            return RedirectToAction("GioHang", "GioHang");
        }

        //hàm sinh mã đơn hàng 
        private string maDonHang(DateTime dt)
        {
            //lấy số thứ tự trong ngày có bao nhiêu đơn hàng
            var sothutu = db.DonHangs.Count(dh => DbFunctions.TruncateTime(dh.NgayDatHang) == dt.Date);

            sothutu = sothutu + 1;

            string madh = "DH" + dt.ToString("ddMMyyyy") + sothutu.ToString();
            return madh;
        }

        //hàm tính số tiền giảm giá
        private double tienGiam(int magg)
        {
            var phantramgiam = db.KhuyenMais.FirstOrDefault(mkm => mkm.MaKM == magg);

            return TongThanhTien() * phantramgiam.PhanTramKM / 100;
        }

        [HttpPost]
        public ActionResult Checkout()
        {
            var user = Session["user"] as QUANLYVANPHONGPHAM.Models.NguoiDung;
            int soluong = TongSoLuong();

            if (user!=null) //nếu người dùng đã đăng nhập tài khoản 
            {
                //nếu người đã đăng nhập
                //lấy danh sách sản phẩm trong giỏ hàng
                List<GioHang> lstgiohang = new List<GioHang>(LayGioHang());

                // Kiểm tra số lượng tồn kho của từng sản phẩm trước khí thanh toán
                foreach (var item in lstgiohang)
                {
                    var sanPham = db.SanPhams.FirstOrDefault(sp => sp.MaSP == item.Masp);
                    if (sanPham == null)
                    {
                        TempData["ErrorMessage"] = $"Sản phẩm {item.Tensp} không tồn tại!";
                        return RedirectToAction("GioHang","GioHang");
                    }

                    if (sanPham.SoLuongTon < item.Soluong)
                    {
                        TempData["ErrorMessage"] = $"Sản phẩm {item.Tensp} chỉ còn {sanPham.SoLuongTon} trong kho. Vui lòng điều chỉnh lại số lượng.";
                        return RedirectToAction("GioHang", "GioHang");
                    }
                }

                //xử lý đơn hàng
                DonHang dh = new DonHang();
                dh.MaDH = maDonHang(DateTime.Now);
                Session["MaDH"] = dh.MaDH;

                dh.MaND = user.MaND;
                dh.NgayDatHang = DateTime.Now;
                dh.TongTien = TongThanhTien();


                //xử lý khuyến mãi 
                if (soluong < 10)
                {
                    dh.MaKM = 1;
                }
                else if (soluong >= 10)
                {
                    dh.MaKM = 2;
                }
                else if (soluong >= 20)
                {
                    dh.MaKM = 3;
                }
                else if (soluong >= 30)
                {
                    dh.MaKM = 4;
                }

                dh.GiamGia = tienGiam(dh.MaKM);
                dh.PhaiTra = dh.TongTien - tienGiam(dh.MaKM);
                dh.TrangThaiDH = "Đơn hàng đang chờ xác nhận";

                //thêm đơn hàng vào csdl
                db.DonHangs.Add(dh);
                db.SaveChanges();

                //duyệt qua danh sách các sản phẩm có trong giỏ hàng để thêm vào bảng chi tiết đơn hàng
                foreach (GioHang item in lstgiohang)
                {
                    ChiTietDonHang ctdh = new ChiTietDonHang();
                    ctdh.MaDH = dh.MaDH;
                    ctdh.MaSP = item.Masp;
                    ctdh.SoLuong = item.Soluong;
                    ctdh.DonGia = item.Dongia;

                    //thêm dòng dữ liệu mới vào bảng ChiTietDonHang
                    db.ChiTietDonHangs.Add(ctdh);
                    db.SaveChanges();

                    // Cập nhật số lượng tồn kho của sản phẩm trong bảng SanPham
                    var sanPham = db.SanPhams.Find(item.Masp);
                    if (sanPham != null)
                    {
                        sanPham.SoLuongTon -= item.Soluong; // Giảm số lượng tồn kho
                        db.SaveChanges();
                    }
                }

                //xử lý thanh toán 
                ThanhToan tt = new ThanhToan();
                tt.MaDH = dh.MaDH;
                tt.NgayTT = DateTime.Now;
                tt.SoTien = dh.PhaiTra;
                tt.PhuongThucTT = string.Empty;
                tt.TrangThai = "Chưa thanh toán";

                //thêm dòng dữ liệu mới vào bảng ThanhToan
                db.ThanhToans.Add(tt);
                db.SaveChanges();

                //sau khi đã nhấn vào nút thanh toán thì giỏ hàng sẽ trống
                lstgiohang.Clear();


                TempData["SuccessMessage"] = "Đơn hàng của bạn đã được đặt thành công!";

                //chuyển đường dẫn đến trang xác nhận đơn hàng lần cuối
                return RedirectToAction("ThongTinDonHang", "DonHang");
            }
            return RedirectToAction("DangNhap", "NguoiDung");
        }

        //Xem chi tiet san pham
        public ActionResult ChiTietSanPham(int id)
        {
            return RedirectToAction("ChiTietSanPham", "SanPham", new { @id = id });
        }

    }
}
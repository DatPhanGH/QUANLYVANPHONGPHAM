using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QUANLYVANPHONGPHAM.Models
{
    public class ThongTinDonHang
    {
        public string MaDH { get; set; }
        public float TongTien { get; set; }
        public float GiamGia { get; set; }
        public float PhaiTra { get; set; }
        public string PhuongThucTT { get; set; }

        //lưu 2 phương thức thanh toán
        public List<SelectListItem> pttt { get; set; }

        //lưu danh sách các sản phẩm có trong giỏ hàng
        public List<GioHang> GioHang { get; set; }
    }
}
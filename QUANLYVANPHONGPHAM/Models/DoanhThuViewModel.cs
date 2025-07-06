using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QUANLYVANPHONGPHAM.Models
{
    public class DoanhThuViewModel
    {
        public string LoaiThongKe { get; set; } // "Ngay" hoặc "Thang"
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public List<DoanhThuChiTiet> DanhSachDoanhThu { get; set; }
    }
}
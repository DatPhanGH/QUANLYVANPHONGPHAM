using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QUANLYVANPHONGPHAM.Models
{
    public class DoanhThuChiTiet
    {
        public DateTime? Ngay { get; set; }
        public int? Thang { get; set; }
        public int? Nam { get; set; }
        public double DoanhThu { get; set; }
    }
}
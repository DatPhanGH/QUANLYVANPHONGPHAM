using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QUANLYVANPHONGPHAM.Models
{
    public class SanPhamViewModel
    {
        public IEnumerable<SanPham> DanhSachSP {  get; set; }
        public SanPham SanPhamMoi { get; set; }
    }
}
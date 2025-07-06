using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QUANLYVANPHONGPHAM.Models
{
    public class DoiMatKhau
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ.")]
        public string MatKhauCu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        public string MatKhauMoi { get; set; }
    }
}
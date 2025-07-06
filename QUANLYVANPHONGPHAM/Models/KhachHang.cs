using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QUANLYVANPHONGPHAM.Models
{
    [MetadataType(typeof(NguoiDungMetadata))]

    public partial class KhachHang:NguoiDung
    {
        string _xacNhanMK;

        [Required(ErrorMessage ="Xác nhận mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
        public string XacNhanMK { get => _xacNhanMK; set => _xacNhanMK = value; }
    }

    public class NguoiDungMetadata
    {
        [Required(ErrorMessage = "Tên tài khoản không được để trống")]
        public string TenTaiKhoan { get; set; }

        [Required(ErrorMessage = "Mật khẩu khẩu không được để trống")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string DiaChi {  get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        public string SDT {  get; set; }    
    }
}
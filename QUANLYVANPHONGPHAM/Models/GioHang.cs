using Antlr.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QUANLYVANPHONGPHAM.Models
{
    public class GioHang
    {
        DB_QLVPPEntities db = new DB_QLVPPEntities();
        int _masp;
        int _maloai;
        string _tensp;
        string _mota;
        Double _dongia;
        int _soluong;
        string _hinhanh;
        double _thanhtien;

        public int Masp { get => _masp; set => _masp = value; }
        public int Maloai { get => _maloai; set => _maloai = value; }
        public string Tensp { get => _tensp; set => _tensp = value; }
        public string Mota { get => _mota; set => _mota = value; }
        public Double Dongia { get => _dongia; set => _dongia = value; }
        public int Soluong { get => _soluong; set => _soluong = value; }
        public string Hinhanh { get => _hinhanh; set => _hinhanh = value; }
        public double Thanhtien { get => _soluong * _dongia; set => _thanhtien = value; }
        public GioHang(int masp)
        {
            Masp = masp;
            SanPham sp = db.SanPhams.Single(s => s.MaSP == Masp);
            Tensp = sp.TenSP;
            Hinhanh = sp.HinhAnh;
            Dongia = double.Parse(sp.GiaBan.ToString());
            Soluong = 1;
        }
    }
}
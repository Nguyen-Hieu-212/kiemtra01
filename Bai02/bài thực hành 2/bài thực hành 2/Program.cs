using System;
using System.Collections.Generic;
using System.Linq;

// ===================== LỚP CHA TRỪU TƯỢNG =====================
abstract class PhuongTien
{
    private string _maPT = "PT000";
    private string _tenHang = "";
    private int _namSanXuat;
    private decimal _giaGoc;

    public string MaPT
    {
        get => _maPT;
        set => _maPT = string.IsNullOrWhiteSpace(value) ? "PT000" : value.Trim();
    }

    public string TenHang
    {
        get => _tenHang;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Tên hãng không được để trống!");
            _tenHang = value.Trim();
        }
    }

    public int NamSanXuat
    {
        get => _namSanXuat;
        set
        {
            if (value < 1900 || value > DateTime.Now.Year)
                throw new ArgumentException("Năm sản xuất không hợp lệ!");
            _namSanXuat = value;
        }
    }

    public decimal GiaGoc
    {
        get => _giaGoc;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Giá gốc phải lớn hơn 0!");
            _giaGoc = value;
        }
    }

    public PhuongTien(string maPT, string tenHang, int namSanXuat, decimal giaGoc)
    {
        MaPT = maPT;
        TenHang = tenHang;
        NamSanXuat = namSanXuat;
        GiaGoc = giaGoc;
    }

    public abstract decimal TinhGiaLanBanh();

    public virtual string GetInfo()
    {
        return $"Mã: {MaPT} | Hãng: {TenHang} | Năm SX: {NamSanXuat} | Giá gốc: {GiaGoc:N0}";
    }
}

// ===================== Ô TÔ =====================
class OTo : PhuongTien
{
    private int _soChoNgoi;
    private double _dungTichDongCo;

    public int SoChoNgoi
    {
        get => _soChoNgoi;
        set
        {
            if (value <= 0) throw new ArgumentException("Số chỗ ngồi phải > 0!");
            _soChoNgoi = value;
        }
    }

    public double DungTichDongCo
    {
        get => _dungTichDongCo;
        set
        {
            if (value <= 0) throw new ArgumentException("Dung tích động cơ phải > 0!");
            _dungTichDongCo = value;
        }
    }

    public OTo(string ma, string ten, int nam, decimal gia, int soCho, double dungTich)
        : base(ma, ten, nam, gia)
    {
        SoChoNgoi = soCho;
        DungTichDongCo = dungTich;
    }

    public override decimal TinhGiaLanBanh()
    {
        if (SoChoNgoi <= 9)
            return GiaGoc + GiaGoc * 0.12m + GiaGoc * 0.30m;
        return GiaGoc + GiaGoc * 0.10m;
    }

    public override string GetInfo()
    {
        return base.GetInfo() + $" | Số chỗ: {SoChoNgoi} | Dung tích ĐC: {DungTichDongCo}";
    }
}

// ===================== XE MÁY =====================
class XeMay : PhuongTien
{
    private int _dungTichXylanh;

    public int DungTichXylanh
    {
        get => _dungTichXylanh;
        set
        {
            if (value <= 0) throw new ArgumentException("Dung tích xylanh phải > 0!");
            _dungTichXylanh = value;
        }
    }

    public XeMay(string ma, string ten, int nam, decimal gia, int cc)
        : base(ma, ten, nam, gia)
    {
        DungTichXylanh = cc;
    }

    public override decimal TinhGiaLanBanh()
    {
        if (DungTichXylanh < 175)
            return GiaGoc + GiaGoc * 0.02m;
        return GiaGoc + GiaGoc * 0.05m;
    }

    public override string GetInfo()
    {
        return base.GetInfo() + $" | Dung tích xylanh: {DungTichXylanh}cc";
    }
}

// ===================== QUẢN LÝ =====================
class QuanLyPhuongTien
{
    private List<PhuongTien> _ds = new List<PhuongTien>();

    public void AddPhuongTien(PhuongTien pt)
    {
        _ds.Add(pt);
    }

    public void DisplayAll()
    {
        foreach (PhuongTien pt in _ds)
        {
            Console.WriteLine(pt.GetInfo());
            Console.WriteLine($"   => Giá lăn bánh: {pt.TinhGiaLanBanh():N0}");
        }
    }

    public PhuongTien FindMaxGiaLanBanh()
    {
        if (_ds.Count == 0) return null;

        PhuongTien max = _ds[0];
        foreach (PhuongTien pt in _ds)
        {
            if (pt.TinhGiaLanBanh() > max.TinhGiaLanBanh())
                max = pt;
        }
        return max;
    }

    public List<PhuongTien> SearchByName(string keyword)
    {
        return _ds.Where(p => p.TenHang.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}

// ===================== CHƯƠNG TRÌNH CHÍNH =====================
class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        QuanLyPhuongTien ql = new QuanLyPhuongTien();
        bool chay = true;

        while (chay)
        {
            Console.WriteLine("\n===== QUẢN LÝ PHƯƠNG TIỆN - AUTOSPEED =====");
            Console.WriteLine("1. Thêm Ô tô");
            Console.WriteLine("2. Thêm Xe máy");
            Console.WriteLine("3. Hiển thị tất cả");
            Console.WriteLine("4. Tìm phương tiện giá lăn bánh cao nhất");
            Console.WriteLine("5. Tìm theo tên hãng");
            Console.WriteLine("0. Thoát");
            Console.Write("Chọn: ");

            switch (Console.ReadLine())
            {
                case "1":
                    ql.AddPhuongTien(NhapOTo());
                    Console.WriteLine("Thêm Ô tô thành công!");
                    break;
                case "2":
                    ql.AddPhuongTien(NhapXeMay());
                    Console.WriteLine("Thêm Xe máy thành công!");
                    break;
                case "3":
                    ql.DisplayAll();
                    break;
                case "4":
                    PhuongTien max = ql.FindMaxGiaLanBanh();
                    if (max == null) Console.WriteLine("Danh sách trống.");
                    else Console.WriteLine(max.GetInfo() + $"\n   => Giá lăn bánh: {max.TinhGiaLanBanh():N0}");
                    break;
                case "5":
                    Console.Write("Nhập tên hãng cần tìm: ");
                    List<PhuongTien> kq = ql.SearchByName(Console.ReadLine());
                    if (kq.Count == 0) Console.WriteLine("Không tìm thấy.");
                    foreach (PhuongTien pt in kq) Console.WriteLine(pt.GetInfo());
                    break;
                case "0":
                    chay = false;
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ!");
                    break;
            }
        }
    }

    // Nhập lại cho đến khi dữ liệu hợp lệ
    static OTo NhapOTo()
    {
        while (true)
        {
            try
            {
                Console.Write("Mã PT: ");
                string ma = Console.ReadLine();
                Console.Write("Tên hãng: ");
                string ten = Console.ReadLine();
                Console.Write("Năm sản xuất: ");
                int nam = int.Parse(Console.ReadLine());
                Console.Write("Giá gốc: ");
                decimal gia = decimal.Parse(Console.ReadLine());
                Console.Write("Số chỗ ngồi: ");
                int soCho = int.Parse(Console.ReadLine());
                Console.Write("Dung tích động cơ: ");
                double dungTich = double.Parse(Console.ReadLine());

                return new OTo(ma, ten, nam, gia, soCho, dungTich);
            }
            catch (ArgumentException ex) { Console.WriteLine("Lỗi: " + ex.Message + " Nhập lại!"); }
            catch (FormatException) { Console.WriteLine("Lỗi: sai định dạng số. Nhập lại!"); }
            catch (OverflowException) { Console.WriteLine("Lỗi: số quá lớn. Nhập lại!"); }
        }
    }

    static XeMay NhapXeMay()
    {
        while (true)
        {
            try
            {
                Console.Write("Mã PT: ");
                string ma = Console.ReadLine();
                Console.Write("Tên hãng: ");
                string ten = Console.ReadLine();
                Console.Write("Năm sản xuất: ");
                int nam = int.Parse(Console.ReadLine());
                Console.Write("Giá gốc: ");
                decimal gia = decimal.Parse(Console.ReadLine());
                Console.Write("Dung tích xylanh (cc): ");
                int cc = int.Parse(Console.ReadLine());

                return new XeMay(ma, ten, nam, gia, cc);
            }
            catch (ArgumentException ex) { Console.WriteLine("Lỗi: " + ex.Message + " Nhập lại!"); }
            catch (FormatException) { Console.WriteLine("Lỗi: sai định dạng số. Nhập lại!"); }
            catch (OverflowException) { Console.WriteLine("Lỗi: số quá lớn. Nhập lại!"); }
        }
    }
}
using bai_thuc_hanh3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace bai_thuc_hanh3
{
    public class MainForm : Form
    {
        // ====== Controls ======
        private TableLayoutPanel tlpMain;
        private MenuStrip menuStrip;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblTotal;

        private TextBox txtProductId, txtProductName, txtUnitPrice, txtQuantity, txtSearch;
        private ComboBox cboCategory;
        private PictureBox picAvatar;
        private Button btnChooseImage, btnAdd, btnUpdate, btnDelete, btnClear, btnExport;
        private DataGridView dgvProducts;
        private ErrorProvider errorProvider;

        // ====== Data ======
        private readonly List<Product> _allProducts = new List<Product>();      // dữ liệu gốc
        private readonly BindingList<Product> _view = new BindingList<Product>(); // dữ liệu đang hiển thị (sau lọc)
        private readonly BindingSource bsProducts = new BindingSource();
        private string _currentImagePath = null;
        private bool _loading = false;

        public MainForm()
        {
            InitializeUi();
            LoadCategories();
            LoadSampleData();

            bsProducts.DataSource = _view;
            dgvProducts.DataSource = bsProducts;

            ApplyFilter();
            ClearInputs();
        }

        // =====================================================
        //  XÂY DỰNG GIAO DIỆN
        // =====================================================
        private void InitializeUi()
        {
            Text = "TechMart Product Manager";
            Size = new Size(1100, 650);
            MinimumSize = new Size(900, 550);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);

            errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.AlwaysBlink };

            // ---- MenuStrip ----
            menuStrip = new MenuStrip();
            var mnuFile = new ToolStripMenuItem("&File");
            var mnuExport = new ToolStripMenuItem("Export CSV")
            {
                ShortcutKeys = Keys.Control | Keys.E
            };
            var mnuExit = new ToolStripMenuItem("Exit")
            {
                ShortcutKeys = Keys.Control | Keys.X
            };
            mnuExport.Click += (s, e) => ExportCsv();
            mnuExit.Click += (s, e) => Close();
            mnuFile.DropDownItems.Add(mnuExport);
            mnuFile.DropDownItems.Add(new ToolStripSeparator());
            mnuFile.DropDownItems.Add(mnuExit);
            menuStrip.Items.Add(mnuFile);

            // ---- StatusStrip ----
            statusStrip = new StatusStrip();
            lblTotal = new ToolStripStatusLabel("Tổng số sản phẩm: 0");
            statusStrip.Items.Add(lblTotal);

            // ---- TableLayoutPanel chính: 35% | 65% ----
            tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(8)
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tlpMain.Controls.Add(BuildInputPanel(), 0, 0);
            tlpMain.Controls.Add(BuildGridPanel(), 1, 0);

            // Thứ tự Add: Fill trước, rồi Menu/Status (Dock Top/Bottom) để không bị đè
            Controls.Add(tlpMain);
            Controls.Add(statusStrip);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
        }

        private Control BuildInputPanel()
        {
            var grp = new GroupBox { Text = "Thông tin sản phẩm", Dock = DockStyle.Fill, Padding = new Padding(8) };

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 5; i++) tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // hàng ảnh co giãn
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // (dự phòng)
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // hàng nút

            txtProductId = new TextBox { ReadOnly = true, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtProductName = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cboCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtUnitPrice = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtQuantity = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };

            AddRow(tlp, 0, "Mã SP:", txtProductId);
            AddRow(tlp, 1, "Tên SP:", txtProductName);
            AddRow(tlp, 2, "Danh mục:", cboCategory);
            AddRow(tlp, 3, "Đơn giá:", txtUnitPrice);
            AddRow(tlp, 4, "Số lượng:", txtQuantity);

            // Hàng ảnh: PictureBox + nút "Chọn ảnh" bên cạnh
            var tlpImg = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            tlpImg.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpImg.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpImg.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            picAvatar = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };
            btnChooseImage = new Button { Text = "Chọn Ảnh", AutoSize = true, Anchor = AnchorStyles.Top };
            btnChooseImage.Click += BtnChooseImage_Click;
            tlpImg.Controls.Add(picAvatar, 0, 0);
            tlpImg.Controls.Add(btnChooseImage, 1, 0);

            var lblImg = new Label { Text = "Ảnh:", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(3, 8, 3, 3) };
            tlp.Controls.Add(lblImg, 0, 5);
            tlp.Controls.Add(tlpImg, 1, 5);

            // Hàng nút hành động
            var flp = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            btnAdd = new Button { Text = "Thêm mới", AutoSize = true };
            btnUpdate = new Button { Text = "Cập nhật", AutoSize = true };
            btnDelete = new Button { Text = "Xóa", AutoSize = true };
            btnClear = new Button { Text = "Làm mới", AutoSize = true };
            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (s, e) => { ClearInputs(); dgvProducts.ClearSelection(); };
            flp.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });
            tlp.Controls.Add(flp, 0, 7);
            tlp.SetColumnSpan(flp, 2);

            grp.Controls.Add(tlp);
            return grp;
        }

        private static void AddRow(TableLayoutPanel tlp, int row, string label, Control input)
        {
            var lbl = new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 8, 8) };
            tlp.Controls.Add(lbl, 0, row);
            tlp.Controls.Add(input, 1, row);
        }

        private Control BuildGridPanel()
        {
            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Thanh tìm kiếm + nút xuất CSV
            var tlpTop = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, AutoSize = true };
            tlpTop.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpTop.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var lblSearch = new Label { Text = "Tìm kiếm:", AutoSize = true, Anchor = AnchorStyles.Left };
            txtSearch = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtSearch.TextChanged += (s, e) => ApplyFilter();   // Live search
            btnExport = new Button { Text = "Xuất CSV", AutoSize = true, Anchor = AnchorStyles.Right };
            btnExport.Click += (s, e) => ExportCsv();
            tlpTop.Controls.Add(lblSearch, 0, 0);
            tlpTop.Controls.Add(txtSearch, 1, 0);
            tlpTop.Controls.Add(btnExport, 2, 0);

            // DataGridView
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã SP", DataPropertyName = "Id", FillWeight = 15 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tên SP", DataPropertyName = "Name", FillWeight = 35 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Danh Mục", DataPropertyName = "CategoryName", FillWeight = 20 });
            var colPrice = new DataGridViewTextBoxColumn { HeaderText = "Đơn Giá", DataPropertyName = "UnitPrice", FillWeight = 20 };
            colPrice.DefaultCellStyle.Format = "#,##0 \"VNĐ\"";
            colPrice.DefaultCellStyle.FormatProvider = CultureInfo.InvariantCulture;
            colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProducts.Columns.Add(colPrice);
            var colQty = new DataGridViewTextBoxColumn { HeaderText = "Số Lượng", DataPropertyName = "Quantity", FillWeight = 10 };
            colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProducts.Columns.Add(colQty);

            // Click chọn dòng -> nạp ngược lên ô nhập liệu
            dgvProducts.SelectionChanged += DgvProducts_SelectionChanged;

            tlp.Controls.Add(tlpTop, 0, 0);
            tlp.Controls.Add(dgvProducts, 0, 1);
            return tlp;
        }

        // =====================================================
        //  DỮ LIỆU
        // =====================================================
        private void LoadCategories()
        {
            var categories = new List<Category>
            {
                new Category(1, "Điện thoại"),
                new Category(2, "Laptop"),
                new Category(3, "Phụ kiện")
            };
            cboCategory.DisplayMember = "CategoryName";
            cboCategory.ValueMember = "CategoryId";
            cboCategory.DataSource = categories;
        }

        private void LoadSampleData()
        {
            _allProducts.Add(new Product { Id = "SP001", Name = "iPhone 15 Pro", CategoryId = 1, CategoryName = "Điện thoại", UnitPrice = 28990000, Quantity = 10 });
            _allProducts.Add(new Product { Id = "SP002", Name = "MacBook Air M2", CategoryId = 2, CategoryName = "Laptop", UnitPrice = 24990000, Quantity = 5 });
            _allProducts.Add(new Product { Id = "SP003", Name = "Chuột Logitech MX Master 3", CategoryId = 3, CategoryName = "Phụ kiện", UnitPrice = 1990000, Quantity = 30 });
        }

        // Lọc theo tên (không phân biệt hoa thường) rồi đổ vào BindingList
        private void ApplyFilter()
        {
            string keyword = txtSearch == null ? "" : txtSearch.Text.Trim();
            string selectedId = txtProductId?.Text;

            _loading = true;
            _view.RaiseListChangedEvents = false;
            _view.Clear();
            foreach (var p in _allProducts)
            {
                if (keyword.Length == 0 ||
                    CultureInfo.CurrentCulture.CompareInfo.IndexOf(p.Name, keyword, CompareOptions.IgnoreCase) >= 0)
                    _view.Add(p);
            }
            _view.RaiseListChangedEvents = true;
            _view.ResetBindings();
            _loading = false;

            // Giữ lại dòng đang chọn nếu còn trong kết quả
            if (!string.IsNullOrEmpty(selectedId)) SelectRowById(selectedId);

            lblTotal.Text = "Tổng số sản phẩm: " + _allProducts.Count;
        }

        private void SelectRowById(string id)
        {
            foreach (DataGridViewRow row in dgvProducts.Rows)
            {
                if (row.DataBoundItem is Product p && p.Id == id)
                {
                    row.Selected = true;
                    if (row.Index >= 0) dgvProducts.CurrentCell = row.Cells[0];
                    return;
                }
            }
        }

        private string GenerateNewId()
        {
            int max = 0;
            foreach (var p in _allProducts)
            {
                if (p.Id != null && p.Id.StartsWith("SP") && int.TryParse(p.Id.Substring(2), out int n) && n > max)
                    max = n;
            }
            return "SP" + (max + 1).ToString("D3");
        }

        // =====================================================
        //  SỰ KIỆN
        // =====================================================
        private void DgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (_loading || dgvProducts.CurrentRow == null || dgvProducts.SelectedRows.Count == 0) return;
            if (!(dgvProducts.CurrentRow.DataBoundItem is Product p)) return;

            errorProvider.Clear();
            txtProductId.Text = p.Id;
            txtProductName.Text = p.Name;
            cboCategory.SelectedValue = p.CategoryId;
            txtUnitPrice.Text = p.UnitPrice.ToString("0.##", CultureInfo.CurrentCulture);
            txtQuantity.Text = p.Quantity.ToString();
            ShowImage(p.ImagePath);
        }

        private void BtnChooseImage_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn ảnh sản phẩm";
                ofd.Filter = "Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Tất cả|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                    ShowImage(ofd.FileName);
            }
        }

        private void ShowImage(string path)
        {
            _currentImagePath = null;
            var old = picAvatar.Image;
            picAvatar.Image = null;
            old?.Dispose();

            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            try
            {
                // Copy ra Bitmap để không khóa file ảnh
                using (var temp = Image.FromFile(path))
                    picAvatar.Image = new Bitmap(temp);
                _currentImagePath = path;
            }
            catch (Exception)
            {
                MessageBox.Show("Không thể mở file ảnh này.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(out decimal price, out int qty)) return;

            var cat = (Category)cboCategory.SelectedItem;
            var p = new Product
            {
                Id = GenerateNewId(),
                Name = txtProductName.Text.Trim(),
                CategoryId = cat.CategoryId,
                CategoryName = cat.CategoryName,
                UnitPrice = price,
                Quantity = qty,
                ImagePath = _currentImagePath
            };
            _allProducts.Add(p);

            txtProductId.Text = p.Id;
            ApplyFilter();
            SelectRowById(p.Id);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            var p = _allProducts.FirstOrDefault(x => x.Id == txtProductId.Text);
            if (p == null)
            {
                MessageBox.Show("Hãy chọn một sản phẩm trên bảng để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ValidateInput(out decimal price, out int qty)) return;

            var cat = (Category)cboCategory.SelectedItem;
            p.Name = txtProductName.Text.Trim();
            p.CategoryId = cat.CategoryId;
            p.CategoryName = cat.CategoryName;
            p.UnitPrice = price;
            p.Quantity = qty;
            p.ImagePath = _currentImagePath;

            ApplyFilter();
            SelectRowById(p.Id);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var p = _allProducts.FirstOrDefault(x => x.Id == txtProductId.Text);
            if (p == null)
            {
                MessageBox.Show("Hãy chọn một sản phẩm trên bảng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc muốn xóa sản phẩm \"{p.Name}\"?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            _allProducts.Remove(p);
            ClearInputs();
            ApplyFilter();
        }

        // =====================================================
        //  VALIDATION
        // =====================================================
        private bool ValidateInput(out decimal price, out int qty)
        {
            errorProvider.Clear();
            bool ok = true;
            price = 0; qty = 0;

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                errorProvider.SetError(txtProductName, "Tên sản phẩm không được để trống");
                ok = false;
            }
            if (!decimal.TryParse(txtUnitPrice.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out price) || price <= 0)
            {
                errorProvider.SetError(txtUnitPrice, "Đơn giá phải là số lớn hơn 0");
                ok = false;
            }
            if (!int.TryParse(txtQuantity.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out qty) || qty < 0)
            {
                errorProvider.SetError(txtQuantity, "Số lượng phải là số nguyên ≥ 0");
                ok = false;
            }
            return ok;
        }

        private void ClearInputs()
        {
            errorProvider.Clear();
            txtProductId.Text = "";
            txtProductName.Text = "";
            txtUnitPrice.Text = "";
            txtQuantity.Text = "";
            if (cboCategory.Items.Count > 0) cboCategory.SelectedIndex = 0;
            ShowImage(null);
            txtProductName.Focus();
        }

        // =====================================================
        //  XUẤT CSV
        // =====================================================
        private void ExportCsv()
        {
            if (_allProducts.Count == 0)
            {
                MessageBox.Show("Danh sách trống, không có gì để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV file (*.csv)|*.csv";
                sfd.FileName = "DanhSachSanPham.csv";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Mã SP,Tên SP,Danh Mục,Đơn Giá,Số Lượng");
                    foreach (var p in _allProducts)
                    {
                        sb.AppendLine(string.Join(",",
                            Csv(p.Id), Csv(p.Name), Csv(p.CategoryName),
                            p.UnitPrice.ToString("0.##", CultureInfo.InvariantCulture),
                            p.Quantity.ToString()));
                    }
                    // UTF-8 có BOM để Excel đọc đúng tiếng Việt
                    File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                    MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi ghi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InitializeComponent()
        {

        }

        private static string Csv(string s)
        {
            if (s == null) return "";
            if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }
    }
}
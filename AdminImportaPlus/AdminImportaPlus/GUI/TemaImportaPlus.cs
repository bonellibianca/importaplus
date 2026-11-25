using System.Drawing;
using System.Windows.Forms;

namespace AdminImportaPlus.GUI;

/// <summary>
/// Paleta Institucional IMPORTA+ fusionada con estética moderna estilo Apple/macOS.
/// </summary>
public static class TemaImportaPlus
{
    public static readonly Color HeaderBurgundy = Color.FromArgb(46, 5, 16);        // #2E0510 (Sidebar profundo elegante)
    public static readonly Color BrandPrimary = Color.FromArgb(83, 6, 24);         // #530618 (Vinotinto oficial Web)
    public static readonly Color BrandHover = Color.FromArgb(115, 16, 40);         // #731028 (Hover luminoso)
    public static readonly Color BrandAccent = Color.FromArgb(155, 48, 64);        // #9B3040
    public static readonly Color BrandTintBg = Color.FromArgb(255, 248, 249);      // #FFF8F9
    public static readonly Color BrandBorderLight = Color.FromArgb(240, 212, 218); // #F0D4DA

    public static readonly Color SurfaceBg = Color.FromArgb(245, 245, 247);        // #F5F5F7 (Apple Canvas)
    public static readonly Color CardWhite = Color.FromArgb(255, 255, 255);        // #FFFFFF
    public static readonly Color BorderNeutral = Color.FromArgb(229, 231, 235);    // #E5E7EB
    public static readonly Color TextDark = Color.FromArgb(29, 29, 31);            // #1D1D1F (Apple Ink)
    public static readonly Color TextBody = Color.FromArgb(110, 110, 115);         // #6E6E73 (Apple Secondary)
    public static readonly Color EmeraldGreen = Color.FromArgb(5, 150, 105);       // #059669

    public static readonly Font FontTituloGrande = new("Segoe UI Semibold", 16F, FontStyle.Regular);
    public static readonly Font FontSubtitulo = new("Segoe UI Semibold", 11.5F, FontStyle.Regular);
    public static readonly Font FontEtiqueta = new("Segoe UI Semibold", 8.5F, FontStyle.Regular);
    public static readonly Font FontNormal = new("Segoe UI", 9.5F, FontStyle.Regular);
    public static readonly Font FontBoton = new("Segoe UI Semibold", 9.5F, FontStyle.Regular);

    public static void EstilizarGrilla(DataGridView dgv)
    {
        dgv.BackgroundColor = CardWhite;
        dgv.BorderStyle = BorderStyle.None;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.GridColor = Color.FromArgb(240, 242, 245);
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 251);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextBody;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 8.5F);
        dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        dgv.ColumnHeadersHeight = 38;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        dgv.DefaultCellStyle.BackColor = CardWhite;
        dgv.DefaultCellStyle.ForeColor = TextDark;
        dgv.DefaultCellStyle.Font = FontNormal;
        dgv.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 240, 243);
        dgv.DefaultCellStyle.SelectionForeColor = BrandPrimary;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 253);

        dgv.RowHeadersVisible = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.ReadOnly = true;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.RowTemplate.Height = 38;
    }
}

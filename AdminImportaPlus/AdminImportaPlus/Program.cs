using System;
using System.Windows.Forms;
using AdminImportaPlus.GUI;

namespace AdminImportaPlus;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FrmLogin());
    }
}

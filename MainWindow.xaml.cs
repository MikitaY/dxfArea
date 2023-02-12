using Microsoft.Win32;
using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using Line = netDxf.Entities.Line;

namespace dxfArea
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new CalcArea(this.ResultLable);
        }

    }
}

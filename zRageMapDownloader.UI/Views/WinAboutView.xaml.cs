﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace zRageMapDownloader.Views
{
    /// <summary>
    /// Interaction logic for WinAboutView.xaml
    /// </summary>
    public partial class WinAboutView : Window
    {
        public WinAboutView()
        {
            InitializeComponent();

            txtVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('.', '0');
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
    }
}

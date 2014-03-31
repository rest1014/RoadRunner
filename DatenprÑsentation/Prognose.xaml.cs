/*
 * Created by SharpDevelop.
 * User: Lukas
 * Date: 01/31/2012
 * Time: 12:36
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Tool.Datenpräsentation
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class WindowPrognose : Window
	{
		public WindowPrognose()
		{
			InitializeComponent();
		}
		
		void button1_Click(object sender, RoutedEventArgs e)
		{
			DataContainer dc = DataContainer.Instance;
			
			dc.GetTeil(1).VerbrauchAktuell = Convert.ToInt32(comboBox1.Text);
            dc.GetTeil(1).VerbrauchPrognose1 = Convert.ToInt32(comboBox4.Text);
            dc.GetTeil(1).VerbrauchPrognose2 = Convert.ToInt32(comboBox7.Text);


            dc.GetTeil(2).VerbrauchAktuell = Convert.ToInt32(comboBox2.Text);
            dc.GetTeil(2).VerbrauchPrognose1 = Convert.ToInt32(comboBox5.Text);
            dc.GetTeil(2).VerbrauchPrognose2 = Convert.ToInt32(comboBox8.Text);


            dc.GetTeil(3).VerbrauchAktuell = Convert.ToInt32(comboBox3.Text);
            dc.GetTeil(3).VerbrauchPrognose1 = Convert.ToInt32(comboBox6.Text);
            dc.GetTeil(3).VerbrauchPrognose2 = Convert.ToInt32(comboBox9.Text);
            
			Window puffer = new Puffer();
            puffer.Show();
		}
		
	}
}
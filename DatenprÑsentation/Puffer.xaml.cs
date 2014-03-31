/*
 * Created by SharpDevelop.
 * User: Lukas
 * Date: 01/31/2012
 * Time: 13:06
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
using System.Diagnostics;

namespace Tool.Datenpräsentation
{
	/// <summary>
	/// Interaction logic for WindowPuffer.xaml
	/// </summary>
	public partial class Puffer : Window
	{
		public Puffer()
		{
			InitializeComponent();
		}
		
		
		void button1_Click(object sender, RoutedEventArgs e)
		{
			DataContainer dc = DataContainer.Instance;
			
			dc.SetPuffer(1, Convert.ToInt32(comboBox1.Text));
            dc.SetPuffer(2, Convert.ToInt32(comboBox2.Text));
            dc.SetPuffer(3, Convert.ToInt32(comboBox3.Text));
            // hier kommen die restlichen Puffereinstellungen her...
            
            //folgende Zeilen berechnen den ganzen Scheiß...
            dc.Optimieren();
            dc = DataContainer.Instance;

            InputOutput.WriteInput();
            
            Process.Start("notepad.exe", System.Windows.Forms.Application.StartupPath + "//derGanzeScheiß.xml");
		}
	}
}
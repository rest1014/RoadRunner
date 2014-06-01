using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;

namespace Tool.Datenpräsentation
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
    public partial class Prognoseeingabe : Window, INotifyPropertyChanged
	{
        DataContainer dc = DataContainer.Instance;

        public Prognoseeingabe()
		{
			InitializeComponent();

            Steps = new ObservableCollection<string>();
            Steps.Add("WILLKOMMEN");
            Steps.Add("STAMMDATEN");
            Steps.Add("VERKAUFSPROGNOSE");
            Steps.Add("PUFFER");
            Steps.Add("NACHBESTELLUNG");
            Steps.Add("AUSGABE");
            this.DataContext = this;

            Progress += 15;
		}

        private int m_progress;
        public int Progress
        {
            get { return m_progress; }
            set
            {
                m_progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public ObservableCollection<string> Steps
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void IncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            Progress += 15;
        }

        private void DecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            Progress -= 30;
        }


        /* //////////////
         * Back Buttons
         *//////////////
        void zurueckZuXMLeinlesen(object sender, RoutedEventArgs e)
        {
            Progress = 15;
        }

        /* //////////////
         * Eventhandler
         *//////////////
        void weiterZuPuffereingabe(object sender, RoutedEventArgs e)
        {
            this.IncreaseButton_Click(sender, e);

            lesePrognose();
        }

        private void lesePrognose()
        {
            dc.GetTeil(1).VerbrauchAktuell = Convert.ToInt32(comboBox1.Text);
            dc.GetTeil(1).VerbrauchPrognose1 = Convert.ToInt32(comboBox4.Text);
            dc.GetTeil(1).VerbrauchPrognose2 = Convert.ToInt32(comboBox7.Text);
            dc.GetTeil(1).VerbrauchPrognose3 = Convert.ToInt32(comboBox10.Text);

            dc.GetTeil(2).VerbrauchAktuell = Convert.ToInt32(comboBox2.Text);
            dc.GetTeil(2).VerbrauchPrognose1 = Convert.ToInt32(comboBox5.Text);
            dc.GetTeil(2).VerbrauchPrognose2 = Convert.ToInt32(comboBox8.Text);
            dc.GetTeil(2).VerbrauchPrognose3 = Convert.ToInt32(comboBox11.Text);

            dc.GetTeil(3).VerbrauchAktuell = Convert.ToInt32(comboBox3.Text);
            dc.GetTeil(3).VerbrauchPrognose1 = Convert.ToInt32(comboBox6.Text);
            dc.GetTeil(3).VerbrauchPrognose2 = Convert.ToInt32(comboBox9.Text);
            dc.GetTeil(3).VerbrauchPrognose3 = Convert.ToInt32(comboBox12.Text);
        }
	}
}
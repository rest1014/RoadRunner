using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data;
using System.IO;

namespace Tool
{
    public class InputOutput
    {

        /// Instanzieren des Datencontainers
        private static DataContainer instance = DataContainer.Instance;

        ///ReadFile Methode
        ///ließt Output Datei und füllt Datenkontainer
        public static void ReadFile()
        {
            XmlReader reader = null;
            if(instance.Xml){
                reader = XmlReader.Create(instance.OpenFile);

                if (!File.Exists(instance.OpenFile))
                {
                    throw new UnknownFileException(instance.OpenFile + " existiert nicht, wählen Sie eine gültige Datei!");
                }
            }
            else if (instance.Inet)
            {
                reader = XmlReader.Create(instance.OpenFile);
                
            }

            bool switchWarehousestock = false;              //Schalter für Lagereinlesen
            bool switchFutureinwardstockmovement = false;   //Schalter für zukünftigen Wareneingang
            bool switchWaitinglist = false;                 //Schalter für Waitinglist Workstations und Stock
            bool switchOrdersinwork = false;                //Schalter für OrdersinWork
            bool switchWorkplace = false;                   //Schalter für Arbeitsplätze

            Arbeitsplatz ap = instance.GetArbeitsplatz(1);

            while (reader.Read())
            {

                switch (reader.Name)
                {
                    case "PeriodResults":
                        if(instance.AktuellePeriode == -1)
                            instance.AktuellePeriode = Convert.ToInt32(reader.GetAttribute("period")) + 1;
                        break;

                    case "WarehouseStock":
                        switchWarehousestock = !switchWarehousestock;
                        break;

                    case "Entry":
                        if (switchWarehousestock)
                        {
                            instance.GetTeil(Convert.ToInt32(reader.GetAttribute(0))).Lagerstand = Convert.ToInt32(reader.GetAttribute(2));

                            instance.GetTeil(Convert.ToInt32(reader.GetAttribute(0))).Lagerpreis = Convert.ToDouble(reader.GetAttribute(4));
                        }
                        break;

                    case "FutureInwardStockMovements":
                        switchWarehousestock = !switchWarehousestock;
                        switchFutureinwardstockmovement = !switchFutureinwardstockmovement;
                        break;

                    case "InwardStockMovements":
                        if (switchFutureinwardstockmovement)
                        {
                            //int test = Convert.ToInt32(reader.GetAttribute(3));
                            //int test2 = Convert.ToInt32(reader.GetAttribute(3));
                            int teilnr = Convert.ToInt32(reader.GetAttribute(3));
                            (instance.GetTeil(teilnr) as Kaufteil).ErwarteteBestellung = Convert.ToInt32(reader.GetAttribute(4)) + (instance.GetTeil(teilnr) as Kaufteil).ErwarteteBestellung;
                            //Kaufteil kaudfds = instance.GetTeil(Convert.ToInt32(reader.GetAttribute(3))) as Kaufteil;

                            Kaufteil kaufds = instance.GetTeil(teilnr) as Kaufteil;
                            kaufds.addBestellung(instance.AktuellePeriode, Convert.ToInt32(reader.GetAttribute(0)), Convert.ToInt32(reader.GetAttribute(2)), Convert.ToInt32(reader.GetAttribute(4)));
                        }
                        break;

                    case "WorkplaceWaitinglist":
                        switchFutureinwardstockmovement = !switchFutureinwardstockmovement;
                        switchWaitinglist = !switchWaitinglist;

                        break;

                    case "WorkplaceCosts":
                        switchWorkplace = false;
                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            break;
                        }
                        if (switchWaitinglist)
                        {
                            int i = Convert.ToInt32(reader.GetAttribute(1));
                            if (Convert.ToInt32(reader.GetAttribute(1)) > 0)
                            {
                                switchWorkplace = !switchWorkplace;
                            }

                            ap = instance.GetArbeitsplatz(Convert.ToInt32(reader.GetAttribute(0)));
                        }
                        if (switchOrdersinwork)
                        {

                            ap = instance.GetArbeitsplatz(Convert.ToInt32(reader.GetAttribute(0)));
                            ap.AddWarteschlange(Convert.ToInt32(reader.GetAttribute(4)), Convert.ToInt32(reader.GetAttribute(5)), true);

                        }
                        break;

                    case "StockWaitinglist":

                        if (switchWorkplace)
                        {
                            ap.AddWarteschlange(Convert.ToInt32(reader.GetAttribute(4)), Convert.ToInt32(reader.GetAttribute(5)), true);

                        }

                        break;

                    case "OrdersBeeingProcessed":
                        switchWaitinglist = !switchWaitinglist;
                        switchOrdersinwork = !switchOrdersinwork;
                        break;

                    case "ProcessedOrders":
                        switchOrdersinwork = !switchOrdersinwork;
                        break;
                }

            }

            reader.Close();

        }				


 
        /// WriteInput Methode
        /// schreibt die ScsimInput.xml zum Einlesen in SCSIM
        public static void WriteInput()
        {
        	CreateOrResetFile();
        		
            //Dateianfang
            WriteFile("<input>");
            
            //Qualitaetskontrolle
            WriteFile("<qualitycontrol type=\"no\" losequantity=\"0\" delay=\"8\"/>");
            
            WriteVerkaufswuensche();
			WriteDirekteVerkaeufe();
			WriteBestellungen();
			WriteProduktionsauftraege();
			WriteArbeitsplaetze();

			//Dateiende
            WriteFile("</input>");
        }
        
        //Datei erstellen. Wenn bereits vorhanden, Inhalt loeschen
        private static void CreateOrResetFile()
        {
        	StreamWriter datei;
//        	datei = File.CreateText(DataContainer.Instance.SaveFile);
            datei = File.CreateText(DataContainer.Instance.SaveInputXML);
        	datei.Close();
        }

        private static void WriteVerkaufswuensche()
        {
        
            WriteFile("<sellwish>");
            WriteFile("<item article=\"1\" quantity=\""+ (instance.GetTeil(1) as ETeil).VerbrauchAktuell +"\"/>");
            WriteFile("<item article=\"2\" quantity=\""+ (instance.GetTeil(2) as ETeil).VerbrauchAktuell +"\"/>");
			WriteFile("<item article=\"3\" quantity=\""+ (instance.GetTeil(3) as ETeil).VerbrauchAktuell +"\"/>");
            WriteFile("</sellwish>");
        }
        
        private static void WriteDirekteVerkaeufe()
        {
        //Direkte Verkaeufe
			WriteFile("<selldirect>");
			WriteFile("<item article=\"1\" quantity=\"0\" price=\"0.0\" penalty=\"0.0\"/>");
			WriteFile("<item article=\"2\" quantity=\"0\" price=\"0.0\" penalty=\"0.0\"/>");
			WriteFile("<item article=\"3\" quantity=\"0\" price=\"0.0\" penalty=\"0.0\"/>");
			WriteFile("</selldirect>");
        }
        
        private static void WriteBestellungen()
        {
        //Bestellungen
            WriteFile("<orderlist>");

            foreach(Bestellposition bp in instance.Bestellung)
            {
            	WriteFile("<order article=\"" + bp.Kaufteil.Nummer + "\" quantity=\""  + bp.Menge + "\" modus=\"" + bp.OutputEil + "\"/>");
            }

            WriteFile("</orderlist>");
        }
        
        private static void WriteProduktionsauftraege()
        {
        //Produktionsaufträge
            WriteFile("<productionlist>");

            foreach(int z in instance.Reihenfolge)
            {
            	if ((instance.GetTeil(z) as ETeil).Produktionsmenge == 0) {
            		WriteFile("<production article=\"" + z + "\" quantity=\"10\"/>");
            	} else {
            	WriteFile("<production article=\"" + z + "\" quantity=\"" + (instance.GetTeil(z) as ETeil).Produktionsmenge + "\"/>");
            	}
            }
            
            WriteFile("</productionlist>");
        }
        
        //Arbeitsplatz Ueberstunden und Schichten
        private static void WriteArbeitsplaetze()
        {
            WriteFile("<workingtimelist>");

            for (int i = 1; i <= 15; i++)
            {
                if (i == 5)
                {
                    i++;
                }
				WriteFile("<workingtime station=\"" + i + "\" shift=\"" +  instance.GetArbeitsplatz(i).Schichten + "\" overtime=\"" +instance.GetArbeitsplatz(i).UeberMin + "\"/>");
           }

            WriteFile("</workingtimelist>");
        }

        /// <summary>
        /// WriteFile Methode
        /// Hilfsmethode zum schreiben von Zeilen in eine Datei
        /// </summary>
        /// <param name="Inhalt">Inhalt der Datei</param>
        /// <param name="Name">Dateiname</param>
        private static void WriteFile(string Inhalt)
        {
        	StreamWriter datei;
//        	datei = File.AppendText(DataContainer.Instance.SaveFile);
            datei = File.AppendText(DataContainer.Instance.SaveInputXML);
            datei.WriteLine(Inhalt);
            datei.Close();
        }

    }
}

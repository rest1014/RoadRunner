using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;


namespace Tool
{
    public class DataContainer
    {
        private static DataContainer instance = new DataContainer();
        private List<Bestellposition> bestellungen;
        private Dictionary<int, Teil> teile;
        private Dictionary<int, Arbeitsplatz> arbeitsplatz;
        private bool sonderproduktion = false;
		private bool pufferChanged = false;
        private int[] reihenfolge;

        public double minLagerwertEnd = 0.0;
        public double normalLagerwertEnd = 0.0;
        public double maxLagerwertEnd = 0.0;
        public double lagerwertStart = 0.0;

        string openFile;
        string saveFile;
        string saveInputXML = "";
        
        private bool ueberstundenErlaubt = true;

        private Produktionsplanung prod;

        private int periode = -1;
        private bool xml = true;

        public int AktuellePeriode
        {
            get { return periode; }
            set { periode = value; }
        }

        public bool Xml
        {
            get { return xml; }
            set { xml = value; }
        }
        private bool inet = false;

        public bool Inet
        {
            get { return inet; }
            set { inet = value; }
        }

        private DataContainer()
        {
            this.bestellungen = new List<Bestellposition>();
            this.teile = new Dictionary<int, Teil>();
            this.arbeitsplatz = new Dictionary<int, Arbeitsplatz>();
            openFile = Application.StartupPath + "//output.jsp.xml";
            saveFile = Application.StartupPath + "//input.xml";
        }


        #region Getter und Setter

        /// <summary>
        /// Gets the Data Cointainer instance.
        /// </summary>
        /// <value>The instance.</value>
        public static DataContainer Instance
        {
            get
            {
                return instance;
            }
        }


        public int[] Reihenfolge
        {
            get
            {
                return this.reihenfolge;
            }
            set
            {
                this.reihenfolge = value;
            }
        }


        /// <summary>
        ///Liste aller kaufteil
        /// </summary>
        /// <value>The kaufteil list.</value>
        public List<Kaufteil> KaufteilList
        {
            get
            {
                List<Kaufteil> res = new List<Kaufteil>();
                foreach (KeyValuePair<int, Teil> kvp in this.teile)
                {
                    if (kvp.Value is Kaufteil)
                    {
                        res.Add(kvp.Value as Kaufteil);
                    }
                }
                return res;
            }
        }


        public List<Bestellposition> Bestellung
        {
            get
            {
                return this.bestellungen;
            }
        }


        /// <summary>
        /// Liste Aller Eteile
        /// </summary>
        /// <value>The E teil list.</value>
        public List<ETeil> ETeilList
        {
            get
            {
                List<ETeil> res = new List<ETeil>();
                foreach (KeyValuePair<int, Teil> kvp in this.teile)
                {
                    if (kvp.Value is ETeil)
                    {
                        res.Add(kvp.Value as ETeil);
                    }
                }
                return res;
            }
        }

        /// <summary>
        /// Pfad zu der auszulesende Datei
        /// </summary>
        /// <value>pfad</value>
        public string OpenFile
        {
            get { return this.openFile; }
            set { this.openFile = value; }
        }

        /// <summary>
        /// Pfad zum speichern
        /// </summary>
        /// <value>The save file.</value>
        public string SaveFile
        {
            get { return this.saveFile; }
            set { 
                    this.saveFile = value;
                    this.saveFile += @"\\ScsimInput.xml";
                }
        }

        public string SaveInputXML
        {
            get { return this.saveInputXML; }
            set { this.saveInputXML = value; }
        }

        /// <summary>
        /// WErt der angibt ob in dieser Periode die KDH teile hergestellt werden
        /// </summary>
        /// <value><c>true</c> if sonderproduktion; otherwise, <c>false</c>.</value>
        public bool Sonderproduktion
        {
            get { return this.sonderproduktion; }
            set { this.sonderproduktion = value; }
        }

        public bool UeberstundenErlaubt
        {
            get { return this.ueberstundenErlaubt; }
            set { this.ueberstundenErlaubt = value; }
        }

        /// <summary>
        /// Gets the teil.
        /// </summary>
        /// <param name="nr">Nummer des TEils.</param>
        /// <returns></returns>
        public Teil GetTeil(int nr)
        {
            if (this.teile.ContainsKey(nr))
            {
                return this.teile[nr];
            }
            else
            {
                throw new UnknownTeilException(nr);
            }
        }

        /// <summary>
        /// Setzt den Pufferwert für ein Teil
        /// </summary>
        /// <param name="nr">Teilnummer</param>
        /// <param name="wert">zu setztender Wert</param>
        public void SetPuffer(int nr, int wert)
        {
            if (!this.teile.ContainsKey(nr))
            {
                throw new UnknownTeilException(nr);
            }

            if (wert < 0)
            {
                throw new InputException(string.Format("Der Pufferwert für das Teil Nummer {0} darf nicht negativ sein", nr));
            }

            if (this.teile[nr].Pufferwert < 0)
            {
                this.teile[nr].Pufferwert = wert;
            }

            if (this.teile[nr].Pufferwert != wert)
            {
                this.teile[nr].Pufferwert = wert;
                this.pufferChanged = true;
            }
        }

        /// <summary>
        /// Gets the data table kaufteil.
        /// </summary>
        /// <value>The data table kaufteil.</value>
        public DataTable DataTableKaufteil
        {
            get
            {
                DataTable table = new DataTable();
                table.Columns.Add(new DataColumn("Kaufteil"));
                table.Columns.Add(new DataColumn("Anzahl"));
                table.Columns.Add(new DataColumn("Typ"));
                foreach (Bestellposition pos in this.bestellungen)
                {
                    DataRow row = table.NewRow();
                    row["Kaufteil"] = pos.Kaufteil.Nummer;
                    row["Anzahl"] = pos.Menge;
                    if (pos.Eil)
                    {
                        row["Typ"] = "Eil";
                    }
                    else
                    {
                        row["Typ"] = "Normal";
                    }
                    table.Rows.Add(row);
                }
                return table;
            }
        }

        /// <summary>
        /// Reloads Bestellung
        /// </summary>
        /// <param name="table">The table.</param>
        public void ReloadDataTableKaufteil(DataTable table)
        {
            this.bestellungen.Clear();
            foreach (DataRow row in table.Rows)
            {
                Teil teil = this.teile[Convert.ToInt32(row["Kaufteil"])];
                int menge = Convert.ToInt32(row["Anzahl"]);
                if (row["Typ"].ToString().Equals("Eil"))
                {
                    this.bestellungen.Add(new Bestellposition(teil as Kaufteil, menge, true));
                }
                else
                {
                    this.bestellungen.Add(new Bestellposition(teil as Kaufteil, menge, false));
                }

            }
        }



        public DataTable DataTableProduktion
        {
            get
            {
                DataTable table = new DataTable();
                table.Columns.Add(new DataColumn("Teil"));
                table.Columns.Add(new DataColumn("Menge"));
                
                
                foreach (int pos in this.reihenfolge)
                {
                    DataRow row = table.NewRow();
                    row["Teil"] = pos;
                    row["Menge"] = (this.teile[pos] as ETeil).Produktionsmenge;
                    table.Rows.Add(row);  
                }
                return table;
            }
        }

        public void ReloadDataTableProduktion(DataTable table)
        {
            this.reihenfolge = new int[table.Rows.Count];
            int count = 0;
            foreach (DataRow row in table.Rows)
            {
                this.reihenfolge[count] = Convert.ToInt32(row[0]);
                count++;

                if ((this.teile[Convert.ToInt32(row[0])] as ETeil).Produktionsmenge < Convert.ToInt32(row[1]))
                {
                    prod.Nachpruefen(this.teile[Convert.ToInt32(row[0])], Convert.ToInt32(row[1]));
                }
                else
                {
                    (this.teile[Convert.ToInt32(row[0])] as ETeil).Produktionsmenge = Convert.ToInt32(row[1]);
                }
            }

        }


        public DataTable DataTableArbeitsPlatz
        {
            get
            {
                DataTable table = new DataTable();
                table.Columns.Add(new DataColumn("Arbeitsplatz"));
                table.Columns.Add(new DataColumn("Schichten"));
                table.Columns.Add(new DataColumn("Minuten"));

                foreach (KeyValuePair<int, Arbeitsplatz> platz in this.arbeitsplatz)
                {
                    DataRow row = table.NewRow();
                    row["Arbeitsplatz"] = platz.Key;
                    row["Schichten"] = platz.Value.Schichten;
                    row["Minuten"] = platz.Value.UeberMin;

                    table.Rows.Add(row);
                }
                return table;
            }
        }

        public void ReloadDataTableArbeitsPlatz(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                this.arbeitsplatz[Convert.ToInt32(row[0])].Schichten = Convert.ToInt32(row[1]);
                this.arbeitsplatz[Convert.ToInt32(row[0])].UeberMin = Convert.ToInt32(row[2]);
            }
        }


        /// <summary>
        /// Einfügen einer neuen Bestellposition bestellposition.
        /// </summary>
        /// <param name="bpos">The bpos.</param>
        public void AddBestellposition(Bestellposition bpos)
        {
            this.bestellungen.Add(bpos);
        }

        /// <summary>
        /// Adds the bestellposition.
        /// </summary>
        /// <param name="kteilNr">The kteil nr.</param>
        /// <param name="menge">The menge.</param>
        /// <param name="eil">if set to <c>true</c> [eil].</param>
        public void AddBestellposition(int kteilNr, int menge, bool eil)
        {
            if (!this.teile.ContainsKey(kteilNr))
            {
                throw new UnknownTeilException(kteilNr);
            }
            if (!(this.teile[kteilNr] is Kaufteil))
            {
                throw new InputException(string.Format("Teil mi der Nummer {0} ist kein Kaufteil!", kteilNr));
            }

            this.bestellungen.Add(new Bestellposition(this.teile[kteilNr] as Kaufteil, menge, eil));
        }

        /// <summary>
        /// Sets the preis.
        /// </summary>
        /// <param name="kostenDict">The kosten dict.</param>
        public void SetPreis(Dictionary<int, double> kostenDict)
        {
            foreach (KeyValuePair<int, double> kostKvp in kostenDict)
            {
                if (!this.teile.ContainsKey(kostKvp.Key))
                {
                    throw new UnknownTeilException(kostKvp.Key);
                }
                if (!(this.teile[kostKvp.Key] is Kaufteil))
                {
                    throw new InputException(string.Format("Teil mi der Nummer {0} ist kein Kaufteil!", kostKvp.Key));
                }
                (this.teile[kostKvp.Key] as Kaufteil).Preis = kostKvp.Value;
            }
        }

        /// <summary>
        /// Fügt ein neues K teil ein
        /// </summary>
        /// <param name="teil">Kaufteil Objekt</param>
        public void AddKTeil(Kaufteil teil)
        {
            if (!this.teile.ContainsKey(teil.Nummer))
            {
                this.teile[teil.Nummer] = teil;
            }
            else
            {
                throw new InputException(string.Format("Die Teilnummer {0} wird doppelt verwendet", teil.Nummer));
            }
        }

        /// <summary>
        /// leigt ein neues Kteil an
        /// </summary>
        /// <param name="nr">Teil Nummer</param>
        /// <param name="preis">Einkaufspreispreis</param>
        /// <param name="bestellk">Bestellkosten</param>
        /// <param name="ldauer">Liefer dauer</param>
        /// <param name="abweichung">Abweichung.</param>
        /// <param name="diskontmenge">Diskontmenge.</param>
        /// <param name="bestand">Lagerbestand.</param>
        /// <param name="verw">Verwendung{K,D,H,KDH}</param>
        public void NewTeil(int nr, string bezeichnung, double preis, double bestellk, double ldauer, double abweichung, int diskontmenge, int bestand, string verw)
        {
            if (!this.teile.ContainsKey(nr))
            {
                Kaufteil kt = new Kaufteil(nr, bezeichnung);
                kt.Preis = preis;
                kt.Bestellkosten = bestellk;
                kt.Lieferdauer = ldauer;
                kt.Abweichung_lieferdauer = abweichung;
                kt.Diskontmenge = diskontmenge;
                kt.Lagerstand = bestand;
                kt.Verwendung = verw;
                this.teile[nr] = kt;

            }
            else
            {
                throw new InputException(string.Format("Es ist bereits ein Teil mit der nummer {0} vorhandet", nr));
            }
        }

        /// <summary>
        /// Fügt ein neues E teil hinzu
        /// </summary>
        /// <param name="teil">Objekt der KLasse E teil</param>
        public void AddETeil(ETeil teil)
        {
            if (!this.teile.ContainsKey(teil.Nummer))
            {
                this.teile[teil.Nummer] = teil;
            }
            else
            {
                throw new InputException(string.Format("Die Teilnummer {0} wird doppelt verwendet", teil.Nummer));
            }
        }

        /// <summary>
        /// Legt ein neues E teil ein
        /// </summary>
        /// <param name="nr">Teilnummer</param>
        /// <param name="bestand">Bestand</param>
        /// <param name="verw">Verwendung{K,D,H,KDH}</param>
        public void NewTeil(int nr, string bezeichnung, int bestand, string verw)
        {
            if (!this.teile.ContainsKey(nr))
            {
                ETeil et = new ETeil(nr, bezeichnung);
                et.Lagerstand = bestand;
                et.Verwendung = verw;
                this.teile[nr] = et;

            }
            else
            {
                throw new InputException(string.Format("Es ist bereits ein Teil mit der nummer {0} vorhandet", nr));
            }
        }


        /// <summary>
        ///Gibt das Arbeitsplatzobjekt mit der nummer nr
        /// </summary>
        /// <param name="nr">Arbeitsplatz nummer</param>
        /// <returns></returns>
        public Arbeitsplatz GetArbeitsplatz(int nr)
        {
            return this.arbeitsplatz[nr];
        }


        /// <summary>
        /// Gets the arbeitsplatz list.
        /// </summary>
        /// <value>The arbeitsplatz list.</value>
        public List<Arbeitsplatz> ArbeitsplatzList
        {
            get
            {
                List<Arbeitsplatz> res = new List<Arbeitsplatz>();

                foreach (KeyValuePair<int, Arbeitsplatz> kvp in this.arbeitsplatz)
                {
                    res.Add(kvp.Value);
                }
                return res;

            }
        }

        public void NeuArbeitsplatz(Arbeitsplatz arb)
        {
            if (!this.arbeitsplatz.ContainsKey(arb.Nummer))
            {
                this.arbeitsplatz[arb.Nummer] = arb;
            }
            else
            {
                throw new Exception("Arbeitsplatz mit der Nummer " + arb.Nummer + " bereits vorhanden");
            }
        }

        public void Optimieren()
        {
            prod = new Produktionsplanung();
            Bestellverwaltung bv = new Bestellverwaltung();
           
            //prod.Aufloesen();
            
            prod.Planen();

            bv.erzeugen_bestellungenlist();
            
        }

        public void ClearData()
        {
            prod = new Produktionsplanung();
            prod.ClearData();

            Bestellverwaltung bv = new Bestellverwaltung();
            bv.ClearData();
        }

        public void Reset()
        {
            foreach (KeyValuePair<int, Arbeitsplatz> arbl in this.arbeitsplatz)
            {
                arbl.Value.UeberMin = 0;
                arbl.Value.Schichten = 1;

            }
            foreach (ETeil teil in this.ETeilList)
            {
                teil.Produktionsmenge = 0;
                teil.VerbrauchAktuell = 0;
                teil.VerbrauchPrognose1 = 0;
                teil.VerbrauchPrognose2 = 0;
            }
        }

        #endregion


    }
}

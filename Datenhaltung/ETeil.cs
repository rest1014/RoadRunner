using System;
using System.Collections.Generic;
using System.Text;

namespace Tool
{
    public class ETeil : Teil
    {
        public Dictionary<Teil, int> zusammensetzung;
        List<int> benutzteArbeitsplaetze;
        int produktion = 0;
        int inWarteschlange = 0;
        int inBearbeitung = 0;
        int kategorie = 0;
        Dictionary<int, int> pos;

        List<ETeil> istTeil = null;

        public ETeil(int nr, string bez)
            : base(nr, bez)
        {
            this.zusammensetzung = new Dictionary<Teil, int>();
            this.benutzteArbeitsplaetze = new List<int>();
            this.pos = new Dictionary<int, int>();
        }

        public List<Arbeitsplatz> BenutzteArbeitsplaetze
        {
            get
            {
                List<Arbeitsplatz> res = new List<Arbeitsplatz>();
                foreach (int arpl in this.benutzteArbeitsplaetze)
                {
                    res.Add(DataContainer.Instance.GetArbeitsplatz(arpl));
                }
                return res;
            }
        }

        public void AddArbeitsplatz(int nr)
        {
            this.benutzteArbeitsplaetze.Add(nr);
        }

        public Dictionary<Teil, int> Zusammensetzung
        {
            get
            {
                return this.zusammensetzung;
            }
        }


        public void AddBestandteil(Teil t, int menge)
        {
            this.zusammensetzung[t] = menge; 
        }

        public void AddBestandteil(int t, int menge)
        {
            DataContainer cont =DataContainer.Instance;
            this.zusammensetzung[cont.GetTeil(t)] = menge;
        }

        public int Produktionsmenge
        {
            get { return this.produktion; }
            set
            {
                this.produktion = value;
                //aktualisiereMengen(value);
            }
        }

       /* public void aktualisiereMengen(int value)
        {
            foreach (KeyValuePair<Teil, int> kvp in this.zusammensetzung)
            {
                kvp.Key.VerbrauchAktuell += kvp.Value * value;
                
                if(kvp.Key is ETeil)
                    Console.WriteLine("ETeil[" + kvp.Key.Nummer+ "] Verbrauch: " + kvp.Key.VerbrauchAktuell);

                //this.produktion = kvp.Value * this.produktion;

                if (kvp.Key is ETeil)
                {
                    (kvp.Key as ETeil).aktualisiereMengen(value);
                }
            }
        }*/

        /// <summary>
        /// Position und Menge in der Reihenfolgen planung
        /// </summary>
        /// <value>The position.</value>
        public Dictionary<int, int> Position
        {
            get
            {
                return this.pos;
            }
        }

        /// <summary>
        /// wo wird das Teil verwendet.
        /// </summary>
        /// <value>Ist</value>
        public List<ETeil> IstTeilVon
        {
            get
            {
                if (this.istTeil == null)
                {
                    List<ETeil> res = new List<ETeil>();
                    foreach (ETeil teil in DataContainer.Instance.ETeilList)
                    {
                        if (teil.Zusammensetzung.ContainsKey(teil))
                        {
                            res.Add(teil);
                        }
                    }
                    this.istTeil = res;
                }
                return this.istTeil;
            }
        }

        /// <summary>
        /// Menge der Eteile in Wartschlange.
        /// </summary>
        /// <value>Menge in der Warteschlange.</value>
        public int InWarteschlange
        {
            get
            {
                return this.inWarteschlange;
            }
            set
            {
                this.inWarteschlange = value;
            }
        }

        /// <summary>
        /// Menge der Eteile in Bearbeitung.
        /// </summary>
        /// <value>Menge in  Bearbeitung.</value>
        public int InBearbeitung
        {
            get
            {
                return this.inBearbeitung;
            }
            set
            {
                this.inBearbeitung = value;
            }
        }

        /// <summary>
        /// Gets or sets the kategorie des Teil.
        /// 1: ProduktionsTeil
        /// 2: 
        /// </summary>
        /// <value>The kategorie.</value>
        public int Kategorie
        {

            get
            {
                return this.kategorie;
            }
            set
            {
                this.kategorie = value;
            }
        }

        public bool Equals(ETeil et)
        {
            if (this.nr == et.Nummer)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
